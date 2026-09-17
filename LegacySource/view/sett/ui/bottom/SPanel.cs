using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;

namespace view.sett.ui.bottom
{
    class SPanel : GuiSection
    {
        public override void render(SPRITE_RENDERER r, float ds)
        {
            COLOR.WHITE20.render(r, body());
            GCOLOR.UI().borderH(r, body(), 0);
            base.render(r, ds);
        }
    }
}