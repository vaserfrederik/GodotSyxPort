using System;
using System.IO;
using snake2d;
using util.rendering;
using static settlement.main.SETT;

namespace settlement.room.spirit.grave
{
    final class CGraveyard : Furnisher
    {
        private readonly ROOM_GRAVEYARD blue;
        public readonly FurnisherStat workers;
        public readonly FurnisherStat services;
        public readonly FurnisherStat respekk;

        private readonly Floor pathway;
        private const int PI = 3;

        protected CGraveyard(ROOM_GRAVEYARD blue, RoomInitData init) : base(init, 4, 3, 88, 44)
        {
            this.blue = blue;

            workers = new FurnisherStat.FurnisherStatEmployees(this, 0);
            services = new FurnisherStat.FurnisherStatI(this);
            respekk = new FurnisherStat.FurnisherStatRelative(this, services);

            Json sp = init.data().json("SPRITES");

            RoomSprite sHead = new RoomSprite1xN(sp, "GRAVE_A_TOP_1X1", true)
            {
                final RoomSprite made = new RoomSprite1xN(sp, "GRAVE_B_TOP_1X1", true);

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    if (blue.is(it.tile()))
                    {
                        if (Grave.isUsed(it.tx() + rot(data).x(), it.ty() + rot(data).y()))
                        {
                            return made.render(r, s, data, it, degrade, isCandle);
                        }
                    }
                    return base.render(r, s, data, it, degrade, isCandle);
                }
            };

            RoomSprite sFoot = new RoomSprite1xN(sp, "GRAVE_A_BOTTOM_1X1", false)
            {
                final RoomSprite made = new RoomSprite1xN(sp, "GRAVE_B_BOTTOM_1X1", false);

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    if (blue.is(it.tile()))
                    {
                        if (Grave.isUsed(it.tx(), it.ty()))
                        {
                            return made.render(r, s, data, it, degrade, isCandle);
                        }
                    }
                    return base.render(r, s, data, it, degrade, isCandle);
                }
            };

            RoomSprite sStone = new RoomSprite1x1(sp, "TOMBSTONE_1X1")
            {
                final RoomSprite made = new RoomSprite1x1(sp, "TOMBSTONE_RUNE_1X1");

                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    OPACITY.O85.bind();
                    made.render(r, s, data, it, degrade, isCandle);
                    OPACITY.unbind();
                    return false;
                }

                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) is RoomSprite1xN;
                }
            };

            final FurnisherItemTile h1 = new FurnisherItemTile(
                this,
                false,
                sHead,
                AVAILABILITY.AVOID_PASS,
                false).setData(Grave.DIG_MARK);

            final FurnisherItemTile t1 = new FurnisherItemTile(
                this,
                true,
                sFoot,
                AVAILABILITY.AVOID_PASS,
                false).setData(Grave.ITEM_MARK);

            final FurnisherItemTile st = new FurnisherItemTile(
                this,
                false,
                sStone,
                AVAILABILITY.SOLID, true);

            new FurnisherItem(new FurnisherItemTile[][] {
                { st },
                { h1 },
                { t1 },
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][] {
                { st, st },
                { h1, h1 },
                { t1, t1 },
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][] {
                { st, st, st },
                { h1, h1, h1 },
                { t1, t1, t1 },
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][] {
                { st, st, st, st },
                { h1, h1, h1, h1 },
                { t1, t1, t1, t1 },
            }, 4);

            new FurnisherItem(new FurnisherItemTile[][] {
                { st, st, st, st, st },
                { h1, h1, h1, h1, h1 },
                { t1, t1, t1, t1, t1 },
            }, 5);

            new FurnisherItem(new FurnisherItemTile[][] {
                { st, st, st, st, st, st },
                { h1, h1, h1, h1, h1, h1 },
                { t1, t1, t1, t1, t1, t1 },
            }, 6);

            new FurnisherItem(new FurnisherItemTile[][] {
                { st, st, st, st, st, st, st },
                { h1, h1, h1, h1, h1, h1, h1 },
                { t1, t1, t1, t1, t1, t1, t1 },
            }, 7);

            flush(1, 3);

            {
                RoomSprite ss = new RoomSprite1x1(sp, "MON_1X1");
                final FurnisherItemTile it = new FurnisherItemTile(
                    this,
                    false,
                    ss,
                    AVAILABILITY.SOLID, false);

                new FurnisherItem(new FurnisherItemTile[][] {
                    { it },
                }, 1);
            }
            {
                RoomSprite ss = new RoomSpriteXxX(sp, "MON_2X2", 2);
                final FurnisherItemTile it = new FurnisherItemTile(
                    this,
                    false,
                    ss,
                    AVAILABILITY.SOLID, false);

                new FurnisherItem(new FurnisherItemTile[][] {
                    { it, it },
                    { it, it },
                }, 4);
            }
            {
                RoomSprite ss = new RoomSpriteXxX(sp, "MON_3X3", 3);
                final FurnisherItemTile it = new FurnisherItemTile(
                    this,
                    false,
                    ss,
                    AVAILABILITY.SOLID, false);

                new FurnisherItem(new FurnisherItemTile[][] {
                    { it, it, it },
                    { it, it, it },
                    { it, it, it },
                }, 9);
            }
            flush(1, 3);

            {
                RoomSprite ss = new RoomSpriteCombo(sp, "FLOWER_COMBO");
                final FurnisherItemTile it = new FurnisherItemTile(
                    this,
                    false,
                    ss,
                    AVAILABILITY.AVOID_LIKE_FUCK, false);
                FurnisherItemTools.makeArea(this, it);
            }

            pathway = SETT.FLOOR().map.read("PATHWAY", init.data());

            {
                RoomSprite ss = new RoomSpriteCombo();
                final FurnisherItemTile it = new FurnisherItemTile(
                    this,
                    false,
                    ss,
                    AVAILABILITY.ROOM, false);
                it.setData(PI);
                FurnisherItemTools.makeArea(this, it);
            }
        }

        public override bool removeFertility()
        {
            return false;
        }

        public override bool usesArea()
        {
            return true;
        }

        public override bool mustBeIndoors()
        {
            return false;
        }

        public override bool mustBeOutdoors()
        {
            return true;
        }

        public override Room create(TmpArea area, RoomInit init)
        {
            return new GraveInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override void putFloor(int tx, int ty, int upgrade, AREA area)
        {
            if (SETT.ROOMS().fData.tileData.get(tx, ty) == PI)
                pathway.placeFixed(tx, ty);
            else
            {
                double f = CLAMP.d((SETT.GROUND().MAP.get(tx, ty).vegitation - 0.2) * 4, 0, 0.5);
                GRASS().current.set(tx, ty, f);
            }
        }
    }
}