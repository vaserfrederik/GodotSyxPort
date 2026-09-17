using game.faction;
using game.faction.diplomacy;
using game.time;
using snake2d.util.rnd;
using util.updating;
using world;
using world.army;
using world.entity.army;
using world.map.pathing;
using world.map.regions;
using world.region;

namespace world.army.ai
{
    final class Rebel : IUpdater
    {
        public Rebel() : base(1, TIME.secondsPerDay() / 2)
        {
        }

        protected override void update(int i, double timeSinceLast)
        {
            updateRebel(aa);
        }

        void updateRebel(WArmy a)
        {
            if (AD.men(null).get(a) == 0)
            {
                a.disband();
                return;
            }

            Region hh = a.region();
            aa = a;
            if (hh == null)
            {
                RegDist rr = WORLD.PATH().regFinder.single(a.ctx(), a.cty(), Treaty.REG_NEIGHS, ally.get(a));
                if (rr != null)
                {
                    a.setDestination(rr.reg.cx(), rr.reg.cy());
                }
                else
                {
                    foreach (RegDist r in WORLD.PATH().regFinder.all(a.ctx(), a.cty(), Treaty.REG_NEIGHS, rebelTarget.get(a)))
                    {
                        if (r != null && AD.power().get(a) > RD.MILITARY().power.getD(r.reg))
                        {
                            a.raid(r.reg);
                            return;
                        }
                        else
                        {
                        }
                    }
                    a.disband();
                }
            }
            else if (hh.faction() == FACTIONS.player())
            {
                if (RD.DEVASTATION().current.getD(hh) < 0.9)
                {
                    a.raid(true);
                    return;
                }

                if (AD.power().get(a) > RD.MILITARY().power.getD(hh))
                {
                    a.besiege(hh);
                }
                else if (RND.oneIn(16))
                {
                    a.disband();
                }
            }
            else
            {
                foreach (RegDist r in WORLD.PATH().regFinder.all(a.ctx(), a.cty(), Treaty.REG_NEIGHS, rebelTarget.get(a)))
                {
                    if (r != null && AD.power().get(a) > RD.MILITARY().power.getD(r.reg))
                    {
                        a.raid(r.reg);
                        return;
                    }
                    else
                    {
                        a.disband();
                    }
                }
            }
        }

        private static WArmy aa;

        private static readonly Sel ally = new Sel()
        {
            public override bool is(Region t)
            {
                return t.faction() == null;
            }
        };

        private static readonly Sel rebelTarget = new Sel()
        {
            public override bool is(Region t)
            {
                return t.faction() == FACTIONS.player() && AD.power().get(aa) > power(t);
            }

            private double power(Region reg)
            {
                double m = 0;
                foreach (WArmy a in WORLD.ENTITIES().armies.fill(reg))
                {
                    if (a.faction() != null && (a.faction() == FACTIONS.player() || DIP.get(a.faction(), FACTIONS.player()).ally))
                        m += AD.power().get(a);
                }
                return m + RD.MILITARY().power.getD(reg);
            }
        };

        static abstract class Sel : WRegSel
        {
            WArmy army;

            public WRegSel get(WArmy army)
            {
                this.army = army;
                return this;
            }
        }
    }
}