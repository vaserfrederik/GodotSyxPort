using System;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.text;
using view.ui.message;

namespace game.raiding
{
    final class MessGoingAway : MessageSection
    {
        /**
         * 
         */
        private static readonly long serialVersionUID = 1L;
        private static readonly CharSequence ¤¤title = "Raider Retreats";
        private static readonly CharSequence ¤¤desc = "Due to unknown reasons, {0} has turned back {1} army and left our lands. Lets pray {2} never returns.";

        static
        {
            D.ts(typeof(MessGoingAway));
        }

        private readonly Raider raider;

        public MessGoingAway(Raider raider) : base(¤¤title)
        {
            this.raider = raider;
        }

        protected override void make(GuiSection section)
        {
            Str s = Str.TMP.clear();
            s.add(¤¤desc);
            s.insert(0, raider.name);
            s.insert(1, raider.indu.race().info.pHIS.get(raider.indu, false));
            s.insert(2, raider.indu.race().info.pHE.get(raider.indu, false));
            paragraph(s);

            section.addRelBody(32, DIR.N, new RaiderPortrait(4).set(raider));
        }
    }
}