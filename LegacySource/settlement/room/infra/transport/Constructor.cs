using System;
using System.IO;
using settlement.room.infra.transport;
using game;
using init.constant;
using init.resources;
using init.sprite;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using settlement.tilemap.floor.Floor;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.infra.transport
{
    public sealed class Constructor : Furnisher
    {
        private readonly ROOM_TRANSPORT blue;
        public readonly FurnisherStat crates;
        private readonly Floor floor2;

        public readonly FurnisherItemTile an;
        public readonly FurnisherItemTile ww;

        protected Constructor(ROOM_TRANSPORT blue, RoomInitData init)
            : base(init, 1, 1)
        {
            this.blue = blue;
            floor2 = SETT.FLOOR().map.Read("FLOOR2", init.data());

            crates = new FurnisherStat(this, 1)
            {
                Get = (area, fromItems) => fromItems,
                Format = (t, value) => GFORMAT.i(t, (int)value)
            };

            Json sp = init.data().json("SPRITES");

            RoomSprite marker = new RoomSprite1x1(sp, "GROUND_THING_1X1")
            {
                RenderBelow = (r, s, data, it, degrade) =>
                {
                    base.Render(r, s, data, it, degrade, false);
                },

                Render = (r, s, data, it, degrade, isCandle) => false,

                Joins = (tx, ty, rx, ry, d, item) => item.Sprite(rx, ry) == this
            };

            RoomSprite sLoad = new RoomSprite.Imp()
            {
                GetData = (tx, ty, rx, ry, item, itemRan) => 0,

                RenderBelow = (r, s, data, it, degrade) =>
                {
                    TransportInstance ins = blue.Get(it.tx(), it.ty());
                    if (ins != null && ins.Resource() != null)
                    {
                        int am = blue.Job.Bamount.Get(it.tx(), it.ty());
                        if (am > 0)
                            ins.Data.Resource().RenderLaying(r, it.x(), it.y(), it.ran(), am);
                    }
                },

                Render = (r, s, data, it, degrade, isCandle) => false
            };

            RoomSprite scart = new RoomSprite.Imp()
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    RESOURCE res = null;
                    int am = 0;

                    TransportInstance ins = blue.Get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        res = ins.Data.Resource();
                        am = ins.Data.Stored();
                        if (am > 0 || ins.Data.CartVisible())
                        {
                            int iii = SETT.ROOMS().fData.tileData.Get(it.Tile()) - 1;
                            int dx = 0;
                            int dy = 0;
                            if ((iii & 1) == 1)
                            {
                                DIR dd = DIR.ORTHO.Get(data);
                                dx = dd.x() * C.TILE_SIZEH;
                                dy = dd.y() * C.TILE_SIZEH;
                            }
                            SETT.HALFENTS().transports.Sprite.RenderBelow(r, s, data * 2, it.x() + dx + C.TILE_SIZEH, it.y() + dy + C.TILE_SIZEH, 0, it.ran(), degrade, res, (double)am / ROOM_TRANSPORT.MAX_LOAD);
                        }
                    }

                    return false;
                },

                RenderAbove = (r, s, data, it, degrade) =>
                {
                    TransportInstance ins = blue.Get(it.tx(), it.ty());
                    if (ins != null && ins.Data.CartVisible())
                    {
                        int iii = SETT.ROOMS().fData.tileData.Get(it.Tile()) - 1;
                        int dx = 0;
                        int dy = 0;
                        if ((iii & 1) == 1)
                        {
                            DIR dd = DIR.ORTHO.Get(data);
                            dx = dd.x() * C.TILE_SIZEH;
                            dy = dd.y() * C.TILE_SIZEH;
                        }
                        SETT.HALFENTS().transports.Sprite.Render(r, s, data * 2, it.x() + dx + C.TILE_SIZEH, it.y() + dy + C.TILE_SIZEH, degrade, false);
                    }
                },

                GetData = (tx, ty, rx, ry, item, itemRan) => (byte)item.rotation,

                RenderPlaceholder = (r, x, y, data, tx, ty, rx, ry, item) =>
                {
                    SPRITES.cons().ICO.arrows.Get(item.rotation).Render(r, x, y);
                }
            };

            RoomSprite dummy = new RoomSprite()
            {
                SData = () => 0,

                Render = (r, s, data, it, degrade, isCandle) => false,

                GetData = (tx, ty, rx, ry, item, itemRan) => 0,

                RenderPlaceholder = (r, x, y, data, tx, ty, rx, ry, item) =>
                {
                    SPRITES.cons().ICO.arrows.Get(item.rotation).Render(r, x, y);
                }
            };

            RoomSprite animal = new RoomSprite.Imp()
            {
                Render = (r, s, data, it, degrade, isCandle) =>
                {
                    TransportInstance ins = blue.Get(it.tx(), it.ty());
                    if (ins != null && ins.Data.OxVisible())
                    {
                        double mov = (GAME.intervals().Get05() + it.ran()) & 0x0FF;
                        mov /= 0x0FF;
                        SETT.ANIMALS().RenderCaravan(r, s, mov, it.x() + C.TILE_SIZEH, it.y() + C.TILE_SIZEH, null, 0, false, data * 2, it.ran());
                    }

                    return false;
                },

                GetData = (tx, ty, rx, ry, item, itemRan) => (byte)item.rotation,

                RenderPlaceholder = (r, x, y, data, tx, ty, rx, ry, item) =>
                {
                    SPRITES.cons().ICO.arrows.Get(item.rotation).Render(r, x, y);
                }
            };

            ww = new FurnisherItemTile(
                this,
                false,
                sLoad,
                AVAILABILITY.ROOM_SOLID,
                false
            );

            FurnisherItemTile xx = new FurnisherItemTile(
                this,
                false,
                dummy,
                AVAILABILITY.ROOM_SOLID,
                false).SetData(1);

            FurnisherItemTile __ = new FurnisherItemTile(
                this,
                false,
                new RoomSprite.Dummy(),
                AVAILABILITY.ROOM,
                false
            );

            an = new FurnisherItemTile(
                this,
                false,
                animal,
                AVAILABILITY.ROOM_SOLID,
                false).SetData(1);

            FurnisherItemTile c0 = new FurnisherItemTile(
                this,
                false,
                scart,
                AVAILABILITY.ROOM_SOLID,
                false).SetData(1);

            FurnisherItemTile c1 = new FurnisherItemTile(
                this,
                false,
                scart,
                AVAILABILITY.ROOM_SOLID,
                false).SetData(2);

            FurnisherItemTile c2 = new FurnisherItemTile(
                this,
                false,
                scart,
                AVAILABILITY.ROOM_SOLID,
                false).SetData(3);

            FurnisherItemTile c3 = new FurnisherItemTile(
                this,
                false,
                scart,
                AVAILABILITY.ROOM_SOLID,
                false).SetData(4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {pp, __, __, __, pp},
                {__, __, __, __, __},
                {mm, ww, an, ww, mm},
                {mm, ww, c0, ww, mm},
                {__, ww, xx, ww, __},
                {mm, ww, c1, ww, mm},
                {mm, ww, c2, ww, mm},
                {__, ww, xx, ww, __},
                {mm, ww, c3, ww, mm},
                {mm, ww, ww, ww, mm},
                {__, __, __, __, __},
                {pp, __, __, __, pp}
            }, 1);

            Flush(3);
        }

        public override bool UsesArea()
        {
            return false;
        }

        public override bool MustBeIndoors()
        {
            return false;
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override void PutFloor(int tx, int ty, int upgrade, AREA area)
        {
            if (SETT.ROOMS().fData.tile.Get(tx, ty) != null && SETT.ROOMS().fData.tile.Get(tx, ty).Data() > 0)
                floor2.PlaceFixed(tx, ty);
            else
                base.PutFloor(tx, ty, upgrade, area);
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new TransportInstance(blue, area, init);
        }
    }
}