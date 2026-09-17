using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.time;
using init.sprite.UI;
using snake2d.util.misc;
using util.data;
using util.text;
using world.map.regions;
using world.region.RD;
using world.region.pop;

namespace world.region
{
    public class RDOwner : RDUpdatable
    {
        private static string ¤¤Affiliation = "¤Support";
        private static string ¤¤AffiliationD = "¤Support towards your majesty. Low support increases the chance of rebellion. When a region is controlled, support will increase with time. For other regions, emissaries can be sent to increase support.";

        static
        {
            D.ts(typeof(RDOwner));
        }

        private static readonly double dTime = 1.0 / (TIME.secondsPerDay() * 8);
        public readonly INT_OE<Region> affiliation;

        private readonly INT_OE<Region> prevOwner;
        private readonly INT_OE<Region> prevOwnerII;
        public readonly INT_OE<Region> ownerI;

        public RDOwner(RDInit init)
        {
            affiliation = init.count.new DataByte("OWNER", ¤¤Affiliation, ¤¤AffiliationD);
            prevOwner = init.count.new DataShort("PREVOWVER");
            prevOwnerII = init.count.new DataNibble("PREVOWNER2");
            ownerI = init.count.new DataByte("OWNERI");
            init.upers.Add(this);

            BOOSTING.connecter(new ACTION()
            {
                public void exe()
                {
                    RBooster b = new RBooster(new BSourceInfo(¤¤Affiliation, UI.icons().s.happy), 0, 1, true)
                    {
                        public double get(Region t)
                        {
                            if (t.faction() == FACTIONS.player())
                                return affiliation.getD(t);
                            return 1;
                        }
                    };

                    foreach (RDRace r in RD.RACES().all)
                    {
                        b.add(r.loyalty.target);
                    }
                }
            });
        }

        public void update(Region reg, double time)
        {
            int tar = 255;
            double d = 255.0 * time * dTime;
            if (reg.faction() == FACTIONS.player())
            {
                tar = 255;
                affiliation.moveTo(reg, d, tar);
            }
            else if (reg.faction() != FACTIONS.player())
            {
                double dd = FACTIONS.player().emissaries.assimilate.getD(reg) * FACTIONS.player().emissaries.penaltyMul();
                if (dd <= 0)
                {
                    affiliation.moveTo(reg, d * 0.25, 0);
                }
                else
                    affiliation.moveTo(reg, 255 * time * dTime * dd, 255);
            }

            if (prevOwner(reg) == null)
            {
                Faction ff = reg.faction();
                if (ff != null)
                {
                    prevOwner.set(reg, ff.index() + 1);
                    if (ff is FactionNPC)
                        prevOwnerII.set(reg, ((FactionNPC)ff).iteration() & 0x0F);
                }
            }
        }

        public void init(Region reg)
        {
            // TODO Auto-generated method stub
        }

        public Faction prevOwner(Region reg)
        {
            int i = prevOwner.get(reg);
            if (i != 0)
            {
                Faction f = FACTIONS.getByIndex(i - 1);
                if (f == null || !f.isActive() || (f is FactionNPC && prevOwnerII.get(reg) != (((FactionNPC)f).iteration() & 0x0F)))
                {
                    prevOwner.set(reg, 0);
                    return null;
                }

                return f;
            }
            return null;
        }
    }
}