using System;
using System.Collections.Generic;
using game.faction;
using game.time;
using init.race;
using init.type;
using settlement.stats;
using util.statistics;
using world.army;

namespace settlement.stats.law
{
    public class StatPunishment
    {
        private readonly HistoryObject<HCLASS_RACE> success;
        public readonly PUNISHMENT punish;

        private double[] rates;

        public StatPunishment(StatsInit init, PUNISHMENT type)
        {
            this.punish = type;

            success = new HistoryObject<HCLASS_RACE>(STATS.DAYS_SAVED, TIME.days(), false, HCLASS_RACE.MAP());
            init.savers.Put("LAW_PUNISHMENT_COUNT_" + type.key, success);
        }

        public void report(HCLASS_RACE cc)
        {
            if (cc.cl == HCLASSES.OTHER() && punish != CRIME_PUNISHMENTS.STOCKS())
            {
                double cruelty = punish.crueltyPerPerson(cc.cl, cc.race);
                AD.stats().mercy().incD(FACTIONS.player(), -cruelty);
                cruelty = punish.mercyPerPerson(cc.cl, cc.race);
                AD.stats().mercy().incD(FACTIONS.player(), cruelty);
            }

            this.success.inc(cc, 1);

            double dd = 1.0 / CRIME_PUNISHMENTS.get(cc.cl).Count;

            rates[cc.index()]++;
            foreach (PUNISHMENT p in CRIME_PUNISHMENTS.ALL())
            {
                p.stat().rates[cc.index()] -= dd;
                if (p.stat().rates[cc.index()] < 0)
                    p.stat().rates[cc.index()] = 0;
            }

            double tot = 0;
            foreach (PUNISHMENT p in CRIME_PUNISHMENTS.get(cc.cl))
            {
                tot += p.stat().rates[cc.index()];
            }

            if (tot > 0)
            {
                foreach (PUNISHMENT p in CRIME_PUNISHMENTS.get(cc.cl))
                {
                    p.stat().rates[cc.index()] /= tot;
                }
            }
        }

        public double rate(HCLASS cl, Race race)
        {
            double tot = 0;
            for (int pi = 0; pi < CRIME_PUNISHMENTS.get(cl).Count; pi++)
            {
                PUNISHMENT p = CRIME_PUNISHMENTS.get(cl)[pi];
                tot += p.stat().recent(cl, race);
            }
            if (tot == 0)
                return 0;

            return recent(cl, race) / tot;
        }

        public double recent(HCLASS cl, Race race)
        {
            if (cl == null)
            {
                double tot = 0;
                for (int i = 0; i < HCLASSES.ALLP().Count; i++)
                {
                    tot += recent(HCLASSES.ALLP()[i], race);
                }
                return tot;
            }

            if (race == null)
            {
                double tot = 0;
                for (int i = 0; i < RACES.all().Count; i++)
                {
                    tot += recent(cl, RACES.all()[i]);
                }
                return tot;
            }

            return rates[HCLASS_RACE.clP(race, cl).index()];
        }

        public void decRate(HCLASS_RACE cc)
        {
            bool dec = false;
            foreach (PUNISHMENT p in CRIME_PUNISHMENTS.get(cc.cl))
            {
                if (p.stat().rates[cc.index()] > 1)
                {
                    dec = true;
                    break;
                }
            }
            if (dec)
            {
                foreach (PUNISHMENT p in CRIME_PUNISHMENTS.get(cc.cl))
                {
                    p.stat().rates[cc.index()] *= 0.85;
                }
            }
        }

        public HISTORY_COLLECTION<HCLASS_RACE> success()
        {
            return success;
        }
    }
}