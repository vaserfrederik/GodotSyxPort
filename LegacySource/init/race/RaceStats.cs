using System;
using System.Collections.Generic;
using init.resources;
using init.type;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using settlement.stats.muls;
using settlement.stats.service;
using settlement.stats.standing;
using settlement.stats.stat;
using settlement.stats.util;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.race
{
    public class RaceStats
    {
        private readonly StandingDef[] reps;
        private readonly double[][] repNormalized;
        private readonly ArrayList<LIST<STAT>> standings = new ArrayList<LIST<STAT>>(HCLASSES.ALL().size());
        private readonly LinkedList<Tuple<STAT, double>> arrival = new LinkedList<Tuple<STAT, double>>();

        public RaceStats(Race race, Json json)  throws IOException
        {
            TRAITS.serRaceData(race, json);
            RESOURCES.SUP().setEfficiency(race, json);

            reps = new StandingDef[STATS.all().size()];
            repNormalized = new double[HCLASSES.ALL().size()][reps.Length];

            for (int i = 0; i < reps.Length; i++)
            {
                reps[i] = STATS.all().get(i).standing().base();
                foreach (HCLASS c in HCLASSES.ALL())
                    repNormalized[c.index()][i] = 0;
            }

            new StatsJson(json)
            {
                public override void doWithTheJson(STAT s, Json j, string key)
                {
                    j = j.json(key);
                    bool prio = j.has("PRIO");
                    StandingDef def = new StandingDef(j);
                    reps[s.index()] = def;
                    if (!prio)
                        reps[s.index()].prio = s.standing().base().prio;
                }

                public override void doWithMultiplier(StatMultiplier m, Json j, string key)
                {
                    // TODO Auto-generated method stub
                }
            };

            foreach (HCLASS c in HCLASSES.ALL())
            {
                double rmax = 0;
                ArrayList<STAT> stats = new ArrayList<STAT>(STATS.all().size());

                foreach (STAT s in STATS.all())
                {
                    StandingDef d = reps[s.index()];
                    if (d.get(c).max > 0)
                    {
                        stats.add(s);
                        if (d.get(c).max > rmax)
                            rmax = d.get(c).max;
                    }
                }

                if (rmax == 0 && (c == HCLASSES.CITIZEN() || c == HCLASSES.SLAVE()))
                    throw new Errors.GameError(c.name + ", race: " + race.info.name + " has no standing boosts!");
                if (rmax <= 0)
                    rmax = 1;
                else
                    foreach (STAT s in STATS.all())
                    {
                        if (reps[s.index()].get(c).max > 0)
                            repNormalized[c.index()][s.index()] = reps[s.index()].get(c).max / rmax;
                    }

                standings.add(new ArrayList<STAT>(stats));

                foreach (StatServiceImp s in STATS.SERVICE().ALL)
                {
                    StandingDef d = reps[s.total().index()];
                    s.permission().set(c.get(race), d.get(c).max > 0);
                }

                foreach (StatGrave s in STATS.BURIAL().graves())
                {
                    StandingDef d = reps[s.index()];
                    s.grave().permission().set(c, race, d.get(c).max > 0);
                }
            }

            if (json.has("STATS_ON_SPAWN"))
            {
                new StatsJson("STATS_ON_SPAWN", json)
                {
                    public override void doWithMultiplier(StatMultiplier m, Json j, string key)
                    {
                        // TODO Auto-generated method stub
                    }

                    public override void doWithTheJson(STAT s, Json j, string key)
                    {
                        arrival.add(new TupleImp<STAT, double>(s, j.d(key)));
                    }
                };
            }
        }

        public LIST<Tuple<STAT, double>> arrivalStats()
        {
            return arrival;
        }

        public int equipArrivalLevel(Equip e)
        {
            return 0;
        }

        public StandingDef def(StatStanding s)
        {
            return reps[s.stat().index()];
        }

        public double defNormalized(HCLASS c, StatStanding s)
        {
            return repNormalized[c.index()][s.stat().index()];
        }

        public LIST<STAT> standings(HCLASS c)
        {
            return standings.get(c.index());
        }
    }
}