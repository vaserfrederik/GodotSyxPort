using System;
using init.sprite.UI;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.gui.misc;

namespace view.sett.ui.bottom
{
    class BButt : GButt.ButtPanel
    {
        public const int HEIGHT = 44;
        public const int WIDTH = 350;

        public BButt(SPRITE icon, string label) : base((SPRITE)new Text(UI.FONT().H2, label).setMaxWidth(WIDTH - 50).setMultipleLines(false))
        {
            SPRITE i = new SPRITE.Wrap(icon, 32, 32);
            icon(i);
            setDim(WIDTH, HEIGHT);
        }

        public BButt(SPRITE icon, string label, int dwidth) : base((SPRITE)new Text(UI.FONT().H2, label).setMaxWidth(WIDTH - 50 - dwidth).setMultipleLines(false))
        {
            SPRITE i = new SPRITE.Wrap(icon, 32, 32);
            icon(i);
            setDim(WIDTH - dwidth, HEIGHT);
        }
    }
}