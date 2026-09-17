using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using util.gui.misc;
using view.ui.div;
using world.army;

namespace view.world.ui.army
{
    class ArmyRecruit : GuiSection
    {
        static readonly CharSequence ¤¤Full = "¤Army unit limit reached!";
        static readonly CharSequence ¤¤NoMapnpower = "¤Insufficient Conscripts available of selected race!";
        static readonly CharSequence ¤¤Time = "¤The amount of days it will take to train this division to specification.";

        static ArmyRecruit()
        {
            D.ts(typeof(ArmyRecruit));
        }

        private readonly UIDivEditor editor = new UIDivEditor(0.75f, false, false, false, RACES.playable());

        public ArmyRecruit()
        {
            add(new GStat
            {
                update = text =>
                {
                    GFORMAT.i(text, AD.conscripts().available(editor.div().race()).get(FACTIONS.player()));
                }
            }.hh(Dic.¤¤Conscripts));

            addRelBody(4, DIR.S, editor);

            {
                GuiSection row = new GuiSection();

                GStat ss = new GStat
                {
                    update = text =>
                    {
                        GFORMAT.i(text, (long)ti());
                    },
                    hoverInfoGet = b => b.text(¤¤Time)
                };

                double ti()
                {
                    int am = WDivRegional.DAYS_TO_TRAIN;

                    foreach (StatTraining t in STATS.BATTLE().TRAINING_ALL)
                    {
                        am += WDivRegional.trainingDays(t, editor.div().training(t), FACTIONS.player());
                    }

                    return am;
                }

                row.addRightC(48, ss.hh(SPRITES.icons().m.time));

                GButt b = new GButt.ButtPanel(Dic.¤¤Recruit)
                {
                    clickA = () =>
                    {
                        if (Army.army.divs().canAdd())
                        {
                            WDivRegional d = AD.regional().create(editor.div().race(), (double)editor.div().men() / Config.battle().MEN_PER_DIVISION, Army.army);
                            d.bannerSet(editor.div().bannerI());

                            foreach (ResSupply s in RESOURCES.SUP().ALL)
                            {
                                if (s.health <= 0)
                                    continue;
                                int am = s.amount(editor.div().race(), editor.div().men());
                                am = CLAMP.i(am, 0, SETT.ROOMS().STOCKPILE.tally().amountReservable.get(s.resource));
                                if (am > 0)
                                {
                                    s.resource.remove(am, RTYPE.ARMY_SUPPLY);
                                    AD.supplies().get(s).current().inc(Army.army, am);
                                }
                            }

                            foreach (StatTraining s in STATS.BATTLE().TRAINING_ALL)
                            {
                                d.target.trainingSet(s, editor.div().training(s));
                            }

                            foreach (EquipBattle s in STATS.EQUIP().BATTLE_ALL())
                            {
                                d.target.equipSet(s, editor.div().equip(s));
                            }
                        }
                    },
                    renAction = () => activeSet(problem() == null),
                    hoverInfoGet = text =>
                    {
                        li.ClearSloppy();
                        li.Add(editor.div());
                        UIDivCardWorld.hoverSendOut(li, text);
                    }
                };

                row.addRightC(48, b);

                addRelBody(8, DIR.S, row);
            }

            add(new GStat
            {
                update = text =>
                {
                    text.setMaxWidth(body().width());
                    text.setMultipleLines(true);
                    CharSequence p = problem();
                    if (p != null)
                        text.errorify().add(p);
                    else
                    {
                        p = warning();
                        if (p != null)
                            text.warnify().add(p);
                    }
                }
            }, body().x1(), body().y2() + 8);

            body().incrH(UI.FONT().S.height() * 4);
        }

        private CharSequence problem()
        {
            if (!Army.army.divs().canAdd())
            {
                return ¤¤Full;
            }

            if (editor.div().men() > AD.conscripts().available(editor.div().race()).get(FACTIONS.player()))
            {
                return ¤¤NoMapnpower;
            }

            if (UIDivCardWorld.supplyError(editor.div()) != null)
                return UIDivCardWorld.supplyError(editor.div());

            return null;
        }

        private CharSequence warning()
        {
            if (editor.div().men() > AD.conscripts().available(editor.div().race()).get(FACTIONS.player()))
            {
                return ¤¤NoMapnpower;
            }

            return null;
        }
    }
}