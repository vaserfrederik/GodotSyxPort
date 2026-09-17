using System;
using System.Collections.Generic;
using game;
using init.settings;
using init.sprite.UI;
using init.type;
using settlement.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using view.main;
using view.sett.ui.room;
using view.ui.div;

namespace settlement.room.law.guard
{
    class Gui : UIRoomModuleImp<GuardInstance, ROOM_GUARD>
    {
        private static readonly string ¤¤guards = "Guard Force";
        private static readonly string ¤¤effDesc = "Degrade and employment determines the efficiency of a guard-post.";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_GUARD s) : base(s)
        {
        }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<GuardInstance> g, int x1, int y1)
        {
            section.addRelBody(16, DIR.S, new GStat
            {
                Update = text =>
                {
                    GFORMAT.perc(text, g.Get().eff());
                }
            }.hv(Dic.¤¤Efficiency, ¤¤effDesc));

            if (S.Get().developer)
            {
                section.addRelBody(16, DIR.S, new GStat
                {
                    Update = text =>
                    {
                        SETT.OVERLAY().envThing(SETT.ENV().map.GUARD);
                        GFORMAT.i(text, blueprint.reporter.crimes(g.Get()));
                        text.s().add(blueprint.reporter.crimes(null));
                    }
                }.hv("crimes"));

                section.addRelBody(16, DIR.S, new GStat
                {
                    Update = text =>
                    {
                        GFORMAT.i(text, blueprint.reporter.executions(g.Get()));
                        text.s().add(blueprint.reporter.executions(null));
                    }
                }.hv("executions"));
            }
        }

        protected override void appendMain(GGrid grid, GGrid text, GuiSection sExtra)
        {
            GuiSection s = new GuiSection();

            s.add(new GHeader(¤¤guards));

            s.addDown(2, new GStat
            {
                Update = text =>
                {
                    int a = 0;
                    foreach (Div d in GAME.ARMIES().player().divisions())
                    {
                        if (blueprint.activeDuty.is(d))
                        {
                            a++;
                        }
                    }
                    GFORMAT.i(text, a);
                }
            }.hh(Dic.¤¤Divisions, 120));

            s.add(new GStat
            {
                Update = text =>
                {
                    GFORMAT.i(text, STATS.POP().pop(HTYPES.GUARD()));
                }
            }.hh(Dic.¤¤Soldiers, 120), 0, s.body().y2() + 4);

            s.add(new GStat
            {
                Update = text =>
                {
                    GFORMAT.f0(text, blueprint.power.get());
                }
            }.hh(Dic.¤¤Power, 120), 0, s.body().y2() + 4);

            s.add(new GStat
            {
                Update = text =>
                {
                    GFORMAT.perc(text, SETT.ENV().map.GUARD.stat().data().getD(null));
                }
            }.hh(STATS.ENV().info.name, 120), 0, s.body().y2() + 4);

            s.add(new GStat
            {
                Update = text =>
                {
                    GFORMAT.f0(text, blueprint.averageUpgrade());
                }
            }.hh(Dic.¤¤Upgrade, 120), 0, s.body().y2() + 4);

            s.addRelBody(64, DIR.E, new GButt.ButtPanel(UI.icons().m.shield)
            {
                RENDEROBJ oo = new UIGuardDivSelector();

                protected override void clickA()
                {
                    VIEW.inters().popup.show(oo, this);
                }
            }.pad(8, 8));

            if (S.Get().developer)
            {
                s.addDown(2, blueprint.patrols.debugButt());
            }

            text.section.addRelBody(8, DIR.S, s);
        }

        protected override void hover(GBox box, GuardInstance i)
        {
            //SETT.OVERLAY().RadiusInter(blueprint, blueprint.finder);
        }
    }
}