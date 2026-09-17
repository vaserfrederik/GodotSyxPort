using System;
using System.Collections.Generic;
using System.Linq;
using game;
using game.battle.div;
using game.faction;
using init.sprite;
using settlement.main;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data;
using util.gui.common;
using util.gui.misc;
using util.text;
using view.keyboard;
using view.main;
using view.ui.div;
using world.army;
using world.entity.army;

namespace view.sett.ui.army
{
    public sealed class Actions : GuiSection
    {
        private static readonly string ¤¤NoValid = "¤The selected division are already attached to a world army. You must recall them first.";
        private static readonly string ¤¤Recall = "¤Recall";
        private static readonly string ¤¤RecallD = "¤Recall these divisions from its world armies and have them return to the city. It will take a few days.";
        private static readonly string ¤¤RecallProblem = "¤No divisions are selected that are currently attached to a world army.";
        private static readonly string ¤¤SendOut = "¤Send Out";
        private static readonly string ¤¤SendOutD = "¤Send this division to join an army on the world map. These soldiers will then have to be supplied through your army depots.";
        private static readonly string ¤¤NotTrained = "¤Some of the soldiers are not fully trained to specification yet, and will continue to train before they join an army.";
        private static readonly string ¤¤NoArmies = "¤There are no armies to send this division to. Recruit one on the world map.";
        private static readonly string ¤¤NoDivs = "No divisions are selected.";
        private static readonly string ¤¤DisbandD = "Are you sure you wish to disband {0} divisions?";
        private static readonly string ¤¤Closed = "Our city is closed, we can not leave.";
        private static readonly string ¤¤Transfer = "Soldiers of this division is still on route back to our city. We must wait until they return";

        static Actions()
        {
            D.ts(typeof(Actions));
        }

        public Actions(ArrayList<Div> list)
        {
            int width = 170;
            int height = 32;
            GButt.ButtPanel c;

            GuiSection f = new GuiSection();

            f.addRightC(0, new GButt.Glow(SPRITES.icons().m.questionmark)
            {
                protected override void clickA()
                {
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    Str tmp = Str.TMP.clear().add(Dic.¤¤Unitinfo);
                    tmp.insert(0, KEYS.MAIN().UNDO.repr());
                    tmp.insert(1, KEYS.MAIN().MOD.repr());
                    text.text(tmp);
                }
            });

            c = new GButt.ButtPanel(Dic.¤¤Create)
            {
                protected override void clickA()
                {
                    Div n = GAME.ARMIES().player().getNextEmptyOrdered();
                    if (n == null)
                        return;
                    n.info.raceSet(FACTIONS.player().race());
                    n.info.menSet(50);
                    foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
                        e.targetSet(n, 0);
                    foreach (StatTraining e in STATS.BATTLE().TRAINING_ALL)
                        n.info.trainingSet(e, 0);
                    clicked = null;
                }

                protected override void renAction()
                {
                    activeSet(false);
                    foreach (Div d in GAME.ARMIES().player().divisions())
                    {
                        if (d.info.men() == 0)
                        {
                            activeSet(true);
                            return;
                        }
                    }
                }
            };
            c.icon(UI.icons().m.plus);
            c.setDim(width, height);
            f.addRightC(0, c);

            c = new GButt.ButtPanel(Dic.¤¤Edit)
            {
                private readonly Edit edit = new Edit();

                protected override void clickA()
                {
                    for (int di = 0; di < list.size(); di++)
                    {
                        if (AD.cityDivs().attachedArmy(list.get(di)) != null)
                        {
                            list.remove(di);
                            di--;
                        }
                    }

                    if (list.size() > 0)
                        VIEW.inters().edit.start(edit, list.get(0));
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    base.hoverInfoGet(text);
                    text.addLine("Edit selected division(s)");
                }
            };
            c.icon(UI.icons().m.edit);
            c.setDim(width, height);
            f.addRightC(0, c);

            c = new GButt.ButtPanel(Dic.¤¤Disband)
            {
                final ACTION a = new ACTION()
                {
                    public override void exe()
                    {
                        foreach (Div div in list)
                        {
                            if (AD.cityDivs().attachedArmy(div) == null)
                                div.info.menSet(0);
                        }
                    }
                };

                protected override void clickA()
                {
                    VIEW.inters().yesNo.activate(Str.TMP.clear().add(¤¤DisbandD).insert(0, list.size()), a, ACTION.NOP, true);
                }

                protected override void renAction()
                {
                    isActive = false;
                    foreach (Div div in list)
                    {
                        if (AD.cityDivs().attachedArmy(div) == null)
                        {
                            isActive = true;
                            break;
                        }
                    }
                }
            };
            c.icon(UI.icons().m.cancel);
            c.setDim(width, height);
            f.addRightC(0, c);

            addRelBody(0, DIR.N, f);
        }

        static CharSequence sendProblem(LIST<Div> divs)
        {
            if (divs.size() == 0)
                return ¤¤NoDivs;
            if (SETT.ENTRY().isClosed())
                return ¤¤Closed;

            if (AD.army(FACTIONS.player()).all().size() <= 0)
                return ¤¤NoArmies;
            for (int i = 0; i < divs.size(); i++)
            {
                Div div = divs.get(i);
                if (AD.cityDivs().attachedArmy(div) == null && UIDivCardWorld.supplyError(div) == null)
                {
                    if (AD.cityDivs().get(div).men() > 0)
                        return ¤¤Transfer;
                    return null;
                }
            }
            for (int i = 0; i < divs.size(); i++)
            {
                Div div = divs.get(i);
                if (UIDivCardWorld.supplyError(div) != null)
                    return UIDivCardWorld.supplyError(div);
            }
            return ¤¤NoValid;
        }

        static void hoverSendOutProblem(LIST<Div> divs, GUI_BOX box)
        {
            GBox b = (GBox)box;
            CharSequence h = sendProblem(divs);
            if (h != null)
            {
                b.error(h);
                b.NL(4);
            }

            if (!SETT.BATTLE().info.sendOutWithoutTraining())
            {
                for (int i = 0; i < divs.size(); i++)
                {
                    Div div = divs.get(i);
                    if (VIEW.UI().div.settCivic.needsTraining(div) > 0)
                    {
                        b.add(b.text().warnify().add(¤¤NotTrained));
                        b.NL(4);
                        break;
                    }
                }
            }

            UIDivCardWorld.hoverSendOut(divs, box);
        }
    }
}