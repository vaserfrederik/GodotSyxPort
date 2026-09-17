using init.race;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main;
using settlement.room.water.pool;
using settlement.stats;
using settlement.stats.stat;
using settlement.tilemap.floor.Floors;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.misc;
using util.gui.misc;
using util.info;
using util.text;

namespace settlement.stats.colls
{
    public class StatsEnv : StatCollection
    {
        public readonly STAT BUILDING_PREF;
        public readonly STAT ROAD_PREF;
        public readonly STAT POOL_PREF;
        public readonly STAT CLIMATE;
        public readonly STAT PATHOGENS;
        public readonly STAT OTHERS;
        public readonly STAT CANNIBALISM;
        public readonly STAT CANNIBALISM_PREF;
        public readonly STAT UNBURRIED;
        public readonly STAT ACCESS_ROAD;

        private static readonly CharSequence ¤¤name = "Environment";
        private static readonly CharSequence ¤¤desc = "External factors";
        private static readonly CharSequence ¤¤exposure = "Exposure";
        private static readonly CharSequence ¤¤pref = "Pref.";
        static
        {
            D.ts(typeof(StatsEnv));
        }

        public StatsEnv(StatsInit init) : base(init, "ENVIRONMENT", ¤¤name, ¤¤desc)
        {
            ACCESS_ROAD = new STATData("ROAD_ACCESS", init, init.count.new DataBit("ENV_ROADA"));
            ACCESS_ROAD.info().icon = UI.icons().m.wheel;
            ROAD_PREF = new STATData("ROAD_PREF", init, init.count.new DataByte("ROAD_PREF"));
            ROAD_PREF.info().icon = UI.icons().m.wheel.twin(UI.icons().m.expand);
            BUILDING_PREF = new STATData("BUILDING_PREF", init, init.count.new DataNibble("BUILDING_PREF"));
            BUILDING_PREF.info().icon = UI.icons().m.building;
            POOL_PREF = new STATData("POOL_PREF", init, init.count.new DataNibble("POOL_PREF"));
            POOL_PREF.info().icon = UI.icons().m.water;
            init.onArrivalStats.add(ACCESS_ROAD);
            init.onArrivalStats.add(ROAD_PREF);
            init.onArrivalStats.add(BUILDING_PREF);
            init.onArrivalStats.add(POOL_PREF);

            CLIMATE = new STATFake("CLIMATE", init)
            {
                protected override double getDD(HCLASS s, Race r, int daysBack)
                {
                    if (r == null)
                    {
                        double m = 0;
                        foreach (Race rr in RACES.all())
                        {
                            m += rr.population().climate(SETT.ENV().climate()) * STATS.POP().POP.data(s).get(rr, daysBack);
                        }
                        double p = STATS.POP().POP.data(s).get(null, daysBack);
                        if (p == 0)
                            return m > 0 ? 1 : 0;
                        return m / p;
                    }
                    return r.population().climate(SETT.ENV().climate());
                }
            };
            CLIMATE.standing = new StatStanding(CLIMATE, 1.0);
            CLIMATE.info().setMatters(true, false);
            CLIMATE.info().icon = UI.icons().s.heat;

            PATHOGENS = new STATData("PATHOGENS", init, init.count.new DataNibble("PATHOGENS"))
            {
                public override void hover(GUI_BOX text, HCLASS cl, Race type)
                {
                    GBox b = (GBox)text;

                    b.textLL(¤¤exposure);
                    b.tab(6);
                    b.add(GFORMAT.percInv(b.text(), data(cl).getD(type)));
                    b.NL();

                    base.hover(text, cl, type);
                }
            };
            PATHOGENS.info().icon = UI.icons().m.disease;

            OTHERS = new STATFake("OTHERS", init)
            {
                protected override double getDD(HCLASS s, Race r, int daysBack)
                {
                    if (r == null)
                    {
                        double p = 0;
                        for (int ri = 0; ri < RACES.all().size(); ri++)
                        {
                            p += getDD(s, RACES.all().get(ri), daysBack) * STATS.POP().POP.data(s).get(RACES.all().get(ri), daysBack);
                        }
                        if (p == 0)
                            return 0;
                        return p / STATS.POP().POP.data(s).get(null, daysBack);
                    }

                    double pop = STATS.POP().POP.data(s).get(r, daysBack);
                    if (pop == 0)
                        return 1.0;
                    pop = 0;
                    double tot = 0;
                    foreach (Race rr in RACES.all())
                    {
                        double p = STATS.POP().POP.data(s).get(rr, daysBack);
                        pop += p;
                        tot += p * r.pref().race(rr);
                    }
                    if (pop == 0)
                        return 1;
                    tot /= pop;
                    return CLAMP.d(tot, 0, 1);
                }
            };
            OTHERS.standing = new StatStanding(OTHERS, 1.0);
            OTHERS.info().setMatters(true, false);
            OTHERS.info().icon = UI.icons().m.descrimination;

            CANNIBALISM = new STATFakeData("CANNIBALISM", init)
            {
                protected override double getDD(HCLASS cl, Race race)
                {
                    double d = 0;
                    foreach (Race r in RACES.all())
                    {
                        d += SETT.ROOMS().CANNIBAL.cannibalism(r);
                    }
                    return CLAMP.d(d, 0, 1);
                }
            };
            CANNIBALISM.info().setMatters(true, false);
            CANNIBALISM.info().icon = UI.icons().s.death;

            CANNIBALISM_PREF = new STATFakeData("CANNIBALISM_PREF", init)
            {
                protected override double getDD(HCLASS cl, Race race)
                {
                    double d = 0;
                    double tot = 0;
                    foreach (Race r in RACES.all())
                    {
                        tot += 1.0 - race.pref().race(r);
                        d += race.pref().race(r) * SETT.ROOMS().CANNIBAL.cannibalism(r);
                    }
                    return tot > 0 ? d / tot : 0;
                }

                public override void hover(GUI_BOX text, HCLASS cl, Race type)
                {
                    GBox b = (GBox)text;

                    b.textLL(¤¤exposure);
                    b.tab(6);
                    b.add(GFORMAT.percInv(b.text(), data(cl).getD(type)));
                    b.NL();

                    base.hover(text, cl, type);
                }
            };
            CANNIBALISM_PREF.info().setMatters(true, false);
            CANNIBALISM_PREF.info().icon = UI.icons().s.death;

            UNBURRIED = new STATData("UNBURRIED", init, init.count.new DataNibble("UNBURRIED"))
            {
                protected override double getDD(HCLASS s, Race r, int daysBack)
                {
                    if (r == null)
                    {
                        double m = 0;
                        foreach (Race rr in RACES.all())
                        {
                            m += rr.population().unburried(SETT.ENV().unburried()) * STATS.POP().POP.data(s).get(rr, daysBack);
                        }
                        double p = STATS.POP().POP.data(s).get(null, daysBack);
                        if (p == 0)
                            return m > 0 ? 1 : 0;
                        return m / p;
                    }
                    return r.population().unburried(SETT.ENV().unburried());
                }
            };
            UNBURRIED.standing = new StatStanding(UNBURRIED, 1.0);
            UNBURRIED.info().setMatters(true, false);
            UNBURRIED.info().icon = UI.icons().m.bone;

            updater = new Updater(this);
            init.onUpdateStats.add(updater);
        }

        private readonly Updater updater;

        private class Updater : IUpdate
        {
            private readonly StatsEnv statsEnv;

            public Updater(StatsEnv statsEnv)
            {
                this.statsEnv = statsEnv;
            }

            public void update()
            {
                foreach (Humanoid h in SETT.ENTITY().HUMANOID.all)
                {
                    Indu i = h.indu();

                    // Update BUILDING_PREF
                    double res = h.race().pref().structure(BUILDING_PREFS.get(h.tc().x(), h.tc().y()));
                    foreach (DIR d in DIR.ORTHO)
                    {
                        res += h.race().pref().structure(BUILDING_PREFS.get(h.tc().x() + d.x(), h.tc().y() + d.y()));
                    }
                    res /= 5;

                    int d = (int)Math.Ceiling((0x0F * res));
                    int n = statsEnv.BUILDING_PREF.indu().get(h.indu());

                    if (d > n * 2)
                    {
                        statsEnv.BUILDING_PREF.indu().inc(i, 2);
                    }
                    else if (d > n)
                    {
                        statsEnv.BUILDING_PREF.indu().inc(i, 1);
                    }
                    else if (d < n && (SETT.CORE().ticks & 0x07) == 0)
                    {
                        statsEnv.BUILDING_PREF.indu().inc(i, -1);
                    }

                    // Update ROAD_PREF and ACCESS_ROAD
                    Room r = SETT.ROOMS().map.get(h.physics.tileC());
                    if (r == null)
                    {
                        int current = statsEnv.ROAD_PREF.indu().get(i);
                        int tar = 0;

                        double deg = 1 - SETT.FLOOR().degrade.get(h.tc().x(), h.tc().y());

                        Floor f = SETT.FLOOR().getter.get(h.physics.tileC());
                        if (f != null && f.isRoad)
                        {
                            statsEnv.PATHOGENS.indu().inc(i, -1);
                            tar = (int)Math.Ceiling(deg * 255 * f.pref(h.race()));
                            statsEnv.ACCESS_ROAD.indu().set(i, deg > 0.5 ? 1 : 0);
                        }
                        else
                        {
                            statsEnv.ACCESS_ROAD.indu().set(i, 0);
                        }

                        if (tar > current)
                        {
                            current += 128;
                            current = CLAMP.i(current, 0, tar);
                        }
                        else if (tar < current)
                        {
                            current -= 48;
                            current = CLAMP.i(current, tar, 255);
                        }
                        statsEnv.ROAD_PREF.indu().set(i, current);
                    }
                    else
                    {
                        if (r.blueprint() is ROOM_POOL p)
                        {
                            double d = h.race().pref().pool(p);
                            statsEnv.POOL_PREF.indu().setD(i, d);
                        }

                        if (SETT.ROOMS().fData.item.get(h.tc()) == null && SETT.FLOOR().getter.get(h.physics.tileC()) == null)
                        {
                            statsEnv.PATHOGENS.indu().setD(i, SETT.GROUND().MOISTURE_BASE.get(h.tc()));
                        }
                    }
                }
            }
        }
    }
}