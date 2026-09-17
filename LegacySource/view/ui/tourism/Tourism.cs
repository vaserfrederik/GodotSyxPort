using System;
using System.Collections.Generic;
using snake2d;
using util.data.INT;
using util.gui.misc;
using util.gui.slider;
using util.gui.table;
using util.info;
using util.statistics;
using util.text;
using view.main;
using view.ui.wiki;

namespace view.ui.tourism
{
    final class Tourism : GuiSection
    {
        int hovered = -1;
        private static CharSequence ¤¤goTo = "¤Go to next tourist";
        private static CharSequence ¤¤Permit = "¤Click to toggle permission for race to visit and sightsee in your city.";
        private static CharSequence ¤¤Generosity = "¤Generosity";
        private static CharSequence ¤¤Attractions = "¤Attracted by:";

        private static CharSequence ¤¤bad = "¤Poor";
        private static CharSequence ¤¤ok = "¤Mixed";
        private static CharSequence ¤¤good = "¤Overwhelmingly Positive";

        private static CharSequence ¤¤attracted = "¤Attracted (year)";

        static
        {
            D.ts(typeof(Tourism));
        }

        Tourism(int height)
        {
            stats();
            perm();
            addRelBody(16, DIR.N, new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, TOURISM.perYear());
                }
            }.hv(¤¤attracted));
            rev(height);

            pad(6, 0);
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            if (htourist() != null)
                SETT.OVERLAY().add(htourist());
            base.render(r, ds);
        }

        private Humanoid htourist()
        {
            if (hovered == -1)
                return null;
            ENTITY e = SETT.ENTITIES().getAllEnts()[MATH.mod(hovered, SETT.ENTITIES().getAllEnts().length)];
            if (e != null && e is Humanoid && !e.isRemoved())
            {
                Humanoid a = (Humanoid)e;
                if (a.indu().hType() == HTYPES.TOURIST())
                    return a;
            }
            hovered = -1;
            return null;
        }

        private void stats()
        {
            int x1 = body().x1();

            add(new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, STATS.POP().pop(HTYPES.TOURIST()));
                }
            }.increase().hh(HTYPES.TOURIST().names));

            addRightC(100, new GButt.ButtPanel(SPRITES.icons().m.crossair)
            {
                protected override void clickA()
                {
                    ENTITY[] all = SETT.ENTITIES().getAllEnts();
                    int mm = hovered + 1;
                    for (int i = 1; i <= all.Length; i++)
                    {
                        int ei = MATH.mod(i + mm, all.Length);
                        ENTITY e = all[ei];
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (a.indu().hType() == HTYPES.TOURIST())
                            {
                                hovered = ei;
                                VIEW.UI().manager.close();
                                VIEW.s().getWindow().centerer.set(a.body().cX(), a.body().cY());
                                return;
                            }
                        }
                    }
                    hovered = -1;
                }

                protected override void renAction()
                {
                    activeSet(STATS.POP().pop(HTYPES.TOURIST()) > 0);
                }
            }.hoverInfoSet(¤¤goTo));

            addRightC(8, new GButt.ButtPanel(SPRITES.icons().m.questionmark)
            {
                protected override void clickA()
                {
                    TOURISM.wiki().exe();
                }
            }.hoverInfoSet(WIKI.¤¤name));

            {
                HISTORY_INT c = TOURISM.history();
                GStaples s = new GStaples(c.historyRecords())
                {
                    protected override void hover(GBox box, int stapleI)
                    {
                        int i = c.historyRecords() - 1 - stapleI;
                        box.textLL(DicTime.setSpanDays(box.text(), (i) * c.time().bitSeconds(), (i + 1) * c.time().bitSeconds()));
                        box.NL();
                        box.add(GFORMAT.iIncr(box.text(), c.get(i)));
                    }

                    protected override double getValue(int stapleI)
                    {
                        int i = c.historyRecords() - 1 - stapleI;
                        return c.get(i);
                    }
                };
                s.body().setWidth(400).setHeight(64);
                add(s, x1, body().y2() + 2);
            }

            HistoryInt c = FACTIONS.player().credits().get(CTYPE.TOURISM).IN;
            add(new GStat()
            {
                public override void update(GText text)
                {
                    GFORMAT.i(text, c.get(0));
                }
            }.increase().hh(Dic.¤¤Earnings), x1, body().y2() + 12);

            GStaples s = new GStaples(c.historyRecords())
            {
                protected override void hover(GBox box, int stapleI)
                {
                    int i = c.historyRecords() - 1 - stapleI;
                    box.textLL(DicTime.setSpanDays(box.text(), (i) * c.time().bitSeconds(), (i + 1) * c.time().bitSeconds()));
                    box.NL();
                    box.add(GFORMAT.iIncr(box.text(), c.get(i)));
                }

                protected override double getValue(int stapleI)
                {
                    int i = c.historyRecords() - 1 - stapleI;
                    return c.get(i);
                }
            };
            s.body().setWidth(400).setHeight(64);
            addRelBody(2, DIR.S, s);
        }

        private void perm()
        {
            int i = 0;
            GuiSection s = new GuiSection();
            foreach (Race r in TOURISM.races())
            {
                s.addGrid(new GButt.ButtPanel(r.appearance().iconBig)
                {
                    protected override void renAction()
                    {
                        selectedSet(TOURISM.permit(r));
                    };

                    protected override void clickA()
                    {
                        TOURISM.permit(r, !TOURISM.permit(r));
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(r.info.names);

                        b.textL(Dic.¤¤Occurrence);
                        b.tab(5);
                        b.add(GFORMAT.perc(b.text(), r.tourism().occurence));
                        b.NL(2);

                        b.textL(¤¤Generosity);
                        b.tab(5);
                        b.add(GFORMAT.perc(b.text(), r.tourism().credits));
                        b.NL(2);

                        b.textLL(¤¤Attractions);
                        b.NL();
                        bool line = false;
                        foreach (RoomBlueprintImp p in r.tourism().attractions)
                        {
                            b.add(p.icon);
                            b.add(" " + p.name);
                            line = !line;
                        }

                        b.textL("");
                    }
                }, i, 0);
                i++;
            }

            addRelBody(4, DIR.S, s);
        }

        private void rev(int height)
        {
            INt in = new INt()
            {
                public override int max()
                {
                    return CLAMP.i(TOURISM.reviews().size() - 1, 0, 100);
                }

                public override int get()
                {
                    return in.get();
                }

                public override void set(int t)
                {
                    in.set(t);
                }
            };

            addRelBody(4, DIR.S, new GHeader(Dic.¤¤Reviews));

            int x1 = getLastX2();
            int cy = getLast().cY();

            GTarget t = new GTarget(100, (SPRITE)null, false, true, new GStat()
            {
                public override void update(GText text)
                {
                    if (in.max() == 0)
                        GFORMAT.iofk(text, in.get(), in.max());
                    else
                        GFORMAT.iofk(text, in.get() + 1, in.max() + 1);
                    text.normalify();
                }
            }, in);
            addRelBody(4, DIR.S, t);

            RENDEROBJ o = new RENDEROBJ.RenderImp(900, height - Tourism.this.body().height() - 16)
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    Review rev = TOURISM.reviews().get(in.get());
                    if (rev != null)
                    {
                        rev.render(r, body().x1(), body().y1(), body().width());
                    }
                }
            };

            addRelBody(16, DIR.S, o);

            addC(new GStat()
            {
                public override void update(GText text)
                {
                    text.add('(');
                    double d = TOURISM.score();
                    if (d < 0.3)
                        text.add(¤¤bad);
                    else if (d < 0.8)
                        text.add(¤¤ok);
                    else
                        text.add(¤¤good);
                    text.add(')');
                    text.lablifySub();
                }

                public override void hoverInfoGet(GBox b)
                {
                    b.add(GFORMAT.perc(b.text(), TOURISM.score()));
                };
            }.r(), x1 + 120, cy);
        }
    }
}