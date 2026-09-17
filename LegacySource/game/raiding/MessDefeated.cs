using System;
using System.Text;
using game.faction;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.text;
using view.ui.message;

namespace game.raiding
{
    final class MessDefeated : MessageSection
    {
        private static readonly string ¤¤title = "Raider Defeated";
        private static readonly string ¤¤desc = "¤The once mighty {0} lies dead at your feet, oh mighty one. This will surely send a powerful message throughout all of Syx.";
        private static readonly string ¤¤loot = "¤In addition, {HIS} personal treasury has been found and looted by our men. A total of {0} denari was found and has been transported to our treasury.";

        static MessDefeated()
        {
            D.ts(typeof(MessDefeated));
        }

        private static readonly long serialVersionUID = 1L;
        private readonly Raider raider;

        public MessDefeated(Raider raider) : base(¤¤title)
        {
            this.raider = raider;
            if (raider.bounty > 0)
                FACTIONS.player().credits().inc(raider.bounty, CTYPE.MISC);
        }

        protected override void make(GuiSection section)
        {
            paragraph(Str.TMP.clear().add(¤¤desc).insert(0, raider.name));
            if (raider.bounty > 0)
            {
                Str.TMP.clear().add(¤¤loot);
                Str.TMP.insert("HIS", raider.indu.race().info.pHIS.get(raider.indu, false));
                Str.TMP.insert(0, Str.TMP2.clear().add((long)raider.bounty, true));
                paragraph(Str.TMP);
            }

            section.addRelBody(32, DIR.N, new RaiderPortrait(4).set(raider).dead(true));
        }
    }
}