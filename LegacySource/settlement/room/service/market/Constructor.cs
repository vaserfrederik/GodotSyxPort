using System;
using System.IO;
using init.constant;
using init.race;
using init.race.RaceResources;
using init.resources;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using util.rendering;

namespace settlement.room.service.market
{
    public sealed class Constructor : Furnisher
    {
        public static readonly int MAX = 16;

        public readonly FurnisherStat storage;
        public readonly FurnisherStat workers;

        private readonly ROOM_MARKET blue;

        private const int ST = 2;
        private const int CR = 1;

        public Constructor(ROOM_MARKET blue, RoomInitData init) : base(init, 1, 2, 88, 44)
        {
            this.blue = blue;
            storage = new FurnisherStat.FurnisherStatServices(this, blue, 1);
            workers = new FurnisherStat.FurnisherStatEmployees(this, 0.01);

            Json sp = init.data().json("SPRITES");

            var spriteCrate = new RoomSprite1x1(sp, "CRATE_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    MarketInstance i = blue.getter.get(it.tile());
                    if (i != null)
                    {
                        int ran = it.ran();
                        for (int ri = 1; ri <= 2; ri++)
                        {
                            RaceResource res = RACES.res().ALL.get((ran & 0x0FF) % RACES.res().ALL.size());
                            ran = ran >> 4;
                            double d = blue.dist.stored(res.res).get(i);
                            d /= i.distData.maxAmount * ri;
                            d *= 16;
                            ran = ran >> 4;
                            res.res.renderLaying(r, it.x(), it.y(), ran, d);
                        }
                    }
                    return false;
                }
            };

            var spriteStall = new RoomSpriteCombo(sp, "STALL_BOTTOM_COMBO")
            {
                RoomSprite1x1 top = new RoomSprite1x1(sp, "STALL_TOP_1X1")
                {
                    protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return j(tx, ty, rx, ry, d, item);
                    }
                };

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == this;
                }

                private bool j(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    if (item.width() == 1 || item.height() == 1)
                        return d.id() == item.rotation;

                    if ((DIR.ORTHO.get(item.rotation).x() * d.x() != 0 || DIR.ORTHO.get(item.rotation).y() * d.y() != 0) && item.sprite(rx, ry) == this)
                        return true;

                    return false;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    top.render(r, s, getData2(it), it, degrade, false);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    MarketInstance ins = blue.getter.get(it.tile());
                    if (blue.dist.isWorked(it.tx(), it.ty()))
                    {
                        long ran = it.bigRan();
                        DIR dir = top.rot(data);

                        int dim = C.TILE_SIZE / 6;

                        int x1 = it.x() + C.TILE_SIZEH - (C.TILE_SIZEH - dim) * dir.next(2).x();
                        int y1 = it.y() + C.TILE_SIZEH - (C.TILE_SIZEH - dim) * dir.next(2).y();
                        x1 -= dim * dir.x();
                        y1 -= dim * dir.y();
                        int start = ((int)ran) % 6;
                        ran = ran >> 3;
                        for (int i = 0; i < 6; i++)
                        {
                            int pos = i + start;
                            pos %= 6;
                            int x = x1 + dir.next(2).x() * pos * dim;
                            int y = y1 + dir.next(2).y() * pos * dim;
                            RESOURCE res = blue.dist.all.get(((int)ran & 0x0F) % blue.dist.all.size());

                            ran = ran >> 4;

                            double d = blue.dist.stored(res).get(ins);
                            if (d > (ran & 0b111) / (double)0b111)
                                res.renderOneC(r, x, y, (int)ran);
                            ran = ran >> 2;
                        }
                    }
                    return false;
                }
            };

            var spriteMisc = new RoomSprite1x1(sp, "MISC_BOTTOM_1X1")
            {
                RoomSprite1x1 top = new RoomSprite1x1(sp, "MISC_1X1");
                public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (!SETT.ROOMS().fData.candle.is(it.tile()))
                        top.renderRandom(r, s, it, it.ran(), degrade);
                };
            };

            var sFrame = new RoomSpriteCombo(sp, "CARPET_COMBO")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return false;
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return false;
                }
            };

            var crateTile = new FurnisherItemTile(new FurnisherItemTile.Sprite[] { spriteCrate }, 0);
            var stallTile = new FurnisherItemTile(new FurnisherItemTile.Sprite[] { spriteStall }, 1);
            var miscTile = new FurnisherItemTile(new FurnisherItemTile.Sprite[] { spriteMisc }, 2);
            var frameTile = new FurnisherItemTile(new FurnisherItemTile.Sprite[] { sFrame }, 3);

            var item1 = new FurnisherItem(new FurnisherItemTile[] { crateTile, stallTile, stallTile, stallTile, miscTile }, 1);
            var item2 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 2);
            var item3 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 3);
            var item4 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 4);
            var item5 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 5);
            var item6 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 6);
            var item7 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 7);
            var item8 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 8);
            var item9 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 9);
            var item10 = new FurnisherItem(new FurnisherItemTile[] { frameTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, stallTile, crateTile, miscTile }, 10);

            flush(1, 3);
        }

        public override bool usesArea()
        {
            return true;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new MarketInstance(blue, area, init);
        }
    }
}