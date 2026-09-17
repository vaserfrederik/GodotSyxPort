using System;
using System.Collections.Generic;
using game.faction;
using snake2d;
using snake2d.util.rnd;
using snake2d.util.sets;
using world;
using world.entity.army;
using world.map.pathing;
using world.region;

namespace world.army.ai
{
    final class Chiller
    {
        Chiller()
        {
        }

        public void chill(Faction f, ArrayList<WArmy> armies)
        {
            if (armies.size() > 0 && RND.oneIn(2) && RD.DIST().factionHasRegionBorderingPlayer(f))
            {
                WArmy a = armies.rnd();

                if (a.state() == WArmyState.fortified)
                {
                    LIST<RegDist> l = WORLD.PATH().regFinder.all(a.ctx(), a.cty(), Treaty.FACTION_BORDERS, WRegSel.FACTION(FACTIONS.player()));
                    if (l.size() == 0)
                        return;
                    RegDist d = l.rnd();
                    PathTile t = WORLD.PATH().path(a.ctx(), a.cty(), d.reg.cx(), d.reg.cy(), Treaty.FACTION_BORDERS);
                    if (t != null)
                    {
                        PathTile ok = null;
                        while (t != null)
                        {
                            if (WORLD.REGIONS().faction.get(t) != a.faction())
                                ok = null;
                            else if (ok == null)
                                ok = t;
                            t = t.getParent();
                        }
                        if (ok != null)
                            a.setDestination(ok.x(), ok.y());
                    }
                }
            }
        }
    }
}