using System;
using System.IO;

namespace Settlement.Room.Service.Hygine.Well
{
    public class Constructor : Furnisher
    {
        private readonly ROOM_WELL blue;

        public FurnisherStat services = new FurnisherStat.FurnisherStatI(this);
        private static readonly Fountain fountain = new Fountain();
        private const int codeService = 1;

        protected Constructor(ROOM_WELL blue, RoomInitData init) : base(init, 1, 1, 88, 44)
        {
            this.blue = blue;

            Json sp = init.data().json("SPRITES");

            RoomSpriteCombo sStencil = new RoomSpriteCombo(sp, "STONE_RING_STENCIL_COMBO");
            RoomSprite sRoof = new RoomSprite1x1(sp, "ROOF_EDGE_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    FurnisherItem i = SETT.ROOMS().fData.item.get(it.tile());
                    if ((i.width() & 1) == 0)
                        it.setOff(0, -C.TILE_SIZEH);
                    return base.render(r, s, data, it, degrade, isCandle);
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    if (rx - d.x() >= item.width() / 2)
                        return d.x() < 0;
                    return d.x() > 0;
                }
            };

            RoomSprite sRoofMid = new RoomSprite1x1(sp, "ROOF_MID_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    FurnisherItem i = SETT.ROOMS().fData.item.get(it.tile());
                    if ((i.width() & 1) == 0)
                        it.setOff(0, -C.TILE_SIZEH);
                    return base.render(r, s, data, it, degrade, isCandle);
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return d.x() > 0;
                }
            };

            RoomSprite sFountain = new RoomSprite1x1(sp, "FOUNTAIN_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    fountain.render(r, s, it.x() + C.TILE_SIZEH, it.y() + C.TILE_SIZEH);
                    return base.render(r, s, data, it, degrade, isCandle);
                }
            };

            RoomSprite sWellR = new RoomSpriteCombo(sp, "STONE_RING_COMBO")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    if (blue.is(it.tile()))
                    {
                        sStencil.render(r, s, data, it, degrade, false);
                        SETT.TERRAIN().WATER.renderOverlayed(it);
                    }
                    it.countWater();
                    it.countWater();
                    return false;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.render(r, s, data, it, degrade, false);
                    int up = blue.is(it.tile()) && blue.getter.get(it.tile()).upgrade() > 0 ? 1 : 0;
                    RoomSprite roo = eSprite(SETT.ROOMS().fData.item.get(it.tile()), data, up);
                    if (roo != null)
                    {
                        roo.render(r, s, getData2(it), it, degrade, false);
                    }
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    RoomSprite roo = eSprite(item, getData(tx, ty, rx, ry, item, itemRan), 0);
                    if (roo != null)
                        return roo.getData(tx, ty, rx, ry, item, itemRan);
                    return 0;
                }

                private RoomSprite eSprite(FurnisherItem item, int data, int up)
                {
                    if (item.width() == 4)
                    {
                        if ((data & DIR.S.mask()) == 0)
                        {
                            return sRoof;
                        }
                    }
                    else if (item.width() == 5)
                    {
                        if (up > 0)
                        {
                            if ((data & 0x0F) == 0x0F)
                                return sFountain;
                        }
                        else
                        {
                            if ((data & DIR.S.mask()) != 0 && (data & DIR.N.mask()) != 0)
                            {
                                if ((data & 0x0F) == 0x0F)
                                    return sRoofMid;
                                return sRoof;
                            }
                        }
                    }
                    return null;
                }
            };

            RoomSprite sService = new RoomSprite1x1(sp, "BUCKET_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    rx -= d.x() * 2;
                    ry -= d.y() * 2;
                    return item.get(rx, ry) == null;
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    if (blue.is(it.tile()))
                    {
                        if (blue.bed.isUsed(it.tile()))
                            return base.render(r, s, data, it, degrade, isCandle);
                    }
                    return false;
                }
            };

            FurnisherItem item1 = new FurnisherItem(sStencil, sRoof, sRoofMid, sFountain, sWellR, sService);
            FurnisherItem item2 = new FurnisherItem(sStencil, sRoof, sRoofMid, sFountain, sWellR, sService);
            FurnisherItem item3 = new FurnisherItem(sStencil, sRoof, sRoofMid, sFountain, sWellR, sService);
            FurnisherItem item4 = new FurnisherItem(sStencil, sRoof, sRoofMid, sFountain, sWellR, sService);

            FurnisherItem[,] items = new FurnisherItem[4, 4]
            {
                { item1, item2, item3, item4 },
                { item1, item2, item3, item4 },
                { item1, item2, item3, item4 },
                { item1, item2, item3, item4 }
            };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    FurnisherItem item = items[i, j];
                    FurnisherItem[] neighbors = new FurnisherItem[4];
                    neighbors[0] = i > 0 ? items[i - 1, j] : null;
                    neighbors[1] = i < 3 ? items[i + 1, j] : null;
                    neighbors[2] = j > 0 ? items[i, j - 1] : null;
                    neighbors[3] = j < 3 ? items[i, j + 1] : null;
                    item.setNeighbors(neighbors);
                }
            }
        }

        public override bool usesGroundTile()
        {
            return true;
        }

        public override COLOR miniColor(int tx, int ty)
        {
            return miniC.get(tx, ty);
        }

        private class Fountain
        {
            private const int AM = 64;
            private byte[] xs = Alloc.bb(AM);
            private byte[] ys = Alloc.bb(AM);
            private double[] rans = new double[AM];
            private COLOR[] cols = new COLOR[AM];

            public Fountain()
            {
                for (int i = 0; i < AM; i++)
                {
                    double rad = RND.rFloat() * Math.PI * 0.5;
                    double dx = Math.cos(rad);
                    double dy = Math.sin(rad);
                    xs[i] = (byte)(dx * (C.TILE_SIZEH / 2 + RND.rFloat() * C.TILE_SIZE));
                    ys[i] = (byte)(dy * (C.TILE_SIZEH / 2 + RND.rFloat() * C.TILE_SIZE));
                    rans[i] = RND.rInt(128) + RND.rFloat();
                }
                cols = COLOR.interpolate(new ColorImp(20, 60, 127), COLOR.WHITE100, AM);
            }

            public void render(SPRITE_RENDERER r, ShadowBatch s, int cx, int cy)
            {
                double time = TIME.currentSecond() * 1.5;
                render(r, s, cx, cy, time, 1, 1);
                time += 0.3;
                render(r, s, cx - C.SCALE, cy, time, -1, 1);
                time += 0.3;
                render(r, s, cx, cy - C.SCALE, time, 1, -1);
                time += 0.3;
                render(r, s, cx - C.SCALE, cy - C.SCALE, time, -1, -1);
            }

            public void render(SPRITE_RENDERER r, ShadowBatch s, int cx, int cy, double time, int dx, int dy)
            {
                int a = AM;
                if (TIME.light().nightIs())
                {
                    a *= 1.0 - TIME.light().partOf() * 10;
                }
                for (int i = 0; i < a; i++)
                {
                    double d = rans[i] + time;
                    int k = (int)d;
                    d = d - k;
                    int x = (int)(xs[i] * d);
                    int y = (int)(ys[i] * d);
                    cols[k & (AM - 1)].bind();
                    CORE.renderer().renderParticle(cx + x * dx, cy + y * dy);
                }
            }
        }
    }
}