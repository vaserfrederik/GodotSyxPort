using System;
using System.Collections.Generic;
using snake2d;
using util.gui.misc;
using util.gui.slider;
using util.gui.text;
using util.data.INT;
using util.colors;
using util.info;
using util.text;
using world;
using util.gui.clickable;

namespace view.world.generator.tools
{
    public class UIWorldGenerateTerrain : GuiSection
    {
        public static string ¤¤MapType = "choose map type";
        private static string ¤¤Random = "Random";

        private static string ¤¤latitude = "¤latitude";
        private static string ¤¤nort = "¤northern";
        private static string ¤¤south = "¤southern";

        private static string ¤¤seed = "¤Random Seed";

        static
        {
            D.ts(typeof(UIWorldGenerateTerrain));
        }

        private int ttt;

        public UIWorldGenerateTerrain(WorldGen spec)
        {
            {
                GuiSection s = new GuiSection();
                RMapType tt = new RMapType();
                s.add(tt);

                WorldGenMapType[] types = WorldGenMapType.getAll(WORLD.TWIDTH());

                ttt = types.Length;
                spec.map = null;
                s.addRelBody(8, DIR.W, new GButt.ButtPanel(UI.icons().m.arrow_left)
                {
                    protected override void clickA()
                    {
                        ttt--;
                        if (ttt < 0)
                            ttt = types.Length;
                        tt.type = ttt < types.Length ? types[ttt] : null;
                        spec.map = ttt < types.Length ? types[ttt].name : null;
                        base.clickA();
                    }
                });

                s.addRelBody(8, DIR.E, new GButt.ButtPanel(UI.icons().m.arrow_right)
                {
                    protected override void clickA()
                    {
                        ttt++;
                        if (ttt > types.Length)
                            ttt = 0;
                        tt.type = ttt < types.Length ? types[ttt] : null;
                        spec.map = ttt < types.Length ? types[ttt].name : null;
                        base.clickA();
                    }
                });

                addRelBody(16, DIR.S, s);
            }

            {
                addRelBody(16, DIR.S, new GHeader(¤¤latitude));

                INTE lat = new INTE()
                {
                    public int min()
                    {
                        return 0;
                    }

                    public int max()
                    {
                        return 100;
                    }

                    public int get()
                    {
                        return (int)Math.Round(spec.lat * 100);
                    }

                    public void set(int t)
                    {
                        spec.lat = t / 100.0;
                    }
                };

                CLICKABLE gg = new GSliderInt(lat, 180, true)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.add(GFORMAT.percGood(b.text(), lat.getD()));
                    }
                };

                addRelBody(8, DIR.S, gg);

                addRightC(16, new GText(UI.FONT().S, ¤¤nort));

                GText t = new GText(UI.FONT().S, ¤¤south);

                int y1 = getLastY1();

                add(t, gg.body().x1() - 16 - t.width(), y1);
            }

            {
                addRelBody(16, DIR.S, new GHeader(¤¤seed));

                GInput seed = new GInput(new StringInputSprite(10, UI.FONT().M)
                {
                    protected override void acceptChar(char c)
                    {
                        if (c >= '0' && c <= '9')
                        {
                            base.acceptChar(c);
                            int se = RND.seed();
                            string s = text().ToString();
                            if (s.Length > 10)
                                s = s.Substring(0, 10);
                            try
                            {
                                se = int.Parse(s);
                                spec.seed = se;
                                RND.setSeed(se);
                                //GAME.world().regenerate();
                            }
                            catch (Exception e)
                            {
                                text().clear().add('1');
                            }
                        }
                    }
                })
                {
                    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
                    {
                        GCOLOR.UI().bg().render(r, body);
                        base.render(r, ds, isActive, isSelected, isHovered);
                    }
                };
                seed.text().clear().add(spec.seed);
                addRelBody(16, DIR.S, seed);
            }
        }

        private class RMapType : RENDEROBJ.RenderImp
        {
            private readonly GText text;
            private WorldGenMapType type;

            public RMapType()
            {
                body.setDim(200 + 10, 200 + 10);
                text = new GText(UI.FONT().M, ¤¤Random);
                text.setMultipleLines(true);
                text.setMaxWidth(200);
                text.adjustWidth();
            }

            public override void render(SPRITE_RENDERER r, float ds)
            {
                GCOLOR.UI().border().render(r, body);
                GCOLOR.UI().bg(true, false, false).render(r, body, -1);
                if (type == null)
                {
                    text.renderC(r, body);
                }
                else
                {
                    type.render(r, body().x1() + 5, body().y1() + 5, 2);
                }
            }
        }
    }
}