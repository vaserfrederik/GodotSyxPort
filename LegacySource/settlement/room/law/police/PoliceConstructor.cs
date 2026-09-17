using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.rendering;

namespace settlement.room.law.police
{
    final class PoliceConstructor : Furnisher
    {
        private readonly ROOM_POLICE blue;

        final FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this, 1);
        final FurnisherStatEfficiency efficiency = new FurnisherStatEfficiency(this, prisoners);

        final static int bitWork = 0b0001;
        final static int bitService = 0b0011;
        final static int bitBed = 0b0111;

        private readonly RoomSprite table;

        protected PoliceConstructor(ROOM_POLICE blue, RoomInitData init)
            : base(init, 5, 2)
        {
            this.blue = blue;

            Json sp = init.data().json("SPRITES");

            final RoomSprite sStrap = new RoomSprite1x1(sp, "BED_STRAP_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry)
                {
                    return base.joins(tx, ty, rx, ry);
                }
            };

            final RoomSprite sBench = new RoomSprite1x1(sp, "BENCH_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry)
                {
                    return base.joins(tx, ty, rx, ry);
                }
            };

            final RoomSprite sChair = new RoomSprite1x1(sp, "CHAIR_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry)
                {
                    return base.joins(tx, ty, rx, ry);
                }
            };

            final RoomSprite sCage1 = new RoomSprite1x1(sp, "CAGE1_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry)
                {
                    return base.joins(tx, ty, rx, ry);
                }
            };

            final RoomSprite sCage2 = new RoomSprite1x1(sp, "CAGE2_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry)
                {
                    return base.joins(tx, ty, rx, ry);
                }
            };

            final RoomSprite sLatch = new RoomSprite1x1(sp, "LATCH_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry)
                {
                    return base.joins(tx, ty, rx, ry);
                }
            };

            final RoomSprite sTable = new RoomSprite1x1(sp, "TABLE_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry)
                {
                    return base.joins(tx, ty, rx, ry);
                }
            };

            table = sTable;

            FurnisherItemTile __ = new FurnisherItemTile(this, true, sTable, AVAILABILITY.ROOM_SOLID, true).setData(bitWork);

            {
                new FurnisherItem(new FurnisherItemTile[,] {
                    { __ },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __, __ },
                }, 2);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __, __, __ },
                }, 3);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __, __, __, __ },
                }, 4);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __, __, __, __, __ },
                }, 5);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __ },
                    { __ },
                }, 2);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __, __ },
                    { __, __ },
                }, 4);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __, __, __ },
                    { __, __, __ },
                }, 6);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __, __, __, __ },
                    { __, __, __, __ },
                }, 8);

                new FurnisherItem(new FurnisherItemTile[,] {
                    { __, __, __, __, __ },
                    { __, __, __, __, __ },
                }, 10);

                flush(3);
            }
        }

        public override bool usesArea()
        {
            return true;
        }

        public override bool mustBeIndoors()
        {
            return true;
        }

        public override bool mustBeOutdoors()
        {
            return false;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new PoliceInstance(blue, area, init);
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