using System.Collections.Generic;
using game.faction;
using game.time;
using util.updating;
using world.entity.army;

namespace world.army
{
    final class ADUpdater : IUpdater
    {
        public ADUpdater(ADInit init) : base(FACTIONS.MAX(), TIME.secondsPerDay() * 0.25)
        {
        }

        protected override void update(int i, double timeSinceLast)
        {
            Faction f = i == 0 ? null : FACTIONS.getByIndex(i - 1);

            foreach (ADInit.Updater u in AD.iinit().updaters)
                u.update(f, timeSinceLast);

            ADArmies as = AD.army(f);

            for (int ai = 0; ai < as.all().Count; ai++)
            {
                WArmy a = as.all()[ai];
                foreach (ADInit.Updater u in AD.iinit().updaters)
                    u.update(a, timeSinceLast);
            }
        }
    }
}