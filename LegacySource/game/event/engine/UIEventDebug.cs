using System;
using System.Collections.Generic;
using System.Linq;
using game;
using game.event.actions;
using game.faction;
using game.faction.royalty;
using init.constant;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.main;
using settlement.stats;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.gui.misc;
using util.gui.panel;
using util.gui.table;
using util.info;
using util.text;
using view.interrupter;
using view.main;
using world.map.regions;

namespace game.event.engine
{
    final class UIEventDebug : GuiSection
    {
        public UIEventDebug(EVENT_HANDLER en)
        {
            addDown(2, new GStat()
            {
                public override void update(GText text)
                {
                    if (en.current() == null)
                        text.add('-');
                    else
                        text.add(en.current().key);
                }
            }.hh("current"));

            addDown(2, new GStat()
            {
                public override void update(GText text)
                {
                    if (en.current() == null)
                        return;
                    text.add(en.timeElapsed());
                    text.add('/');
                    text.add(en.current().duration.seconds);
                }
            }.hh("time"));

            addDown(2, new GButt.ButtPanel("Expire")
            {
                protected override void clickA()
                {
                    en.expire();
                }

                protected override void renAction()
                {
                    activeSet(en.current() != null);
                }
            });

            addDown(2, new GButt.ButtPanel("#")
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    foreach (string s in en.tags.keys())
                    {
                        if (en.tags.get(s) == Boolean.TRUE)
                            text.text(s);
                    }
                }
            });

            GInput in = new GInput(new StringInputSprite(16, UI.FONT().S));

            addDown(2, in);

            LinkedList<RENDEROBJ> rows = new LinkedList<>();

            foreach (Event a in Event.all)
            {
                SPRITE sp = new SPRITE.Imp(700, 24)
                {
                    public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        int CY = Y1 + (Y2 - Y1) / 2;
                        (en.can(a) ? COLOR.GREEN100 : COLOR.RED100).bind();
                        UI.icons().s.dot.renderCY(r, X1 + 6, CY);
                        COLOR.unbind();
                        UI.FONT().S.renderCY(r, X1 + 32, CY, a.key);
                        GCOLOR.T().H1.bind();
                        UI.FONT().S.renderCY(r, X1 + 382, CY, a.info.name);
                        Str.TMP.clear().add(en.acc(a), 2);
                        COLOR.unbind();
                        UI.FONT().S.renderCY(r, X2 - 64, CY, Str.TMP);
                    }
                };

                GButt b = new GButt.ButtPanel(sp)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        UIEventDebug.this.hover(b, a);
                    }

                    protected override void clickA()
                    {
                        en.set(a, false, false, true, true);
                        GuiSection pop = new GuiSection();
                        EContext c = en.context();
                        GText t = new GText(UI.FONT().S, "indu: ");
                        t.add(c.indu.am).s();
                        int am = 0;
                        for (int i = 0; i < en.context().indu.max(); i++)
                        {
                            Induvidual tt = c.indu.get(i);
                            if (tt != null && c.indu.eventGet(tt))
                            {
                                if (am++ < 4)
                                {
                                    t.add(STATS.APPEARANCE().name(tt)).s();
                                }
                            }
                        }
                        t.s().add(am);
                        t.adjustWidth();
                        pop.addDownC(8, t);

                        t = new GText(UI.FONT().S, "regs: ");
                        t.add(c.regs.am).s();
                        am = 0;
                        for (int i = 0; i < c.regs.max(); i++)
                        {
                            Region tt = c.regs.get(i);
                            if (tt != null && c.regs.eventGet(tt))
                            {
                                if (am++ < 4)
                                {
                                    t.add(tt.info.name()).s();
                                }
                            }
                        }
                        t.s().add(am);
                        t.adjustWidth();
                        pop.addDownC(8, t);

                        t = new GText(UI.FONT().S, "roys: ");
                        t.add(c.royalty.am).s();
                        am = 0;
                        for (int i = 0; i < c.royalty.max(); i++)
                        {
                            Royalty tt = c.royalty.get(i);
                            if (tt != null && c.royalty.eventGet(tt))
                            {
                                if (am++ < 4)
                                {
                                    t.add(tt.name() + " (" + tt.court.faction.name + ") ");
                                }
                            }
                        }
                        t.s().add(am);
                        t.adjustWidth();
                        pop.addDownC(8, t);

                        t = new GText(UI.FONT().S, "fact: ");
                        t.add(c.faction.am).s();
                        am = 0;
                        for (int i = 0; i < c.faction.max(); i++)
                        {
                            Faction tt = c.faction.get(i);
                            if (tt != null && c.faction.eventGet(tt))
                            {
                                if (am++ < 4)
                                {
                                    t.add(tt.name).s();
                                }
                            }
                        }
                        t.s().add(am);
                        t.adjustWidth();
                        pop.addDownC(8, t);

                        VIEW.inters().popup2.show(pop, this);
                    }
                };
                rows.add(b);
            }

            addDown(2, new GScrollRows(rows, 400)
            {
                protected override bool passesFilter(int i, RENDEROBJ o)
                {
                    if (in.text().length() == 0)
                        return true;

                    Event a = Event.all.get(i);
                    return Str.containsText(a.key, in.text()) || Str.containsText(a.info.name, in.text());
                }
            }.view());

            add(new GPanel(body()));
            moveLastToBack();
            body().centerIn(C.DIM());

            IDebugPanel.add("event engine", new ACTION()
            {
                public override void exe()
                {
                    VIEW.inters().section.activate(UIEventDebug.this);
                }
            });
        }

        private void hover(GBox b, Event a)
        {
            b.textLL(a.info.name);
            b.NL();

            b.text("Description:");
            b.add(UI.icons().s.info);
            b.text(a.info.description);
            b.NL();

            b.text("Occurrence:");
            b.NL();
            b.text("Base Rate:");
            b.add(GFORMAT.f0(b.text(), a.occurence.baseRate()));
            b.NL();
            b.text("Modifier:");
            b.add(GFORMAT.f0(b.text(), a.occurence.modifier()));
            b.NL();
            b.text("Total Rate:");
            b.add(GFORMAT.f0(b.text(), a.occurence.totalRate()));
            b.NL();
            b.text("Current Rate:");
            b.add(GFORMAT.f0(b.text(), a.occurence.currentRate()));
            b.NL();

            b.sep();

            b.text("Tags:");
            b.NL();
            b.text("Required Tags:");
            foreach (string tag in a.tags.requires)
            {
                b.text(tag);
                b.NL();
            }
            b.text("Forbidden Tags:");
            foreach (string tag in a.tags.forbids)
            {
                b.text(tag);
                b.NL();
            }

            b.sep();

            b.text("Actions:");
            b.NL();
            foreach (EventAction action in a.actions())
            {
                b.text(action.key);
                b.NL();
            }

            b.sep();

            b.text("Duration Actions:");
            b.NL();
            foreach (EventAction action in a.duration.actions)
            {
                b.text(action.key);
                b.NL();
            }
        }
    }
}