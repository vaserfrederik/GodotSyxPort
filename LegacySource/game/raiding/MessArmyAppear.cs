using System;
using System.Text;
using game.raiding;
using init.sprite.UI;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using view.main;
using view.ui.message;

namespace game.raiding
{
    public sealed class MessArmyAppear : MessageSection
    {
        private static readonly CharSequence ¤¤title = "Raiders Arrived";
        private static readonly CharSequence ¤¤desc = "{0} has been spotted at our borders Milord. Death and destruction will follow in {1} path to our capital. We must put a stop to this now.";

        static MessArmyAppear()
        {
            D.ts(typeof(MessArmyAppear));
        }

        private readonly Raider raider;
        private readonly int x;
        private readonly int y;

        public MessArmyAppear(Raider raider, int x, int y) : base(¤¤title)
        {
            this.raider = raider;
            this.x = x;
            this.y = y;
        }

        protected override void make(GuiSection section)
        {
            Str.TMP.Clear().Add(¤¤desc);
            Str.TMP.Insert(0, raider.name);
            Str.TMP.Insert(1, raider.indu.race().info.pHIS.Get(raider.indu, false));
            paragraph(Str.TMP);
            section.AddRelBody(32, DIR.N, new RaiderPortrait(4).Set(raider));

            section.AddRelBody(16, DIR.S, new GButt.ButtPanel(UI.icons().m.crossair)
            {
                protected override void clickA()
                {
                    VIEW.world().activate();
                    VIEW.world().window.setZoomout(0);
                    VIEW.world().window.centererTile.set(x, y);
                }
            }.SetDim(48));
        }
    }
}