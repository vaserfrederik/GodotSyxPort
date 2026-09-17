using System.Collections.Generic;
using snake2d;
using util.gui.misc;
using view.main;

namespace settlement.room.main.construction
{
    public class CONSTRUCTION
    {
        private readonly ConstructionBlueprint construction;
        private readonly Coo ctmp = new Coo();

        public CONSTRUCTION(ROOMS m)
        {
            construction = new ConstructionBlueprint(m);
        }

        public MAP_BOOLEAN isser = new MAP_BOOLEAN()
        {
            public bool Is(int tx, int ty) => SETT.ROOMS().map.blueprint.Get(tx, ty) == construction;
            public bool Is(int tile) => SETT.ROOMS().map.blueprint.Get(tile) == construction;
        };

        public void BreakIt(TmpArea area, ConstructionInit init, int tx, int ty)
        {
            if (init.b.Resources() == 0 && !init.b.MustBeIndoors() && !init.b.NeedFlooring())
            {
                area.Clear();
                return;
            }

            ConstructionData.dBroken.Set(area, tx, ty, 1);
            ConstructionData.dFloored.Set(area, tx, ty, 0);
            FurnisherItem it = SETT.ROOMS().fData.item.Get(tx, ty);
            if (it != null)
            {
                COORDINATE c = SETT.ROOMS().fData.itemX1Y1(tx, ty, ctmp);
                if (c != null)
                {
                    for (int y = 0; y < it.Height(); y++)
                    {
                        for (int x = 0; x < it.Width(); x++)
                        {
                            if (it.Get(x, y) != null)
                            {
                                int dx = x + c.X();
                                int dy = y + c.Y();
                                if (area.Is(dx, dy))
                                {
                                    ConstructionData.dBroken.Set(area, dx, dy, 1);
                                }
                            }
                        }
                    }
                }
            }

            pcreate(area, init);
        }

        public void CreateClean(TmpArea area, ConstructionInit init)
        {
            foreach (COORDINATE c in area.Body())
            {
                if (!area.Is(c))
                    continue;
                SETT.ROOMS().data.Set(area, c.X(), c.Y(), 0);
            }
            pcreate(area, init);
        }

        public void CreateWithConstructionData(TmpArea area, ConstructionInit init)
        {
            foreach (COORDINATE c in area.Body())
            {
                if (!area.Is(c))
                    continue;
                int d = ConstructionData.dData.Get(c);
                SETT.ROOMS().data.Set(area, c.X(), c.Y(), 0);
                ConstructionData.dData.Set(area, c, d);
            }
            pcreate(area, init);
        }

        private void pcreate(TmpArea area, ConstructionInit init)
        {
            if (init.b.Resources() == 0 && !init.b.MustBeIndoors() && !init.b.NeedFlooring())
            {
                RoomInit i = new RoomInit(init.b.Blue(), 0);
                ppCreate(area, i, init.b, init.upgrade, init.state);
            }
            else
            {
                construction.Create(area, init);
            }
            area.Clear();
        }

        private static Rec tmp = new Rec();

        public static void ppCreate(TmpArea a, RoomInit init, Furnisher blueprint, int upgrade, RoomState state)
        {
            int x1 = a.Mx();
            int y1 = a.My();
            tmp.Set(a);
            blueprint.Create(a, init);
            a.Clear();

            Room r = SETT.ROOMS().map.Get(x1, y1);
            r.UpgradeSet(x1, y1, upgrade);
            if (state != null)
            {
                state.Apply(SETT.ROOMS().map.Get(x1, y1), x1, y1);
            }

            if (r != null)
            {
                foreach (COORDINATE c in tmp)
                {
                    if (r.IsSame(x1, y1, c.X(), c.Y()) && SETT.TERRAIN().Get(c) is TILE_FIXABLE fixable)
                    {
                        fixable.GetTerrain(c.X(), c.Y()).PlaceFixed(c.X(), c.Y());
                    }
                }
            }
        }

        public bool IsRepair(int tx, int ty)
        {
            ConstructionInstance i = construction.Get(tx, ty);
            return (i != null && i.broken);
        }

        public void Construct(int tx, int ty)
        {
            construction.Construct(tx, ty);
        }

        public TBuilding Structure(int tx, int ty)
        {
            Room room = ROOMS().map.Get(tx, ty);
            if (room is ConstructionInstance instance)
            {
                return instance.structure();
            }
            return null;
        }

        public int Instances()
        {
            return construction.all.Count;
        }

        public int Area(bool countFarm)
        {
            int a = 0;
            foreach (ConstructionInstance i in construction.all)
            {
                if (i.blueprint == null)
                    continue;
                if (countFarm && i.blueprint.blue() is ROOM_FARM)
                    a += (int)(0.1 * i.area());
                else
                    a += i.area();
            }

            return a;
        }

        public void RenderButt(SPRITE_RENDERER r, int x1, int cy, int ins)
        {
            if (ins >= 0 && ins < construction.all.Count)
            {
                ConstructionInstance i = construction.all[ins];
                construction.hoverer.RenderButt(i, r, x1, cy);
            }
        }

        public void HoverButt(GBox box, int ins)
        {
            if (ins >= 0 && ins < construction.all.Count)
            {
                ConstructionInstance i = construction.all[ins];
                construction.hoverer.Hover(box, i, i.MX(), i.MY());
                SETT.OVERLAY().Add(i.MX(), i.MY());
            }
        }

        public void ClickButt(int ins)
        {
            if (ins >= 0 && ins < construction.all.Count)
            {
                ConstructionInstance i = construction.all[ins];
                VIEW.s().getWindow().centererTile.Set(i.body().CX(), i.body().CY());
            }
        }

        public RoomState State(int tx, int ty)
        {
            if (isser.Is(tx, ty))
            {
                ConstructionInstance ins = (ConstructionInstance)SETT.ROOMS().map.Get(tx, ty);
                return ins.state;
            }
            return null;
        }
    }
}