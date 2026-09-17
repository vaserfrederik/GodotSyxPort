using System;
using System.IO;
using System.Collections.Generic;
using settlement.path;
using settlement.room.main;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using util.rendering;

namespace settlement.room.knowledge.university
{
    public sealed class UniversityConstructor : Furnisher
    {
        public readonly FurnisherStat students = new FurnisherStat.FurnisherStatI(this, 1);
        public readonly FurnisherStat quality = new FurnisherStat.FurnisherStatEfficiency(this, students, 1);

        private readonly ROOM_UNIVERSITY blue;

        private const int IWORK = 1;
        private const int IWORKE = 2;

        public UniversityConstructor(ROOM_UNIVERSITY blue, RoomInitData init) : base(init, 2, 2, 88, 44)
        {
            this.blue = blue;

            Json sp = init.data().json("SPRITES");

            RoomSprite sBench = new RoomSprite1x1(sp, "BENCH_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (item.sprite(rx + d.x() * i, ry + d.y() * i) != null && item.sprite(rx + d.x() * i, ry + d.y() * i).sData() == 2)
                            return true;
                    }
                    return false;
                }

                public override void renderPlaceholder(SPRITE_RENDERER r, int x, int y, int data, int tx, int ty, int rx, int ry, FurnisherItem item)
                {
                    type().renderOverlay(x, y, r, AVAILABILITY.AVOID_PASS, 0, data, true);
                }
            };

            RoomSprite sCarpet = new RoomSpriteCombo(sp, "CARPET_COMBO")
            {
                public override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.render(r, s, data, it, degrade, false);
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return false;
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                }
            }.sData(1);
            RoomSprite sCarpetCandle = new RoomSpriteCombo(sCarpet)
            {
                private RoomSprite ca = new RoomSprite1x1(sp, "TORCH_1X1");

                public override void renderBelow(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    base.render(r, s, data, it, degrade, false);
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    return ca.render(r, s, getData2(it), it, degrade, isCandle);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return ca.getData(tx, ty, rx, ry, item, itemRan);
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                }
            }.sData(1);

            RoomSprite podium = new RoomSpriteCombo(sp, "PODIUM_COMBO")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    data = getData2(it);
                    if (data != 0)
                        sCarpet.renderBelow(r, s, getData2(it), it, degrade);
                    return false;
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    int m = 0;
                    if (!isC(rx, ry, item))
                        return 0;
                    for (int di = 0; di < DIR.ORTHO.size(); di++)
                    {
                        DIR d = DIR.ORTHO.get(di);
                        if (isC(rx + d.x(), ry + d.y(), item))
                            m |= d.mask();
                    }
                    return (byte)m;
                }

                private bool isC(int rx, int ry, FurnisherItem item)
                {
                    for (int di = 0; di < DIR.ORTHO.size(); di++)
                    {
                        DIR d = DIR.ORTHO.get(di);
                        if (item.sprite(rx + d.x(), ry + d.y()) != this)
                            return false;
                    }
                    return true;
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                }
            };

            RoomSprite sShelf = new RoomSprite1x1(sp, "SHELF");

            FurnisherItemTile benchTile = new FurnisherItemTile(sBench, 0);
            FurnisherItemTile carpetTile = new FurnisherItemTile(sCarpet, 0);
            FurnisherItemTile carpetCandleTile = new FurnisherItemTile(sCarpetCandle, 0);
            FurnisherItemTile podiumTile = new FurnisherItemTile(podium, 0);
            FurnisherItemTile shelfTile = new FurnisherItemTile(sShelf, 0);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { benchTile, benchTile, benchTile, benchTile, benchTile, benchTile, benchTile, benchTile },
                new FurnisherItemTile[] { carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile },
                new FurnisherItemTile[] { carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile },
                new FurnisherItemTile[] { carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile },
                new FurnisherItemTile[] { carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile },
                new FurnisherItemTile[] { carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile },
                new FurnisherItemTile[] { carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile },
                new FurnisherItemTile[] { carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile, carpetTile },
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { carpetTile },
                new FurnisherItemTile[] { carpetTile, shelfTile },
            }, 2);

            // Add more FurnisherItem configurations as needed...

            flush(1, 3);
        }

        public override bool usesArea()
        {
            return true;
        }

        public override bool mustBeIndoors()
        {
            return true;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new UniversityInstance(blue, area, init);
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