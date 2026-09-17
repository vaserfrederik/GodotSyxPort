using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Service.Food.Eatery
{
    class Constructor : Furnisher
    {
        static readonly int MAX = 16;

        readonly FurnisherStat storage;
        readonly FurnisherStat workers;
        private readonly ROOM_EATERY blue;

        private const int CR = 1;
        private const int DP = 2;

        bool IsCrate(int tx, int ty)
        {
            return SETT.ROOMS().fData.tileData.Get(tx, ty) == CR;
        }

        bool IsDeposit(int tx, int ty)
        {
            return SETT.ROOMS().fData.tileData.Get(tx, ty) == DP;
        }

        protected Constructor(ROOM_EATERY blue, RoomInitData init) : base(init, 1, 2, 88, 44)
        {
            this.blue = blue;
            storage = new FurnisherStat.FurnisherStatServices(this, blue, 1);
            workers = new FurnisherStat.FurnisherStatEmployees(this, 0.01);

            Json sp = init.Data().Json("SPRITES");

            RoomSprite spriteCrate = new RoomSprite1x1(sp, "CRATE_BOTTOM_A_1X1")
            {
                RoomSprite top = new RoomSprite1x1(sp, "CRATE_TOP_1X1");

                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    top.Render(r, s, data, it, degrade, false);
                }

                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, bool isCandle)
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    EateryInstance i = blue.Getter.Get(it.Tile());
                    if (i != null)
                    {
                        int ran = it.Ran();
                        ResGroup<ResGEat> es = RESOURCES.EDI();
                        for (int ri = 1; ri <= 2; ri++)
                        {
                            ResG res = es.All().Get((ran & 0x0F) % es.All().Count);
                            ran = ran >> 4;
                            double d = blue.Dist.Stored(res.Resource).Get(i);
                            d /= i.DistData.MaxAmount * ri;
                            d *= 16;
                            ran = ran >> 4;
                            res.Resource.RenderLaying(r, it.X(), it.Y(), ran, d);
                        }
                    }
                    return false;
                }
            };

            RoomSprite spriteStall = new RoomSprite1x1(sp, "STALL_BOTTOM_1X1")
            {
                RoomSprite1x1 top = new RoomSprite1x1(sp, "STALL_TOP_1X1")
                {
                    protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return J(tx, ty, rx, ry, d, item);
                    }
                };

                protected override bool Joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return J(tx, ty, rx, ry, d, item);
                }

                private bool J(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    if (item.Width() == 1 || item.Height() == 1)
                        return d.Id() == item.Rotation;

                    if ((DIR.ORTHO.Get(item.Rotation).X() * d.X() != 0 || DIR.ORTHO.Get(item.Rotation).Y() * d.Y() != 0) && item.Sprite(rx, ry) == this)
                        return true;

                    return false;
                }

                public override void RenderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    top.Render(r, s, GetData2(it), it, degrade, false);
                }

                public override byte GetData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.GetData(tx, ty, rx, ry, item, itemRan);
                }

                public override bool Render(SPRITE_RENDERER r, ShadowBatch s, int data, util.rendering.RenderData.RenderIterator it, double degrade, bool isCandle)
                {
                    base.Render(r, s, data, it, degrade, isCandle);
                    EateryInstance ins = blue.Getter.Get(it.Tile());
                    if (blue.Dist.IsWorked(it.Tx(), it.Ty()))
                    {
                        long ran = it.BigRan();
                        ResGroup<ResGEat> es = RESOURCES.EDI();
                        DIR dir = rot(data);

                        int dim = C.TILE_SIZE / 6;

                        int x1 = it.X() + C.TILE_SIZEH - (C.TILE_SIZEH - dim) * dir.Next(2).X();
                        int y1 = it.Y() + C.TILE_SIZEH - (C.TILE_SIZEH - dim) * dir.Next(2).Y();
                        x1 -= dim * dir.X();
                        y1 -= dim * dir.Y();
                        int start = ((int)ran) % 6;
                        ran = ran >> 3;
                        for (int i = 0; i < 6; i++)
                        {
                            int pos = i + start;
                            pos %= 6;
                            int x = x1 + pos * dim;
                            int y = y1;
                            res.Resource.Render(r, x, y, ran, dim);
                            ran = ran >> 4;
                        }
                    }
                    return false;
                }
            };

            RoomSprite spriteMisc = new RoomSprite1x1(sp, "CRATE_BOTTOM_A_1X1");

            FurnisherItemTile cr = new FurnisherItemTile(this, false, spriteCrate, AVAILABILITY.SOLID, true);
            FurnisherItemTile st = new FurnisherItemTile(this, false, spriteStall, AVAILABILITY.SOLID, true);
            FurnisherItemTile mm = new FurnisherItemTile(this, false, spriteMisc, AVAILABILITY.SOLID, true);
            FurnisherItemTile __ = new FurnisherItemTile(this, false, sFrame, AVAILABILITY.ROOM, false);

            new FurnisherItem(new FurnisherItemTile[][] {
                {cr,st,st,st,mm},
                {__,__,__,__,__},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][] {
                {cr,st,st,st,st,mm},
                {__,__,__,__,__,__},
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][] {
                {cr,st,st,st,st,st,mm},
                {__,__,__,__,__,__,__},
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][] {
                {cr,st,st,st,st,st,st,mm},
                {__,__,__,__,__,__,__,__},
            }, 7);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__,__,__,__,},
                {mm,st,st,cr,},
                {cr,st,st,mm,},
                {__,__,__,__,},
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__,__,__,__,__,},
                {mm,st,st,st,cr,},
                {cr,st,st,st,mm,},
                {__,__,__,__,__,},
            }, 8);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__,__,__,__,__,},
                {mm,st,st,st,st,cr,},
                {cr,st,st,st,st,mm,},
                {__,__,__,__,__,},
            }, 10);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__,__,__,__,__,},
                {mm,st,st,st,st,st,cr,},
                {cr,st,st,st,st,st,mm,},
                {__,__,__,__,__,},
            }, 12);

            new FurnisherItem(new FurnisherItemTile[][] {
                {__,__,__,__,__,},
                {mm,st,st,st,st,st,st,cr},
                {cr,st,st,st,st,st,st,mm},
                {__,__,__,__,__,},
            }, 14);

            Flush(1, 3);
        }

        public override bool UsesArea()
        {
            return true;
        }

        public override bool MustBeIndoors()
        {
            return false;
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            return new EateryInstance(blue, area, init);
        }
    }
}