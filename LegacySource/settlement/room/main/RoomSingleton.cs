using System;
using System.Collections.Generic;
using System.IO;
using init.resources;
using init.sprite.UI;
using settlement.main;
using settlement.maintenance;
using settlement.room.main.construction;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using util.rendering;

namespace settlement.room.main
{
    public abstract class RoomSingleton : Room, Serializable
    {
        private static readonly long serialVersionUID = 1L;
        private int size;
        private readonly Rec tiles = new Rec(0, 0, 0, 0);
        private readonly Coo dataCoo = new Coo();
        private readonly Coo upperLeft = new Coo();
        private int dataTile;
        private int data;
        protected transient FurnisherItem item;

        protected static readonly transient RoomAreaWrapper wrap = new RoomAreaWrapper();
        protected static readonly transient RoomAreaWrapper wrapD = new RoomAreaWrapper();

        protected RoomSingleton(ROOMS m, RoomBlueprint p) : base(m, p, true) { }

        public final RoomSingleton Place(TmpArea area)
        {
            int mx = area.Mx();
            int my = area.My();

            area.ReplaceAndClear(this);
            IniHard(mx, my);
            ROOMA a = wrap.Init(this, mx, my);
            AddAction(a);

            SETT.ROOMS().Map.Init(a);
            IsolationSet(a.Mx(), a.My(), SETT.ROOMS().Isolation.GetProspect(Blueprint(), a, null));
            wrap.Done();
            SETT.MAINTENANCE().InitRoomDegrade(this, a.Mx(), a.My());

            return this;
        }

        protected override bool Render(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            RoomSprite s = ROOMS().fData.sprite.Get(i.Tile());
            if (s != null)
                return s.Render(r, shadowBatch, ROOMS().fData.spriteData.Get(i.Tile()), i, GetDegrade(i.Tx(), i.Ty()), ROOMS().fData.candle.Is(i.Tile()));
            return false;
        }

        protected override bool RenderAbove(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            RoomSprite s = ROOMS().fData.sprite.Get(i.Tile());
            if (s != null)
                s.RenderAbove(r, shadowBatch, ROOMS().fData.spriteData.Get(i.Tile()), i, GetDegrade(i.Tx(), i.Ty()));
            return false;
        }

        protected override bool RenderBelow(Renderer r, ShadowBatch shadowBatch, RenderIterator i)
        {
            RoomSprite s = ROOMS().fData.sprite.Get(i.Tile());
            if (s != null)
                s.RenderBelow(r, shadowBatch, ROOMS().fData.spriteData.Get(i.Tile()), i, GetDegrade(i.Tx(), i.Ty()));
            return false;
        }

        protected override AVAILABILITY GetAvailability(int tile)
        {
            FurnisherItemTile t = ROOMS().fData.tile.Get(tile);
            if (t != null)
                return t.availability;
            return AVAILABILITY.ROOM;
        }

        public override Furnisher Constructor()
        {
            return BlueprintI().Constructor();
        }

        public override bool DestroyTileCan(int tx, int ty)
        {
            return ROOMS().fData.availability.Get(tx, ty).player < 0 || ROOMS().fData.availability.Get(tx, ty).enemy < 0;
        }

        public override void DestroyTile(int tx, int ty)
        {
            IniHard(tx, ty);
            ConstructionInit init = new ConstructionInit(this, tx, ty, true);
            ROOMA ar = wrap.Init(this, tx, ty);

            foreach (COORDINATE c in ar.Body())
            {
                if (!ar.Is(c))
                    continue;
                PATH().availability.UpdateService(c.X(), c.Y());
                if (!TERRAIN().Get(c).Clearing().IsStructure())
                    TERRAIN().NADA.PlaceFixed(c.X(), c.Y());
            }
            wrap.Done();
            TmpArea a = Remove(tx, ty, false, this, true);
            ROOMS().construction.BreakIt(a, init, tx, ty);
        }

        public override int Area(int tx, int ty)
        {
            IniHard(tx, ty);
            return size;
        }

        public override TmpArea Remove(int tx, int ty, bool scatter, object obj, bool force)
        {
            IniHard(tx, ty);
            ROOMA a = wrap.Init(this, tx, ty);
            SETT.ROOMS().stats.broken().Remove(a.Mx(), a.My());
            RemoveAction(a);
            if (scatter)
                Deleter.ScatterMaterials(a, Constructor(), Upgrade(tx, ty), GetDegrade(tx, ty));

            foreach (COORDINATE c in a.Body())
            {
                if (!a.Is(c))
                    continue;
                SETT.LIGHTS().Remove(c.X(), c.Y());
                FLOOR().clearer.Clear(c.X(), c.Y());
                SETT.ROOMS().data.Set(a, c, 0);
                ConstructionData.dConstructed.Set(a, c, 1);
                ConstructionData.dFloored.Set(a, c, 1);
            }

            TmpArea ar = ROOMS().map.Delete(this, a.Mx(), a.My(), obj);

            wrap.Done();
            wrap.Clear();
            tiles.SetDim(0).MoveX1Y1(-1, -1);

            return ar;
        }

        protected void AddAction(ROOMA ins)
        {

        }

        protected void RemoveAction(ROOMA ins)
        {

        }

        public override ROOM_DEGRADER Degrader(int tx, int ty)
        {
            IniHard(tx, ty);
            degA = wrapD.Init(this, dataCoo.X(), dataCoo.Y());
            wrapD.Done();
            return degrader;
        }

        private transient ROOMA degA;
        private readonly Degrader degrader = new Degrader();

        private class Degrader : ROOM_DEGRADER, Serializable
        {
            private static readonly long serialVersionUID = 1L;

            public override int ResSize()
            {
                return item.group.blueprint.Resources();
            }

            public override int ResAmount(int i)
            {
                return (int)Math.Ceiling(item.cost2(i, Upgrade(degA.Mx(), degA.My())));
            }

            public override RESOURCE Res(int i)
            {
                return item.group.blueprint.Resource(i);
            }

            public override double DegRate()
            {
                return degradeResNeeded();
            }

            public override int GetData()
            {
                return data;
            }

            protected override void SetData(int v, bool b)
            {
                data = v;
            }

            public override Icon Icon()
            {
                return BlueprintI().iconBig();
            }

            private bool ini(int tx, int ty)
            {
                if (tiles.HoldsPoint(tx, ty) && item != null && item == ROOMS().fData.item.Get(tx, ty))
                    return true;

                ROOMS().fData.itemX1Y1(tx, ty, upperLeft, this);
                item = ROOMS().fData.item.Get(tx, ty);
                dataCoo.Set(upperLeft);
                dataCoo.Increment(item.firstX(), item.firstY());

                tiles.MoveX1Y1(upperLeft);
                tiles.SetWidth(item.width());
                tiles.SetHeight(item.height());
                size = item.width() * item.height();
                dataTile = dataCoo.X() + dataCoo.Y() * TWIDTH;
                data = ROOMS().data.Get(dataTile);

                return true;
            }

            protected void iniHard(int tx, int ty)
            {
                if (!ini(tx, ty))
                {
                    throw new RuntimeException(tx + " " + ty + " " + this + " " + SETT.ROOMS().map.Get(tx, ty));
                }
            }

            public override bool IsSame(int tx, int ty, int ox, int oy)
            {
                if (Blueprint().Is(tx, ty))
                {
                    iniHard(tx, ty);
                    return iss(ox, oy);
                }
                return false;
            }

            private static Coo upperLeftTest = new Coo();

            private bool iss(int tx, int ty)
            {
                if (tiles.HoldsPoint(tx, ty) && item == ROOMS().fData.item.Get(tx, ty))
                {
                    ROOMS().fData.itemX1Y1(tx, ty, upperLeftTest, this);
                    if (upperLeftTest.IsSameAs(upperLeft))
                        return true;
                }
                return false;
            }

            public override int Mx(int tx, int ty)
            {
                iniHard(tx, ty);
                return dataCoo.X();
            }

            public override int My(int tx, int ty)
            {
                iniHard(tx, ty);
                return dataCoo.Y();
            }

            public override int X1(int tx, int ty)
            {
                iniHard(tx, ty);
                return tiles.X1();
            }

            public override int Y1(int tx, int ty)
            {
                iniHard(tx, ty);
                return tiles.Y1();
            }

            public override int Width(int tx, int ty)
            {
                iniHard(tx, ty);
                return tiles.Width();
            }

            public override int Height(int tx, int ty)
            {
                iniHard(tx, ty);
                return tiles.Height();
            }
        }
    }
}