using System;
using System.Collections.Generic;
using game.faction;
using game.faction.diplomacy;
using snake2d.util.sets;
using world;
using world.army;
using world.entity.army;
using world.map.pathing;
using world.map.regions;

namespace world.army.ai
{
    internal sealed class Defender
    {
        private readonly Tree<WArmy> tree = new Tree<WArmy>(100);

        protected override bool IsGreaterThan(WArmy current, WArmy cmp)
        {
            if (AD.power().get(current) > AD.power().get(cmp))
                return true;
            return current.armyIndex() > cmp.armyIndex();
        }

        private readonly Threat[] threats = new Threat[16];

        static Defender()
        {
            for (int i = 0; i < threats.Length; i++)
                threats[i] = new Threat();
        }

        public LIST<WArmy> defend(Faction f, ArrayList<WArmy> available)
        {
            tree.clear();
            for (int ri = 0; ri < f.realm().regions(); ri++)
            {
                Region reg = f.realm().region(ri);
                foreach (WArmy a in WORLD.ENTITIES().armies.fill(reg))
                {
                    if (DIP.WAR().is(a.faction(), f))
                    {
                        tree.add(a);
                    }
                }
            }

            int threatI = 0;
            while (tree.hasMore() && threatI < threats.Length)
            {
                threats[threatI].a = tree.pollGreatest();
                threats[threatI].powerAdded = 0;
                threatI++;
            }

            for (int i = 0; i < threatI && available.size() > 0; i++)
            {
                WArmy a = getClosest(threats[i].a, available);
                if (a != null)
                {
                    a.intercept(threats[i].a);
                    threats[i].powerAdded += AD.power().get(a);
                    if (threats[i].powerAdded * 0.5 < AD.power().get(threats[i].a))
                        i--;
                }
            }

            return available;
        }

        private WArmy getClosest(WArmy enemy, ArrayList<WArmy> available)
        {
            double bestValue = int.MaxValue;
            WArmy best = null;

            foreach (WArmy a in available)
            {
                Region ar = WORLD.REGIONS().map.get(a.ctx(), a.cty());
                if (ar != null)
                {
                    RegDist rr = WORLD.PATH().regFinder.single(enemy.ctx(), enemy.cty(), Treaty.DUMMY, WRegSel.SINGLE(ar));
                    if (rr != null)
                    {
                        if (AD.power().get(a) * 0.75 > AD.power().get(enemy) && rr.distance < bestValue)
                        {
                            if (a.region() != null && a.region().faction() == a.faction() && AD.supplies().health(a) < 1)
                            {
                                a.stop();
                                continue;
                            }

                            best = a;
                            bestValue = rr.distance;
                        }
                    }
                }
            }
            return best;
        }

        private class Threat
        {
            public WArmy a;
            public int powerAdded;
        }
    }
}