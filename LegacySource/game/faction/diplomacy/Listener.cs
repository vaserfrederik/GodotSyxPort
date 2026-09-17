using System;
using game.faction;
using game.faction.diplomacy;
using init.sprite.UI;
using snake2d.util.sprite.text;
using util.text;
using view.ui.message;
using world;

namespace game.faction.diplomacy
{
    final class Listener : DipActivityListener
    {
        private static readonly CharSequence ¤¤warDeclared = "The realm of {0} has declared war on {1}.";
        private static readonly CharSequence ¤¤warPeace = "{0} and {1} have agreed to a truce.";
        private static readonly CharSequence ¤¤mTitle = "Distant war.";
        private static readonly CharSequence ¤¤mBody = "One of your neighbours have gone to war. This could be an opportunity to snatch a cheap alliance, or join one of the sides to take part of the spoils.";
        private static readonly CharSequence ¤¤trade = "{FACTION_A} and {FACTION_B} are now trade partners.";

        static Listener()
        {
            D.ts(typeof(Listener));
        }

        public override void change(Faction faction, Faction other, DipStance old, DipStance nn)
        {
            if (nn == DIP.WAR() || old == DIP.WAR())
            {
                Str.TMP.clear().add(nn == DIP.WAR() ? ¤¤warDeclared : ¤¤warPeace);
                Str.TMP.insert(0, faction.name);
                Str.TMP.insert(1, other.name);
                WORLD.LOG().log(faction, other, UI.icons().s.sword, Str.TMP, other.cx(), other.cy());
                if (other != FACTIONS.player() && faction != FACTIONS.player() && RD.DIST().factionHasRegionBorderingPlayer(other) || RD.DIST().factionHasRegionBorderingPlayer(faction))
                {
                    new MessageText(¤¤mTitle).paragraph(¤¤mBody).send();
                }
            }
            else if (!old.trades && nn.trades)
            {
                Str.TMP.clear().add(¤¤trade);
                Str.TMP.insert(0, faction.name);
                Str.TMP.insert(1, other.name);
                if (other.capitolRegion() != null)
                    WORLD.LOG().log(faction, other, UI.icons().s.trade, Str.TMP, other.cx(), other.cy());
            }
        }
    }
}