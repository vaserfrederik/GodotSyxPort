using System;
using System.Collections.Generic;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.text;
using view.ui.message;

namespace game.raiding
{
    internal class MessDemandTY : MessageSection
    {
        private static readonly long serialVersionUID = 1L;
        private static readonly string ¤¤title = "Raider Satisfied";
        private static readonly string ¤¤body = "Since we paid the ransom, this raider will leave us alone. For now...";

        static MessDemandTY()
        {
            D.ts(typeof(MessDemandTY));
        }

        private readonly Raider raider;

        public MessDemandTY(Raider raider) : base(¤¤title)
        {
            this.raider = raider;
        }

        protected override void make(GuiSection section)
        {
            foreach (string s in raider.text.payed)
                paragraph(s);

            section.addRelBody(16, DIR.S, new GText(UI.FONT().S, ¤¤body).lablifySub().setMaxWidth(WIDTH));

            section.addRelBody(32, DIR.N, new RaiderPortrait(4).set(raider));
        }
    }
}