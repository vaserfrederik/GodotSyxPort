using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.law.stocks;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.tilemap.floor;
using snake2d;
using util.rendering;

namespace Settlement.Room.Law.Stocks
{
    final class MConstructor : Furnisher
    {
        FurnisherStat spectators;

        private readonly ROOM_STOCKS blue;
        private readonly FurnisherItemTile ss;

        MConstructor(ROOM_STOCKS blue, RoomInitData init)
            : base(init, 1, 1, 88, 44)
        {
            spectators = new FurnisherStat.FurnisherStatServices(this, blue);
            this.blue = blue;

            Json sData = init.data().json("SPRITES");

            RoomSprite ssprite = new RoomSpriteBoxN(sData, "BOX")
            {
                public override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.render(r, s, data, it, degrade, false);
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return false;
                }
            };

            RoomSprite sservice = new RoomSpriteBoxN(ssprite)
            {
                RoomSprite ssmall = new RoomSprite1x1(sData, "STOCK_BELOW_1X1")
                {
                    protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return d == DIR.ORTHO.get(item.rotation).next(2);
                    }
                };

                RoomSprite stop = new RoomSprite1x1(sData, "STOCK_TOP_1X1");

                public override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.render(r, s, data, it, degrade, false);
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return ssmall.render(r, s, getData2(it), it, degrade, isCandle);
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    Tile t = blue.tile.get(it.tx(), it.ty());
                    if (t != null && t.state() == STATE.used)
                    {
                        stop.render(r, s, getData2(it), it, degrade, false);
                    }
                }

                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
                {
                    ssmall.renderPlaceholder(r, x, y, (item.rotation + 1) % 4, tx, ty, rx, ry, item);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return ssmall.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            ss = new FurnisherItemTile(
                this,
                false,
                sservice,
                AVAILABILITY.AVOID_LIKE_FUCK,
                false
            );

            FurnisherItemTile tt = new FurnisherItemTile(
                this,
                false,
                ssprite,
                AVAILABILITY.ROOM,
                false
            );

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { tt, tt, tt },
                { tt, ss, tt },
                { tt, tt, tt }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { tt, tt, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, tt, tt }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { tt, tt, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, tt, tt }
            }, 3);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { tt, tt, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, tt, tt }
            }, 4);

            new FurnisherItem(new FurnisherItemTile[,]
            {
                { tt, tt, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, ss, tt },
                { tt, tt, tt }
            }, 5);

            flush(3);
        }

        public bool service(int tx, int ty)
        {
            return blue.is(tx, ty) && SETT.ROOMS().fData.tile.get(tx, ty) == ss;
        }

        public override bool usesArea()
        {
            return false;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new Instance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
            base.putFloor(tx, ty, upgrade, area);
            floor(tx, ty, upgrade);
        }

        private void floor(int tx, int ty, int up)
        {
            Floor res = floor(up);
            int am = 1;
            foreach (DIR d in DIR.ORTHO)
            {
                if (SETT.ROOMS().map.is(tx, ty, d))
                    continue;
                Floor f = SETT.FLOOR().getter.get(tx, ty, d);
                if (f != null && f != res)
                {
                    int a = testFloor(tx, ty, f);
                    if (a > am)
                    {
                        am = a;
                        res = f;
                    }
                }
            }

            if (SETT.FLOOR().getter.get(tx, ty) != res)
                res.placeFixed(tx, ty);
        }

        private int testFloor(int tx, int ty, Floor f)
        {
            int am = 0;
            foreach (DIR d in DIR.ALL)
            {
                if (SETT.ROOMS().map.is(tx, ty, d))
                    continue;
                Floor f2 = SETT.FLOOR().getter.get(tx, ty, d);
                if (f2 == f)
                {
                    am++;
                }
            }
            return am;
        }
    }
}