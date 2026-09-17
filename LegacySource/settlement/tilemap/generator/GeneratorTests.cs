using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.rnd;
using util.data.DOUBLE;
using util.gui.slider;
using view.interrupter;
using view.main;
using view.sett;

namespace settlement.tilemap.generator
{
    public static class GeneratorTests
    {
        private static Debug debug;

        public GeneratorTests()
        {
            IDebugPanelSett.add("terrain test", new ACTION()
            {
                public void exe()
                {
                    if (debug == null)
                        debug = new Debug();
                    VIEW.s().panels.add(debug, true);
                }
            });

            IDebugPanelSett.add("Growth Test", new ACTION()
            {
                public void exe()
                {
                    for (int y = 0; y < THEIGHT; y++)
                    {
                        for (int x = 0; x < TWIDTH; x++)
                        {
                            if (SETT.TILE_MAP().growth.current(x, y) != SETT.TILE_MAP().growth.nothing)
                            {
                                TERRAIN().NADA.placeFixed(x, y);
                                SETT.TILE_MAP().growth.nothing.set(x, y, 0);
                            }
                        }
                    }
                    new GeneratorGrowth();
                    for (int y = 0; y < THEIGHT; y++)
                    {
                        for (int x = 0; x < TWIDTH; x++)
                        {
                            SETT.TERRAIN().get(x, y).placeFixed(x, y);
                            SETT.PATH().availability.updateAvailability(x, y);
                        }
                    }
                }
            });

            IDebugPanelSett.add("Generate Road", new ACTION()
            {
                public void exe()
                {
                    for (int y = 0; y < THEIGHT; y++)
                    {
                        for (int x = 0; x < TWIDTH; x++)
                        {
                            SETT.FLOOR().clearer.clear(x, y);
                        }
                    }
                    new GeneratorRoads(SETT.WORLD_AREA());
                }
            });

            // IDebugPanelSett.add("Generate River", new ACTION()
            // {
            //     public void exe()
            //     {
            //         for (int y = 0; y < THEIGHT; y++)
            //         {
            //             for (int x = 0; x < TWIDTH; x++)
            //             {
            //                 SETT.TERRAIN().NADA.placeFixed(x, y);
            //             }
            //         }
            //         new GeneratorRiver(SETT.WORLD_AREA());
            //     }
            // });
        }

        private class Debug : ISidePanel
        {
            private readonly int size = 128;
            private readonly int scale = 4;
            private readonly HeightMap ferMap = new HeightMap(size, size, 32, 2);
            private readonly HeightMap height = new HeightMap(size, size, 128, 4);
            private double base2 = 1;
            private double base = 0;
            private byte[][] res = new byte[size][];
            private readonly OpacityImp o = new OpacityImp(0);

            public Debug()
            {
                for (int i = 0; i < size; i++)
                {
                    res[i] = new byte[size];
                }

                section.add(new RENDEROBJ.RenderImp(size * scale)
                {
                    public void render(SPRITE_RENDERER r, float ds)
                    {
                        for (int y = 0; y < size; y++)
                        {
                            for (int x = 0; x < size; x++)
                            {
                                int px = body.x1() + x * scale;
                                int py = body.y1() + y * scale;
                                o.set(res[y][x] & 0x0FF);
                                o.bind();
                                COLOR.WHITE100.render(r, px, px + scale, py, py + scale);
                                // CORE.renderer().renderParticle(px, py);
                            }
                        }
                        OPACITY.unbind();
                    }
                });

                GGaugeMutable g;

                g = new GGaugeMutable(new DOUBLE_MUTABLE()
                {
                    public double getD()
                    {
                        return base;
                    }

                    public DOUBLE_MUTABLE setD(double d)
                    {
                        base = d;
                        fixbase();
                        return this;
                    }
                }, 200);
                section.addDown(8, g);

                g = new GGaugeMutable(new DOUBLE_MUTABLE()
                {
                    public double getD()
                    {
                        return base2;
                    }

                    public DOUBLE_MUTABLE setD(double d)
                    {
                        base2 = d;
                        fixbase2(base2);
                        return this;
                    }
                }, 200);
                section.addDown(8, g);

                titleSet("");
            }

            private void fixbase()
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        double f = ferMap.get(x, y);
                        double h = height.get(x, y);
                        base = CLAMP.d(base, 0, 1);
                        f = GeneratorFertilityInit.get(base, f, h);
                        res[y][x] = (byte)(0x0FF * CLAMP.d(f, 0, 1));
                    }
                }
            }

            private void fixbase2(double base)
            {
                base *= 0.8;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        double f = height.get(x, y);
                        f = Math.Pow(f, 1 + 8 * (1 - base));
                        // f = Math.Pow(f, 1 + (1-base));

                        f = CLAMP.d(base + f, 0, 1);
                        f -= 0.2 * ferMap.get(x, y);
                        res[y][x] = (byte)(0x0FF * CLAMP.d(f, 0, 1));
                    }
                }
            }
        }
    }
}