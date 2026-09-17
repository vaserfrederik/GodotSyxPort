using System;
using System.IO;
using settlement.main;
using settlement.misc.util;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.rendering;

namespace settlement.room.knowledge.school
{
    final class SchoolConstructor : Furnisher
    {
        public readonly FurnisherStat stations;
        public readonly FurnisherStat quality;

        private readonly ROOM_SCHOOL blue;

        static readonly int ISERVICE = 1;
        static readonly int IWORK = 2;

        protected SchoolConstructor(ROOM_SCHOOL blue, RoomInitData init) : base(init, 3, 2, 88, 44)
        {
            this.blue = blue;
            stations = new FurnisherStat.FurnisherStatServices(this, blue);
            quality = new FurnisherStat.FurnisherStatEfficiency(this, stations);

            Json sp = init.data().json("SPRITES");

            final RoomSprite sService = new RoomSprite1x1(sp, "TABLE_1X1")
            {
                final RoomSprite top = new RoomSprite1x1(sp, "BOOK_1X1")
                {
                    protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                    }
                };

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 1;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (blue.is(it.tile()))
                    {
                        DIR d = DIR.ORTHO.get(getRot(data));
                        FSERVICE f = blue.station.service(it.tx() + d.x(), it.ty() + d.y());
                        if (f != null && (f.findableReservedCanBe() || f.findableReservedIs()))
                        {
                            top.render(r, s, getData2(it), it, degrade, false);
                        }
                    }
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }
            }.sData(2);

            final RoomSprite sBench = new RoomSprite1x1(sp, "STOOL_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 2;
                }
            }.sData(1);

            final RoomSprite sShelf = new RoomSprite1x1(sp, "SHELF_1X1")
            {
                final RoomSprite top = new RoomSprite1x1(sp, "SHELF_TOP_1X1");

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    if (item.width() > 1 && item.height() > 1)
                    {
                        return (d == DIR.ORTHO.get(item.rotation) || d == DIR.ORTHO.get(item.rotation).perpendicular()) && item.sprite(rx, ry) == this;
                    }
                    return DIR.ORTHO.get(item.rotation) == d;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    top.render(r, s, data, it, degrade, false);
                }
            };

            final RoomSprite sTable = new RoomSprite1x1(sService)
            {
                final RoomSprite top = new RoomSprite1x1(sp, "TABLE_TOP_1X1");

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    if (item.width() > 1 && item.height() > 1)
                    {
                        return (d == DIR.ORTHO.get(item.rotation) || d == DIR.ORTHO.get(item.rotation).perpendicular()) && item.sprite(rx, ry) == this;
                    }
                    return DIR.ORTHO.get(item.rotation) == d;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (!SETT.ROOMS().fData.candle.is(it.tile()))
                    {
                        top.render(r, s, getData2(it), it, degrade, false);
                    }
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            final FurnisherItemTile ss = new FurnisherItemTile(this, true, sService, AVAILABILITY.ROOM_SOLID, false);
            ss.setData(IWORK);
            final FurnisherItemTile bb = new FurnisherItemTile(this, true, sBench, AVAILABILITY.AVOID_PASS, false);
            bb.setData(ISERVICE);
            final FurnisherItemTile sh = new FurnisherItemTile(this, false, sShelf, AVAILABILITY.ROOM_SOLID, false);
            final FurnisherItemTile ta = new FurnisherItemTile(this, false, sTable, AVAILABILITY.ROOM_SOLID, true);

            final FurnisherItemTile __ = null;

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ss, ta, },
                {__, bb, __, }, 
            }, 1, 1);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ss, ss, ta, },
                {__, bb, bb, __, }, 
            }, 2, 2);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ss, ss, ss, ta, },
                {__, bb, bb, bb, __, }, 
            }, 3, 3);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ss, ss, ss, ss, ta, },
                {__, bb, bb, bb, bb, __, }, 
            }, 4, 4);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, ss, ss, ss, ss, ta, },
                {__, bb, bb, bb, bb, __, }, 
            }, 5, 5);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, sh, }, 
                {ta, sh, }, 
            }, 4, 4);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, sh, sh, }, 
                {ta, sh, sh, }, 
            }, 6, 6);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, sh, sh, sh, }, 
                {ta, sh, sh, sh, }, 
            }, 8, 8);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, sh, sh, sh, sh }, 
                {ta, sh, sh, sh, sh }, 
            }, 10, 10);

            new FurnisherItem(new FurnisherItemTile[][] {
                {ta, sh, sh, sh, sh, sh }, 
                {ta, sh, sh, sh, sh, sh }, 
            }, 12, 12);

            flush(3);

            FurnisherItemTools.makeUnder(this, sp, "CARPET_COMBO");
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
            return new SchoolInstance(blue, area, init);
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