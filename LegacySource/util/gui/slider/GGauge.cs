using snake2d;
using util.data;
using util.gui.common;
using util.gui.misc;

namespace util.gui.slider
{
    public abstract class GGauge : SPRITE.Imp, TITLEABLE, DOUBLE
    {
        private readonly GMeter.GMeterCol col;

        public GGauge(int width, int height)
            : this(width, height, GMeter.C_REDGREEN)
        {
            SetDim(width, height);
        }

        public GGauge()
            : this(48, 16, GMeter.C_REDGREEN)
        {
        }

        public GGauge(int width, int height, GMeter.GMeterCol col)
        {
            this.col = col;
            SetDim(width, height);
        }

        public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            GMeter.Render(r, col, GetD(), X1, X2, Y1, Y2);
        }

        private void Render(SPRITE_RENDERER r, COLOR col, int x1, int w, int y1, int h, int dx, int dy)
        {
            if (w > 2 * dx && h > 2 * dy)
                col.Render(r, x1 + dx, x1 + w - dx, y1 + dy, y1 + h - dy);
        }
    }
}