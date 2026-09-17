using System;
using System.Collections.Generic;
using game.boosting;
using game.faction.npc;
using game.time;
using init.resources;
using init.sprite.UI;
using snake2d.util.misc;
using util.data;
using util.text;
using world;
using world.entity.army;
using world.map.regions;
using world.region.RD;
using world.region.RDOutputs;
using world.region.pop;

namespace world.region
{
    public class RDDevastation
    {
        private static readonly CharSequence ¤¤Name = "¤Devastation";
        private static readonly CharSequence ¤¤Desc = "¤Devastation comes from military actions. Devastated regions produce less, and have slower population growth. Devastation takes 2 years to subside.";

        static
        {
            D.ts(typeof(RDDevastation));
        }

        private static readonly double dTime = 1.0 / (TIME.secondsPerDay() * 32);
        public readonly INT_OE<Region> current;

        public RDDevastation(RDInit init)
        {
            current = init.count.newDataShort("DEVASTATION", ¤¤Name, ¤¤Desc);

            BOOSTING.connecter(new ACTION
            {
                public void exe()
                {
                    RBooster b = new RBooster(new BSourceInfo(¤¤Name, UI.icons().s.heat), 0.25, 1.0, true)
                    {
                        public double get(Region t)
                        {
                            return 1.0 - current.getD(t);
                        }
                    };
                    b.add(RD.RACES().capacity);

                    foreach (RDRace r in RD.RACES().all)
                        b.add(r.pop.growth);

                    foreach (RDOutput o in RD.OUTPUT().ALL)
                    {
                        b.add(o.boost);
                        b.add(o.boostYearlyPart);
                    }
                }
            });

            init.upers.add(new RDUpdatable
            {
                public void update(Region reg, double time)
                {
                    if (reg.faction() != null)
                    {
                        foreach (WArmy a in WORLD.ENTITIES().armies.fill(reg))
                        {
                            if (a.raiding())
                                return;
                        }
                        current.incFraction(reg, -current.max(reg) * time * dTime);
                    }
                }

                public void init(Region reg)
                {
                    current.set(reg, 0);
                }
            });
        }

        public int raidCredits(Region reg)
        {
            double pop = RD.RACES().population.get(reg);
            double d = pop * RESOURCES.ALL().size();
            if (reg.faction() != null && reg.faction() is FactionNPC)
            {
                FactionNPC f = (FactionNPC)reg.faction();
                d *= 1 + CLAMP.d(f.credits().credits() / RD.RACES().population.faction().get(f), 0, 100);
            }
            return (int)(d * (1.0 - current.getD(reg)));
        }
    }
}