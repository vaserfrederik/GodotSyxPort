using System;
using System.IO;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using settlement.tilemap.floor.Floors;
using snake2d;
using snake2d.util.datatypes;
using util.rendering;

namespace settlement.room.infra.inn
{
    final class Constructor : Furnisher
    {
        private readonly ROOM_INN blue;

        public readonly FurnisherStat beds = new FurnisherStat.FurnisherStatI(this, 1);
        public readonly FurnisherStat coziness = new FurnisherStat.FurnisherStatRelative(this, beds);
        public readonly FurnisherStat workers = new FurnisherStat.FurnisherStatI(this);
        private readonly FurnisherItemTile cc;
        private readonly Floor floor2;

        public const int IHEAD = 1;
        public const int ITAIL = 2;
        private const int IWALL = 3;
        private readonly RoomSpriteCombo walls;

        FurnisherItemGroup mgroup;

        protected Constructor(ROOM_INN blue, RoomInitData init) : base(init, 3, 3, 88, 44)
        {
            this.blue = blue;
            floor2 = SETT.FLOOR().map.get(init.data().value("FLOOR2"), init.data());

            Json sp = init.data().json("SPRITES");

            RoomSprite sHead = new RoomSprite1xN(sp, "BED_UNMADE_HEAD_1X1", false)
            {
                RoomSprite made = new RoomSprite1xN(sp, "BED_MADE_HEAD_1X1", false);

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    int x = it.tx() + offX(data);
                    int y = it.ty() + offY(data);
                    if (blue.is(it.tile()) && ABed.isUnmade(x, y))
                        return base.render(r, s, data, it, degrade, isCandle);
                    return made.render(r, s, data, it, degrade, isCandle);
                }
            };

            RoomSprite sTail = new RoomSprite1xN(sp, "BED_UNMADE_TAIL_1X1", true)
            {
                RoomSprite made = new RoomSprite1xN(sp, "BED_MADE_TAIL_1X1", true);
                RoomSprite top = new RoomSprite1x1(sp, "BED_CLAIMED_1X1");

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    int x = it.tx() + offX(data);
                    int y = it.ty() + offY(data);
                    if (blue.is(it.tile()) && ABed.isUnmade(x, y))
                    {
                        base.render(r, s, data, it, degrade, isCandle);
                        if (blue.is(it.tile()) && ABed.isClaimed(x, y))
                            top.render(r, s, getData2(it), it, degrade, false);
                        return false;
                    }
                    return made.render(r, s, data, it, degrade, isCandle);
                }

                public override byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }
            };

            RoomSprite stop = new RoomSprite1x1(sp, "TABLE_TOP_1X1");

            RoomSprite sTable = new RoomSpriteCombo(sp, "TABLE_COMBO")
            {
                public override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade)
                {
                    walls.render(r, s, m, it, degrade, false);
                }
            };

            RoomSprite sh = new RoomSprite1x1(sp, "SH");
            RoomSprite ss = new RoomSprite1x1(sp, "SS");

            FurnisherItem item1 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ta, ch, h1 },
                new FurnisherItemTile[] { ta, cc, t1 },
                new FurnisherItemTile[] { sh, ss, sh },
            }, 1);

            FurnisherItem item2 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ta, ch, h1, ta, ch, h1 },
                new FurnisherItemTile[] { ta, cc, t1, ta, cc, t1 },
                new FurnisherItemTile[] { sh, ss, sh, sh, ss, sh },
            }, 2);

            FurnisherItem item3 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ta, ch, h1, ta, ch, h1, ta, ch, h1 },
                new FurnisherItemTile[] { ta, cc, t1, ta, cc, t1, ta, cc, t1 },
                new FurnisherItemTile[] { sh, ss, sh, sh, ss, sh, sh, ss, sh },
            }, 3);

            FurnisherItem item4 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ta, ch, h1, ta, ch, h1, ta, ch, h1, ta, ch, h1 },
                new FurnisherItemTile[] { ta, cc, t1, ta, cc, t1, ta, cc, t1, ta, cc, t1 },
                new FurnisherItemTile[] { sh, ss, sh, sh, ss, sh, sh, ss, sh, sh, ss, sh },
            }, 4);

            mgroup = flush(1, 3);

            FurnisherItem item5 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { nt },
            }, 1);

            FurnisherItem item6 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { nt, ni },
            }, 2);

            FurnisherItem item7 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ni, nt, ni },
            }, 3);

            FurnisherItem item8 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ni, nt, nt, ni, sh },
            }, 4);

            FurnisherItem item9 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ni, nt, nt, ni, sh, sh },
            }, 5);

            FurnisherItem item10 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ni, nt, nt, ni, sh, sh, sh },
            }, 6);

            FurnisherItem item11 = new FurnisherItem(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { ni, nt, nt, ni, sh, sh, sh, sh },
            }, 7);

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

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public void aboveR(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it, double degrade)
        {
            if (SETT.ROOMS().fData.tileData.get(it.tile()) != 0)
            {
                int m = 0;
                foreach (DIR d in DIR.ALL)
                {
                    if (SETT.ROOMS().fData.tile.get(it.tx(), it.ty(), d) == cc)
                    {
                        if (!d.isOrtho())
                        {
                            m |= d.next(-1).mask();
                            m |= d.next(1).mask();
                        }
                        else
                        {
                            m |= d.mask();
                            m |= d.next(-2).mask();
                            m |= d.next(2).mask();
                        }
                    }
                }
                if (m != 0 && m != 0x0F)
                {
                    walls.render(r, s, m, it, degrade, false);
                }
            }
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
            FurnisherItem t = SETT.ROOMS().fData.item.get(tx, ty);
            if (t != null && t.group() == mgroup)
                base.putFloor(tx, ty, upgrade, area);
            else
                floor2.placeFixed(tx, ty);
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new InnInstance(blue, area, init);
        }

        public override bool isHeavy()
        {
            return true;
        }
    }
}