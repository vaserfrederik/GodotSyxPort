using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.renderable;
using snake2d.util.sprite;
using util.colors;

namespace util.gui.panel
{
    public class GFrame : RENDEROBJ.RenderImp, RENDEROBJ
    {
        public const int MARGIN = 3;

        private Rec bounds = new Rec();

        public GFrame(RECTANGLE body)
        {
            Frame(body);
        }

        public override Rec Body()
        {
            return bounds;
        }

        public void Frame(RECTANGLE body)
        {
            this.bounds.SetDim(body.Width() + MARGIN * 2, body.Height() + MARGIN * 2);
            this.bounds.CenterIn(body);
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            Render(r, ds, bounds);
        }

        public static void Render(SPRITE_RENDERER r, float ds, RECTANGLE b)
        {
            Render(r, b.X1(), b.X2(), b.Y1(), b.Y2());
        }

        public static void Render(int MARGIN, SPRITE_RENDERER r, int x1, int x2, int y1, int y2)
        {
            x1 -= MARGIN;
            x2 += MARGIN;
            y1 -= MARGIN;
            y2 += MARGIN;
            GCOLOR.UI().BorderH(r, x1, x2, y1, y2);
        }

        public static void Render(SPRITE_RENDERER r, int x1, int x2, int y1, int y2)
        {
            Render(MARGIN, r, x1, x2, y1, y2);
        }

        public static SPRITE Separator(int width)
        {
            return new SPRITE()
            {
                public override int Width()
                {
                    return width;
                }

                public override int Height()
                {
                    return 16;
                }

                public override void RenderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
                {
                    // TODO Auto-generated method stub
                }

                public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    GCOLOR.UI().BorderH(r, X1, X2, Y1 + Height() / 2 - 1, Y1 + Height() / 2 + 2);
                }
            };
        }
    }
}