using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Water
{
    using Settlement.Environment;
    using Settlement.Main;
    using Settlement.Maintenance;
    using Settlement.Overlay;
    using Settlement.Path;
    using Settlement.Room.Main;
    using Settlement.Room.Sprite;
    using Settlement.Room.Water;
    using Snake2D;
    using Util;
    using Util.Gui.Misc;
    using Util.Rendering;
    using Util.Text;
    using View.Sett.Ui.Room;
    using View.Tool;

    public class Canal : RoomBlueprintImp, IROOM_PUMPABLE
    {
        public readonly CanalConstructor Constructor;
        public readonly CanalInstance Instance;

        private static readonly CharSequence ¤¤problem = "Currently not operational. Make sure it's connected to a water pump's outlet, and that the connected pumps produce enough flow to reach it.";
        private static readonly CharSequence ¤¤ok = "Operational";

        static Canal()
        {
            D.ts(typeof(Canal));
        }

        public Canal(RoomInitData init, RoomCategorySub cat) : base(init, 0, "_WATERCANAL", cat)
        {
            this.Instance = new CanalInstance(init.M, this);
            Constructor = new CanalConstructor(init);
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new UIRoomModule
            {
                Hover = (box, room, rx, ry) => hover(box, rx, ry)
            });
        }

        public override SFinderFindable Service(int tx, int ty)
        {
            return null;
        }

        public override Furnisher Constructor()
        {
            return Constructor;
        }

        protected override void Save(FilePutter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void Load(FileGetter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void Clear()
        {
            // TODO Auto-generated method stub
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public static void Hover(GUI_BOX box, int tx, int ty)
        {
            GBox b = (GBox)box;
            b.NL();
            bool flow = SETT.ROOMS().data.Get(tx, ty) != 0;
            if (!flow)
            {
                b.Add(b.text().warnify().add(¤¤problem));
            }
            else
                b.Add(b.text().normalify2().add(¤¤ok));
            b.NL();
            PumpGui.HoverSystem(b, tx, ty);
        }

        private class CanalConstructor : Furnisher
        {
            private readonly Overlay overlay = new Overlay();

            private readonly RoomSprite sp;
            protected CanalConstructor(RoomInitData init) : base(init, 1, 0)
            {
                sp = new WSprite.RSprite(((Canal)blue()), Instance.pump, true);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    new FurnisherItemTile[] { new FurnisherItemTile(this, false, sp, AVAILABILITY.AVOID_PASS, false) },
                }, 1);

                Flush(1, 0);
            }

            public override bool JoinsWithFloor()
            {
                return true;
            }

            public override bool UsesArea()
            {
                return false;
            }

            public override bool MustBeIndoors()
            {
                return false;
            }

            public override Room Create(TmpArea area, RoomInit init)
            {
                int tx = area.MX();
                int ty = area.MY();
                Instance.Place(area);
                SETT.ROOMS().fData.spriteData2.Set(tx, ty, 1);
                foreach (DIR d in DIR.ORTHO)
                {
                    if (((Canal)blue()).Is(tx, ty, d))
                    {
                        SETT.ROOMS().fData.spriteData2.Set(tx, ty, d, 1);
                    }
                }

                return SETT.ROOMS().map.Get(tx, ty);
            }

            public override RoomBlueprintImp Blue()
            {
                return (Canal)blue();
            }

            public override bool EnvValue(SettEnv e, SettEnvValue v, int tx, int ty)
            {
                if (((Canal)blue()).Is(tx, ty) && SETT.ROOMS().data.Get(tx, ty) != 0 && e == SETT.ENV().map.WATER_SWEET)
                {
                    v.value = 1;
                    v.radius = 1;
                    return true;
                }
                return false;
            }

            public override bool EnvValue(SettEnv e)
            {
                return e == SETT.ENV().map.WATER_SWEET;
            }

            public override bool RemoveFertility()
            {
                return false;
            }

            public override bool IsSpecialAreaPlacable()
            {
                return true;
            }

            public override void RenderExtra(SPRITE_RENDERER r, int x, int y, int tx, int ty, int rx, int ry, FurnisherItem item)
            {
                base.RenderExtra(r, x, y, tx, ty, rx, ry, item);
            }

            public override Addable Overlay()
            {
                return overlay;
            }
        }

        private class Overlay : Addable
        {
            public Overlay() : base(true, false)
            {
            }

            public override void RenderBelow(Renderer r, RenderIterator it)
            {
                if (SETT.ROOMS().construction.isser.Is(it.Tile()))
                    return;
                double d = SETT.GROUND().MOISTURE_TOT.Get(it.Tile());
                if (GUTIL.Flooder().HasBeenPushed(it.Tx(), it.Ty()))
                    d += 2 * (1 - CLAMP.D(GUTIL.Flooder().GetValue(it.Tx(), it.Ty()) / 15.0, 0, 1));

                d = CLAMP.D(d, 0, 1);
                RenderUnder(d, r, it, false);
                if (d > 0.75)
                {
                    d = (d - 0.75) * 4;
                    RenderPluses(d, r, it);
                }
            }

            public override void InitBelow(RenderData data)
            {
                foreach (COORDINATE c in data.TBounds())
                {
                    if (SETT.IN_BOUNDS(c))
                        GUTIL.Flooder().SetValue2(c, 0);
                }

                AREA a = ToolPlacer.Area();
                if (a.Area() == 0)
                    return;

                GUTIL.Flooder().Init(this);

                foreach (COORDINATE c in a.Body())
                {
                    if (a.Contains(c))
                    {
                        GUTIL.Flooder().Push(c);
                    }
                }

                GUTIL.Flooder().FinalizePush();
            }

            public override void RenderAbove(Renderer r, RenderIterator it)
            {
                // TODO: Implement rendering above logic if needed
            }
        }

        private class CanalInstance : RoomInstance
        {
            protected override object ReadResolve()
            {
                return blueprintI().Instance;
            }

            public CanalInstance(ROOMS M, RoomBlueprint p) : base(M, p)
            {
            }

            public override Canal BlueprintI()
            {
                return (Canal)blueprint();
            }

            public override ROOM_DEGRADER Degrader(int tx, int ty)
            {
                return null;
            }

            public override void UpdateTileDay(int tx, int ty)
            {
            }

            protected override void RemoveAction(ROOMA ins)
            {
                base.RemoveAction(ins);
                RoomPumpable.ReportChange(ins.MX(), ins.MY(), 0);
            }

            protected override void addAction(ROOMA ins)
            {
                base.removeAction(ins);
                RoomPumpable.ReportChange(ins.MX(), ins.MY(), 0);
            }
        }

        public override RoomPumpable Pumpable(int tx, int ty)
        {
            if (Is(tx, ty))
                return Instance.pump;
            return null;
        }
    }
}