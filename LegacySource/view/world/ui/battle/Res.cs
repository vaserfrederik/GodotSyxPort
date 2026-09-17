using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.gui.table;
using util.info;
using util.text;
using world.army;
using world.battle.spec;

namespace view.world.ui.battle
{
    class Res : GuiSection
    {
        private static CharSequence ¤¤Victory = "Victory";
        private static CharSequence ¤¤victoryD = "¤The gods have smiled upon your name. Victory is ours and our foe has been beaten.";
        private static CharSequence ¤¤Retreat = "Retreat";
        private static CharSequence ¤¤RetreatD = "¤Our army has retreated to fight another day.";
        private static CharSequence ¤¤Defeat = "¤Defeat";
        private static CharSequence ¤¤DefeatD = "¤A dark day in the annals. The enemy has snatched victory from us.";
        private static CharSequence ¤¤RetreatDefeat = "¤Our army attempted to retreat, but was destroyed in the process.";
        private static CharSequence ¤¤Capture = "Capture";
        private static CharSequence ¤¤CaptureD = "Ship the selected captives to your capital.";
        private static CharSequence ¤¤Execute = "Execute";
        private static CharSequence ¤¤ExecuteD = "Execute the selected captives.";
        private static CharSequence ¤¤Release = "Release";
        private static CharSequence ¤¤ReleaseD = "Have mercy and release all captives. Surely they will never bear arms against you again?";
        private static CharSequence ¤¤eret = "¤Enemy Retreats";
        private static CharSequence ¤¤eretD = "¤Enemy forces trembled before our might and ran before any engagement. We managed to hunt some down and plunder their baggage train.";

        public static readonly int width = 600;

        static Res()
        {
            D.ts(typeof(Res));
        }

        private Slaves slaves;
        private Spoils spoils;
        private readonly CharSequence name;

        public Res(ACTION close, WBattleResult result, bool enemyRetreats)
        {
            CharSequence desc = null;
            if (result.result == BATTLE_RESULT.VICTORY)
            {
                if (enemyRetreats)
                {
                    name = ¤¤eret;
                    desc = ¤¤eretD;
                }
                else
                {
                    name = ¤¤Victory;
                    desc = ¤¤victoryD;
                }
            }
            else if (result.result == BATTLE_RESULT.RETREAT)
            {
                name = ¤¤Retreat;
                desc = ¤¤RetreatD;
            }
            else
            {
                name = ¤¤Defeat;
                desc = ¤¤DefeatD;
            }

            if (desc != null)
            {
                add(new GText(desc));
            }

            add(new GText(result.result.ToString()));

            if (result.result == BATTLE_RESULT.VICTORY)
            {
                spoils = new Spoils(result.loot);
                add(spoils);
            }

            if (result.prisoners > 0)
            {
                slaves = new Slaves(result.prisoners);
                add(slaves);
            }

            add(new GButt.ButtPanel("Close", 180)
            {
                ClickA = () => close.Invoke()
            });
        }

        private class Spoils : GuiSection
        {
            private readonly Dictionary<int, int> loot;

            public Spoils(Dictionary<int, int> loot)
            {
                this.loot = loot;
                int am = loot.Count;
                GRows rows = new GRows(am);

                foreach (var kvp in loot)
                {
                    int index = kvp.Key;
                    int amount = kvp.Value;

                    GButt.ButtPanel butt = new GButt.ButtPanel($"Item {index}: {amount}", 200)
                    {
                        ClickA = () => Console.WriteLine($"Item {index} clicked")
                    };

                    rows.Add(butt);
                }

                add(new GScrollRows(rows.Rows(), 28 * am).View());
            }
        }

        private class Slaves : GuiSection
        {
            private readonly int prisoners;
            private int accepted = 0;

            public Slaves(int prisoners)
            {
                this.prisoners = prisoners;

                add(new GButt.ButtPanel("Accept", 180)
                {
                    ClickA = () => accepted = prisoners
                });

                add(new GButt.ButtPanel("Reject", 180)
                {
                    ClickA = () => accepted = 0
                });

                add(new GStat()
                {
                    Update = text => GFORMAT.iIncr(text, accepted)
                }.Hv("Accepted Prisoners"));
            }
        }
    }
}