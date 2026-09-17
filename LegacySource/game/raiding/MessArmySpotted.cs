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
    final class MessArmySpotted : MessageSection
    {
        private static readonly string ¤¤title = "Raiders Approaching";
        private static readonly string ¤¤desc = "{0} and {1} band of raiders are approaching our borders Milord. They are only 3 days march away. We must prepare for {2} arrival.";

        static MessArmySpotted()
        {
            D.ts(typeof(MessArmySpotted));
        }

        private readonly Raider raider;
        private readonly int x;
        private readonly int y;

        public MessArmySpotted(Raider raider, int x, int y) : base(¤¤title)
        {
            this.raider = raider;
            this.x = x;
            this.y = y;
        }

        protected override void Make(GuiSection section)
        {
            Str.TMP.Clear().Add(¤¤desc);
            Str.TMP.Insert(0, raider.name);
            Str.TMP.Insert(1, raider.indu.race().info.pHIS.Get(raider.indu, false));
            Str.TMP.Insert(2, raider.indu.race().info.pHIS.Get(raider.indu, false));
            Paragraph(Str.TMP);
            section.AddRelBody(32, DIR.N, new RaiderPortrait(4).Set(raider));

            section.AddRelBody(16, DIR.S, new GButt.ButtPanel(UI.Icons().M.Crossair)
            {
                protected override void ClickA()
                {
                    VIEW.World().Activate();
                    VIEW.World().Window.SetZoomout(0);
                    VIEW.World().Window.CentererTile.Set(x, y);
                }
            }.SetDim(48));
        }
    }
}