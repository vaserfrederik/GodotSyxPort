using System;
using System.Collections.Generic;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.service;
using settlement.stats.standing;
using settlement.stats.stat;
using snake2d.util.sets;
using snake2d.util.sprite.text;

namespace init.race.bio
{
    final class BioOpinion
    {
        private readonly BioOpinionData datas;
        private readonly List<List<Prio>> prios = new List<List<Prio>>(HCLASSES.ALL().Count);
        private readonly List<Prio> tmp = new List<Prio>(STATS.all().Count);
        private readonly double[] tres = new double[] { 0.25, 0.5, 0.95 };

        BioOpinion(BioOpinionData normal, Race race)
        {
            this.datas = normal;

            foreach (HCLASS cl in HCLASSES.ALL())
            {
                Race r = race;
                Tree<Prio> sort = new Tree<Prio>(STATS.all().Count)
                {
                    IsGreaterThan = (current, cmp) => current.prio > cmp.prio
                };

                bool[] has = new bool[STATS.all().Count];
                {
                    double m = 0;
                    STAT ss = null;
                    foreach (StatServiceImp s in STATS.SERVICE().ALL)
                    {
                        has[s.total().index()] = true;
                        if (s.total().standing().def(cl, r) > m)
                        {
                            ss = s.total();
                            m = s.total().standing().def(cl, r);
                        }
                        if (ss != null)
                        {
                            double pri = (ss.standing().definition(r).prio) * ss.standing().definition(r).get(cl).max;
                            if (pri > 0)
                                sort.add(new Prio(ss, pri));
                        }
                    }
                }

                {
                    double m = 0;
                    STAT ss = null;
                    foreach (StatGrave s in STATS.BURIAL().graves())
                    {
                        has[s.index()] = true;
                        if (s.standing().def(cl, r) > m)
                        {
                            ss = s;
                            m = s.standing().def(cl, r);
                        }
                    }
                    if (ss != null)
                    {
                        double pri = (ss.standing().definition(r).prio) * ss.standing().definition(r).get(cl).max;
                        if (pri > 0)
                            sort.add(new Prio(ss, pri));
                    }
                }

                foreach (STAT s in STATS.all())
                {
                    if (has[s.index()])
                        continue;
                    if (s.standing() != null)
                    {
                        double pri = (s.standing().definition(r).prio) * s.standing().definition(r).get(cl).max;
                        if (pri > 0)
                            sort.add(new Prio(s, pri));
                    }
                }

                List<Prio> pris = new List<Prio>(sort.size());

                while (sort.hasMore())
                {
                    pris.add(sort.pollGreatest());
                }
                prios.Add(pris);
            }
        }

        private BioOpinionData get(Induvidual indu)
        {
            return datas;
        }

        private BioOpinionData get(HCLASS cl, Race race)
        {
            return datas;
        }

        public CharSequence title(Humanoid indu, double value)
        {
            return get(indu.indu()).title(indu, value);
        }

        public void get(LIST<Str> res, Humanoid a, long ran)
        {
            foreach (Str s in res)
                s.clear();
            HCLASS cl = a.indu().clas();
            Race race = a.race();
            BioOpinionData data = get(cl, race);

            int index = 0;
            if ((ran & 0x01F) == 0)
            {
                res.get(index++).add(data.funny(ran));
            }
            List<Prio> pp = prios[cl.index()];

            {
                tmp.clearSloppy();
                long r = ran;
                foreach (Prio p in pp)
                {
                    if ((r & 0b1) == 1)
                    {
                        tmp.add(p);
                    }
                    r = r >> 1;
                }
            }

            for (double t : tres)
            {
                for (int i = 0; i < tmp.size(); i++)
                {
                    Prio p = tmp.get(i);
                    double v = value(p, cl, race, p.stat.data(cl).getD(race));
                    if (v < t)
                    {
                        res.get(index++).add(data.get(p.stat, a, ran));
                        tmp.removeOrdered(i);
                        i--;
                        if (index >= res.size())
                            return;
                    }
                }
            }

            {
                tmp.clearSloppy();
                long r = ran;
                foreach (Prio p in pp)
                {
                    if ((r & 0b1) == 0)
                    {
                        tmp.add(p);
                    }
                    r = r >> 1;
                }
            }

            for (double t : tres)
            {
                for (int i = 0; i < tmp.size(); i++)
                {
                    Prio p = tmp.get(i);
                    double v = value(p, cl, race, p.stat.data(cl).getD(race));
                    if (v < t)
                    {
                        res.get(index++).add(data.get(p.stat, a, ran));
                        tmp.removeOrdered(i);
                        i--;
                        if (index >= res.size())
                            return;
                    }
                }
            }

            if (res.size() == 0)
            {
                res.get(index).add(data.full(ran));
            }
        }

        public void get(LIST<Str> res, Humanoid h)
        {
            foreach (Str s in res)
                s.clear();

            Induvidual indu = h.indu();
            BioOpinionData data = get(indu);

            long ran = STATS.RAN().get(indu, 0);
            ran = ran << 32;
            ran |= STATS.RAN().get(indu, 36);

            int index = 0;
            if ((ran & 0x01F) == 0)
            {
                res.get(index++).add(data.funny(ran));
            }
            List<Prio> pp = prios[indu.clas().index()];

            {
                tmp.clearSloppy();
                long r = ran;
                foreach (Prio p in pp)
                {
                    if ((r & 0b1) == 1)
                    {
                        tmp.add(p);
                    }
                    r = r >> 1;
                }
            }

            for (double t : tres)
            {
                for (int i = 0; i < tmp.size(); i++)
                {
                    Prio p = tmp.get(i);
                    double v = value(p, indu.clas(), indu.race(), p.stat.indu().getD(indu));
                    if (v < t)
                    {
                        res.get(index++).add(data.get(p.stat, h, ran));
                        tmp.removeOrdered(i);
                        i--;
                        if (index >= res.size())
                            return;
                    }
                }
            }

            {
                tmp.clearSloppy();
                long r = ran;
                foreach (Prio p in pp)
                {
                    if ((r & 0b1) == 0)
                    {
                        tmp.add(p);
                    }
                    r = r >> 1;
                }
            }

            for (double t : tres)
            {
                for (int i = 0; i < tmp.size(); i++)
                {
                    Prio p = tmp.get(i);
                    double v = value(p, indu.clas(), indu.race(), p.stat.indu().getD(indu));
                    if (v < t)
                    {
                        res.get(index++).add(data.get(p.stat, h, ran));
                        tmp.removeOrdered(i);
                        i--;
                        if (index >= res.size())
                            return;
                    }
                }
            }

            if (res.size() == 0)
            {
                res.get(index).add(data.full(ran));
            }

            foreach (Str s in res)
            {
                BioLine.insert.set(s, h);
            }
        }

        private double value(Prio p, HCLASS cl, Race r, double v)
        {
            StandingData def = p.stat.standing().definition(r).get(cl);
            if (def.from > def.to)
                v = 1.0 - v;
            return v;
        }

        private static readonly class Prio
        {
            private readonly STAT stat;
            private readonly float prio;

            Prio(STAT stat, double prio)
            {
                this.stat = stat;
                this.prio = (float)prio;
            }
        }
    }
}