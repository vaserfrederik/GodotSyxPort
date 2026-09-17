using System;
using System.Collections.Generic;
using Init.Race;
using Init.Sprite;
using Init.Type;
using Settlement.Stats;
using Settlement.Stats.Muls;
using Snake2D.Util.Datatypes;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Misc;
using Snake2D.Util.Sets;
using Util.Data;
using Util.Gui.Misc;
using Util.Gui.Slider;
using Util.Gui.Table;
using Util.Info;
using Util.Text;
using View.Interrupter;
using View.Main;

namespace View.Sett.Ui.Standing.Decree
{
    final class DPanel : ISidePanel
    {
        private static readonly string ¤¤Cancel = "Click to cancel action for {0} subjects.";
        private static readonly string ¤¤Set = "Set action for:";
        private static readonly string ¤¤Projected = "Projected fulfillment increase";
        private static readonly string ¤¤AutoPer = "Automatically execute this decree for {0} % of your population.";
        private static readonly string ¤¤AutoAm = "Automatically execute this decree when population is above {0} people.";

        static
        {
            D.ts(typeof(DPanel));
        }

        public DPanel(HCLASS cl, GETTER<Race> race)
        {
            titleSet(UIDecreeButt.¤¤title);

            section = new GuiSection
            {
                Render = (r, ds) =>
                {
                    if (cl == HCLASSES.CITIZEN() && race.get() == null)
                    {
                        VIEW.s().panels.remove(this);
                        return;
                    }
                    base.render(r, ds);
                }
            };

            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            foreach (StatMultiplier m in STATS.MULTIPLIERS().get(cl))
            {
                if (m.available(cl) && m is StatMultiplierAction)
                {
                    StatMultiplierAction dec = (StatMultiplierAction)m;
                    other(rows, dec, cl, race);
                }
            }

            section.add(new GScrollRows(rows, HEIGHT - 16).view());
        }

        private void other(LinkedList<RENDEROBJ> rows, StatMultiplierAction dec, HCLASS cl, GETTER<Race> race)
        {
            GuiSection s = new GuiSection();

            s.add(new Header(dec, cl, race));
            II ii = new II(cl, race, dec);
            GSliderInt sl = slider(dec, cl, race, ii);
            s.addDown(2, sl);

            s.addRelBody(16, DIR.E, marker(dec, cl, race, ii).pad(4, 4));
            if (dec.canUnmark())
            {
                s.addRelBody(2, DIR.E, unmarker(dec, cl, race).pad(4, 4));
            }

            s.add(UI.icons().s.cog, 0, s.body().y2() + 2);
            INTE ee = new INTE
            {
                min = () => dec.auto(cl, race.get()).min,
                max = () => dec.auto(cl, race.get()).max,
                get = () => dec.auto(cl, race.get()).get(),
                set = t => dec.auto(cl, race.get()).set(t)
            };
            s.addRightC(8, new GSliderInt(ee, 100, true)
            {
                HoverInfoGet = text =>
                {
                    GBox b = (GBox)text;
                    GText t = b.text();
                    if (dec == STATS.MULTIPLIERS().EMANCIPATE || dec == STATS.MULTIPLIERS().PROSECUTION)
                    {
                        t.add(¤¤AutoAm).insert(0, ee.get());
                    }
                    else
                        t.add(¤¤AutoPer).insert(0, ee.get());
                    b.add(t);
                    base.hoverInfoGet(text);
                }
            });

            s.pad(8, 10);

            rows.add(s);
        }

        private GButt.ButtPanel unmarker(StatMultiplierAction dec, HCLASS cl, GETTER<Race> rr)
        {
            return new GButt.ButtPanel(SPRITES.icons().m.cancel)
            {
                ClickA = () => dec.unmark(cl, rr.get()),
                RenAction = () => activeSet(dec.unmarkable(cl, rr.get()) > 0),
                HoverInfoGet = text =>
                {
                    GBox b = (GBox)text;
                    GText t = b.text();
                    t.add(¤¤Cancel);
                    t.insert(0, dec.unmarkable(cl, rr.get()));
                    b.add(t);
                }
            };
        }

        private GSliderInt slider(StatMultiplierAction dec, HCLASS cl, GETTER<Race> rr, INTE ii)
        {
            return new GSliderInt(ii, 280, true)
            {
                HoverInfoGet = text => hov(text, cl, rr.get(), dec, ii.get())
            };
        }

        private GButt.ButtPanel marker(StatMultiplierAction dec, HCLASS cl, GETTER<Race> rr, INT ii)
        {
            return new GButt.ButtPanel(SPRITES.icons().m.ok)
            {
                ClickA = () => dec.mark(cl, rr.get(), ii.get()),
                RenAction = () => activeSet(ii.get() != 0),
                HoverInfoGet = text => hov(text, cl, rr.get(), dec, ii.get())
            };
        }

        static void hov(GUI_BOX text, HCLASS cl, Race race, StatMultiplierAction dec, int am)
        {
            GBox b = (GBox)text;
            b.title(dec.name);

            b.textLL(¤¤Set);
            b.NL();
            b.add(GFORMAT.i(b.text(), am));
            b.text(race.info.names);

            b.NL(8);

            GText t = b.text();
            t.add(¤¤Projected);
            t.lablify();
            b.add(t);
            b.NL(2);

            double d = (double)am / POP.pop(cl, race);
            dec.boosters.hover(text, d, null, -1);

            b.NL(8);

            dec.info(b, am);
        }

        private static class Header : GuiSection
        {
            private readonly HCLASS cl;
            private readonly GETTER<Race> race;
            private readonly StatMultiplierAction dec;

            public Header(StatMultiplierAction dec, HCLASS cl, GETTER<Race> race)
            {
                this.cl = cl;
                this.race = race;
                this.dec = dec;
                add(dec.icon, 0, 0);

                addCentredY(new GHeader(dec.verb), 48);

                addCentredY(new GStat
                {
                    Update = text =>
                    {
                        GFORMAT.f0(text, dec.value(cl, race.get(), 0));
                    }
                }, 260);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                b.title(dec.name);
                b.text(dec.desc);
                b.NL(8);

                dec.boosters.hover(text, HCLASS_RACE.clP(race.get(), cl));
            }
        }

        private final class II : INTE
        {
            int i = 0;
            private readonly HCLASS cl;
            private readonly GETTER<Race> race;
            private readonly StatMultiplierAction dec;

            public II(HCLASS cl, GETTER<Race> race, StatMultiplierAction dec)
            {
                this.cl = cl;
                this.race = race;
                this.dec = dec;
            }

            public int min()
            {
                return 0;
            }

            public int max()
            {
                return dec.maxAmount(cl, race.get());
            }

            public int get()
            {
                return CLAMP.i(i, 0, max());
            }

            public void set(int t)
            {
                i = t;
            }
        }
    }
}