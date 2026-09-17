using System;
using snake2d;
using util.data.INT;
using util.gui.misc;

namespace util.gui.slider
{
    public class GSliderIntInput : GuiSection
    {
        public GSliderIntInput(INTE inValue)
        {
            addRightC(2, new GSliderInt(inValue, 80, true, false));
            addRightC(8, new GInputInt(inValue));
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            base.render(r, ds);
        }
    }
}