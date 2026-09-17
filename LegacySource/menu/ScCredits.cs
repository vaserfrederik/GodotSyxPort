using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.gui.table;
using util.text;
using view.menu;

namespace menu
{
    class ScCredits : SC
    {
        private readonly GuiSection current;
        static readonly CharSequence ¤¤name = "¤credits";

        static
        {
            D.ts(typeof(ScCredits));
        }

        private readonly SC fame;

        public ScCredits(Menu menu)
        {
            GuiSection main = new GUI.Shadower();

            MenuScreen sc = new MenuScreen(¤¤name, GUI.labelColor)
            {
                protected override void back()
                {
                    menu.switchScreen(menu.main);
                }
            };

            main.add(sc);

            Json json = new Json(PATHS.BASE().DATA.gets("Credits"));

            int width = MenuScreen.inner.width();

            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            foreach (string s in json.keys())
            {
                Json jj = json.json(s);
                rows.add(new RENDEROBJ.RenderImp(width, 48)
                {
                    public override void render(SPRITE_RENDERER r, float ds)
                    {
                        GUI.COLORS.copper.bind();
                        UI.FONT().H2.renderCX(r, body().cX(), body().y2() - 24, jj.text("TITLE"));
                    }
                });

                string[] nn = jj.texts("CREDS");
                for (int i = 0; i < nn.Length; i++)
                {
                    int from = i;
                    int w = 0;
                    for (; i < nn.Length; i++)
                    {
                        if (w > 0 && w + 50 + UI.FONT().M.getDim(nn[i]).x() > width)
                            break;
                        w += 50 + UI.FONT().M.getDim(nn[i]).x();
                    }
                    int wi = w;
                    int to = i;
                    rows.add(new RENDEROBJ.RenderImp(width, 32)
                    {
                        public override void render(SPRITE_RENDERER r, float ds)
                        {
                            GUI.COLORS.label.bind();
                            Font f = UI.FONT().M;
                            int x = body().cX() - wi / 2 + 25;
                            for (int j = from; j < to; j++)
                            {
                                f.render(r, nn[j], x, body().y1());
                                x += f.getDim(nn[j]).x() + 50;
                            }
                        }
                    });
                }
            }

            RENDEROBJ ta = new GScrollRows(rows, 32 * 13).view();

            ta.body().centerIn(main.body());
            main.add(ta);

            current = main;
            fame = new ScCreditsFame(menu);
            CLICKABLE cl = getNavButt(ScCreditsFame.¤¤name);
            cl.clickActionSet(new ACTION()
            {
                public void exe()
                {
                    menu.switchScreen(fame);
                }
            });
            sc.addButt(cl);
        }

        public bool hover(COORDINATE mCoo)
        {
            if (current.hover(mCoo))
                return true;
            return false;
        }

        public bool click()
        {
            current.click();
            return false;
        }

        public void render(SPRITE_RENDERER r, float ds)
        {
            current.render(r, ds);
        }

        public bool back(Menu menu)
        {
            menu.switchScreen(menu.main);
            return true;
        }
    }
}