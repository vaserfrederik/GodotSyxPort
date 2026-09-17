using System;
using System.Collections.Generic;
using game.boosting;
using game.boosting.BSourceInfo;
using game.boosting.BValue;
using game.boosting.BoostSpec;
using game.boosting.Booster;
using game.boosting.BoosterValue;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.environment.SettEnvMap;
using settlement.main;
using settlement.room.infra.monument;
using settlement.room.main;
using settlement.stats;
using settlement.stats.Induvidual;
using settlement.stats.STATS;
using settlement.stats.StatsInit;
using settlement.stats.StatsInit.StatUpdatableI;
using settlement.stats.standing;
using settlement.stats.stat;
using settlement.stats.stat.STAT;
using settlement.stats.stat.STATData;
using settlement.stats.stat.STATFake;
using settlement.stats.stat.StatCollection;
using settlement.stats.stat.StatInfo;
using settlement.stats.stat.StatBooster;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data.INT_O;
using util.gui.misc;
using util.info;
using util.text;

namespace settlement.stats.colls
{
    public class StatsAccess : StatCollection
    {
        private static readonly CharSequence ¤¤name = "Other";
        private static readonly CharSequence ¤¤desc = "Access to different environments.";

        private static readonly CharSequence ¤¤nameA = "Surroundings";
        private static readonly CharSequence ¤¤descA = "Access to different environments.";

        public readonly StatsMonuments MONUMENTS;

        public readonly StatsA ACCESS;
        private static readonly CharSequence ¤¤monumentName = "Decorations";
        private static readonly CharSequence ¤¤MonumentsD = "The value is based on access, upgrade and general decoration degrade in your city. The access can be higher than 100%, in which case it will compensate in places where there is no access.";
        private static readonly CharSequence ¤¤MonumentsDeg = "Degrade";

        static StatsAccess()
        {
            Dic.¤¤(¤¤name);
            Dic.¤¤(¤¤desc);
            Dic.¤¤(¤¤nameA);
            Dic.¤¤(¤¤descA);
            Dic.¤¤(¤¤monumentName);
            Dic.¤¤(¤¤MonumentsD);
            Dic.¤¤(¤¤MonumentsDeg);
        }

        public StatsAccess(StatsInit init) : base(init, "ACCESS", ¤¤name, ¤¤desc)
        {
            MONUMENTS = new StatsMonuments(init);
            ACCESS = new StatsA(init);
            Add(MONUMENTS);
            Add(ACCESS);
        }
    }

    public class StatsMonuments : StatCollection
    {
        public readonly STAT degrade;
        public readonly List<StatMonument> ALL;

        public StatsMonuments(StatsInit init) : base(init, "MONUMENTS", ¤¤monumentName, ¤¤MonumentsD)
        {
            degrade = new STATData(null, init, init.count.new DataCrumb("MON_DEGRADE"), null);
            ALL = new List<StatMonument>(SETT.ROOMS.MONUMENTS.all.size);
            ROOM_MONUMENT[] rr = new ROOM_MONUMENT[SETT.ROOMS.MONUMENTS.all.size];

            for (int i = 0; i < SETT.ROOMS.MONUMENTS.all.size; i++)
            {
                rr[SETT.ROOMS.MONUMENTS.all[i].monumentIndex] = SETT.ROOMS.MONUMENTS.all[i];
            }

            foreach (var m in rr)
            {
                ALL.Add(new StatMonument(m, init, degrade));
            }
        }

        public LIST<StatMonument> ALL()
        {
            return ALL;
        }
    }

    public class StatMonument : STATFake
    {
        public ROOM_MONUMENT m;

        public readonly STAT upgrade;
        public readonly STAT amount;
        public readonly STAT access;
        private readonly STAT degrade;

        public StatMonument(ROOM_MONUMENT m, StatsInit init, STAT degrade) : base(m.key, init, new StatInfo(m.info.name, m.info.names, ¤¤MonumentsD))
        {
            access = new STATData(null, init, init.count.new DataBit("ACCESS_" + m.key));
            amount = new STATData(null, init, init.count.new DataNibble("AMOUNT_" + m.key));
            upgrade = new STATData(null, init, init.count.new DataCrumb(m.key + "dupgrade"), null);
            this.degrade = degrade;
            this.m = m;
            info.icon = m.icon;
            info.setOpinion(m.opinion);
            standing = new StatStanding(this, 0, m.defaultStanding);
            info().setMatters(true, false);
            BOOSTING.connecter(new ACTION
            {
                exe = () =>
                {
                    foreach (BoostSpec t in m.boosts.all)
                    {
                        BValue v = StatBooster.make(this);
                        BSourceInfo inInfo = new BSourceInfo(info.names, m.icon.small);
                        Booster b = new BoosterValue(v, inInfo, t.booster.from(), t.booster.to(), t.booster.isMul);

                        boosters.push(b, t.boostable);
                    }
                }
            });
        }

        protected override double getDD(HCLASS s, Race r, int daysBack)
        {
            double acc = access.data(s).get(r, daysBack);
            if (acc == 0)
                return 0;

            double d = amount.data(s).get(r, daysBack) / (acc * m.maxEnv());
            d = CLAMP.d(d, 0, 1);

            d *= 1.0 - degrade.data(s).get(r, daysBack) / acc;

            if (m.upgrades().max() > 0)
            {
                d *= (double)(upgrade.data(s).get(r) + acc) / (acc * (1 + m.upgrades().max()));
            }

            return d * access.data(s).getD(r, daysBack);
        }

        public override double induGet(Induvidual t)
        {
            double a = (double)amount.indu().get(t) / m.maxEnv();
            a = CLAMP.d(a, 0, 1);
            a *= 1 - degrade.indu().get(t);
            return a;
        }

        public override int dataDivider()
        {
            return 128;
        }

        public override void hover(GUI_BOX text, HCLASS cl, Race type)
        {
            GBox b = (GBox)text;
            b.text(m.info.desc);
            b.sep();
            double acc = access.data(cl).get(type);

            b.textL(Dic.¤¤Access);
            b.tab(6);
            double d = 0;
            if (acc > 0)
                d = amount.data(cl).get(type) / (acc * m.maxEnv());
            b.add(GFORMAT.perc(b.text(), d));
            b.NL();

            b.textL(¤¤MonumentsDeg);
            b.tab(6);
            d = 0;
            if (acc > 0)
                d = degrade.data(cl).get(type) / acc;
            b.add(GFORMAT.percInv(b.text(), d));
            b.NL();

            if (m.upgrades().max() > 0)
            {
                b.textL(Dic.¤¤Upgrade);
                b.tab(6);
                b.add(GFORMAT.perc(b.text(), (double)(upgrade.data(cl).get(type) + acc) / (acc * (1 + m.upgrades().max()))));
                b.NL(4);
            }

            base.hover(text, cl, type);
        }

        public override void hover(GUI_BOX text, Induvidual indu)
        {
            GBox b = (GBox)text;
            b.text(m.info.desc);
            b.sep();

            b.textL(Dic.¤¤Access);
            b.tab(7);
            double d = (double)(amount.indu().get(indu)) / m.maxEnv();
            b.add(GFORMAT.perc(b.text(), d));
            b.NL();

            b.textL(¤¤MonumentsDeg);
            b.tab(7);
            b.add(GFORMAT.perc(b.text(), degrade.indu().getD(indu)));
            b.NL();

            if (m.upgrades().max() > 0)
            {
                b.textL(Dic.¤¤Upgrade);
                b.tab(6);
                b.add(GFORMAT.perc(b.text(), (double)upgrade.indu().get(indu) / m.upgrades().max()));
                b.NL(4);
            }

            base.hover(text, indu);
        }
    }

    public class StatsA : StatCollection
    {
        public StatsA(StatsInit init) : base(init, "ACCESS", ¤¤nameA, ¤¤descA)
        {
            foreach (Type t in Sett.ENVIRONMENT.SHAPE.all)
            {
                Add(new Env(init, t));
            }
        }
    }

    public class Env
    {
        private readonly Type t;
        public readonly STAT stat;

        public Env(StatsInit init, Type t)
        {
            this.t = t;
            string key = "SHAPE_" + t.key;

            STATData d = new STATData(key, init, init.count.new DataCrumb("D_" + key), null);
            d.info().icon = t.icon;
            init.onArrivalStats.add(d);

            this.stat = d;
        }
    }
}