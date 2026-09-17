using System;
using System.IO;
using init.constant;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.rendering;

namespace settlement.room.service.nursery
{
    final class NurseryConstructor : Furnisher
    {
        private readonly ROOM_NURSERY blue;
        public const int TABLE = 1;
        public const int CARPET = 2;
        public readonly FurnisherStat workers;
        public readonly FurnisherStat kids;
        public readonly FurnisherStat coziness;

        protected NurseryConstructor(ROOM_NURSERY blue, RoomInitData init)
            : base(init, 3, 3)
        {
            this.blue = blue;

            kids = new FurnisherStat.FurnisherStatI(this);
            workers = new FurnisherStat.FurnisherStatEmployeesR(this, kids, 0.2);
            coziness = new FurnisherStat.FurnisherStatRelative(this, kids);

            Json sData = init.data().json("SPRITES");

            RoomSprite sStuff = new RoomSprite1x1(sData, "STUFF_1X1");

            RoomSprite sAbove = new RoomSprite.Dummy()
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it,
                        double degrade, bool isCandle)
                {
                    if (!isCandle)
                    {
                        long rr = it.bigRan();
                        for (int i = 0; i < blue.ss.stuff(it.tx(), it.ty()); i++)
                        {
                            it.ranOffset(DIR.ORTHO.get(i).x(), DIR.ORTHO.get(i).y());
                            DIR dd = DIR.ALL.getC((int)(rr & 0x0111));
                            it.setOff((int)(dd.xN() * C.TILE_SIZEH / 2), (int)(dd.yN() * C.TILE_SIZEH / 2));
                            rr = rr >> 3;
                            int data2 = (int)(rr & 0x0F);
                            rr = rr >> 4;
                            sStuff.render(r, s, data2, it, degrade, false);
                        }

                    }
                    return false;
                }
            };

            RoomSprite sTable = new RoomSpriteCombo(sData, "TABLE_COMBO")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
                        bool isCandle)
                {
                    bool ret = base.render(r, s, data, it, degrade, isCandle);

                    sAbove.render(r, s, data, it, degrade, isCandle);

                    return ret;
                }
            };
            RoomSprite sChair = new RoomSprite1x1(sData, "1x1_CHAIR")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) is RoomSpriteCombo;
                }
            };
            RoomSprite sShelf = new RoomSprite1x1(sData, "SHELF_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return d.orthoID() == item.rotation;
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade,
                        bool isCandle)
                {

                    base.render(r, s, data, it, degrade, isCandle);
                    //DIR d = rot(data);
                    //it.setOff(d.x() * C.TILE_SIZEH / 2, d.y() * C.TILE_SIZEH / 2);
                    sStuff.render(r, s, getData2(it), it, degrade, false);
                    return false;
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return sStuff.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            final FurnisherItemTile tt = new FurnisherItemTile(
                    this,
                    sTable,
                    AVAILABILITY.ROOM_SOLID,
                    true);
            tt.setData(TABLE);

            final FurnisherItemTile ss = new FurnisherItemTile(
                    this,
                    true,
                    sChair,
                    AVAILABILITY.AVOID_PASS,
                    false);

            final FurnisherItemTile sh = new FurnisherItemTile(
                    this,
                    sShelf,
                    AVAILABILITY.ROOM_SOLID,
                    false);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ss, tt, ss},
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ss, tt, ss},
                {ss, tt, ss},
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
            }, 7);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
                {ss, tt, ss},
            }, 8);

            flush(3);

            FurnisherItemTile cc = FurnisherItemTools.makeUnder(this, sData, "CARPET_COMBO", sAbove);
            cc.setData(CARPET);

            new FurnisherItem(new FurnisherItemTile[][] {
                {sh},
            }, 2);
            new FurnisherItem(new FurnisherItemTile[][] {
                {sh, sh},
            }, 4);
            new FurnisherItem(new FurnisherItemTile[][] {
                {sh, sh, sh},
            }, 6);
            new FurnisherItem(new FurnisherItemTile[][] {
                {sh, sh, sh, sh},
            }, 8);
            new FurnisherItem(new FurnisherItemTile[][] {
                {sh, sh, sh, sh, sh},
            }, 10);

            flush(3);
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
            return new NurseryInstance(blue, area, init);
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