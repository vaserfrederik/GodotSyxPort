using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.tilemap.floor;
using snake2d;
using util.rendering;

namespace settlement.room.law.prison
{
    internal class Constructor : Furnisher
    {
        private readonly ROOM_PRISON blue;

        internal readonly int PRISONERS_PER_CELL;
        internal readonly int WORKERS_PER_CELL;

        internal readonly FurnisherStat prisoners = new FurnisherStat.FurnisherStatI(this);
        internal readonly FurnisherStat guards = new FurnisherStat.FurnisherStatI(this);

        private readonly FurnisherItemTile cc;
        private readonly RoomSprite1x1 sCandle;
        private readonly Floor floor2;

        static internal readonly int CODE_ENTRANCE = 1;
        static internal readonly int CODE_LATRINE = 2;
        static internal readonly int CODE_FOOD = 3;

        protected Constructor(ROOM_PRISON blue, RoomInitData init)
            : base(init, 1, 2, 88, 44)
        {
            this.blue = blue;
            floor2 = SETT.FLOOR().map.Get(init.data().Value("FLOOR2"), init.data());

            Json sp = init.data().Json("SPRITES");

            RoomSpriteCombo sWall = new RoomSpriteCombo(sp, "WALLS_COMBO");
            RoomSprite1x1 sBars = new RoomSprite1x1(sp, "BARS_1X1");

            RoomSprite sLatrine = new SCellOther(sWall, sBars, new RoomSprite1x1(sp, "LATRINE_EMPTY_1X1"))
            {
                RoomSprite1x1 full = new RoomSprite1x1(sp, "LATRINE_FULL_1X1");

                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    if (blue.Is(it.tile()) && Latrine.latrineUsed(SETT.ROOMS().data.Get(it.tile())))
                        return full.Render(r, s, data, it, degrade, isCandle);
                    return base.Render(r, s, data, it, degrade, isCandle);
                }
            };

            RoomSprite sFood = new SCellOther(sWall, sBars, new RoomSprite1x1(sp, "FOOD_1X1"))
            {
                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    if (blue.Is(it.tile()))
                    {
                        int am = Food.foodAmount(SETT.ROOMS().data.Get(it.tile()));
                        if (am > 1)
                        {
                            return base.Render(r, s, data, it, degrade, isCandle);
                        }
                    }
                    return false;
                }
            };

            RoomSprite sMisc = new SCellOther(sWall, sBars, new RoomSprite1x1(sp, "MISC_1X1"));

            RoomSprite1x1 sOpeningp = new RoomSprite1x1(sp, "OPENING_1X1")
            {
                protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.Get(rx, ry) == null;
                }
            };

            RoomSprite sOpening = new SCellOther(sWall, sBars, sOpeningp)
            {
                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return false;
                }

                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    sBars.Render(r, s, SETT.ROOMS().fData.item.Get(it.tile()).rotation, it, degrade, rotates);
                    sOpeningp.Render(r, s, GetData2(it), it, degrade, rotates);
                }
            };

            sCandle = new RoomSprite1x1(sp, "CANDLE_SHELF_1X1")
            {
                protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.Get(rx, ry) != null;
                }
            };

            FurnisherItemTile c1 = new FurnisherItemTile(
                this,
                sCandle,
                AVAILABILITY.ROOM, true);
            FurnisherItemTile dd = new FurnisherItemTile(
                this,
                sMisc,
                AVAILABILITY.NOT_ACCESSIBLE, false);
            FurnisherItemTile ll = new FurnisherItemTile(
                this,
                sLatrine,
                AVAILABILITY.NOT_ACCESSIBLE, false).SetData(CODE_LATRINE);
            FurnisherItemTile ff = new FurnisherItemTile(
                this,
                sFood,
                AVAILABILITY.NOT_ACCESSIBLE, false).SetData(CODE_FOOD);
            cc = new FurnisherItemTile(
                this,
                sMisc,
                AVAILABILITY.ROOM, false);

            FurnisherItemTile ss = new FurnisherItemTile(
                this,
                true,
                sOpening,
                AVAILABILITY.ROOM,
                false).SetData(CODE_ENTRANCE);

            FurnisherItemTile __ = null;

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {dd,dd,dd,dd,dd},
                {dd,cc,cc,cc,dd},
                {ll,ss,ff,dd,dd},
                {__,__,__,c1,__},
            }, 1);

            Flush(1, 3);

            PRISONERS_PER_CELL = (int)item(1).Stat(prisoners);
            if (PRISONERS_PER_CELL <= 0 || PRISONERS_PER_CELL > 8)
                throw new Errors.GameError("Prisoner stat must be between 1-8" + " " + " " + PRISONERS_PER_CELL);

            WORKERS_PER_CELL = (int)item(1).Stat(guards);
            if (WORKERS_PER_CELL <= 0 || WORKERS_PER_CELL > 3)
                throw new Errors.GameError("Worker stat must be between 1-3 " + " " + WORKERS_PER_CELL);
        }

        internal bool IsWithinCell(int nx, int ny, int cx, int cy)
        {
            if (SETT.ROOMS().fData.item.Get(nx, ny) != null && SETT.ROOMS().fData.sprite.Get(nx, ny) != sCandle && SETT.ROOMS().fData.item.Get(cx, cy) != null)
            {
                COORDINATE c = SETT.ROOMS().fData.itemX1Y1(nx, ny, Coo.TMP);
                nx = c.X();
                ny = c.Y();
                return SETT.ROOMS().fData.itemX1Y1(cx, cy, Coo.TMP).IsSameAs(nx, ny);
            }
            return false;
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return true;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new PrisonInstance(blue, area, init);
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override bool IsHeavy()
        {
            return true;
        }

        private class SCellOther : RoomSpriteCombo
        {
            private readonly RoomSprite other;
            private readonly RoomSprite1x1 sBars;

            public SCellOther(RoomSpriteCombo sWall, RoomSprite1x1 sBars, RoomSprite other) : base(sWall)
            {
                this.sBars = sBars;
                this.other = other;
            }

            public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
            {
                return other.Render(r, s, GetData2(it), it, degrade, isCandle);
            }

            public override byte GetData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
            {
                return other.GetData(tx, ty, rx, ry, item, itemRan);
            }

            public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
            {
                sBars.Render(r, s, SETT.ROOMS().fData.item.Get(it.tile()).rotation, it, degrade, rotates);
                base.Render(r, s, data, it, degrade, rotates);
            }
        }
    }
}