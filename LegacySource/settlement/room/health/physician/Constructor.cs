using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using util.rendering;

namespace settlement.room.health.physician
{
    final class Constructor : Furnisher
    {
        private readonly ROOM_PHYSICIAN blue;
        public readonly FurnisherStat workers;
        public readonly FurnisherStat services;
        public readonly FurnisherStat quality;

        static readonly int BIT_SERVICE = 3;

        protected Constructor(ROOM_PHYSICIAN blue, RoomInitData init) : base(init, 2, 3, 88, 44)
        {
            this.blue = blue;

            workers = new FurnisherStat.FurnisherStatI(this);
            services = new FurnisherStat.FurnisherStatServices(this, blue, 1);
            quality = new FurnisherStat.FurnisherStatRelative(this, services);

            Json js = init.data().json("SPRITES");
            RoomSprite sShelf = new RoomSprite1x1(js, "SHELF_1X1")
            {
                RoomSprite top = new RoomSprite1x1(js, "SHELF_ONTOP_1X1");

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return d.orthoID() == item.rotation;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    top.render(r, s, data, it, degrade, false);
                }
            };
            RoomSprite sBunkA = new RoomSprite1xN(js, "BUNK_1X1_TOP", false);
            RoomSprite sBunkB = new RoomSprite1xN(js, "BUNK_1X1_BOTTOM", true);

            final RoomSprite1x1 top = new RoomSprite1x1(js, "TABLE_ONTOP_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    if (!(item.sprite(rx + d.perpendicular().x() * 2, ry + d.perpendicular().y() * 2) is RoomSpriteCombo))
                        return true;
                    return false;
                }
            };

            RoomSprite sTable = new RoomSpriteCombo(js, "TABLE_COMBO")
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (!SETT.LIGHTS().is(it.tx(), it.ty()))
                    {
                        top.render(r, s, SETT.ROOMS().fData.spriteData2.get(it.tile()), it, degrade, false);
                    }
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            RoomSprite sStorage = new RoomSprite1x1(js, "STORAGE_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == sBunkB;
                }
            };

            RoomSprite sDummy = new RoomSprite.Dummy();

            final FurnisherItemTile sh = new FurnisherItemTile(
                this,
                sShelf,
                AVAILABILITY.ROOM_SOLID,
                false);

            final FurnisherItemTile ch = new FurnisherItemTile(
                this,
                sStorage,
                AVAILABILITY.ROOM_SOLID,
                false);

            final FurnisherItemTile ta = new FurnisherItemTile(
                this,
                sTable,
                AVAILABILITY.ROOM_SOLID,
                true);

            final FurnisherItemTile tt = new FurnisherItemTile(
                this,
                true,
                sTable,
                AVAILABILITY.ROOM_SOLID,
                true);

            final FurnisherItemTile b1 = new FurnisherItemTile(
                this,
                true,
                sBunkA,
                AVAILABILITY.NOT_ACCESSIBLE,
                false).setData(BIT_SERVICE);

            final FurnisherItemTile b2 = new FurnisherItemTile(
                this,
                sBunkB,
                AVAILABILITY.NOT_ACCESSIBLE,
                false);

            final FurnisherItemTile ee = new FurnisherItemTile(
                this,
                true,
                sDummy,
                AVAILABILITY.ROOM,
                false);
            ee.noWalls = true;

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ch },
                new FurnisherItemTile[] { b1 },
                new FurnisherItemTile[] { b2 },
                new FurnisherItemTile[] { ta }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ch, ch },
                new FurnisherItemTile[] { b1, b1 },
                new FurnisherItemTile[] { b2, b2 },
                new FurnisherItemTile[] { ta, ta }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { b2, b2 },
                new FurnisherItemTile[] { b1, b1 },
                new FurnisherItemTile[] { ta, ta },
                new FurnisherItemTile[] { b1, b1 },
                new FurnisherItemTile[] { b2, b2 },
                new FurnisherItemTile[] { ch, ch }
            }, 4);

            flush(1, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { tt }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { tt, sh }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { tt, sh, sh }
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { tt, sh, sh, sh }
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { sh, tt, sh, sh, sh }
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { sh, sh, tt, sh, sh, sh }
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { sh, sh, sh, tt, sh, sh, sh }
            }, 7);

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