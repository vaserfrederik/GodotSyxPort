using System;
using System.Collections.Generic;
using game.faction.diplomacy;
using game.faction.diplomacy.deal;
using game.faction.npc;
using init.race;
using settlement.main;
using snake2d.util.rnd;
using util.text;
using view.ui.diplomacy;
using world.army;

namespace game.events.faction.player
{
    class Peace
    {
        public bool Update()
        {
            if (SETT.INVADOR().Invading())
                return false;

            if (!RND.oneIn(6))
                return true;

            FactionNPC f = null;

            foreach (FactionNPC ff in DIP.WAR().Player())
            {
                if (f == null || AD.power().Get(ff) > AD.power().Get(f))
                    f = ff;
            }

            if (f != null && !f.Request.Has())
            {
                KingMessages m = f.Court().King().Roy().Induvidual.Race().KingMessage();
                Deal d = DIP.TMP();
                d.SetFactionAndClear(f);
                d.bools.PEACE.Set(true);
                CharSequence desc = null;
                double credits = d.ValueCredits();
                if (credits > 0)
                {
                    desc = m.PEACE_GOOD.Get(f);
                }
                else
                {
                    desc = m.PEACE_BAD.Get(f);
                }
                credits += (1 + RND.rFloat() * 0.5) * Math.Abs(credits);
                DealDrawfter.Draft(d, credits, true, true);
                new UIDipMessDeal(Dic.¤¤peace, desc, d, 0.5, -0.5).Send();
                return true;
            }
            return false;
        }
    }
}