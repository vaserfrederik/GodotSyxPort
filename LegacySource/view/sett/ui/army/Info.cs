using System;
using System.Collections.Generic;
using game.battle.div;
using init.sprite;
using settlement.main;
using settlement.room.military.training;
using settlement.stats;
using settlement.stats.equip;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using util.gui.misc;
using util.gui.panel;
using util.gui.slider;
using util.info;
using util.text;
using world.army;

namespace view.sett.ui.army
{
    class Info : GuiSection
    {
        private static readonly string ¤¤RecruitD = "¤The amount of men currently training to be able to join a division.";
        private static readonly string ¤¤SoldierD = "The amount of soldiers that are ready to be deployed in your city.";
        private static readonly string ¤¤Campaigning = "¤Campaigning";
        private static readonly string ¤¤CampaigningD = "¤Soldiers that are out campaigning.";

        static Info()
        {
            D.ts(typeof(Info));
        }

        public Info()
        {
            int gi = 0;
            int wi = 100;
            int hi = 58;
            int cols = 6;
            DIR al = DIR.N;

            addGridD(new GStat
            {
                Update = text =>
                {
                    GFORMAT.iBig(text, (int)AD.cityDivs().total());
                },
                HoverInfoGet = b =>
                {
                    b.title(¤¤Campaigning);
                    b.text(¤¤CampaigningD);
                    b.NL();
                    b.add(GFORMAT.iBig(b.text(), (int)AD.cityDivs().total()));
                }
            }.hv(SPRITES.icons().m.arrow_left), gi++, cols, wi, hi, al);

            addGridD(new GStat
            {
                Update = text =>
                {
                    GFORMAT.iofk(text, (int)STATS.BATTLE().DIV.stat().data().get(null, 0), SETT.BATTLE().info.targetMen());
                },
                HoverInfoGet = b =>
                {
                    b.title(Dic.¤¤Soldiers);
                    b.text(¤¤SoldierD);
                    b.NL();
                    b.add(GFORMAT.iBig(b.text(), (int)STATS.BATTLE().DIV.stat().data().get(null, 0)));
                    b.NL(4);

                    b.textLL(Dic.¤¤Recruits);
                    b.add(GFORMAT.iBig(b.text(), (int)STATS.BATTLE().RECRUIT.stat().data().get(null, 0)));
                    b.NL();
                    b.text(¤¤RecruitD);
                }
            }.hv(SPRITES.icons().m.sword), gi++, cols, wi, hi, al);

            foreach (ROOM_M_TRAINER ro in ROOM_M_TRAINER.ALL())
            {
                addGridD(new GStat
                {
                    Update = text =>
                    {
                        GFORMAT.iofk(text, ro.employment().employed(), ro.employment().neededWorkers());
                    },
                    HoverInfoGet = b =>
                    {
                        b.title(ro.tInfo.name);
                        b.text(ro.tInfo.desc);
                        b.NL(4);
                        b.textLL(ro.info.names);
                        b.add(GFORMAT.iofk(b.text(), ro.employment().employed(), ro.employment().neededWorkers()));

                        b.NL(8);
                        b.NL();

                        ro.boosters.hover(b, 1.0, -1);
                    }
                }.hv(ro.icon), gi++, cols, wi, hi, al);
            }

            foreach (EquipBattle e in STATS.EQUIP().BATTLE_MELEE())
            {
                GGauge g = new GGauge(40, 16, GMeter.C_REDGREEN)
                {
                    GetD = () =>
                    {
                        int needs = 0;
                        double has = SETT.ROOMS().STOCKPILE.tally().amountTotal(e.resource());
                        foreach (Div d in GAME.ARMIES().player().divisions())
                        {
                            has += e.stat().div().get(d);
                            needs += d.info.men() * e.target(d);
                        }
                        return CLAMP.d(has / needs, 0, 1);
                    }
                };
                addGridD(new CLICKABLE.Pair(new RENDEROBJ.Sprite(e.resource.icon()), new RENDEROBJ.Sprite(g), DIR.S, 4)
                {
                    HoverInfoGet = text =>
                    {
                        GBox b = (GBox)text;
                        b.title(e.stat().info().name);
                        b.text(e.stat().info().desc);
                        b.NL(4);
                        int needs = 0;
                        int has = SETT.ROOMS().STOCKPILE.tally().amountTotal(e.resource());
                        foreach (Div d in GAME.ARMIES().player().divisions())
                        {
                            has += e.stat().div().get(d);
                            needs += d.info.men() * e.target(d);
                        }
                        b.add(GFORMAT.iofkInv(b.text(), has, needs));
                        b.NL(4);
                        b.textLL(Dic.¤¤Boosts);
                        b.NL(4);
                        e.stat().boosters.hover(text, 1.0, -1);
                    }
                }, gi++, cols, wi, hi, al);
            }

            foreach (EquipRange e in STATS.EQUIP().RANGED())
            {
                GGauge g = new GGauge(48, 16, GMeter.C_REDGREEN)
                {
                    GetD = () =>
                    {
                        int needs = 0;
                        double has = SETT.ROOMS().STOCKPILE.tally().amountTotal(e.resource());
                        foreach (Div d in GAME.ARMIES().player().divisions())
                        {
                            has += e.stat().div().get(d);
                            needs += d.info.men() * e.target(d);
                        }
                        return CLAMP.d(has / needs, 0, 1);
                    }
                };

                addGridD(new CLICKABLE.Pair(new RENDEROBJ.Sprite(e.resource.icon()), new RENDEROBJ.Sprite(g), DIR.S, 4)
                {
                    HoverInfoGet = text =>
                    {
                        GBox b = (GBox)text;
                        b.title(e.stat().info().name);
                        b.text(e.stat().info().desc);
                        b.NL(4);
                        int needs = 0;
                        int has = SETT.ROOMS().STOCKPILE.tally().amountTotal(e.resource());
                        foreach (Div d in GAME.ARMIES().player().divisions())
                        {
                            has += e.stat().div().get(d);
                            needs += d.info.men() * e.target(d);
                        }
                        b.add(GFORMAT.iofkInv(b.text(), has, needs));
                        b.NL(4);
                        e.hover(text);
                    }
                }, gi++, cols, wi, hi, al);
            }

            addRelBody(6, DIR.S, GFrame.separator(body().width()));
        }
    }
}