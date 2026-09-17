using System;
using System.Collections.Generic;
using game.raiding;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.text;
using view.ui.message;

namespace game.raiding
{
    public class MessVictory : MessageSection
    {
        /**
         * 
         */
        private static readonly long serialVersionUID = 1L;

        private static readonly CharSequence ¤¤title = "Raider Raids";

        static MessVictory()
        {
            D.ts(typeof(MessVictory));
        }

        private readonly Raider raider;
        private string[] mm;

        public MessVictory(Raider raider) : base(¤¤title)
        {
            this.raider = raider;
            mm = new string[raider.text.afterRaid.Count];
            int mi = 0;
            foreach (string s in raider.text.afterRaid)
            {
                mm[mi++] = s;
            }
        }

        protected override void make(GuiSection section)
        {
            foreach (string s in mm)
            {
                paragraph(s);
            }

            section.addRelBody(32, DIR.N, new RaiderPortrait(4).Set(raider));
        }
    }
}