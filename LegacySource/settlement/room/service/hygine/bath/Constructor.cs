using System;
using System.IO;
using game.time;
using init.constant;
using init.sprite;
using settlement.main;
using settlement.path;
using settlement.room.main;
using settlement.room.main.furnisher;
using settlement.room.sprite;
using snake2d;
using util.gui.misc;
using util.info;
using util.rendering;

namespace settlement.room.service.hygine.bath
{
    final class Constructor : Furnisher
    {
        private readonly ROOM_BATH blue;
        public readonly FurnisherItemTile tileBasin;

        public readonly FurnisherStat baths = new FurnisherStat(this, 1)
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

        public readonly FurnisherStat relaxation = new FurnisherStat.FurnisherStatRelative(this, baths, 1.5);

        protected Constructor(ROOM_BATH blue, RoomInitData init) : base(init, 2, 2)
        {
            this.blue = blue;

            Json sp = init.data().json("SPRITES");

            RoomSprite spriteNormal = new RoomSpriteCombo(sp, "FRAME_COMBO")
            {
                RoomSprite spriteFloor = new RoomSpriteTex(sp, "POOL_FLOOR_TEXTURE");
                COLOR wColor = new ColorImp(init.data(), "WATER_COLOR");
                double wOp = init.data().d("WATER_OPACITY", 0, 1);
                OpacityImp opacity = new OpacityImp((int)(255 * wOp * 0.5));
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    spriteFloor.render(r, ShadowBatch.DUMMY, 0, it, degrade, isCandle);
                    if (blue.is(it.tile()))
                    {
                        int i = SETT.ROOMS().data.get(it.tile()) & 0b01;
                        if (i > 0)
                        {
                            renderB(r, s, it);
                        }
                    }
                    return base.render(r, s, data, it, degrade, isCandle);
                }

                public void renderB(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
                {
                    int x2 = it.x() + C.TILE_SIZE;
                    int y2 = it.y() + C.TILE_SIZE;

                    wColor.bind();
                    opacity.bind();
                    TextureCoords oo = SPRITES.textures().dis_small.get(it.tx() * C.T_PIXELS + SETT.WEATHER().wind.time.getD() * 16, it.ty() * C.T_PIXELS + SETT.WEATHER().wind.time.getD() * 16);
                    CORE.renderer().renderSprite(it.x(), x2, it.y(), y2, oo);
                    oo = SPRITES.textures().dis_small.get((it.tx() + 1) * C.T_PIXELS - 8 * TIME.currentSecond(), (it.ty() + 1) * C.T_PIXELS - 8 * TIME.currentSecond());
                    CORE.renderer().renderSprite(it.x(), x2, it.y(), y2, oo);
                    COLOR.unbind();
                    OPACITY.unbind();
                }
            };

            final RoomSprite spriteWork = new RoomSprite1x1(sp, "WORK_1X1")
            {
                public override bool render(SPRITE_RENDERER r, ShadowBatch s, int data, RenderIterator it, double degrade, bool isCandle)
                {
                    animate(0);
                    if (blue.is(it.tile()) && (Crank.working(SETT.ROOMS().data.get(it.tile()))))
                    {
                        animate(1);
                    }
                    return base.render(r, s, data, it, degrade, isCandle);
                }
            };

            final RoomSprite spriteOven = new RoomSprite1x1(sp, "OVEN_1X1");

            final RoomSprite spriteService = new RoomSprite1x1(sp, "ENTRANCE_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == spriteNormal;
                }
            };

            final RoomSprite spritePipe = new RoomSprite1x1(sp, "PIPE_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == spriteOven || item.sprite(rx, ry) == this;
                }
            };

            final RoomSprite spriteBenchHead = new RoomSprite1x1(sp, "BENCH_1X1")
            {
                protected override bool joins(int tx, int ty, int rx, int ry, DIR d, FurnisherItem item)
                {
                    return item.sprite(rx, ry) == spriteNormal;
                }
            };

            FurnisherItemTile ww = new FurnisherItemTile(ww, spriteWork, Bits.EMPLOYEE);
            FurnisherItemTile ss = new FurnisherItemTile(ss, spriteService, Bits.EMPLOYEE);
            FurnisherItemTile mm = new FurnisherItemTile(mm, spriteNormal, Bits.EMPLOYEE | Bits.EMPLOYEE2 | Bits.EMPLOYEE3);
            FurnisherItemTile oo = new FurnisherItemTile(oo, spriteOven, Bits.EMPLOYEE);
            FurnisherItemTile pp = new FurnisherItemTile(pp, spritePipe, Bits.EMPLOYEE);

            FurnisherItemTile b1 = new FurnisherItemTile(b1, spriteBenchHead, Bits.EMPLOYEE);
            FurnisherItemTile b2 = new FurnisherItemTile(b2, spriteBenchHead, Bits.EMPLOYEE);

            new FurnisherItem(new FurnisherItemTile[][] {
                { ww, mm },
                { ss, null }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][] {
                { ww, mm, ww },
                { ss, null, ss }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][] {
                { ww, mm, ww, ww },
                { ss, null, ss, ss }
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][] {
                { ww, ww, mm, ww, ww },
                { ss, ss, null, ss, ss }
            }, 4);

            flush(1, 3);

            new FurnisherItem(new FurnisherItemTile[][] {
                { b1, mm },
                { b2, null }
            }, 1);

            new FurnisherItem(new FurnisherItemTile[][] {
                { b1, mm, b1 },
                { b2, null, b2 }
            }, 2);

            new FurnisherItem(new FurnisherItemTile[][] {
                { b1, mm, b1, b1 },
                { b2, null, b2, b2 }
            }, 3);

            new FurnisherItem(new FurnisherItemTile[][] {
                { b1, b1, mm, b1, b1 },
                { b2, b2, null, b2, b2 }
            }, 4);

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
            return new BathInstance(blue, area, init);
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