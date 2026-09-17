using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using util.rendering;

namespace settlement.room.infra.elderly
{
    final class ResthomeConstructor : Furnisher
    {
        public readonly FurnisherStat stations = new FurnisherStat.FurnisherStatI(this, 1);
        public readonly FurnisherStat quality = new FurnisherStat.FurnisherStatRelative(this, stations);

        private readonly ROOM_RESTHOME blue;

        static readonly int ITABLE = 1;
        static readonly int ISTAGE = 2;
        static readonly int ICHAIR = 3;

        protected ResthomeConstructor(ROOM_RESTHOME blue, RoomInitData init)
            : base(init, 4, 2, 88, 44)
        {
            this.blue = blue;

            Json js = init.data().json("SPRITES");
            RoomSpriteImp sChair_wall = new RoomSprite1x1(js, "CHAIR_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return d.perpendicular().orthoID() == item.rotation;
                }
            };

            RoomSprite sChair_table = new RoomSprite1x1(sChair_wall)
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.get(rx, ry) != null && item.get(rx, ry).data() == ITABLE;
                }
            };

            RoomSprite1x1 sOnTopDecor = new RoomSprite1x1(js, "ON_TOP_DECOR_1X1");
            RoomSprite1x1 sTableSingle = new RoomSprite1x1(js, "TABLE_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    bool ret = base.render(r, s, data, it, degrade, isCandle);
                    if (!isCandle)
                    {
                        sOnTopDecor.renderRandom(r, null, it, it.ran(), degrade);
                    }
                    return ret;
                }
            };

            RoomSprite1x1 sOnTopCards = new RoomSprite1x1(js, "ON_TOP_CARDS_1X1");
            RoomSpriteCombo sTable_clean = new RoomSpriteCombo(js, "TABLES_COMBO")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    if (blue.job.used(it.tx(), it.ty()))
                    {
                        for (int i = 0; i < DIR.ORTHO.size(); i++)
                        {
                            if (SETT.ROOMS().fData.sprite.get(it.tx(), it.ty(), DIR.ORTHO.get(i)) == sChair_table)
                            {
                                sOnTopCards.render(r, s, i, it, degrade, isCandle);
                            }
                        }
                    }
                    return false;
                }
            };

            RoomSpriteBoxN sStage = new RoomSpriteBoxN(js, "STAGE_COMBO");

            RoomSprite sTable_nick = new RoomSpriteCombo(sTable_clean)
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    if (!isCandle)
                    {
                        sOnTopDecor.renderRandom(r, ShadowBatch.DUMMY, it, it.ran(), degrade);
                    }
                    return false;
                }
            };

            RoomSprite sShelf = new RoomSprite1x1(js, "SHELF_1X1")
            {
                RoomSprite1x1 ontop = new RoomSprite1x1(js, "SHELF_ON_TOP_1X1");

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return d.orthoID() == item.rotation;
                }

                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    ontop.render(r, s, data, it, degrade, false);
                }
            };

            RoomSprite sNickNack = new RoomSprite1x1(js, "NICKNACK_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return d.orthoID() == item.rotation;
                }
            };

            FurnisherItemTile __ = null;
            {
                FurnisherItemTile cc = new FurnisherItemTile(this, true, sChair_wall, AVAILABILITY.AVOID_PASS, false);
                cc.setData(ICHAIR);
                FurnisherItemTile ch = new FurnisherItemTile(this, true, sChair_table, AVAILABILITY.AVOID_PASS, false);
                FurnisherItemTile ta = new FurnisherItemTile(this, false, sTable_clean, AVAILABILITY.ROOM_SOLID, false);
                ta.setData(ITABLE);
                FurnisherItemTile tt = new FurnisherItemTile(this, false, sTable_nick, AVAILABILITY.ROOM_SOLID, true);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { tt, cc, tt },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { tt, cc, cc, tt },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { tt, cc, cc, cc, tt },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { tt, cc, cc, cc, cc, tt },
                }, 1);

                flush(1, 3);
            }

            {
                FurnisherItemTile tt = new FurnisherItemTile(this, false, sStage, AVAILABILITY.ROOM_SOLID, false);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { tt, tt },
                    { tt, tt },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { tt, tt, tt },
                    { tt, tt, tt },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { tt, tt, tt },
                    { tt, tt, tt },
                    { tt, tt, tt },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { tt, tt, tt, tt },
                    { tt, tt, tt, tt },
                    { tt, tt, tt, tt },
                    { tt, tt, tt, tt },
                }, 1);

                flush(1, 3);
            }

            {
                FurnisherItemTile sh = new FurnisherItemTile(this, false, sShelf, AVAILABILITY.ROOM_SOLID, false);
                FurnisherItemTile ni = new FurnisherItemTile(this, false, sNickNack, AVAILABILITY.ROOM_SOLID, false);
                FurnisherItemTile ta = new FurnisherItemTile(this, false, sTableSingle, AVAILABILITY.ROOM_SOLID, false);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { sh, ta },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { sh, ta, ni },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { sh, sh, ta, ni },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { ni, sh, sh, ta, ni },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { ni, sh, sh, ta, ni, sh },
                }, 1);

                new FurnisherItem(new FurnisherItemTile[][]
                {
                    { ni, sh, sh, ta, ni, sh, sh },
                }, 1);

                flush(3);
            }

            FurnisherItemTools.makeUnder(this, js, "CARPET_COMBO");
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
            return new ResthomeInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override bool isHeavy()
        {
            return true;
        }

        //private readonly FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][]
        //{
        //    {0,0,0,0,0,0,0,0},
        //    {0,1,1,1,1,1,1,1},
        //    {0,1,1,1,1,1,0,1},
        //    {0,1,1,1,1,1,0,1},
        //    {0,1,1,1,1,1,0,1},
        //    {0,1,1,1,1,1,0,1},
        //    {0,1,1,1,1,1,1,1},
        //    {0,0,0,0,0,0,0,0},
        //},
        //miniColor);

        //public override COLOR miniColor(int tx, int ty)
        //{
        //    return miniC.get(tx, ty);
        //}
    }
}