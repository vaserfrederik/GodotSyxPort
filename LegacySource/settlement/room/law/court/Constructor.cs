using System;
using System.IO;

namespace Settlement.Room.Law.Court
{
    public class Constructor : Furnisher
    {
        private readonly ROOM_COURT blue;

        public FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this, 1);
        public FurnisherStat workers = new FurnisherStat.FurnisherStatI(this);
        public FurnisherStat spectators = new FurnisherStat.FurnisherStatI(this);
        public const int codeWork = 1;
        public const int codeCriminal = 2;
        public const int codeSpectator = 3;
        public const int distance = 4;

        protected Constructor(ROOM_COURT blue, RoomInitData init) : base(init, 2, 3, 88, 44)
        {
            this.blue = blue;
            Json sp = init.data().json("SPRITES");

            station(sp);
            bench(sp);
        }

        private void station(Json sp)
        {
            RoomSprite table = new RoomSpriteCombo(sp, "TABLE_COMBO")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == this;
                }
            };

            RoomSprite carpets = new RoomSpriteCombo(sp, "CARPET_COMBO")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == this;
                }
            };

            RoomSprite candle = new RoomSprite1x1(sp, "TORCH_1X1")
            {
                private RoomSprite1x1 top = new RoomSprite1x1(sp, "GEM_1X1");

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    if (!isCandle)
                    {
                        top.renderRandom(r, s, it, it.ran(), degrade);
                    }
                    return false;
                }
            };

            RoomSprite pedistal = new RoomSpriteCombo(sp, "STAND_COMBO")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == this;
                }
            };

            RoomSprite chair = new RoomSprite1x1(sp, "CHAIR_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == table;
                }
            };

            RoomSprite decor = new RoomSprite1x1(sp, "DECOR_A_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                }
            }.sData(1);

            RoomSprite decorC = new RoomSprite1x1(sp, "DECOR_B_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                }
            }.sData(1);

            FurnisherItemTile xx = new FurnisherItemTile(
                this,
                table,
                AVAILABILITY.ROOM_SOLID, false);

            FurnisherItemTile cc = new FurnisherItemTile(
                this,
                carpets,
                AVAILABILITY.AVOID_PASS, false);

            FurnisherItemTile ca = new FurnisherItemTile(
                this,
                candle,
                AVAILABILITY.ROOM_SOLID, true);

            FurnisherItemTile ii = new FurnisherItemTile(
                this,
                chair,
                AVAILABILITY.AVOID_PASS, false).setData(codeWork);

            FurnisherItemTile pp = new FurnisherItemTile(
                this,
                true,
                pedistal,
                AVAILABILITY.AVOID_PASS, false).setData(codeCriminal);

            FurnisherItemTile dd = new FurnisherItemTile(
                this,
                decor,
                AVAILABILITY.ROOM_SOLID, false);

            FurnisherItemTile dc = new FurnisherItemTile(
                this,
                decorC,
                AVAILABILITY.ROOM_SOLID, false);

            FurnisherItemTile __ = new FurnisherItemTile(
                this,
                null,
                AVAILABILITY.ROOM, false);

            FurnisherItemTile _r = new FurnisherItemTile(
                this,
                true,
                null,
                AVAILABILITY.ROOM, false);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ca, dd, dc, dd, ca },
                { _r, __, ii, __, _r },
                { xx, xx, xx, xx, xx },
                { xx, xx, xx, xx, xx },
                { cc, cc, cc, cc, cc },
                { __, __, pp, __, },
            }, 1, 1);

            flush(3);
        }

        private void bench(Json sp)
        {
            RoomSprite ssa = new RoomSprite1xN(sp, "BENCH_A_1X1", true);
            RoomSprite ssb = new RoomSprite1xN(sp, "BENCH_B_1X1", false);
            RoomSprite ssc = new RoomSprite1xN(sp, "BENCH_C_1X1", false);

            RoomSprite candle = new RoomSprite1x1(sp, "TORCH_1X1");

            FurnisherItemTile ss = new FurnisherItemTile(
                this,
                true,
                ssa,
                AVAILABILITY.PENALTY4, false).setData(codeSpectator);

            FurnisherItemTile sc = new FurnisherItemTile(
                this,
                true,
                ssb,
                AVAILABILITY.PENALTY4, false).setData(codeSpectator);

            FurnisherItemTile se = new FurnisherItemTile(
                this,
                true,
                ssc,
                AVAILABILITY.PENALTY4, false).setData(codeSpectator);

            FurnisherItemTile ca = new FurnisherItemTile(
                this,
                candle,
                AVAILABILITY.ROOM_SOLID, true);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ca, ss, se, ca, },
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ca, ss, sc, se, ca, },
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ca, ss, sc, sc, se, ca, },
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ca, ss, sc, sc, sc, se, ca, },
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ca, ss, sc, sc, sc, sc, se, ca, },
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { ca, ss, sc, sc, sc, sc, sc, se, ca, },
            }, 7);

            flush(1);
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
            return new CourtInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override bool isHeavy()
        {
            return true;
        }

        // private readonly FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][]
        // {
        //     { 0, 0, 0, 0, 0, 0, 0, 0 },
        //     { 0, 1, 1, 1, 0, 0, 0, 0 },
        //     { 0, 1, 1, 1, 0, 0, 0, 0 },
        //     { 0, 1, 1, 1, 1, 1, 1, 0 },
        //     { 0, 1, 1, 1, 1, 1, 1, 0 },
        //     { 0, 1, 1, 1, 0, 0, 0, 0 },
        //     { 0, 1, 1, 1, 0, 0, 0, 0 },
        //     { 0, 0, 0, 0, 0, 0, 0, 0 },
        // },
        // miniColor
        // );

        // public override COLOR miniColor(int tx, int ty)
        // {
        //     return miniC.get(tx, ty);
        // }
    }
}