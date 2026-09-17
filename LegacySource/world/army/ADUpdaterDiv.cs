using System;
using System.Collections.Generic;
using game.faction;
using game.time;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;
using util.updating;
using view.ui.message;
using world.entity.army;

namespace world.army
{
    final class ADUpdaterDiv : IUpdater
    {
        private static readonly CharSequence ¤¤Desertion = "¤Desertion!";
        private static readonly CharSequence ¤¤DesertionD = "¤Army supplies are low, and as a result {0} soldiers have deserted from {1}.";

        static ADUpdaterDiv()
        {
            D.ts(typeof(ADUpdaterDiv));
        }

        public ADUpdaterDiv(ADInit init) : base(FACTIONS.MAX(), TIME.secondsPerDay())
        {
        }

        protected override void update(int i, double timeSinceLast)
        {
            Faction f = i == 0 ? null : FACTIONS.getByIndex(i - 1);

            ADArmies asArmies = AD.army(f);
            for (int ai = 0; ai < asArmies.all().size(); ai++)
            {
                WArmy a = asArmies.all().get(ai);
                if (a.faction() != FACTIONS.player() || AD.supplies().health(a) >= 1)
                {
                    train(a);
                }
                else
                {
                    starve(a);
                }
                if (!a.added())
                    ai--;
            }
        }

        void train(WArmy a)
        {
            for (int di = 0; di < a.divs().size(); di++)
            {
                WDIV div = a.divs().get(di);
                if (div is WDivRegional)
                {
                    WDivRegional d = div as WDivRegional;
                    d.updateDay();
                }
                else if (div is WDivStored)
                {
                    ((WDivStored)div).age();
                }
            }
        }

        void starve(WArmy a)
        {
            double health = AD.supplies().health(a);
            int am = 0;
            for (int di = 0; di < a.divs().size(); di++)
            {
                ADDiv div = a.divs().get(di);
                if (health < RND.rFloat())
                {
                    if (div.needSupplies())
                    {
                        int aa = (int)(div.men() * (1.0 - health) * (0.5 + 0.5 * RND.rFloat()));
                        am += aa;
                        div.menSet(div.men() - aa);
                    }
                }
            }

            if (am > 0)
            {
                Str.TMP.clear();
                Str.TMP.add(¤¤DesertionD).insert(0, am).insert(1, a.name);
                new MessageText(¤¤Desertion, Str.TMP).send();
                if (AD.men(null).get(a) <= 0)
                    a.stop();
            }
        }
    }
}