using System;
using System.Collections.Generic;
using System.Linq;
using game.boosting;
using game.faction.npc;
using init.constant;
using init.race;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using world.army;
using world.entity.army;
using world.map.regions;

namespace world.army.ai
{
    internal sealed class Recruiter
    {
        private readonly Tree<WArmy> tree = new Tree<WArmy>(100)
        {
            protected override bool IsGreaterThan(WArmy current, WArmy cmp)
            {
                if (AD.menTarget(null).Get(current) > AD.menTarget(null).Get(cmp))
                    return true;
                return current.armyIndex() > cmp.armyIndex();
            }
        };

        public void Recruit(FactionNPC f)
        {
            if (f.Realm().All().Count == 0)
                return;

            int menTarget = (int)(AD.conscripts().Available(null).Get(f));
            int men = AD.menTarget(null).Faction(f);
            int recruits = menTarget - men;
            if (recruits < 10)
                return;

            int armies = CLAMP.I(1 + menTarget / 5000, 1, 3);

            while (f.Armies().All().Count < armies && f.Armies().CanCreate())
            {
                Region r = f.Realm().All().Rnd();
                COORDINATE c = WORLD.PATH().Rnd(r);
                WORLD.ENTITIES().Armies.Create(c.X, c.Y, f);
            }

            tree.Clear();
            for (int ai = 0; ai < f.Armies().All().Count; ai++)
            {
                WArmy a = f.Armies().All()[ai];
                tree.Add(a);
            }

            while (tree.HasMore())
            {
                WArmy a = tree.PollGreatest();
                int target = menTarget;
                if (tree.HasMore())
                {
                    target *= 0.75;
                }

                target = CLAMP.I(target, 0, Config.battle().MEN_PER_ARMY);
                Recruit(f, a, target);

                menTarget -= AD.menTarget(null).Get(a);
            }
        }

        private void Recruit(FactionNPC f, WArmy a, int target)
        {
            main:
            while (AD.menTarget(null).Get(a) < target && a.Divs().CanAdd())
            {
                int ri = RND.RInt(RACES.All().Count);

                for (int i = 0; i < RACES.All().Count; i++)
                {
                    Race r = RACES.All()[(ri + i) % RACES.All().Count];

                    int am = AD.conscripts().Available(r).Get(f);

                    am = CLAMP.I(am, 0, Config.battle().MEN_PER_DIVISION);

                    int min = (int)(Config.battle().MEN_PER_DIVISION * a.Divs().Size / 10.0);
                    min = Math.Min(min, Config.battle().MEN_PER_DIVISION);
                    if (am < min)
                        continue;

                    if (am > 0)
                    {
                        if (r.Playable)
                            am = CLAMP.I(am, 5, Config.battle().MEN_PER_DIVISION);

                        double trai = 0.1 + 0.9 * 0.25 * BOOSTABLES.NOBLE().AGRESSION.Get(f.Court().King().Roy().Induvidual);
                        double equip = 0.1 + 0.9 * 0.5 * BOOSTABLES.NOBLE().COMPETANCE.Get(f.Court().King().Roy().Induvidual);
                        WDivRegional d = AD.Regional().Create(r, (double)am / Config.battle().MEN_PER_DIVISION, a);
                        d.Randomize(trai, equip);
                        continue main;
                    }
                }
                break;
            }

            int arts = AD.menTarget(null).Get(a);
            arts /= 200 + RND.RInt(100);
            arts *= Math.Abs(AD.Rnd(a));
            arts = CLAMP.I(arts, 0, ADSupplies.ArtilleryMax);
            foreach (ADArtillery aa in AD.Supplies().Arts())
            {
                arts -= aa.Target.Get(a);
            }

            foreach (ADArtillery aa in AD.Supplies().Arts())
            {
                aa.Target.Set(a, arts);
            }
        }
    }
}