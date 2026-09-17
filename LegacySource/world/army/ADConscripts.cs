using System;
using System.Collections.Generic;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;
using util.text;
using view.world.panel;
using world.army.ADInit;
using world.entity.army;
using world.region;
using world.region.pop;

public sealed class ADConscripts
{
    private readonly List<INT_OE<Faction>> total = new List<INT_OE<Faction>>(RACES.all().Count);
    private readonly INT_O<Faction> totalAll;
    private readonly List<INT_O<Faction>> available = new List<INT_O<Faction>>(RACES.all().Count);
    private readonly INT_O<Faction> availableAll;
    private readonly List<INT_OE<Faction>> used = new List<INT_OE<Faction>>(RACES.all().Count);
    private readonly INT_O<Faction> usedAll;

    public INT_O<Faction> Total(Race race)
    {
        if (race == null)
            return totalAll;
        return total[race.index];
    }

    public INT_O<Faction> Available(Race race)
    {
        if (race == null)
            return availableAll;
        return available[race.index];
    }

    public INT_O<Faction> Used(Race race)
    {
        if (race == null)
            return usedAll;
        return used[race.index];
    }

    public bool CanTrain(Race race, Faction f)
    {
        if (f == null)
            return true;
        return AD.men(race).faction(f) - AD.cityDivs().total(race) < Total(race).get(f);
    }

    public int CanTrainI(Race race, Faction f)
    {
        if (f == null)
            return 100000;
        return Total(race).get(f) - AD.men(race).faction(f) + AD.cityDivs().total(race);
    }

    public void Kill(Race race, Faction f, int men)
    {
        if (f != null)
            total[race.index].inc(f, -men);
    }

    public ADConscripts(ADInit init)
    {
        foreach (Race r in RACES.all())
        {
            total.Add(init.dataT.new DataInt("CONSCRIPTABLE_" + r.key, Dic.¤¤Conscriptable, Dic.¤¤ConscriptsD)
            {
                public override int get(Faction t)
                {
                    if (r.population().max <= 0)
                    {
                        if (t == FACTIONS.player())
                        {
                            return 0;
                        }
                    }
                    return base.get(t);
                }
            });

            available.Add(new INT_O<Faction>()
            {
                public override int get(Faction t)
                {
                    return total[r.index].get(t) - used[r.index].get(t);
                }

                public override int min(Faction t)
                {
                    return 0;
                }

                public override int max(Faction t)
                {
                    return int.MaxValue;
                }
            });

            used.Add(init.dataT.new DataInt("CONSCRIPTABLE_USED_" + r.key, Dic.¤¤Conscriptable, Dic.¤¤ConscriptsD));
        }

        totalAll = Tot(total);
        availableAll = Tot(available);
        usedAll = Tot(used);

        IDebugPanelWorld.add("Conscripts 1000", new ACTION()
        {
            public override void exe()
            {
                foreach (RDRace rr in RD.RACES().all)
                    total[rr.race.index].inc(FACTIONS.player(), 1000);
            }
        });

        init.inits.Add(new ACTION_O<Faction>()
        {
            public override void exe(Faction t)
            {
                foreach (Race r in RACES.all())
                {
                    total[r.index()].set(t, RD.MILITARY().conscripts(r, t));
                }
            }
        });

        init.registers.Add(new Register()
        {
            public override void register(ADDiv div, int d)
            {
                if (div.needConscripts())
                {
                    used[div.race().index].inc(div.faction(), d * div.menTarget());
                }
            }
        });

        init.updaters.Add(new Updater()
        {
            public override void update(Faction f, double timeSinceLast)
            {
                if (f == null || !f.isActive())
                    return;

                foreach (Race r in RACES.all())
                {
                    int n = Total(r).get(f);
                    int t = RD.MILITARY().conscripts(r, f);
                    if (t < n)
                    {
                        AD.conscripts().total.get(r.index()).set(f, t);
                    }
                    else
                    {
                        double d = t - n;

                        if (d > 0)
                        {
                            d *= TIME.secondsPerDayI() * timeSinceLast / 8.0;
                            n += (int)d;
                            if (RND.rFloat() < (d - (int)d))
                                n++;
                        }
                        else if (d < 0)
                        {
                            n = t;
                        }

                        n = CLAMP.i(n, 0, t);

                        AD.conscripts().total.get(r.index()).set(f, n);
                    }
                }
            }

            public override void update(WArmy a, double timeSinceLast)
            {
                // TODO Auto-generated method stub
            }
        });
    }

    private static INT_O<Faction> Tot(List<INT_O<Faction>> li)
    {
        return new INT_O<Faction>()
        {
            public override int get(Faction t)
            {
                int am = 0;
                foreach (INT_O<Faction> f in li)
                    am += f.get(t);
                return am;
            }

            public override int min(Faction t)
            {
                return 0;
            }

            public override int max(Faction t)
            {
                return int.MaxValue;
            }
        };
    }
}