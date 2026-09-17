using System;
using System.Collections.Generic;
using game.boosting;
using game.time;
using init.race;
using init.sprite;
using init.sprite.UI;
using init.type;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.stat;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.standing.Cats;
using world.army;

namespace view.sett.ui.standing
{
    internal sealed class CatPopulation : Cat
    {
        private static readonly CharSequence ¤¤age = "¤¤age";
        private static readonly CharSequence ¤¤ageAverage = "¤¤ageAverage";
        private static readonly CharSequence ¤¤Type = "¤¤Type";
        private static readonly CharSequence ¤¤Total = "¤¤Total";
        private static readonly CharSequence ¤¤Population = "¤¤Population";
        private static readonly CharSequence ¤¤Type = "¤¤Type";
        private static readonly CharSequence ¤¤Wrongful = "¤¤Wrongful";

        public CatPopulation() : base()
        {
        }

        protected override void init()
        {
            base.init();
            StatSet stats = new StatSet();
            stats.add(STATS.POP.POP);
            stats.add(STATS.POP.COUNT.enters());
            stats.add(STATS.POP.COUNT.leaves());
            stats.add(STATS.POP.TYPE);
            stats.add(STATS.POP.age.AGE_DAYS);
            stats.add(STATS.POP.COUNT.leaves());
            stats.add(STATS.POP.WRONGFUL);

            init(stats);
        }

        private void init(StatSet stats)
        {
            foreach (Stat stat in stats)
            {
                if (stat is StatType typeStat)
                {
                    initTypeStat(typeStat);
                }
                else if (stat is StatCount countStat)
                {
                    initCountStat(countStat);
                }
                else if (stat is StatAge ageStat)
                {
                    initAgeStat(ageStat);
                }
                else if (stat is StatWrongful wrongfulStat)
                {
                    initWrongfulStat(wrongfulStat);
                }
            }
        }

        private void initTypeStat(StatType typeStat)
        {
            // Initialize type stat logic
        }

        private void initCountStat(StatCount countStat)
        {
            // Initialize count stat logic
        }

        private void initAgeStat(StatAge ageStat)
        {
            // Initialize age stat logic
        }

        private void initWrongfulStat(StatWrongful wrongfulStat)
        {
            // Initialize wrongful stat logic
        }
    }
}