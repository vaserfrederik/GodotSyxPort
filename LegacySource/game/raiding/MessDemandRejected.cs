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
    public sealed class MessDemandRejected : MessageSection
    {
        private static readonly string ¤¤title = "Raider Rejected";
        private static readonly string ¤¤body = "This is ill news for us indeed. Perhaps we should have paid the ransom. Now we must prepare for an attack.";

        static MessDemandRejected()
        {
            D.ts(typeof(MessDemandRejected));
        }

        private readonly Raider raider;

        public MessDemandRejected(Raider raider) : base(¤¤title)
        {
            this.raider = raider;
        }

        protected override void make(GuiSection section)
        {
            foreach (string s in raider.text.rejected)
                paragraph(s);

            section.addRelBody(16, DIR.S, new GText(UI.FONT().S, ¤¤body).lablifySub().setMaxWidth(WIDTH));

            section.addRelBody(32, DIR.N, new RaiderPortrait(4).set(raider));
        }
    }
}