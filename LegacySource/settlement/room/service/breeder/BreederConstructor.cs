using System;
using System.IO;

namespace Settlement.Room.Service.Breeder
{
    public class BreederConstructor : Furnisher
    {
        private readonly ROOM_BREEDER blue;
        public readonly FurnisherStat workers;
        public readonly FurnisherStat coziness;
        private const int WORK = 1;

        protected BreederConstructor(ROOM_BREEDER blue, RoomInitData init) : base(init, 2, 2)
        {
            this.blue = blue;

            workers = new FurnisherStat.FurnisherStatEmployees(this);
            coziness = new FurnisherStat.FurnisherStatRelative(this, workers);

            Json sData = init.data().json("SPRITES");

            RoomSprite bug = new RoomSprite1x1(sData, "1x1_WORM")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    int ll = (int)(it.ran() + TIME.currentSecond());
                    double sp = ((ll >> 4) & 3) / 3.0;
                    animate(sp);
                    return base.render(r, s, data, it, degrade, isCandle);
                }
            };

            RoomSprite rimC = new SRim(sData, "1x1_RIM_CORNER", bug, 3);
            RoomSprite rim = new SRim(sData, "1x1_RIM_EDGE", bug, 2);
            RoomSprite mid = new RoomSprite.Imp()
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    long rr = it.bigRan();

                    for (int i = 0; i < 4; i++)
                    {
                        int ran = (int)(rr & 0xFF);
                        rr = rr >> 16;

                        if (blue.station.worm(it.tx(), it.ty(), ran))
                        {
                            bug.render(r, s, ran, it, degrade, false);

                            DIR d = DIR.ALL.getC((ran) & 7);
                            ran = ran >> 3;
                            it.setOff(d.x() * C.TILE_SIZEH / 2, d.y() * C.TILE_SIZEH / 2);
                        }
                    }

                    int am = blue.station.resources(it.tx(), it.ty(), it.ran());
                    if (am > 0)
                    {
                        blue.indus.get(0).ins().get(0).resource.renderLaying(r, it.x(), it.y(), it.ran(), am);
                    }

                    return false;
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return bug.getData(tx, ty, rx, ry, item, itemRan);
                }

                public override byte getData(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return 0;
                }
            }.sDataSet(2);

            RoomSprite sRimDec = new RoomSprite1x1(sData, "1x1_RIM_DECOR")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    animate(blue.station.aSpeed(it.tx(), it.ty()));
                    return base.render(r, s, data, it, degrade, isCandle);
                }
            };

            RoomSprite sCorner = new RoomSprite1x1(sData, "1x1_CORNER");

            RoomSprite sExtra1 = new RoomSprite1x1(sData, "1x1_DECOR");
            RoomSprite sExtra2 = new RoomSpriteXxX(sData, "2x2_DECOR", 2);

            final FurnisherItemTile ee = new FurnisherItemTile(
                this,
                rimC,
                AVAILABILITY.SOLID,
                false
            );
            final FurnisherItemTile cc = new FurnisherItemTile(
                this,
                rim,
                AVAILABILITY.SOLID,
                false
            );
            final FurnisherItemTile mm = new FurnisherItemTile(
                this,
                mid,
                AVAILABILITY.ROOM_SOLID,
                false
            );

            final FurnisherItemTile xx = new FurnisherItemTile(
                this,
                false,
                sRimDec,
                AVAILABILITY.SOLID,
                false
            );

            final FurnisherItemTile ww = new FurnisherItemTile(
                this,
                true,
                sRimDec,
                AVAILABILITY.AVOID_PASS,
                false
            );

            ww.setData(WORK);

            final FurnisherItemTile __ = new FurnisherItemTile(
                this,
                sCorner,
                AVAILABILITY.ROOM_SOLID,
                true
            );

            final FurnisherItemTile ex = new FurnisherItemTile(
                this,
                sExtra2,
                AVAILABILITY.ROOM_SOLID,
                false
            );

            final FurnisherItemTile e1 = new FurnisherItemTile(
                this,
                sExtra1,
                AVAILABILITY.ROOM_SOLID,
                false
            );

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { __, xx, xx, xx, __ },
                { ww, ee, cc, ee, ww },
                { ww, cc, mm, cc, ww },
                { ww, ee, cc, ee, ww },
                { __, xx, xx, xx, __ }
            }, init);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { __, __, xx, xx, __ },
                { __, ww, ee, cc, ee, ww },
                { __, ww, cc, mm, cc, ww },
                { __, ww, ee, cc, ee, ww },
                { __, __, xx, xx, __ }
            }, init);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { __, __, __, xx, xx, __ },
                { __, __, ww, ee, cc, ee, ww },
                { __, __, ww, cc, mm, cc, ww },
                { __, __, ww, ee, cc, ee, ww },
                { __, __, __, xx, xx, __ }
            }, init);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { e1, ex, ex, ex, e1 },
                { ex, __, __, __, ex },
                { ex, __, __, __, ex },
                { ex, __, __, __, ex },
                { e1, ex, ex, ex, e1 }
            }, init);
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new BreederInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override bool isHeavy()
        {
            return true;
        }

        private class SRim : RoomSprite1x1
        {
            private readonly RoomSprite worm;
            private readonly int dirOff;

            public SRim(Json json, string key, RoomSprite worm, int dirOff) : base(json, key)
            {
                this.worm = worm;
                this.dirOff = dirOff;
                sData(1);
            }

            protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
            {
                rx -= d.x();
                ry -= d.y();
                d = d.next(dirOff);
                rx += d.x();
                ry += d.y();

                return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 2;
            }

            private readonly Coo coo = new Coo();

            public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
            {
                COORDINATE c = SETT.ROOMS().fData.itemX1Y1(it.tx(), it.ty(), coo);
                if (c != null)
                {
                    it.ranOffset(c.x() - it.tx(), c.y() - it.ty());
                }
                base.render(r, s, data, it, degrade, false);
            }

            public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
            {
                long ran = it.bigRan();

                DIR d = rot(data);
                d = d.next(dirOff);

                if (blue.station.worm(it.tx(), it.ty(), (int)ran))
                {
                    it.setOff(d.x() * C.TILE_SIZEH / 2, d.y() * C.TILE_SIZEH / 2);
                    worm.render(r, s, (int)ran, it, degrade, false);
                }
                ran = ran >> 32;
                if (blue.station.worm(it.tx(), it.ty(), (int)ran))
                {
                    it.setOff(d.x() * C.TILE_SIZEH, d.y() * C.TILE_SIZEH);
                    it.ranOffset(data, data);
                    it.ranSwap();
                    worm.render(r, s, (int)ran, it, degrade, false);
                }
                return false;
            }

            public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
            {
                return worm.getData(tx, ty, rx, ry, item, itemRan);
            }

            public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
            {
                DIR d = rot(data);
                int s = 0;
                if ((dirOff & 1) != 0)
                {
                    s |= d.next(2).mask();
                    s |= d.next(4).mask();
                }
                else
                {
                    s |= d.mask();
                    s |= d.next(2).mask();
                    s |= d.next(4).mask();
                }
                SPRITES.cons().BIG.outline.render(r, s, x, y);
            }
        }
    }
}