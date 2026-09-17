using System;
using System.Collections.Generic;
using game.faction;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.sets;
using util.text;

namespace game.faction.diplomacy
{
    public sealed class DWar : DipStance
    {
        private static readonly string ¤¤name = "Enemies";
        private static readonly string ¤¤desc = "Enemies are at war and bent on destroying one another.";

        static DWar()
        {
            D.ts(typeof(DWar));
        }

        public DWar(LISTE<DipStance> all) : base(all, "WAR", 0, 0, 0.8f, false, false, false, ¤¤name, ¤¤desc, UI.icons().s.sword.createColored(COLOR.REDISH))
        {
        }

        public override bool Is(Faction faction, Faction other)
        {
            if (faction == other)
                return false;
            if (faction == null || other == null)
                return true;
            return base.Is(faction, other);
        }
    }
}