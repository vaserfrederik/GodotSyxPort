using System;
using System.Collections.Generic;
using util.data;
using util.gui.misc;
using util.info;
using util.statistics;

namespace settlement.stats.law
{
    public class StatsLaw : StatCollection
    {
        private static readonly string ¤¤name = "Law";
        private static readonly string ¤¤descc = "Statistics regarding law";
        private static readonly string ¤¤guardPower = "Guard Power";
        public static readonly string ¤¤tyranny = "Tyranny";
        public static readonly string ¤¤tyrannyD = "Tyranny is gained from punishing crimes harshly and is detrimental to happiness";

        static StatsLaw()
        {
            D.ts(typeof(StatsLaw));
        }

        public readonly LIST<StatCrime> crimes;
        public readonly LIST<StatPunishment> punishments;
        public readonly STAT guards;
        public readonly STAT executeArena;
        public readonly STAT EQUALITY;
        public readonly STAT EX_CON;
        public readonly STAT ESCAPED;
        readonly CrimesData cd;
        public readonly GETTER_TRANSE<Induvidual, CRIME> prisonerType;
        public double debug = 0;

        public StatsLaw(StatsInit init) : base(init, "LAW", ¤¤name, ¤¤descc)
        {
            var crimesList = new ArrayListGrower<StatCrime>();
            var punishmentsList = new ArrayListGrower<StatPunishment>();

            cd = new CrimesData(init);

            foreach (var c in CRIMES.ALL())
            {
                if (c == CRIMES.PERSECUTED(HCLASSES.CITIZEN()) || c == CRIMES.PERSECUTED(HCLASSES.SLAVE()))
                    crimesList.add(new StatCrimePersecute(init, c, cd));
                else
                    crimesList.add(new StatCrime(init, c, cd));
            }

            foreach (var c in CRIME_PUNISHMENTS.ALL())
                punishmentsList.add(new StatPunishment(init, c));

            crimes = crimesList;
            punishments = punishmentsList;

            executeArena = new STATFakeData("EXECUTE_ARENA", init)
            {
                protected override double getDD(HCLASS cl, Race race)
                {
                    double tot = 0;
                    double access = 0;
                    double rate = 0;
                    double rateTot = CRIME_PUNISHMENTS.EXECUTE().stat().recent(null, null);
                    foreach (var r in SETT.ROOMS().FIGHTPITS)
                    {
                        tot++;
                        access += r.service().stats().access().data(cl).getD(race);

                        foreach (var race2 in RACES.all())
                        {
                            if (r.punishEnabled().is(race2))
                                rate += CRIME_PUNISHMENTS.EXECUTE().stat().recent(null, race2);
                        }
                    }

                    access /= tot;
                    return access * (rate / rateTot);
                }
            };

            guards = new STAT(init, "GUARDS", "Guards")
            {
                protected override double getD(HCLASS cl, Race race)
                {
                    if (race == null)
                    {
                        double tot = 0;
                        double v = 0;
                        for (int ri = 0; ri < RACES.all().size(); ri++)
                        {
                            Race r = RACES.all().get(ri);
                            double pop = STATS.POP().POP.data(cl).get(r);
                            tot += pop;
                            v += pop * guards.data().getD(cl, r);
                        }
                        if (tot == 0)
                            return 0;
                        return v / tot;
                    }
                    double value = (double)SETT.ROOMS().GUARD.power.get() / STATS.POP().POP.data(cl).get(race);
                    return value * 0.25;
                }
            };

            EQUALITY = new STAT(init, "EQUALITY", "Equality")
            {
                protected override double getD(HCLASS cl, Race race)
                {
                    double tot = 0;
                    double v = 0;
                    for (int ri = 0; ri < RACES.all().size(); ri++)
                    {
                        Race r = RACES.all().get(ri);
                        double pop = STATS.POP().POP.data(cl).get(r);
                        tot += pop;
                        v += pop * EQUALITY.data().getD(cl, r);
                    }
                    if (tot == 0)
                        return 0;
                    return v / tot;
                }
            };

            EX_CON = new STAT(init, "EX_CON", "Ex Con")
            {
                protected override double getD(HCLASS cl, Race race)
                {
                    double tot = 0;
                    double v = 0;
                    for (int ri = 0; ri < RACES.all().size(); ri++)
                    {
                        Race r = RACES.all().get(ri);
                        double pop = STATS.POP().POP.data(cl).get(r);
                        tot += pop;
                        v += pop * EX_CON.data().getD(cl, r);
                    }
                    if (tot == 0)
                        return 0;
                    return v / tot;
                }
            };

            ESCAPED = new STAT(init, "ESCAPED", "Escaped")
            {
                protected override double getD(HCLASS cl, Race race)
                {
                    double tot = 0;
                    double v = 0;
                    for (int ri = 0; ri < RACES.all().size(); ri++)
                    {
                        Race r = RACES.all().get(ri);
                        double pop = STATS.POP().POP.data(cl).get(r);
                        tot += pop;
                        v += pop * ESCAPED.data().getD(cl, r);
                    }
                    if (tot == 0)
                        return 0;
                    return v / tot;
                }
            };

            prisonerType = new GETTER_TRANSE<Induvidual, CRIME>()
            {
                private readonly INT_OE<Induvidual> data = init.count.new DataByte("LAW_PRISONERT");

                public CRIME get(Induvidual f)
                {
                    int i = data.get(f);
                    if (i == 0)
                        return null;
                    return CRIMES.ALL().get(i - 1);
                }

                public void set(Induvidual f, CRIME t)
                {
                    if (f.hType() == HTYPES.PRISONER() && f.added() && get(f) != null)
                    {
                        get(f).stat().criminalsTot--;
                        get(f).stat().criminals[f.race().index()]--;
                        cd.count(f, -1);
                    }
                    data.set(f, t.index() + 1);
                    if (f.hType() == HTYPES.PRISONER() && f.added())
                    {
                        get(f).stat().criminalsTot++;
                        get(f).stat().criminals[f.race().index()]++;
                        cd.count(f, 1);
                    }
                }
            };

            new StatLawBoosts(this);
        }

        public double lawMultiplier(HCLASS cl, Race race)
        {
            if (race == null)
            {
                double tot = 0;
                double v = 0;
                for (int ri = 0; ri < RACES.all().size(); ri++)
                {
                    Race r = RACES.all().get(ri);
                    double pop = STATS.POP().POP.data(cl).get(r);
                    tot += pop;
                    v += pop * lawMultiplier(cl, r);
                }
                if (tot == 0)
                    return 0;
                return v / tot;
            }
            double l = 0;
            foreach (var c in crimes)
            {
                l += c.law(cl, race);
            }
            return l;
        }

        public int criminals(HCLASS cl, Race race)
        {
            return cd.criminals(cl, race);
        }

        public void hoverGuards(GUI_BOX text, HCLASS cl, Race type)
        {
            GBox b = (GBox)text;

            b.add(UI.icons().s.shield);
            b.text(¤¤guardPower);
            b.tab(6);
            b.add(GFORMAT.f0(b.text(), (int)SETT.ROOMS().GUARD.power.get()));
            b.NL();

            b.text(cl.names);
            b.tab(6);
            b.add(GFORMAT.i(b.text(), STATS.POP().POP.data(cl).get(null)));
            b.NL();

            b.text(Dic.¤¤Value);
            b.tab(6);
            GText tt = b.text();

            tt.add('s').add('q').add('r').add('t').add('(');
            tt.add(0.25).add('*');
            tt.add(((int)SETT.ROOMS().GUARD.power.get()));
            tt.add('/');
            tt.add(STATS.POP().POP.data(cl).get(null));
            tt.add(')');
            tt.add('=');
            b.add(tt);
            b.add(GFORMAT.f(b.text(), guards.data().getD(null)));
            b.NL();
        }

        public double escapees()
        {
            return cd.escapedPrisoners;
        }

        public void escapeInc()
        {
            cd.escapedPrisoners++;
        }

        public double persecution(HCLASS cl, Race type)
        {
            StatCrimePersecute p = (StatCrimePersecute)CRIMES.PERSECUTED(cl).stat();
            return p.value(cl, type);
        }

        public HISTORY_COLLECTION<HCLASS_RACE> crimeHistory()
        {
            return cd.crimesComitted;
        }

        public Curfew getCurfew()
        {
            return cd.curfew;
        }

        public void punish(Induvidual i, PUNISHMENT p)
        {
            CRIME c = prisonerType.get(i);
            HCLASS cl = c.cl;
            Race race = i.race();
            punish(c, cl, race, p);
        }

        public void punish(CRIME c, HCLASS cl, Race race, PUNISHMENT p)
        {
            c.stat().punish(cl, race, p);
            p.stat().report(HCLASS_RACE.clP(race, cl));
        }

        public double lawHistory(HCLASS_RACE ra, int days)
        {
            return cd.history(ra, days);
        }

        public DOUBLE_O<HCLASS_RACE> tyranny = new DOUBLE_O<HCLASS_RACE>()
        {
            private readonly INFO info = new INFO(¤¤tyranny, ¤¤tyrannyD);

            public double getD(HCLASS_RACE hrace)
            {
                HCLASS cl = hrace.cl;
                Race race = hrace.race;
                return tyrrany(cl, race);
            }

            public INFO info()
            {
                return info;
            }
        };

        public double tyrrany(HCLASS cl, Race race)
        {
            if (cl == null)
            {
                double tot = 0;
                double v = 0;
                for (int ci = 0; ci < HCLASSES.ALLP().size(); ci++)
                {
                    HCLASS c = HCLASSES.ALLP().get(ci);
                    double pop = STATS.POP().POP.data(c).get(null);
                    tot += pop;
                    v += pop * tyrrany(c, null);
                }
                if (tot == 0)
                    return 0;
                return v / tot;
            }

            if (race == null)
            {
                double tot = 0;
                double v = 0;
                for (int ri = 0; ri < RACES.all().size(); ri++)
                {
                    Race r = RACES.all().get(ri);
                    double pop = STATS.POP().POP.data(cl).get(r);
                    tot += pop;
                    v += pop * tyrrany(cl, r);
                }
                if (tot == 0)
                    return 0;
                return v / tot;
            }
            double am = 0;
            for (int q = 0; q < CRIMES.all(cl).size(); q++)
            {
                int ci = CRIMES.all(cl).get(q).index();
                if (crimes.get(ci).crime == CRIMES.PERSECUTED(cl))
                    continue;
                am += crimes.get(ci).tyrrany(cl, race);
            }
            return am;
        }
    }
}