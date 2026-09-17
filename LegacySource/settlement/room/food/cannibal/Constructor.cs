using System;
using System.IO;
using settlement.room.food.cannibal;
using init.constant;
using init.race;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.main.util;
using settlement.room.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.food.cannibal
{
    internal class Constructor : Furnisher
    {
        private readonly ROOM_CANNIBAL blue;

        internal FurnisherStat workers = new FurnisherStat(this)
        {
            public double get(AREA area, double fromItems)
            {
                return fromItems;
            }

            public GText format(GText t, double value)
            {
                return GFORMAT.i(t, (int)value);
            }
        };

        internal FurnisherItemTile ww;
        internal FurnisherItemTile rm;
        internal FurnisherItemTile rr;
        internal FurnisherItemTile cc;

        protected Constructor(ROOM_CANNIBAL blue, RoomInitData init) : base(init, 1, 1)
        {
            this.blue = blue;

            Json sp = init.data().json("SPRITES");

            RoomSprite table = new RoomSpriteCombo(sp, "TABLE_COMBO")
            {
                public bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    base.render(r, s, data, it, degrade, isCandle);
                    if (blue.is(it.tile()))
                    {
                        RACES.all().get(Job.race.get(SETT.ROOMS().data.get(it.tile()))).appearance().colors.blood.bind();
                        long ran = it.bigRan();
                        int a = Job.gore.get(SETT.ROOMS().data.get(it.tile()));
                        int cx = it.x() + C.TILE_SIZEH;
                        int cy = it.y() + C.TILE_SIZEH;
                        for (int i = 0; i < a; i++)
                        {
                            int xx = (int)(cx + (-4 + (ran & 0x07)) * C.SCALE);
                            ran = ran >> 3;
                            int yy = (int)(cy + (-4 + (ran & 0x07)) * C.SCALE);
                            ran = ran >> 3;
                            SETT.THINGS().sprites.bloodPool.render(r, (int)(ran & 0x0F), xx, yy);
                            ran = ran >> 4;
                        }
                        COLOR.unbind();
                    }
                    return false;
                }
            };

            RoomSprite1x1 top = new RoomSprite1x1(sp, "ON_TABLE_1X1")
            {
                protected bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.get(rx, ry) == null;
                }
            };

            RoomSprite table2 = new RoomSpriteCombo(table)
            {
                public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    top.renderRandom(r, s, it, data, degrade);
                }
            };

            RoomSprite cage = new RoomSprite1x1(sp, "CAGE_1X1")
            {
                RoomSprite top = new RoomSprite1x1(sp, "CAGE_TOP_1X1");

                public byte getData2(int tx, int ty, int rx, int ry, FurnisherItem item, int itemRan)
                {
                    return top.getData(tx, ty, rx, ry, item, itemRan);
                }

                public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    top.render(r, s, data, it, degrade, false);
                }
            };

            RoomSprite sCandle = new RoomSprite1x1(sp, "CANDLE_BASE_1X1")
            {
                public void renderAbove(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade)
                {
                    if (!SETT.ROOMS().fData.candle.is(it.coo()))
                        top.renderRandom(r, s, it, data, degrade);
                }
            };

            RoomSprite misc = new RoomSprite1x1(sp, "MISC_1X1");

            ww = new FurnisherItemTile(this, true, table, AVAILABILITY.ROOM_SOLID, false);
            rm = new FurnisherItemTile(this, true, table, AVAILABILITY.ROOM_SOLID, false);
            rr = new FurnisherItemTile(this, true, table, AVAILABILITY.ROOM_SOLID, false);
            cc = new FurnisherItemTile(this, true, cage, AVAILABILITY.AVOID_LIKE_FUCK, false);
            FurnisherItemTile ca = new FurnisherItemTile(this, false, sCandle, AVAILABILITY.ROOM_SOLID, true);

            FurnisherItemTile mm = new FurnisherItemTile(this, false, table2, AVAILABILITY.ROOM_SOLID, false);
            FurnisherItemTile nn = new FurnisherItemTile(this, false, misc, AVAILABILITY.ROOM_SOLID, false);

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm, mm, mm, nn },
                { cc, rm, ww, rr, ca }
            }, 1);

            if (false)
            {
                // missed one cc here
            }

            new FurnisherItem(new FurnisherItemTile[][]
            {
                { nn, mm, mm, mm, mm, mm, mm, nn },
                { cc, rm, ww, rr, rm, ww, rr, ca }
            }, 2);

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
            return new CannibalInstance(blue, area, init);
        }

        public override RoomBlueprintImp blue()
        {
            return blue;
        }

        public override bool isHeavy()
        {
            return true;
        }

        // private readonly FurnisherMinimapColor miniC = new FurnisherMinimapColor(new byte[][]
        // {
        // {0,0,0,0,0,0,0,0},
        // {0,1,1,1,0,0,0,0},
        // {0,0,1,0,1,0,0,0},
        // {0,0,1,0,1,0,0,0},
        // {0,0,1,0,1,0,0,0},
        // {0,0,1,0,1,0,0,0},
        // {0,1,1,1,0,0,0,0},
        // {0,0,0,0,0,0,0,0},
        // },
        // miniColor
        // );

        // public override COLOR miniColor(int tx, int ty)
        // {
        // return miniC.get(tx, ty);
        // }
    }
}