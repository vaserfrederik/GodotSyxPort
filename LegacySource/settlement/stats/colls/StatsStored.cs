using System;
using System.Collections.Generic;
using init.race;
using init.resources;
using init.type;
using settlement.main;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.sets;
using util.text;

namespace settlement.stats.colls
{
    public class StatsStored : StatCollection
    {
        private static readonly CharSequence ¤¤descc = "¤The amount of resources stored in warehouses per capita.";
        private static readonly CharSequence ¤¤more = "¤{0}, we need more of it.";

        private static readonly CharSequence ¤¤name = "Storage";
        private static readonly CharSequence ¤¤desc = "How many items are stored per capita.";

        static StatsStored()
        {
            D.ts(typeof(StatsStored));
        }

        public StatsStored(StatsInit init)
            : base(init, "STORED", ¤¤name, ¤¤desc)
        {
            D.t(this);

            foreach (RESOURCE res in RESOURCES.ALL())
            {
                StatInfo info = new StatInfo(res.name, res.names, ¤¤descc);
                info.setOpinion(¤¤more, null);

                STATFake s = new STATFake(res.key, init, info)
                {
                    protected override double getDD(HCLASS s, Race r, int daysBack)
                    {
                        if (pdivider(null, null, daysBack) == 0)
                            return ROOMS.STOCKPILE.tally().amountsDay()[res.bIndex()][daysBack] > 0 ? 1 : 0;
                        return (double)ROOMS.STOCKPILE.tally().amountsDay()[res.bIndex()][daysBack] / pdivider(null, null, daysBack);
                    }
                };
                s.info().icon = res.icon();
                s.info().setMatters(true, false);
                s.info().setInt();
            }
        }

        public LIST<STAT> createTheOnesThatMatter(HCLASS cl)
        {
            ArrayList<STAT> res = new ArrayList<STAT>(all().size());

            foreach (STAT s in all())
            {
                bool added = false;
                foreach (Race r in RACES.all())
                {
                    if (!added && s.standing().max(cl, r) > 0)
                    {
                        res.add(s);
                        added = true;
                    }
                }
            }

            return res;
        }
    }
}