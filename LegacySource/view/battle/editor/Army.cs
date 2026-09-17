using System;
using game;
using game.battle.util;
using game.faction;
using game.faction.npc;
using init.constant;
using init.race;
using init.sprite.UI;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.rnd;
using util.data;
using util.gui.misc;
using util.text;
using view.ui.div;
using world.region;

namespace view.battle.editor
{
    class Army : GuiSection
    {
        private readonly GETTER_IMP<ArmySide> current;
        static readonly CharSequence ¤¤name = "armies";
        private static readonly CharSequence ¤¤generate = "Generate";
        private static readonly CharSequence ¤¤add = "Add Unit";

        static Army()
        {
            D.ts(typeof(Army));
        }

        public Army(ArmySide player, ArmySide enemy)
        {
            current = new GETTER_IMP<ArmySide>(player);

            FACTIONS.player().setRace(RACES.playable()[0]);
            ((FactionNPC)FACTIONS.all()[1]).generate(RD.RACES().all.rnd(), false);

            {
                GuiSection s = new GuiSection();
                s.add(UI.icons().l.battle, 0, 0);
                s.addDownC(6, new GButt.ButtPanel(UI.icons().m.rotate)
                {
                    protected override void clickA()
                    {
                        double pow = 1000 + Config.battle().MEN_PER_ARMY * RND.rFloat() * RND.rFloat() * RND.rFloat() * GAME.battle().power.HIGH_POWER;

                        player.generate(pow);
                        enemy.generate(pow);

                        base.clickA();
                    }
                }.hoverInfoSet(¤¤generate));

                s.addRelBody(8, DIR.W, new ArmyFactionButt(FACTIONS.player(), player, current));
                s.addRelBody(8, DIR.E, new ArmyFactionButt(FACTIONS.all()[1], enemy, current));

                addRelBody(8, DIR.S, s);
                addRelBody(8, DIR.S, UI.decor().borderTop(800));
            }

            {
                UIDivEditor editor = new UIDivEditor(STATS.BATTLE().TRAINING_ALL.size(), true, true, false, RACES.all());
                editor.div().raceSet(RACES.playable()[0]);

                GuiSection s = new GuiSection();
                s.add(editor);

                {
                    GuiSection butts = new GuiSection();
                    butts.addRightC(0, new GButt.ButtPanel(¤¤add)
                    {
                        protected override void clickA()
                        {
                            current.get().divs.add(new DIV_SPECImp().copyFrom(editor.div()));
                        }

                        protected override void renAction()
                        {
                            activeSet(current.get().divs.hasRoom());
                        }
                    }.pad(32, 0).repetativeSet(true));
                    butts.addRightC(32, new GButt.ButtPanel(UI.icons().m.rotate)
                    {
                        protected override void clickA()
                        {
                            editor.div().generate();
                        }
                    }.hoverInfoSet(Dic.¤¤Generate));

                    s.addRelBody(8, DIR.S, butts);
                }

                s.addRelBody(8, DIR.W, new ArmyDivs(current, editor));
                addRelBody(8, DIR.S, s);
            }

            addRelBody(8, DIR.S, UI.decor().borderBottom(800));
        }
    }
}