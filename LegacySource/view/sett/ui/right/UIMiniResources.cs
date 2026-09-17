using System;
using System.Collections.Generic;
using game;
using game.faction;
using init.constant;
using init.resources;
using init.sprite;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using util.colors;
using util.data;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using view.sett.ui.right;

namespace view.sett.ui.right
{
    final class UIMiniResources : Expansion
    {
        private static readonly CharSequence ¤¤desc = "¤Click to open resource details, right click to go to warehouse.";

        static
        {
            D.ts(typeof(UIMiniResources));
        }

        private GuiSection mini;
        private GuiSection full;

        public UIMiniResources(int index, int y1) : base(index)
        {
            full = new Full(y1);
            mini = new Mini(y1);

            Add(full);

            CLICKABLE c = new GButt.Glow(SPRITES.icons().s.arrow_left)
            {
                protected override void clickA()
                {
                    int y1 = Body().y1();
                    Clear();
                    Add(full);
                    Body().MoveY1(y1);
                }
            };
            mini.Add(c, mini.Body().x2() - c.Body().width() - 4, mini.Body().y1() + 4);
            c = new GButt.Glow(SPRITES.icons().s.arrow_right)
            {
                protected override void clickA()
                {
                    int y1 = Body().y1();
                    Clear();
                    Add(mini);
                    Body().MoveY1(y1);
                }
            };
            full.Add(c, full.Body().x2() - c.Body().width() - 4, full.Body().y1() + 4);
        }

        private static class Mini : GuiSection
        {
            private readonly INTE t;

            Mini(int y1)
            {
                RENDEROBJ row = mini(RESOURCES.ALL()[0]);
                int width = row.Body().width();
                Body().MoveY1(y1);
                int cats = 0;
                foreach (RESOURCE r in RESOURCES.ALL())
                {
                    if (r.category > cats)
                    {
                        cats = r.category;
                    }
                }
                LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
                {
                    int cat = RESOURCES.ALL()[0].category;

                    foreach (RESOURCE r in RESOURCES.ALL())
                    {
                        if (r.category != cat)
                        {
                            rows.Add(new RENDEROBJ.RenderImp(width, 16)
                            {
                                public override void render(SPRITE_RENDERER r, float ds)
                                {
                                    GCOLOR.UI().borderH(r, body().x1() + 4, body().x2() - 4, body().y1() + 7, body().y1() + 10);
                                }
                            });
                            cat = r.category;
                        }

                        rows.Add(mini(r));
                    }
                }

                Body().SetDim(width + 6, C.HEIGHT() - y1);

                RENDEROBJ c;
                y1 = y1 + 4;

                c = new GButt.Glow(UI.decor().up)
                {
                    protected override void renAction()
                    {
                        ActiveSet(t.Get() > 0);
                    }
                    protected override void clickA()
                    {
                        t.Inc(-1);
                    }
                };
                c.Body().MoveCX(Body().CX() + 2);
                c.Body().MoveY1(y1);
                Add(c);

                GScrollRows sc = new GScrollRows(rows, C.HEIGHT() - GetLastY2() - c.Body().height() - 8, 0, false);
                AddDownC(0, sc.View());

                t = sc.Target;

                c = new GButt.Glow(UI.decor().down)
                {
                    protected override void renAction()
                    {
                        ActiveSet(t.Get() != t.Max());
                    }
                    protected override void clickA()
                    {
                        t.Inc(1);
                    }
                };
                AddDownC(4, c);
            }
        }

        private static class Full : GuiSection
        {
            private readonly INTE t;

            Full(int y1)
            {
                RENDEROBJ row = big(RESOURCES.ALL()[0]);
                int width = row.Body().width() * 2;
                Body().SetDim(width + 6, C.HEIGHT() - y1);
                Body().MoveY1(y1);
                int cats = 0;
                foreach (RESOURCE r in RESOURCES.ALL())
                {
                    if (r.category > cats)
                    {
                        cats = r.category;
                    }
                }
                LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();
                {
                    GuiSection s = null;
                    int cat = RESOURCES.ALL()[0].category;

                    foreach (RESOURCE r in RESOURCES.ALL())
                    {
                        if (r.category != cat)
                        {
                            rows.Add(new RENDEROBJ.RenderImp(width, 16)
                            {
                                public override void render(SPRITE_RENDERER r, float ds)
                                {
                                    GCOLOR.UI().borderH(r, body().x1() + 4, body().x2() - 4, body().y1() + 7, body().y1() + 10);
                                }
                            });
                            s = new GuiSection();
                            rows.Add(s);
                            cat = r.category;
                        }

                        if (s == null || s.Elements().Count >= 2)
                        {
                            s = new GuiSection();
                            rows.Add(s);
                        }

                        s.AddRightC(0, big(r));
                    }
                }

                RENDEROBJ c;
                y1 = y1 + 4;

                c = new GButt.Glow(UI.decor().up)
                {
                    protected override void renAction()
                    {
                        ActiveSet(t.Get() > 0);
                    }
                    protected override void clickA()
                    {
                        t.Inc(-1);
                    }
                };

                c.Body().CenterX(this);
                c.Body().MoveY1(y1);
                Add(c);

                GScrollRows sc = new GScrollRows(rows, C.HEIGHT() - GetLastY2() - c.Body().height() - 8, 0, false);
                AddDownC(0, sc.View());

                t = sc.Target;

                c = new GButt.Glow(UI.decor().down)
                {
                    protected override void renAction()
                    {
                        ActiveSet(t.Get() != t.Max());
                    }
                    protected override void clickA()
                    {
                        t.Inc(1);
                    }
                };
                AddDownC(4, c);
            }
        }

        private static RENDEROBJ resBody(RESOURCE res)
        {
            return new GuiSection()
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    if (visableIs())
                    {
                        GCOLOR.UI().panBG.render(r, body());
                        GCOLOR.UI().borderH(r, body(), 0);
                        super.render(r, ds);
                    }
                }
            };
        }

        private static RENDEROBJ stat(RESOURCE res)
        {
            return new GStat()
            {
                public override void update(GText text)
                {
                    text.setFont(UI.FONT().S);
                    int a = SETT.ROOMS().STOCKPILE.tally().amountTotal(res);
                    GFORMAT.i(text, a);

                    if (a == 0)
                    {
                        if (SETT.PATH().finders.resource.scattered.has(res))
                            text.normalify();
                        else
                            text.errorify();
                    }
                }
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    OPACITY.O018.bind();
                    COLOR.BLACK.render(r, X1 - 1, X2 + 1, Y1 - 1, Y2 + 1);
                    OPACITY.unbind();
                    base.render(r, X1, X2, Y1, Y2);

                };
            }.r(DIR.NW);
        }

        private static RENDEROBJ mini(RESOURCE res)
        {
            GuiSection s = resBody(res);

            s.Add(res.icon().small, 0, 0);
            RENDEROBJ r = stat(res);

            s.AddRightC(3, r);
            s.body().incrW(40);

            s.pad(2, 4);
            return s;
        }

        private static RENDEROBJ big(RESOURCE res)
        {
            GuiSection s = resBody(res);

            s.Add(res.icon(), 0, 0);
            RENDEROBJ r = stat(res);

            s.AddRightC(1, r);
            s.body().incrW(42);

            s.pad(2, 4);
            return s;
        }
    }
}