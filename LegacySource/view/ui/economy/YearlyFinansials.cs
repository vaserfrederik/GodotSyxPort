using System;
using System.Collections.Generic;
using game.faction;
using game.faction.FCredits;
using game.faction.FResources;
using game.faction.player;
using game.time;
using init.sprite.UI;
using init.trade;
using init.type;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.ui.message;

namespace view.ui.economy
{
    public class YearlyFinansials : MessageSection
    {
        private static readonly string ¤¤title = "Yearly Report";
        private static readonly string ¤¤in = "in";
        private static readonly string ¤¤out = "out";
        private static readonly string ¤¤profits = "net";

        private readonly Data[] all;

        public YearlyFinansials() : base(¤¤title)
        {
            var all = new List<Data>();

            foreach (CTYPE t in Enum.GetValues(typeof(CTYPE)))
            {
                Data d = new Data(UI.icons().s.money, Dic.¤¤Curr + " " + t.ToString(), FACTIONS.player().credits().get(t).yearly);
                if (d.active())
                    all.Add(d);
            }

            Data d = new Data(UI.icons().s.money, Dic.¤¤Curr + " " + Dic.¤¤Total, FACTIONS.player().credits().yearly);
            if (d.active())
                all.Add(d);

            foreach (TRADABLE t in TR.ALL())
            {
                d = new Data(t);
                if (d.active())
                    all.Add(d);
            }

            foreach (HGROUP h in HGROUP.all())
            {
                d = new Data(h);
                if (d.active())
                    all.Add(d);
            }

            this.all = all.ToArray();
        }

        protected override void make(GuiSection section)
        {
            GTableBuilder bu = new GTableBuilder
            {
                nrOFEntries = () => all.Length
            };

            bu.column("", 200, new GRowBuilder
            {
                build = (GETTER<int> ier) => new GStat
                {
                    update = text =>
                    {
                        text.lablify();
                        text.add(all[ier.get()].name);
                    }
                }.r(DIR.NW)
            });

            bu.column(¤¤in, 80, new GRowBuilder
            {
                build = (GETTER<int> ier) => new GStat
                {
                    update = text => GFORMAT.i(text, all[ier.get()].in)
                }.r(DIR.NW)
            });

            bu.column(¤¤out, 80, new GRowBuilder
            {
                build = (GETTER<int> ier) => new GStat
                {
                    update = text => GFORMAT.i(text, all[ier.get()].out)
                }.r(DIR.NW)
            });

            bu.column(¤¤profits, 80, new GRowBuilder
            {
                build = (GETTER<int> ier) => new GStat
                {
                    update = text => GFORMAT.iIncr(text, all[ier.get()].in + all[ier.get()].out)
                }.r(DIR.NW)
            });

            section.add(bu.create(10, true));
        }

        private static class Data : Serializable
        {
            public int inValue = 0;
            public int outValue = 0;
            private readonly string name;

            public Data(SPRITE icon, string name, Yearly h)
            {
                this.name = name;
                int yy = 1;
                inValue = h.PROFITS.get(yy);
                outValue = h.LOSSES.get(yy);
            }

            public Data(TRADABLE tr)
            {
                this.name = tr.names;
                int year = (int)TIME.years().bitConversion(TIME.days());
                foreach (RTYPE t in RTYPE.all)
                {
                    inValue += FACTIONS.player().res().in(t).history(tr).getPeriodSum(-year, 0);
                    outValue -= FACTIONS.player().res().out(t).history(tr).getPeriodSum(-year, 0);
                }
            }

            public Data(HGROUP h)
            {
                this.name = h.name;
                int year = (int)TIME.years().bitConversion(TIME.days());
                int ii = STATS.POP().POP.data(h.type).get(h.race, 0) - STATS.POP().POP.data(h.type).get(h.race, year);
                if (ii < 0)
                    outValue += -ii;
                else
                    inValue += ii;
            }

            private bool active()
            {
                return inValue != 0 || outValue != 0;
            }
        }
    }
}