using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using init.paths;
using init.sprite.UI;
using menu.GUI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.menu;

namespace menu
{
    class ScRandomSettings : Shadower, SC
    {
        public readonly KeyMap<double> ADD = new KeyMap<double>();
        public readonly KeyMap<double> MUL = new KeyMap<double>();

        private CharSequence desc;
        private static readonly CharSequence ¤¤name = "¤Game Config";
        private static readonly CharSequence ¤¤ok = "ok";
        private static readonly CharSequence ¤¤clear = "clear!";

        static ScRandomSettings()
        {
            D.ts(typeof(ScRandomSettings));
        }

        public ScRandomSettings(Menu menu)
        {
            MenuScreen screen = new MenuScreen(¤¤name, GUI.labelColor)
            {
                protected override void back()
                {
                    menu.switchScreen(menu.sandbox2);
                }
            };

            add(screen);

            screen.addButt(new MenuScreen.ScreenButton(¤¤ok)
            {
                protected override void clickA()
                {
                    menu.switchScreen(menu.sandbox2);
                }
            });

            screen.addButt(new MenuScreen.ScreenButton(¤¤clear)
            {
                protected override void clickA()
                {
                    foreach (string s in ADD.keys())
                    {
                        ADD.putReplace(s, 0.0);
                        MUL.putReplace(s, 1.0);
                    }
                }
            });

            GuiSection options = new GuiSection();

            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            foreach (BoostableCat cat in BOOSTABLES.colls())
            {
                if (cat == BOOSTABLES.PHYSICS())
                    continue;

                if (cat.all().size() == 0)
                    continue;

                rows.add(new HOVERABLE.Sprite(UI.FONT().H2.getText(cat.name), COLORS.label));

                foreach (Boostable b in cat.all())
                {
                    ADD.put(b.key, 0.0);
                    MUL.put(b.key, 1.0);
                    GuiSection s = new GuiSection()
                    {
                        public override bool hover(COORDINATE mCoo)
                        {
                            if (base.hover(mCoo))
                            {
                                desc = b.desc;
                                return true;
                            }
                            return false;
                        }

                        public override void render(SPRITE_RENDERER r, float ds)
                        {
                            if (hoveredIs())
                            {
                                COLOR.WHITE100.render(r, body().x1(), body().x2(), body().y2() - 2, body().y2() - 1);
                            }
                            base.render(r, ds);
                        }
                    };

                    GText str = new GText(UI.FONT().M, 16);
                    s.add(GUI.getSmallText(b.name), 0, 0);

                    s.addRightCAbs(350, new GUI.Button(UI.FONT().M.getText("<<"))
                    {
                        protected override void clickA()
                        {
                            ADD.putReplace(b.key, ADD.get(b.key) - 0.25);
                            base.clickA();
                        }
                    });

                    s.addRightC(8, new SPRITE.Imp(100, 16)
                    {
                        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                        {
                            double d = ADD.get(b.key);
                            str.clear();
                            GFORMAT.f0(str, d);
                            str.adjustWidth();
                            str.renderC(r, X1, X2, Y1, Y2);
                        }
                    });
                    s.addRightCAbs(100, new GUI.Button(GUI.getSmallText(">>"))
                    {
                        protected override void clickA()
                        {
                            ADD.putReplace(b.key, ADD.get(b.key) + 0.25);
                            base.clickA();
                        }
                    });

                    s.addRightC(32, new GUI.Button(GUI.getSmallText("<<"))
                    {
                        protected override void clickA()
                        {
                            MUL.putReplace(b.key, MUL.get(b.key) - 0.1);
                            base.clickA();
                        }
                    });

                    s.addRightC(8, new SPRITE.Imp(100, 16)
                    {
                        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                        {
                            double d = MUL.get(b.key);
                            if (d < 0)
                                d = 0;
                            str.clear();
                            GFORMAT.f0(str, d, 2);
                            str.clear();
                            str.add('x');
                            str.add(d);
                            str.adjustWidth();
                            str.renderC(r, X1, X2, Y1, Y2);
                        }
                    });
                    s.addRightCAbs(100, new GUI.Button(GUI.getSmallText(">>"))
                    {
                        protected override void clickA()
                        {
                            MUL.putReplace(b.key, MUL.get(b.key) + 0.1);
                            base.clickA();
                        }
                    });
                    s.pad(8, 2);
                    rows.add(s);
                }
            }

            if (PATHS.local().PROFILE.exists("MODE_SETTINGS"))
            {
                Json j = new Json(PATHS.local().PROFILE.gets("MODE_SETTINGS"));
                Json a = j.json("ADD");
                foreach (string k in a.keys())
                    if (ADD.containsKey(k))
                        ADD.putReplace(k, CLAMP.d(a.d(k), -10000, 10000));
                a = j.json("MUL");
                foreach (string k in a.keys())
                    if (MUL.containsKey(k))
                        MUL.putReplace(k, CLAMP.d(a.d(k), 0, 10000));
            }

            options.add(new GScrollRows(rows, rows.get(0).body().height() * 16).view());

            options.addRelBody(8, DIR.S, new RENDEROBJ.RenderImp(MenuScreen.inner.width() - 4, 64)
            {
                GText str = new GText(UI.FONT().M, 128);
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    COLOR.WHITE150.bind();
                    if (desc != null)
                    {
                        str.clear().add(desc);
                        str.setMultipleLines(true);
                        str.setMaxWidth(800);
                        str.renderC(r, body);
                    }
                    COLOR.unbind();
                }
            });

            options.body().centerIn(body());

            add(options);
        }

        public bool back(Menu menu)
        {
            menu.switchScreen(menu.sandbox2);
            return true;
        }

        public void save()
        {
            JsonE a = new JsonE();
            JsonE m = new JsonE();

            foreach (string k in MUL.keysSorted())
            {
                double d = MUL.get(k);
                if (d != 1)
                    m.add(k, d);
                d = ADD.get(k);
                if (d != 0)
                    a.add(k, d);
            }

            JsonE j = new JsonE();
            j.add("ADD", a);
            j.add("MUL", m);

            PATHS.local().PROFILE.create("MODE_SETTINGS");

            j.save(PATHS.local().PROFILE.get("MODE_SETTINGS"));
        }

        public void apply()
        {
            foreach (string k in MUL.keys())
            {
                double m = MUL.get(k);
                double a = ADD.get(k);
                if (m != 1)
                {
                    FACTIONS.player().bonusesCustom.add(k, m, true);
                }
                if (a != 0)
                {
                    FACTIONS.player().bonusesCustom.add(k, a, false);
                }
            }
        }
    }
}