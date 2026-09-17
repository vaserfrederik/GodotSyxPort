using System;
using System.Collections.Generic;
using settlement.stats.service;
using game;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.room.main;
using settlement.room.service.module;
using settlement.stats;
using settlement.stats.stat;
using snake2d.util.gui;
using snake2d.util.misc;
using snake2d.util.sets;
using util.gui.misc;
using util.info;
using util.text;

namespace settlement.stats.service
{
    public sealed class StatServiceChild : StatServiceImp
    {
        private static readonly CharSequence ¤¤children = "Children (with access)";
        private static readonly CharSequence ¤¤access = "Child Access";

        static StatServiceChild()
        {
            D.ts(typeof(StatServiceChild));
        }

        private readonly STATData access;
        private readonly STAT stat;

        public readonly RoomBlueprintIns blue;
        public readonly ROOM_SERVICE_HASER blueS;

        public StatServiceChild(LISTE<StatServiceImp> all, RoomBlueprintIns ho, StatsInit init)
            : base(ho.key, all, init, ho.info.name, ho.info.desc, ho.icon, null)
        {
            if (!(ho is ROOM_SERVICE_HASER))
                throw new RuntimeException(ho + " " + ho.key);

            blue = ho;
            blueS = (ROOM_SERVICE_HASER)ho;

            access = new STATData(null, ho.key + "access", init, init.count.new DataBit(ho.key + "access"));

            stat = new STATFakeData(ho.key, ho.key + "D", init, new StatInfo(ho.info.name, ho.info.desc))
            {
                public void hover(GUI_BOX text, HCLASS cl, Race type)
                {
                    GBox b = (GBox)text;

                    b.NL();
                    b.textL(¤¤children);
                    b.tab(6);
                    b.add(GFORMAT.i(b.text(), (int)children()));
                    b.NL();
                    b.textL(¤¤access);
                    b.tab(6);
                    b.add(GFORMAT.i(b.text(), blueS.service().total()));
                    b.NL();

                    b.textL(Dic.¤¤Value);
                    b.tab(6);
                    b.add(GFORMAT.perc(b.text(), getDD(cl, type)));
                    b.NL();

                    base.hover(text, cl, type);
                }

                public void hover(GUI_BOX text, Induvidual indu)
                {
                    hover(text, indu.clas(), indu.race());
                }

                double totChildren = 0;
                int updateI = -1;

                protected override double getDD(HCLASS cl, Race r)
                {
                    if (!permission().is(HCLASS_RACE.clP(r, cl)))
                        return 0;
                    double tot = blueS.service().total();
                    if (tot <= 0)
                        return 0;

                    return CLAMP.d(tot / children(), 0, 1);
                }

                private double children()
                {
                    if (updateI != GAME.updateI())
                    {
                        updateI = GAME.updateI();
                        totChildren = 0;
                        foreach (Race ra in RACES.all())
                        {
                            if (permission().is(HCLASS_RACE.clP(ra, HCLASSES.CITIZEN())))
                                totChildren += STATS.POP().pop(ra, HTYPES.CHILD());
                            if (permission().is(HCLASS_RACE.clP(ra, HCLASSES.SLAVE())))
                                totChildren += STATS.POP().pop(ra, HTYPES.CHILD_SLAVE());
                        }
                    }
                    return totChildren;
                }
            };
            stat.info().icon = ho.icon.resized(Icon.S);
        }

        public override bool accessRequest(Humanoid h)
        {
            if (h.indu().hType().parent() != h.indu().hType())
            {
                return permission().is(HCLASS_RACE.clP(h.race(), h.indu().hType().parent().parentClass()));
            }
            return false;
        }

        public override bool access(Humanoid h)
        {
            return stat.indu().getD(h.indu()) >= 0;
        }

        public void setAccess(Humanoid h, bool is)
        {
            access.indu().set(h.indu(), is ? 1 : 0);
        }

        public override STAT total()
        {
            return stat;
        }

        public override void clearAccess(Induvidual i)
        {
            access.indu().set(i, 0);
        }

        public override void cheatSetTotal(Induvidual i, double tot)
        {
            access.indu().set(i, 1);
        }
    }
}