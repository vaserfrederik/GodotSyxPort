using System;
using System.IO;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using util.rendering;

namespace settlement.room.home.chamber
{
    final class Constructor : Furnisher
    {
        private readonly ROOM_CHAMBER blue;

        final FurnisherStat servants;
        final FurnisherStat users;
        final FurnisherItemTile bb;
        private const int WORK_NEEDED = 22;

        protected Constructor(ROOM_CHAMBER blue, RoomInitData init)
            : base(init, 1, 2, 88, 44)
        {
            this.blue = blue;
            servants = new FurnisherStat.FurnisherStatI(this);
            users = new FurnisherStat.FurnisherStatI(this);

            Json sp = init.data().json("SPRITES");

            final RoomSprite1x1 snick = new RoomSprite1x1(sp, "MISC_1X1");

            RoomSprite sBed = new RoomSpriteXxX(2)
            {
                public override Sheets sheet(RenderIterator it)
                {
                    ChamberInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null && ins.occupant() != null)
                        return ins.occupant().race().home().clas(ins.occupant().indu()).masterBed.get(ins);
                    return null;
                }
            };

            RoomSprite sBenchEnd = new RoomSprite1x1(sp, "BENCH_END_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) is RoomSprite1x1;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    snick.renderRandom(r, s, it, it.ran(), degrade);
                }
            };

            RoomSprite sBenchMid = new RoomSprite1x1(sp, "BENCH_CENTRE_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) is RoomSprite1x1;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    snick.renderRandom(r, s, it, it.ran(), degrade);
                }
            };

            RoomSprite sMantel1 = new RoomSprite1x1(sp, "MANTEL_A_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == sBenchMid;
                }
            };

            RoomSprite sMantel2 = new RoomSprite1x1(sp, "MANTEL_B_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == sBenchMid;
                }
            };

            RoomSprite sBedpost1 = new RoomSprite1x1(sp, "BEDPOST_A_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) is RoomSpriteXxX;
                }
            };

            RoomSprite sBedpost2 = new RoomSprite1x1(sp, "BEDPOST_B_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) is RoomSpriteXxX;
                }
            };

            RoomSprite sCarpets = new RoomSpriteCombo(sp, "CARPET_COMBO")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == this;
                }
            };

            RoomSprite sStatues = new RoomSpriteXxX(2)
            {
                public override Sheets sheet(RenderIterator it)
                {
                    ChamberInstance ins = blue.get(it.tx(), it.ty());
                    if (ins != null && ins.occupant() != null)
                        return ins.occupant().race().home().clas(ins.occupant().indu()).statue.get(ins);
                    return null;
                }
            };

            RoomSprite sDummy = new RoomSprite.Dummy();

            bb = new FurnisherItemTile(
                this,
                sBed,
                AVAILABILITY.PENALTY4,
                false);

            final FurnisherItemTile bc = new FurnisherItemTile(
                this,
                sBed,
                AVAILABILITY.PENALTY4,
                false);

            final FurnisherItemTile b1 = new FurnisherItemTile(
                this,
                sBedpost1,
                AVAILABILITY.ROOM_SOLID,
                false);

            final FurnisherItemTile b2 = new FurnisherItemTile(
                this,
                sBedpost2,
                AVAILABILITY.ROOM_SOLID,
                false);

            final FurnisherItemTile x1 = new FurnisherItemTile(
                this,
                sBenchEnd,
                AVAILABILITY.ROOM_SOLID,
                true);

            final FurnisherItemTile xx = new FurnisherItemTile(
                this,
                sBenchMid,
                AVAILABILITY.ROOM_SOLID,
                true);

            final FurnisherItemTile m1 = new FurnisherItemTile(
                this,
                sMantel1,
                AVAILABILITY.ROOM_SOLID,
                true);

            final FurnisherItemTile m2 = new FurnisherItemTile(
                this,
                sMantel2,
                AVAILABILITY.ROOM_SOLID,
                true);

            final FurnisherItemTile cc = new FurnisherItemTile(
                this,
                sCarpets,
                AVAILABILITY.ROOM,
                false);

            final FurnisherItemTile ss = new FurnisherItemTile(
                this,
                sStatues,
                AVAILABILITY.ROOM_SOLID,
                false);

            final FurnisherItemTile ee = new FurnisherItemTile(
                this,
                true,
                sDummy,
                AVAILABILITY.ROOM,
                false);
            ee.noWalls = true;

            final FurnisherItemTile __ = new FurnisherItemTile(
                this,
                sDummy,
                AVAILABILITY.ROOM,
                false);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { x1, cc, b1, b2, cc, x1 },
                new FurnisherItemTile[] { xx, cc, bb, bc, cc, m1 },
                new FurnisherItemTile[] { xx, cc, bc, bc, cc, m2 },
                new FurnisherItemTile[] { x1, cc, __, __, cc, x1 },
                new FurnisherItemTile[] { __, cc, __, __, cc, __ },
                new FurnisherItemTile[] { ss, ss, __, __, ss, ss },
                new FurnisherItemTile[] { ss, ss, ee, ee, ss, ss }
            }, 1);

            flush(1, 3);
        }

        public override bool usesArea()
        {
            return false;
        }

        public override bool mustBeIndoors()
        {
            return true;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new ChamberInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override bool isHeavy()
        {
            return true;
        }
    }
}