using System;
using System.Collections.Generic;
using game.boosting;
using game.boosting.BSourceInfo;
using game.boosting.BValue;
using game.boosting.BoostSpec;
using game.boosting.Booster;
using game.boosting.BoosterValue;
using init.race;
using init.race.bio;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.room.service.module;
using settlement.stats;
using settlement.stats.Induvidual;
using settlement.stats.STATS;
using settlement.stats.StatsInit;
using settlement.stats.standing;
using settlement.stats.stat;
using settlement.stats.stat.STATData;
using settlement.stats.stat.STATFake;
using settlement.stats.stat.StatInfo;
using settlement.stats.util;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;

namespace settlement.stats.service
{
    public sealed class StatServiceRoom : StatServiceImp
    {
        public const int TARGET_MAX = 16;
        private readonly STAT access;
        private readonly STAT quality;
        private readonly STAT proximity;
        private readonly STAT upgrade;
        private readonly STAT total;
        private readonly RoomServiceAccess room;

        public StatServiceRoom(LISTE<StatServiceImp> all, RoomServiceAccess service, StatsInit init)
            : base(service.room().key, all, init, service.room().info.name, service.room().info.desc, service.room().icon, service.need)
        {
            this.room = service;

            access = new STATData(null, init, init.count.new DataBit("SERVICEA_" + service().room().key), new StatInfo(¤¤Access, ¤¤Access, ¤¤AcessDesc));
            access.info().setMatters(false, true);
            quality = new STATData(null, init, init.count.new DataNibble("SERVICEQ_" + service().room().key), new StatInfo(¤¤Quality, ¤¤Quality, ¤¤QualityDesc));
            quality.info().setMatters(false, true);
            proximity = new STATData(null, init, init.count.new DataNibble("SERVICEP_" + service().room().key), new StatInfo(¤¤Distance, ¤¤Distance, ¤¤DistanceDesc));
            proximity.info().setMatters(false, true);
            upgrade = new STATData(null, init, init.count.new DataNibble("SERVICEUP_" + service().room().key), new StatInfo(Dic.¤¤Upgrade, Dic.¤¤Upgrade, ¤¤UpDesc));
            upgrade.info().setMatters(false, true);

            init.onArrivalStats.add(access);
            init.onArrivalStats.add(quality);
            init.onArrivalStats.add(proximity);
            init.onArrivalStats.add(upgrade);

            StatInfo info = new StatInfo(service.room().info.names, ¤¤TotalDesc);
            info.setOpinion(new Opinion().setMore(service.induMore));

            total = new STATFake(service().room().key, init, info)
            {
                protected override double getDD(HCLASS s, Race r, int daysBack)
                {
                    double a = access.data(s).getD(r, daysBack);
                    if (a == 0)
                        return 0;
                    double q = quality.data(s).getD(r, daysBack) / a;
                    double p = proximity.data(s).getD(r, daysBack) / a;
                    double u = upgrade.data(s).getD(r, daysBack) / a;
                    return a * (0.2 + 0.8 * u) * (0.2 + 0.8 * q) * (0.5 + 0.5 * p);
                }

                public override double induGet(Induvidual t)
                {
                    double a = access.indu().getD(t);
                    if (a == 0)
                        return 0;
                    double q = quality.indu().getD(t);
                    double p = proximity.indu().getD(t);
                    double u = upgrade.indu().getD(t);

                    return a * (0.2 + 0.8 * u) * (0.2 + 0.8 * q) * (0.5 + 0.5 * p);
                }

                public override void hover(GUI_BOX text, HCLASS cl, Race race)
                {
                    GBox b = (GBox)text;
                    StatHoverer.hover(b, this);
                    b.sep();
                    b.textLL(access().info().name);
                    b.add(GFORMAT.perc(b.text(), access().data(cl).getD(race)));
                    b.NL().text(access().info().desc);
                    b.NL(4);
                    b.textLL(proximity().info().name);
                    double p = proximity().data(cl).getD(race) / access().data(cl).getD(race);

                    b.add(GFORMAT.perc(b.text(), p));
                    b.NL().text(proximity().info().desc);
                    b.NL(4);
                    b.textLL(quality().info().name);
                    b.add(GFORMAT.perc(b.text(), CLAMP.d(quality().data(cl).getD(race) / access().data(cl).getD(race), 0, 1)));
                    b.NL().text(quality().info().desc);
                    b.NL(4);
                    b.textLL(upgrade().info().name);
                    b.add(GFORMAT.perc(b.text(), upgrade().data(cl).getD(race) / access().data(cl).getD(race)));
                    b.NL().text(upgrade().info().desc);
                    b.NL(4);
                    b.textLL(Dic.¤¤Total);
                    b.add(GFORMAT.perc(b.text(), data(cl).getD(race)));

                    b.sep();
                    StatHoverer.hover(text, this, cl, race);
                }

                public override void hover(GUI_BOX text, Induvidual indu)
                {
                    GBox b = (GBox)text;
                    StatHoverer.hover(b, this);
                    b.sep();
                    b.textLL(access().info().name);
                    b.add(GFORMAT.perc(b.text(), access().indu().getD(indu)));
                    b.NL().text(access().info().desc);
                    b.NL(4);
                    b.textLL(proximity().info().name);

                    b.add(GFORMAT.perc(b.text(), proximity().indu().getD(indu) / access().indu().getD(indu)));
                    b.NL().text(proximity().info().desc);
                    b.NL(4);
                    b.textLL(quality().info().name);
                    b.add(GFORMAT.perc(b.text(), quality().indu().getD(indu) / access().indu().getD(indu)));
                    b.NL().text(quality().info().desc);
                    b.NL(4);
                    b.textLL(upgrade().info().name);
                    b.add(GFORMAT.perc(b.text(), upgrade().indu().getD(indu) / access().indu().getD(indu)));
                    b.NL().text(upgrade().info().desc);
                    b.NL(4);
                    b.textLL(Dic.¤¤Total);
                    b.add(GFORMAT.perc(b.text(), indu().getD(indu)));

                    b.sep();
                    StatHoverer.hover(text, this, indu);
                }
            };
            total.standing = new StatStanding(total, 0, service().standingDef);
            total.info().icon = service.room().icon;
            BOOSTING.register(new ACTION(() =>
            {
                foreach (BoostSpec spec in BOOSTING.allBoosts())
                {
                    foreach (Booster booster in spec.allBoosters())
                    {
                        if (booster is BoosterValue)
                        {
                            ((BoosterValue)booster).setValue(1.0);
                        }
                    }
                }
            }));

            BOOSTING.register(new ACTION(() =>
            {
                foreach (BoostSpec spec in BOOSTING.allBoosts())
                {
                    foreach (Booster booster in spec.allBoosters())
                    {
                        if (booster is BoosterValue)
                        {
                            ((BoosterValue)booster).setValue(0.0);
                        }
                    }
                }
            }));

            BOOSTING.register(new ACTION(() =>
            {
                foreach (BoostSpec spec in BOOSTING.allBoosts())
                {
                    foreach (Booster booster in spec.allBoosters())
                    {
                        if (booster is BoosterValue)
                        {
                            ((BoosterValue)booster).setValue(0.5);
                        }
                    }
                }
            }));
        }

        protected double pdivider(HCLASS c, Race r, int daysback)
        {
            return STATS.POP().POP.data(c).get(r, daysback);
        }

        public void setAccess(Humanoid h, bool access, double quality, double proximity, int upgrade)
        {
            setAccess(h.indu(), access, quality, proximity, upgrade);
        }

        public void setAccess(Induvidual i, bool access, double quality, double proximity, int upgrade)
        {
            this.access.indu().set(i, access ? 1 : 0);
            if (!access)
            {
                quality = 0;
                proximity = 0;
                upgrade = -1;
            }

            this.quality.indu().setD(i, quality);
            this.proximity.indu().setD(i, proximity);

            this.upgrade.indu().setD(i, (upgrade + 1.0) / (room.room().upgrades().max() + 1.0));
        }

        public override void clearAccess(Induvidual i)
        {
            setAccess(i, false, 0, 0, 0);
        }

        public void setProximity(Humanoid h, double proximity)
        {
            Induvidual i = h.indu();
            this.proximity.indu().setD(i, proximity);
        }

        public RoomServiceAccess service()
        {
            return room;
        }

        public STAT access()
        {
            return access;
        }

        public STAT quality()
        {
            return quality;
        }

        public STAT upgrade()
        {
            return upgrade;
        }

        public STAT proximity()
        {
            return proximity;
        }

        public override STAT total()
        {
            return total;
        }

        public override void cheatSetTotal(Induvidual i, double tot)
        {
            access.indu().set(i, tot > 0 ? 1 : 0);
            quality.indu().setD(i, tot);
            upgrade.indu().setD(i, tot);
            proximity.indu().setD(i, tot);
        }
    }
}