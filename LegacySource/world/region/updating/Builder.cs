using System;
using System.Collections.Generic;
using System.Linq;
using snake2d.util.misc;
using snake2d.util.sets;
using world.map.regions;
using world.region;
using world.region.RDOutputs;
using world.region.RDReligions;
using world.region.building;
using world.region.pop.RDEdicts;
using world.region.pop;

namespace world.region.updating
{
    internal sealed class Builder
    {
        private readonly Resources res = new Resources();
        private readonly BRace race = new BRace();
        private readonly BReligion religion = new BReligion();
        private readonly BMil military = new BMil();
        private readonly BOther civic;

        private readonly RebelBuilder rebBuilder = new RebelBuilder();

        public Builder()
        {
            LinkedList<RDBuilding> all = new LinkedList<RDBuilding>();
            foreach (RDBuilding bu in RD.BUILDINGS().all)
            {
                all.Add(bu);
            }

            foreach (RBuilding<?> b in res.all)
                all.Remove(b.bu);

            foreach (RBuilding<?> b in race.all)
                all.Remove(b.bu);

            foreach (RBuilding<?> b in religion.all)
                all.Remove(b.bu);

            foreach (RBuilding<?> b in military.all)
                all.Remove(b.bu);

            civic = new BOther(all);
        }

        public void Build(Region reg)
        {
            RealmBuilder builder = rebBuilder;
            if (reg.faction() is FactionNPC)
            {
                builder = ((FactionNPC)reg.faction()).court().king().builder;
            }
            Build(reg, builder);
        }

        public void Build(Region reg, RealmBuilder builder)
        {
            if (builder == null)
                builder = rebBuilder;

            if (RD.OWNER().prevOwner(reg) == FACTIONS.player())
            {
                foreach (RDBuilding bu in RD.BUILDINGS().all)
                {
                    if (bu.level.get(reg) > 0)
                    {
                        bu.level.set(reg, 0);
                        return;
                    }
                }
            }

            foreach (RDBuilding bu in RD.BUILDINGS().all)
                bu.level.set(reg, 0);

            foreach (RDRace rr in RD.RACES().all)
            {
                foreach (RDRaceEdict edict in RD.RACES().edicts.all)
                {
                    edict.toggled(rr).set(reg, 0);
                }
            }

            res.build(reg, builder);
            race.build(reg, builder);
            religion.build(reg, builder);
            military.build(reg, builder);
            civic.build(reg, builder);
        }

        private static class Resources
        {
            private readonly LinkedList<RBuilding<RDResource>> all = new LinkedList<RBuilding<RDResource>>();
            private readonly Sort<RDResource> tree;

            public Resources()
            {
                KeyMap<RDResource> map = new KeyMap<RDResource>();

                foreach (RDResource r in RD.OUTPUT().RES)
                {
                    map.put(r.boost.key, r);
                }

                foreach (RDBuilding bu in RD.BUILDINGS().all)
                {
                    RBuilding<RDResource> br = new RBuilding<RDResource>(bu)
                    {
                        value = (RSpec<RDResource> b, RealmBuilder builder, Region reg) =>
                            builder.priority(b.t.res, reg) * bu.baseEfficiency(reg)
                    };
                    foreach (BoostSpec s in bu.boosters().all())
                    {
                        if (map.containsKey(s.boostable.key))
                        {
                            br.bos.Add(new RSpec<RDResource>(s, map.get(s.boostable.key)));
                        }
                    }
                    if (bu.AIBuild && br.bos.Count > 0)
                        all.Add(br);
                }
                tree = new Sort<RDResource>(all);
            }

            public void build(Region reg, RealmBuilder builder)
            {
                tree.build(reg, builder, points(builder, reg, 1.0));
            }
        }

        private abstract static class RBuilding<T>
        {
            public readonly RDBuilding bu;
            public readonly ArrayListGrower<RSpec<T>> bos = new ArrayListGrower<RSpec<T>>();

            public RBuilding(RDBuilding bu)
            {
                this.bu = bu;
            }

            public double value(RealmBuilder current, Region rcurrent)
            {
                double v1 = 0;
                foreach (RSpec<T> b in bos)
                {
                    v1 += b.bo.booster.to() * value(b.t, current, rcurrent);
                }
                return v1;
            }

            public abstract double value(T t, RealmBuilder builder, Region reg);
        }

        private static class RSpec<T>
        {
            public readonly BoostSpec bo;
            public readonly T t;

            public RSpec(BoostSpec bo, T t)
            {
                this.bo = bo;
                this.t = t;
            }
        }

        private static class Sort<T> : Tree<RBuilding<T>>
        {
            private readonly LIST<RBuilding<T>> all;

            public Sort(LIST<RBuilding<T>> all) : base(all.size())
            {
                this.all = all;
            }

            protected RealmBuilder current;
            protected Region rcurrent;

            public double init(Region reg, RealmBuilder builder)
            {
                current = builder;
                rcurrent = reg;
                clear();
                double vv = 0;
                foreach (RBuilding<T> b in all)
                {
                    double v = b.value(builder, reg);
                    if (v > 0)
                    {
                        vv += v;
                        add(b);
                    }
                }
                return vv;
            }

            protected override bool isGreaterThan(RBuilding<T> curr, RBuilding<T> cmp)
            {
                return curr.value(current, rcurrent) > cmp.value(current, rcurrent);
            }

            public void build(Region reg, RealmBuilder builder, int points)
            {
                double mid = init(reg, builder);

                while (hasMore() && points > 0)
                {
                    RBuilding<T> b = pollGreatest();
                    if (b.bu.level.get(reg) != 0)
                        continue;
                    double v = b.value(builder, reg);

                    int l = (int)(Math.Ceiling((b.bu.levels.size() - 1) * v / mid));
                    l = CLAMP.i(l, 0, b.bu.levels.size() - 1);
                    l = CLAMP.i(l, 0, points);
                    b.bu.level.set(reg, l);
                    points -= l;
                }
            }
        }

        private static class RebelBuilder : RealmBuilder
        {
            private Region cacheReg = null;
            private double[] races = new double[RACES.all().size()];
            private double[] religions = new double[RACES.all().size()];
            private double mil;

            private void init(Region reg)
            {
                if (cacheReg == reg)
                    return;
                cacheReg = reg;
                mil = RD.RAN().get(reg, 20, 8) / 1024.0;

                double max = -double.MaxValue;
                RDRace mrace = null;
                int ri = RD.OWNER().ownerI.get(reg);
                foreach (RDRace rr in RD.RACES().all)
                {
                    double d = rr.pop.base(reg) * (0.25 + RD.RAN().get(reg, ri + rr.race.index() * 3, 3) / 7.0);
                    double dr = RD.RAN().get(reg, ri + rr.race.index() * 5, 4);
                    dr /= 0b111;
                    races[rr.race.index()] = -1.0 + dr;
                    if (d > max)
                    {
                        mrace = rr;
                        max = d;
                    }
                }

                double rasism = STATS.ENV().OTHERS.standing().definitionD(mrace.race);

                foreach (RDRace rr in RD.RACES().all)
                {
                    if (rr == mrace)
                    {
                        races[rr.race.index()] = 1 + 2 * rasism;
                        continue;
                    }
                    races[rr.race.index()] += mrace.race.pref().race(rr.race);
                    races[rr.race.index()] *= rasism;
                }

                RDReligion tr = RD.RELIGION().all().get(0);
                max = 0;
                foreach (RDReligion rr in RD.RELIGION().all())
                {
                    religions[rr.religion.index()] = 0;
                    if (rr.current.get(reg) > max)
                    {
                        max = rr.current.get(reg);
                        tr = rr;
                    }
                }
                religions[tr.religion.index()] = 1.0;
            }

            public double policy(Race race, Region reg)
            {
                init(reg);
                return races[race.index()];
            }

            public double priority(TRADABLE res, Region reg)
            {
                return 1.0;
            }

            public double priority(Religion religion, Region reg)
            {
                init(reg);
                return religions[religion.index()];
            }

            public double military(Region reg)
            {
                return mil;
            }

            public double size()
            {
                return 0.25;
            }
        }
    }
}