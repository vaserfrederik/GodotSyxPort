using System;
using System.Collections.Generic;
using settlement.stats.service;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using settlement.stats.stat;
using settlement.stats.util;
using snake2d.util.gui;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;

public class StatServiceBench : StatServiceImp
{
    public readonly STAT access;
    public readonly STAT quality;
    public readonly STAT upgrade;
    public readonly STAT total;

    public StatServiceBench(LISTE<StatServiceImp> all, StatsInit init) : base("BENCH", all, init, SETT.ROOMS().BENCH.info.name, SETT.ROOMS().BENCH.info.desc, SETT.ROOMS().BENCH.icon, null)
    {
        access = new STATData(null, init, init.count.new DataBit("ACCESS_BENCH"), new StatInfo(Dic.¤¤Access, Dic.¤¤Access, ¤¤AcessDesc));
        quality = new STATData(null, init, init.count.new DataCrumb("QUALITY_BENCH"), new StatInfo(Dic.¤¤Quality, Dic.¤¤Quality, ¤¤QualityDesc));
        upgrade = new STATData(null, init, init.count.new DataNibble("UPGRADE_BENCH"), new StatInfo(Dic.¤¤Upgrade, Dic.¤¤Upgrade, ¤¤UpDesc));
        upgrade.info().setMatters(false, true);
        access.info().setMatters(false, false);
        quality.info().setMatters(false, false);

        init.onArrivalStats.add(access);
        init.onArrivalStats.add(quality);

        access.info().setMatters(false, true);
        quality.info().setMatters(false, true);

        init.onArrivalStats.add(access);

        StatInfo info = new StatInfo(SETT.ROOMS().BENCH.info.name, Dic.¤¤Access + ": " + SETT.ROOMS().BENCH.info.name);

        total = new STATFake("BENCH", init, info)
        {
            protected override double getDD(HCLASS s, Race r, int daysBack)
            {
                double a = access.data(s).getD(r, daysBack);
                if (a == 0)
                    return 0;
                double q = quality.data(s).getD(r, daysBack) / a;
                double u = upgrade.data(s).getD(r, daysBack) / a;
                return a * (0.2 + 0.8 * u) * (0.5 + 0.5 * q);
            }

            public override void hover(GUI_BOX text, HCLASS cl, Race race)
            {
                GBox b = (GBox)text;
                StatHoverer.hover(b, this);
                b.sep();
                b.textLL(access.info().name);
                b.add(GFORMAT.perc(b.text(), access.data(cl).getD(race)));
                b.NL().text(access.info().desc);
                b.NL(4);
                b.textLL(quality.info().name);
                b.add(GFORMAT.perc(b.text(), quality.data(cl).getD(race) / access.data(cl).getD(race)));
                b.NL().text(quality.info().desc);
                b.NL(4);
                b.textLL(upgrade.info().name);
                b.add(GFORMAT.perc(b.text(), upgrade.data(cl).getD(race) / access.data(cl).getD(race)));
                b.NL().text(upgrade.info().desc);
                b.NL(4);
                b.textLL(Dic.¤¤Total);
                b.add(GFORMAT.perc(b.text(), data(cl).getD(race)));

                b.sep();
                StatHoverer.hover(text, this, cl, race);
            }

            protected override double induGet(Induvidual t)
            {
                double a = access.indu().getD(t);
                if (a == 0)
                    return 0;
                double q = quality.indu().getD(t);
                double u = upgrade.indu().getD(t);

                return a * (0.2 + 0.8 * u) * (0.5 + 0.5 * q);
            }

            public override void hover(GUI_BOX text, Induvidual indu)
            {
                GBox b = (GBox)text;
                StatHoverer.hover(b, this);
                b.sep();
                b.textLL(access.info().name);
                b.add(GFORMAT.perc(b.text(), access.indu().getD(indu)));
                b.NL().text(access.info().desc);
                b.NL(4);
                b.textLL(quality.info().name);
                b.add(GFORMAT.perc(b.text(), quality.indu().getD(indu) / access.indu().getD(indu)));
                b.NL().text(quality.info().desc);
                b.NL(4);
                b.textLL(upgrade.info().name);
                b.add(GFORMAT.perc(b.text(), upgrade.indu().getD(indu) / access.indu().getD(indu)));
                b.NL().text(upgrade.info().desc);
                b.NL(4);
                b.textLL(Dic.¤¤Total);
                b.add(GFORMAT.perc(b.text(), indu().getD(indu)));

                b.sep();
                StatHoverer.hover(text, this, indu);
            }
        };

        total.info().icon = SETT.ROOMS().BENCH.icon.resized(Icon.S);
    }

    public void setAccess(Induvidual i, bool access, double quality, int upgrade)
    {
        this.access.indu().set(i, access ? 1 : 0);
        if (!access)
        {
            quality = 0;
            upgrade = -1;
        }

        this.quality.indu().setD(i, quality);

        this.upgrade.indu().setD(i, (upgrade + 1.0) / (SETT.ROOMS().BENCH.upgrades().max() + 1.0));
    }

    public override bool access(Humanoid h)
    {
        return this.access.indu().get(h.indu()) == 1;
    }

    public override STAT total()
    {
        return total;
    }

    public override void clearAccess(Induvidual i)
    {
        access.indu().set(i, 0);
        quality.indu().set(i, 0);
        upgrade.indu().set(i, 0);
    }

    public override void cheatSetTotal(Induvidual i, double tot)
    {
        access.indu().set(i, tot > 0 ? 1 : 0);
        quality.indu().setD(i, tot);
        upgrade.indu().set(i, 0);
    }
}