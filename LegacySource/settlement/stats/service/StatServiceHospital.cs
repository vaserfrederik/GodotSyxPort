using System;
using System.Collections.Generic;
using settlement.stats.service;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.health.hospital;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;

public sealed class StatServiceHospital : StatServiceImp
{
    private readonly STAT stat;

    private StatServiceHospital(LISTE<StatServiceImp> all, ROOM_HOSPITAL ho, StatsInit init) : base(ho.key, all, init, ho.info.name, ho.info.desc, ho.icon, null)
    {
        stat = new STATFakeData(ho.key, ho.key + "D", init, new StatInfo(ho.info.name, ho.info.desc))
        {
            public override void hover(GUI_BOX text, HCLASS cl, Race type)
            {
                GBox b = (GBox)text;
                b.textLL(SETT.ROOMS().HOSPITAL.employment().title);
                b.NL();
                b.textL(Dic.¤¤Employees);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), SETT.ROOMS().HOSPITAL.employment().employed()));
                b.NL();
                b.textL(Dic.¤¤Target);
                b.tab(6);
                b.add(GFORMAT.i(b.text(), (int)Math.Ceiling((POP.tot(null, null) + 1) / 75.0)));
                b.NL();
                b.textL(Dic.¤¤Access);
                b.tab(6);
                b.add(GFORMAT.bool(b.text(), permission().is(HCLASS_RACE.clP(type, cl))));
                b.NL();
                b.textL(Dic.¤¤Value);
                b.tab(6);
                b.add(GFORMAT.perc(b.text(), CLAMP.d(100.0 * SETT.ROOMS().HOSPITAL.employment().employed() / (POP.tot(null, null) + 1), 0, 1)));
                b.NL();

                base.hover(text, cl, type);
            }

            public override void hover(GUI_BOX text, Induvidual indu)
            {
                hover(text, indu.clas(), indu.race());
            }

            protected override double getDD(HCLASS cl, Race r)
            {
                if (!permission().is(HCLASS_RACE.clP(r, cl)))
                    return 0;
                double e = SETT.ROOMS().HOSPITAL.employment().employed();
                return 75.0 * e / (1.0 + POP.tot(null, null));
            }
        };
        stat.info().icon = SETT.ROOMS().HOSPITAL.icon.resized(Icon.S);
    }

    public override bool access(Humanoid h)
    {
        return stat.indu().getD(h.indu()) >= 1;
    }

    public override STAT total()
    {
        return stat;
    }

    public override void clearAccess(Induvidual i)
    {
    }

    public override void cheatSetTotal(Induvidual i, double tot)
    {
    }
}