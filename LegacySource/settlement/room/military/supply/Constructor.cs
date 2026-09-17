using System;
using System.IO;
using game;
using init.constant;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using settlement.tilemap.floor;
using snake2d;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.military.supply
{
    internal sealed class Constructor : Furnisher
    {
        private readonly Floors.Floor floor2;

        internal readonly FurnisherStat workers = new FurnisherStat.FurnisherStatI(this)
        {
            Get = (area, fromItems) => fromItems,
            Format = (t, value) => GFORMAT.i(t, (int)value)
        };

        internal readonly FurnisherStat storage = new FurnisherStat(this, 0)
        {
            Get = (area, fromItems) => fromItems * ROOM_SUPPLY.STORAGE,
            Format = (t, value) => GFORMAT.i(t, (int)value)
        };

        protected Constructor(ROOM_SUPPLY b, RoomInitData init)
            : base(init, 1, 2, 88, 44)
        {
            floor2 = SETT.FLOOR().map.Read("FLOOR2", init.data());

            Json sp = init.data().Json("SPRITES");

            RoomSprite sFence = new RoomSpriteCombo(sp, "FENCE_COMBO");

            RoomSprite sStone = new RoomSprite1x1(sp, "TORCH_1X1");
            Crate crate = new Crate(b);
            RoomSprite sCrate = new RoomSprite.Imp
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    if (crate.Get(it.tx(), it.ty()) != null && !crate.Away())
                    {
                        SETT.HALFENTS().transports.sprite.RenderBelow(r, s, data * 2, it.x() + C.TILE_SIZEH, it.y() + C.TILE_SIZEH, 0, it.ran(), degrade, crate.RealResource(), crate.ResAmount());
                    }
                    return false;
                },
                RenderAbove = (r, s, data, it, degrade) =>
                {
                    if (crate.Get(it.tx(), it.ty()) != null && !crate.Away())
                    {
                        SETT.HALFENTS().transports.sprite.Render(r, s, data * 2, it.x() + C.TILE_SIZEH, it.y() + C.TILE_SIZEH, degrade, true);
                    }
                },
                GetData = (tx, ty, rx, ry, item, itemRan) => (byte)item.rotation,
                RenderPlaceholder = (r, x, y, data, tx, ty, rx, ry, item) => SPRITES.cons().ICO.arrows.Get(data).Render(r, x, y)
            };

            RoomSprite sAnimal = new RoomSprite.Imp
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    DIR d = DIR.ORTHO.Get(data).Perpendicular();
                    int tx = it.tx() + d.x();
                    int ty = it.ty() + d.y();
                    if (crate.Get(tx, ty) != null && !crate.Away() && crate.AnimalHas())
                    {
                        double mov = (GAME.Intervals().Get05() + it.ran()) & 0x0FF;
                        mov /= 0x0FF;
                        SETT.ANIMALS().RenderCaravan(r, s, mov, it.x() + C.TILE_SIZEH, it.y() + C.TILE_SIZEH, null, 0, false, data * 2, it.ran());
                    }
                    return false;
                },
                GetData = (tx, ty, rx, ry, item, itemRan) => (byte)item.rotation,
                RenderPlaceholder = (r, x, y, data, tx, ty, rx, ry, item) => SPRITES.cons().ICO.arrows.Get(data).Render(r, x, y)
            };

            RoomSprite smarker = new RoomSprite1x1(sp, "OVERLAY_1X1")
            {
                RenderBelow = (r, s, data, it, degrade) =>
                {
                    OPACITY.O50.Bind();
                    base.Render(r, s, data, it, degrade, false);
                    OPACITY.Unbind();
                },
                Render = (r, s, data, it, degrade, isCandle) => false,
                Joins = (tx, ty, rx, ry, d, item) => item.sprite(rx, ry) == this,
                RenderPlaceholder = (r, x, y, data, tx, ty, rx, ry, item) => SPRITES.cons().ICO.arrows.Get(data).Render(r, x, y)
            };

            FurnisherItemTile ff = new FurnisherItemTile(this, false, sFence, AVAILABILITY.ROOM_SOLID, false);
            FurnisherItemTile ss = new FurnisherItemTile(this, false, sStone, AVAILABILITY.ROOM_SOLID, true);
            FurnisherItemTile cc = new FurnisherItemTile(this, false, sCrate, AVAILABILITY.ROOM_SOLID, false).SetData(1);
            FurnisherItemTile aa = new FurnisherItemTile(this, false, sAnimal, AVAILABILITY.ROOM_SOLID, false).SetData(2);
            FurnisherItemTile oo = new FurnisherItemTile(this, true, smarker, AVAILABILITY.ROOM, false).SetData(3);
            FurnisherItemTile __ = new FurnisherItemTile(this, false, null, AVAILABILITY.ROOM, false);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss,__,oo,__,oo,__,ss},
                {ff,__,aa,__,aa,__,ff},
                {ff,__,cc,__,cc,__,ff},
                {ff,__,__,__,__,__,ff},
                {ff,ff,ff,ff,ff,ff,ff},
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss,__,oo,__,oo,__,oo,__,ss},
                {ff,__,aa,__,aa,__,aa,__,ff},
                {ff,__,cc,__,cc,__,cc,__,ff},
                {ff,__,__,__,__,__,__,__,ff},
                {ff,ff,ff,ff,ff,ff,ff,ff,ff},
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss,__,oo,__,oo,__,oo,__,oo,__,ss},
                {ff,__,aa,__,aa,__,aa,__,aa,__,ff},
                {ff,__,cc,__,cc,__,cc,__,cc,__,ff},
                {ff,__,__,__,__,__,__,__,__,__,ff},
                {ff,ff,ff,ff,ff,ff,ff,ff,ff,ff,ff},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {ss,__,oo,__,oo,__,oo,__,oo,__,oo,__,ss},
                {ff,__,aa,__,aa,__,aa,__,aa,__,aa,__,ff},
                {ff,__,cc,__,cc,__,cc,__,cc,__,cc,__,ff},
                {ff,__,oo,__,oo,__,oo,__,oo,__,oo,__,ff},
                {ff,__,aa,__,aa,__,aa,__,aa,__,aa,__,ff},
                {ff,__,cc,__,cc,__,cc,__,cc,__,cc,__,ff},
                {ff,__,__,__,__,__,__,__,__,__,__,ff},
                {ff,ff,ff,ff,ff,ff,ff,ff,ff,ff,ff},
            }, 8);

            Flush(3);
        }

        public override bool UsesArea() => false;

        public override bool MustBeIndoors() => false;

        public override bool MustBeOutdoors() => false;

        public override void PutFloor(int tx, int ty, int upgrade, AREA area)
        {
            if (SETT.ROOMS().fData.tileData.Get(tx, ty) != 0)
                base.PutFloor(tx, ty, upgrade, area);
            else
                floor2.PlaceFixed(tx, ty);
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new SupplyInstance(Blue(), area, init);
        }

        public override ROOM_SUPPLY Blue() => SETT.ROOMS().SUPPLY;
    }
}