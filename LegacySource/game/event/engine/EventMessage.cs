using System;
using System.Collections.Generic;
using game;
using game.event.actions;
using game.faction;
using init.settings;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.main;
using view.ui.message;

namespace game.event.engine
{
    class EventMessage : MessageSection
    {
        private static string ¤¤active = "This event is no longer relevant.";

        static
        {
            D.ts(typeof(EventMessage));
        }

        private readonly string eKey;
        private readonly int ei;
        private readonly EContext data;
        private readonly int iteration;
        private int choice = -1;

        private readonly string[] mess;

        public EventMessage(Event abs, EContext data) : base(abs.info.name)
        {
            eKey = abs.key;
            ei = abs.allIndex;
            mess = new string[abs.info.messages.Length];
            for (int i = 0; i < mess.Length; i++)
            {
                Str s = new Str(128);
                s.add(abs.info.messages[i]);
                EContext.insert.set(s, data);
                mess[i] = "" + s;
            }

            this.data = new EContext(data);
            iteration = GAME.EVENT().occ(abs);
        }

        protected override void make(GuiSection section)
        {
            Event e = GAME.EVENT().read(ei, eKey);
            if (e == null)
                return;

            LinkedList<RENDEROBJ> rr = new LinkedList<RENDEROBJ>();

            foreach (var par in mess)
            {
                foreach (var s in UI.FONT().M.getRows(par, 800))
                {
                    if (s.Length > 0)
                        rr.add(new GTextR(UI.FONT().M, s));
                }

                rr.add(new RENDEROBJ.RenderDummy(10, 8));
            }

            if (data.coo.x() >= 0)
            {
                GButt.ButtPanel b = new GButt.ButtPanel(UI.icons().m.crossair)
                {
                    clickA = () => VIEW.s().getWindow().centererTile.set(data.coo)
                };
                b.body.setDim(100);
                section.addRelBody(8, DIR.S, b);
                rr.add(new RENDEROBJ.RenderDummy(10, 8));
            }

            foreach (EventAction a in e.on_spawn)
            {
                if (!a.hideUI)
                {
                    a.addToMessageBody(rr, e, data, section.body());
                    rr.add(new RENDEROBJ.RenderDummy(10, 8));
                }
            }

            GRows butts = new GRows(2);

            int i = 0;
            foreach (EChoice c in e.choices)
            {
                int k = i++;
                GButt.ButtPanel b = new GButt.ButtPanel(c.name)
                {
                    clickA = () =>
                    {
                        VIEW.messages().hide();
                        foreach (EventAction a in c.actions)
                        {
                            if (a.problem(e, data) != null)
                                return;
                        }
                        if (!c.request.passes(FACTIONS.player()))
                            return;
                        GAME.EVENT().choiceSelect(e, k);

                        choice = k;
                        foreach (EventAction a in c.actions)
                            a.exe(e, data);
                        if (GAME.EVENT().current() == e)
                            GAME.EVENT().set(null, false, false, false, false);

                        base.clickA();
                    },
                    render = (r, ds, isActive, isSelected, isHovered) =>
                    {
                        base.render(r, ds, isActive, isSelected, isHovered);
                        if (!c.request.passes(FACTIONS.player()))
                        {
                            OPACITY.O50.bind();
                            COLOR.WHITE50.render(r, body, -2);
                            OPACITY.unbind();
                            return;
                        }
                        foreach (EventAction a in c.actions)
                        {
                            if (a.problem(e, data) != null)
                            {
                                OPACITY.O50.bind();
                                COLOR.WHITE50.render(r, body, -2);
                                OPACITY.unbind();
                                return;
                            }
                        }
                    },
                    renAction = () =>
                    {
                        selectedSet(choice == k);
                        activeSet(iteration == GAME.EVENT().occ(e) && GAME.EVENT().current() == e);
                    },
                    hoverInfoGet = text =>
                    {
                        GBox b = (GBox)text;
                        b.title(c.name);

                        if (iteration != GAME.EVENT().occ(e) || GAME.EVENT().current() != e)
                        {
                            b.error(¤¤active);
                            b.NL();
                            if (!S.get().developer)
                                return;

                        }

                        foreach (EventAction a in c.actions)
                        {
                            if (!a.hideUI)
                            {
                                a.hover(b, e, data);
                                b.NL(8);
                            }
                        }
                        b.NL();
                        foreach (EventAction a in c.actions)
                        {
                            string p = a.problem(e, data)?.ToString();
                            if (p != null)
                            {
                                b.error(p);
                                b.NL();
                            }
                        }
                        if (c.request.all().size() > 0)
                        {
                            c.request.hover(text, FACTIONS.player());
                        }

                        if (S.get().developer)
                        {
                            foreach (EventAction a in c.actions)
                                b.text(a.key);
                        }
                        base.hoverInfoGet(text);
                    }
                };

                b.body.setWidth((WIDTH - 100) / 2);

                butts.add(b);
            }

            SPRITE icon = null;
            if (icon == null && e.selection.indu.useAsIcon)
                icon = data.indu.sprite();
            if (icon == null && e.selection.reg.useAsIcon)
                icon = data.regs.sprite();
            if (icon == null && e.selection.faction.useAsIcon)
                icon = data.faction.sprite();
            if (icon == null && e.selection.royalty.useAsIcon)
                icon = data.royalty.sprite();
            if (icon == null)
                icon = e.info.icon;

            int hmax = 700 - icon.height() - butts.height() - 16;
            foreach (RENDEROBJ r in rr)
            {
                hmax -= r.body().height();
            }

            if (hmax > 0)
            {
                foreach (RENDEROBJ o in rr)
                    section.addDown(0, o);
            }
            else
            {
                GScrollRows sr = new GScrollRows(rr, 700 - icon.height() - butts.height() - 16);
                section.addRelBody(8, DIR.S, sr.view());
            }

            SPRITE ic = icon;
            if (icon != null)
            {
                int sc = 1;
                if (icon.width() < Icon.HUGE)
                    sc = 2;

                SPRITE sp = new SPRITE.Imp(icon.width() * sc, icon.height() * sc)
                {
                    render = (r, X1, X2, Y1, Y2) =>
                    {
                        ic.render(r, X1, X2, Y1, Y2);
                    }
                };
                section.addRelBody(8, DIR.N, sp);
            }

            foreach (RENDEROBJ o in butts.rowsCentered(WIDTH))
                section.addRelBody(0, DIR.S, o);
        }
    }
}