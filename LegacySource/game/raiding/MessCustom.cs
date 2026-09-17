using System;
using System.Collections.Generic;
using game.raiding;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.text;
using view.ui.message;

namespace game.raiding
{
    final class MessCustom : MessageSection
    {
        private static readonly long serialVersionUID = 1L;
        private static readonly CharSequence ¤¤title = "Raider";
        private string body;

        static
        {
            D.ts(typeof(MessCustom));
        }

        private readonly Raider raider;

        public MessCustom(Raider raider, string body) : base(¤¤title)
        {
            this.raider = raider;
            this.body = body;
        }

        protected override void make(GuiSection section)
        {
            foreach (string s in raider.text.payed)
            {
                paragraph(s);
            }

            section.addRelBody(16, DIR.S, new GText(UI.FONT().S, body).lablifySub().setMaxWidth(WIDTH));

            section.addRelBody(32, DIR.N, new RaiderPortrait(4).set(raider));
        }
    }
}