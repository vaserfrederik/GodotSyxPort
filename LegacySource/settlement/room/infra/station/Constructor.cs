using System;
using System.IO;
using game;
using init.constant;
using init.resources;
using settlement.main;
using settlement.path;
using settlement.room.infra.transport;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.tilemap.floor.Floors;
using snake2d;
using util;
using util.rendering;

namespace settlement.room.infra.station
{
    final class Constructor : Furnisher
    {
        private readonly ROOM_STATION blue;
        private readonly Floor floor2;

        public const double RAND = 1.0 / 0x0FF;
        public const int BIT_WORK = 0b00001;
        public const int BIT_CRATE = 0b00010;
        private const int BIT_FLOOR = 0b00100;
        public const int BIT_DEST = 0b01000;

        protected Constructor(ROOM_STATION blue, RoomInitData init) : base(init, 1, 0)
        {
            this.blue = blue;
            floor2 = SETT.FLOOR().map.read("FLOOR2", init.data());

            Json sp = init.data().json("SPRITES");

            final RoomSprite sWork = new RoomSprite1x1(sp, "WORK_1X1")
            {
                final RoomSprite top = new RoomSprite1x1(sp, "WORK_TOP_1X1");

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    StationInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        if (ins.prepD() + (GUTIL.ran2().get(it.tile()) & 0x0FF) * RAND >= 1)
                            return;
                    }
                    top.render(r, s, getData2(it), it, degrade, false);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            final RoomSprite sBasin = new RoomSprite1x1(sp, "ANIMAL_TOP_1X1")
            {
                final RoomSprite water = new RoomSprite1x1(sp, "ANIMAL_BOTTOM_1X1");

                public override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    StationInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        if (ins.prepD() + (GUTIL.ran2().get(it.tile()) & 0x0FF) * RAND >= 1)
                            water.render(r, s, data, it, degrade, false);
                    }
                }
            };

            final RoomSprite sAnimal = new RoomSprite1x1(sp, "ROOF_MID_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == this;
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    StationInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        if (ins.prepD() + (GUTIL.ran2().get(it.tile()) & 0x0FF) * RAND >= 1)
                        {
                            DIR d = DIR.ORTHO.get(SETT.ROOMS().fData.item.get(it.tile()).rotation);

                            double mov = (GAME.intervals().get05() + it.ran()) & 0x0FF;
                            mov /= 0x0FF;
                            SETT.ANIMALS().renderCaravan(r, s, mov, it.x() + C.TILE_SIZEH + d.x() * C.TILE_SIZEH, it.y() + C.TILE_SIZEH + d.y() * C.TILE_SIZEH, null, 0, false, d.id(), it.ran());
                        }
                    }
                    return false;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    s.setSoft();
                    base.render(r, s, data, it, degrade, false);
                    s.setPrev();
                }
            };

            final RoomSprite sAnimalEdge = new RoomSprite1x1(sp, "ROOF_EDGE_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == sAnimal;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    s.setSoft();
                    base.render(r, s, data, it, degrade, false);
                    s.setPrev();
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return false;
                }
            };

            final RoomSprite spriteCrate = new RoomSprite1x1(sp, "CRATE_1X1")
            {
                final RoomSprite water = new RoomSprite1x1(sp, "CRATE_BOTTOM_1X1");

                public override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    StationInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null)
                    {
                        if (ins.prepD() + (GUTIL.ran2().get(it.tile()) & 0x0FF) * RAND >= 1)
                            water.render(r, s, data, it, degrade, false);
                    }
                }
            };

            FurnisherItemTile[][] layout1 = {
                { new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile() },
                { new FurnisherItemTile(), in, af, af, af, in, new FurnisherItemTile() },
                { new FurnisherItemTile(), ae, an, an, an, ae, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, new FurnisherItemTile(), ww, pp, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile() },
            };

            FurnisherItemTile[][] layout2 = {
                { new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile() },
                { new FurnisherItemTile(), in, af, af, af, af, in, new FurnisherItemTile() },
                { new FurnisherItemTile(), ae, an, an, an, an, ae, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, pp, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile() },
                { new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile() },
            };

            FurnisherItemTile[][] layout3 = {
                { new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile() },
                { new FurnisherItemTile(), in, af, af, af, af, af, af, in, new FurnisherItemTile() },
                { new FurnisherItemTile(), ae, an, an, an, an, an, an, ae, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, pp, ww, new FurnisherItemTile(), st, st, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile(), st, st, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile(), st, st, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile(), st, st, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile(), st, st, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile(), st, st, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile(), st, st, new FurnisherItemTile() },
                { new FurnisherItemTile(), st, st, new FurnisherItemTile(), ww, in, ww, new FurnisherItemTile(), st, st, new FurnisherItemTile() },
                { new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile(), new FurnisherItemTile() },
            };

            new FurnisherItem(layout1, 1);
            new FurnisherItem(layout2, 1.15);
            new FurnisherItem(layout3, 1.6);

            flush(3);
        }

        public override bool usesArea()
        {
            return false;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
            if (SETT.ROOMS().fData.tile.get(tx, ty) != null && (SETT.ROOMS().fData.tile.get(tx, ty).data() & BIT_FLOOR) != 0)
                floor2.placeFixed(tx, ty);
            else
                base.putFloor(tx, ty, upgrade, area);
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new StationInstance(blue, area, init);
        }
    }
}