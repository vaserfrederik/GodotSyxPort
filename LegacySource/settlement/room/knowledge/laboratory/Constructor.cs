using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.knowledge.laboratory
{
    final class Constructor : Furnisher
    {
        public readonly FurnisherStat workers = new FurnisherStat(this)
        {
            public override double get(AREA area, double fromItems)
            {
                return fromItems;
            }

            public override GText format(GText t, double value)
            {
                return GFORMAT.i(t, (int)value);
            }
        };

        public readonly FurnisherStat knowledge = new FurnisherStat(this)
        {
            public override double get(AREA area, double fromItems)
            {
                return fromItems;
            }

            public override GText format(GText t, double value)
            {
                return GFORMAT.i(t, (int)(value * blue.data.knowledgePerStation));
            }
        };

        public const int WORK = 1;

        private readonly ROOM_LABORATORY blue;
        public readonly RoomSprite1x1 schair;

        protected Constructor(ROOM_LABORATORY blue, RoomInitData init) : base(init, 1, 2, 88, 44)
        {
            this.blue = blue;

            Json sj = init.data().json("SPRITES");

            schair = new RoomSprite1x1(sj, "CHAIR_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) != null && item.sprite(rx, ry) != this;
                }
            };

            final RoomSprite1x1 tablet = new RoomSprite1x1(sj, "TABLE_KNOWLEDGE_ONTOP_1X1");

            final RoomSpriteImp sTableWork = new RoomSpriteCombo(sj, "TABLE_COMBO")
            {
                final RoomSprite1x1 ontop = new RoomSprite1x1(sj, "WORK_TABLE_1X1")
                {
                    protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return (item.sprite(rx + d.perpendicular().x() * 2, ry + d.perpendicular().y() * 2) == schair);
                    }
                };

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (blue.job.used(it.tx(), it.ty()))
                    {
                        int i = 0;
                        foreach (DIR d in DIR.ORTHO)
                        {
                            if (SETT.ROOMS().fData.sprite.is(it.tx(), it.ty(), d, schair))
                            {
                                tablet.render(r, s, i, it, degrade, false);
                                break;
                            }
                            i++;
                        }
                    }

                    ontop.render(r, s, getData2(it), it, degrade, false);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return ontop.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            final RoomSprite sTableStorage = new RoomSpriteCombo(sTableWork)
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (!SETT.ROOMS().fData.candle.is(it.tile()) && blue.is(it.tx(), it.ty()))
                    {
                        int f = (blue.data.usedD & 0x0FF);
                        int d = (it.ran() & 0x03F);

                        int m = 4;
                        while (f > d && m-- > 0)
                        {
                            tablet.renderRandom(r, s, it, it.ran(), degrade);
                            it.ranOffset(1, 0);
                            f -= d;
                        }
                    }
                }
            };

            final RoomSprite sShelf = new RoomSprite1x1(sj, "SHELF_1X1")
            {
                final RoomSprite top = new RoomSprite1x1(sj, "SHELF_DECOR_1x1")
                {
                    protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                    {
                        return item.sprite(rx, ry) == this;
                    }
                };

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (blue.job.used(it.tx(), it.ty()))
                    {
                        top.render(r, s, getData2(it), it, degrade, false);
                    }
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == this;
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            final RoomSprite sExtra = new RoomSprite1x1(sj, "WORK_STANDALONE_1X1");

            final FurnisherItemTile __ = null;
            final FurnisherItemTile sh = new FurnisherItemTile(this, true, sShelf, AVAILABILITY.ROOM_SOLID, true);
            final FurnisherItemTile st = new FurnisherItemTile(this, false, sTableStorage, AVAILABILITY.ROOM_SOLID, true);
            final FurnisherItemTile ch = new FurnisherItemTile(this, true, schair, AVAILABILITY.AVOID_PASS, true);
            final FurnisherItemTile ex = new FurnisherItemTile(this, true, sExtra, AVAILABILITY.ROOM_SOLID, true);
            final FurnisherItemTile ww = new FurnisherItemTile(this, false, sTableWork, AVAILABILITY.ROOM_SOLID, false);
            ex.setData(WORK);
            ww.setData(WORK);
            sh.setData(WORK);
            st.setData(WORK);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {sh, st, ww, st, ex },
                {__, ch, ch, ch, __ }
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {sh, st, ww, ww, st, ex },
                {__, ch, ch, ch, ch, __ }
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {sh, st, ww, ww, ww, st, ex },
                {__, ch, ch, ch, ch, ch, __ }
            }, 14);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {sh, st, ww, ww, ww, ww, st, ex },
                {__, ch, ch, ch, ch, ch, ch, __ }
            }, 16);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                {sh, st, ww, ww, ww, ww, ww, st, ex },
                {__, ch, ch, ch, ch, ch, ch, ch, __ }
            }, 18);

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
            return new LaboratoryInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override bool isHeavy()
        {
            return true;
        }

        // private final FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][]
        // {
        //     {0, 0, 0, 0, 0, 0, 0, 0},
        //     {0, 1, 1, 1, 1, 1, 1, 1},
        //     {0, 1, 1, 1, 1, 1, 0, 1},
        //     {0, 1, 1, 1, 1, 1, 0, 1},
        //     {0, 1, 1, 1, 1, 1, 0, 1},
        //     {0, 1, 1, 1, 1, 1, 0, 1},
        //     {0, 1, 1, 1, 1, 1, 1, 1},
        //     {0, 0, 0, 0, 0, 0, 0, 0},
        // },
        // miniColor
        // );

        // public override COLOR miniColor(int tx, int ty)
        // {
        //     return miniC.get(tx, ty);
        // }
    }
}