using System;
using System.IO;
using settlement.constant;
using settlement.main;
using settlement.misc.util;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using util.rendering;

namespace settlement.room.service.barber
{
    class Constructor : Furnisher
    {
        private readonly ROOM_BARBER blue;

        public const int IWORK = 1;

        public readonly FurnisherStat latrines;
        public readonly FurnisherStat workers;
        public readonly FurnisherStat quality;

        protected Constructor(ROOM_BARBER blue, RoomInitData init) : base(init, 2, 3, 88, 44)
        {
            this.blue = blue;

            latrines = new FurnisherStat.FurnisherStatServices(this, blue);
            workers = new FurnisherStat.FurnisherStatEmployeesR(this, latrines, 1);
            quality = new FurnisherStat.FurnisherStatRelative(this, latrines);

            Json sp = init.data().json("SPRITES");

            RoomSprite sNick = new RoomSprite1x1(sp, "NICKNACK_1X1");
            RoomSprite sCentre = new RoomSprite1x1(sp, "TABLE_CENTRE_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 22;
                }
            };
            RoomSprite sCentreTop = new RoomSprite1x1(sp, "TABLE_CENTRE_TOP_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry).sData() == 22;
                }
            };
            RoomSprite sTable = new RoomSpriteCombo(sp, "TABLE_COMBO")
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (!SETT.ROOMS().fData.candle.is(it.tile()))
                    {
                        sNick.render(r, s, getData2(it), it, degrade, false);
                    }
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return sNick.getData(tx, ty, rx, ry, item, itemRan);
                }
            };
            RoomSprite sTableC = new RoomSpriteCombo(sTable)
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    sCentre.render(r, s, getData2(it), it, degrade, false);
                    sCentreTop.render(r, s, getData2(it), it, degrade, false);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return sCentre.getData(tx, ty, rx, ry, item, itemRan);
                }
            };
            RoomSprite sChair = new RoomSprite1x1(sp, "CHAIR_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == sTableC;
                }

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    FSERVICE ss = blue.ll.service(it.tx(), it.ty());
                    if (ss == null)
                        return false;
                    if (ss.findableReservedIs() || ss.findableReservedCanBe())
                        return base.render(r, s, data, it, degrade, false);
                    return false;
                }
            }.sData(22);
            RoomSprite sSeparator = new RoomSprite1x1(sp, "SEPARATOR_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    DIR rot = rot(data);
                    it.setOff(-rot.x() * C.TILE_SIZEH / 2, -rot.y() * C.TILE_SIZEH / 2);
                    return base.render(r, s, data, it, degrade, isCandle);
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    rx -= d.x();
                    ry -= d.y();
                    d = d.next(-2);
                    rx += d.x();
                    ry += d.y();
                    return item.sprite(rx, ry) == sTableC;
                }
            };

            final FurnisherItemTile tt = new FurnisherItemTile(this, false, sTable, AVAILABILITY.ROOM_SOLID, true);
            final FurnisherItemTile tc = new FurnisherItemTile(this, false, sTableC, AVAILABILITY.ROOM_SOLID, false);
            final FurnisherItemTile oo = new FurnisherItemTile(this, true, sChair, AVAILABILITY.AVOID_PASS, false);
            oo.setData(IWORK);
            final FurnisherItemTile __ = new FurnisherItemTile(this, false, sSeparator, AVAILABILITY.ROOM_SOLID, false);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { tt, __, },
                { tc, oo, },
                { tt, __, },
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
            }, 3);
            new FurnisherItem(new FurnisherItemTile[][]
            {
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
            }, 4);
            new FurnisherItem(new FurnisherItemTile[][]
            {
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
            }, 8);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tc, oo, },
                { tt, __, },
            }, 10);

            flush(1, 3);

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
            return new Instance(blue, area, init);
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