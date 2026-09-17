using System;
using System.Collections.Generic;
using game.faction;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using view.tool;
using view.world.panel;
using world;
using world.army;
using world.entity.army;
using world.map.pathing;
using world.map.regions;

namespace world.army.ai
{
    internal sealed class Attacker
    {
        private readonly Bitmap1D check = new Bitmap1D(WREGIONS.MAX, false);

        public Attacker()
        {
            IDebugPanelWorld.Add(new PlacableSimpleTile("army debug")
            {
                public override void Place(int tx, int ty)
                {
                    LOG.ln("test " + tx + " " + ty);
                    LIST<RegDist> ds = WORLD.PATH().regFinder.All(tx, ty, Treaty.FACTION_BORDERS, WRegSel.ENEMY(WORLD.REGIONS().map.Get(tx, ty).faction()));

                    foreach (RegDist d in ds)
                    {
                        LOG.ln(d.reg + " " + d.reg.faction());
                    }
                }

                public override CharSequence IsPlacable(int tx, int ty)
                {
                    if (WORLD.PATH().map.Is.Is(tx, ty) && WORLD.REGIONS().map.Get(tx, ty) != null && WORLD.REGIONS().map.Get(tx, ty).faction() != null)
                    {
                        return null;
                    }
                    return E;
                }
            });
        }

        public void Attack(Faction f, ArrayList<WArmy> armies)
        {
            if (armies.Size == 0)
                return;

            check.Clear();

            double enemyPower = 0;

            foreach (Faction e in DIP.WAR().All(f))
            {
                enemyPower += AD.power().Get(e);
            }

            double power = 0;

            foreach (WArmy a in armies)
                power += AD.power().Get(a);

            power -= enemyPower;
            if (War.logging)
            {
                War.log(f, "" + power);
            }

            while (power > 0 && armies.Size > 0)
            {
                WArmy a = armies.RemoveLast();
                power -= AD.power().Get(a);
                Attack(a);
            }

            while (armies.Size > 0)
            {
                WArmy a = armies.RemoveLast();
                Guard(a);
            }
        }

        private void Guard(WArmy a)
        {
            if (a.Faction() == null)
                return;
            if (DIP.WAR().All(a.Faction()).Size > 0)
            {
                if (a.Region() == a.Faction().CapitolRegion())
                    return;
                if (a.State() == WArmyState.moving && WORLD.REGIONS().map.Get(a.Path().destX(), a.Path().destY()) == a.Faction().CapitolRegion())
                    return;
                COORDINATE c = WORLD.PATH().Rnd(a.Faction().CapitolRegion());
                if (c != null)
                {
                    a.SetDestination(c.x(), c.y());
                }
                return;
            }
        }

        private void Attack(WArmy a)
        {
            if (a.Region() != null && a.Region().Faction() == a.Faction() && AD.supplies().health(a) < 1)
            {
                a.Stop();
                if (War.logging)
                {
                    War.log(a, "stop");
                }
                return;
            }

            LIST<RegDist> ds = WORLD.PATH().regFinder.All(a.Ctx(), a.Cty(), Treaty.FACTION_BORDERS, WRegSel.ENEMY(a.Faction()));
            Region best = null;
            double bestValue = 0;
            double pow = AD.power().Get(a);

            foreach (RegDist d in ds)
            {
                if (DIP.WAR().Is(a.Faction(), FACTIONS.player()))
                {

                }
                double v = pow / (RD.MILITARY().power.GetD(d.Reg) + 100);
                if (v > 1.0)
                {
                    v /= d.distance;
                    if (v > bestValue)
                    {
                        best = d.Reg;
                    }
                }

            }

            if (War.logging)
            {
                War.log(a, " " + best);
            }

            if (best != null)
            {
                if (a.State() != WArmyState.besieging || a.Region() != best)
                    a.Besiege(best);
            }
            else
            {
                Guard(a);
            }
        }
    }
}