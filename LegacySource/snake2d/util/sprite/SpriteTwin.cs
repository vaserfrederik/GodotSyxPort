using System;

namespace Snake2D.Util.Sprite
{
    public class SpriteTwin : SPRITE
    {
        private SPRITE a;
        private SPRITE b;
        private int offX;
        private int offY;

        public SpriteTwin()
        {
        }

        public SpriteTwin(SPRITE a, SPRITE b)
        {
            Set(a, b);
        }

        public void Set(SPRITE a, SPRITE b)
        {
            this.a = a;
            this.b = b;
            offX = (a.Width() - b.Width()) / 2;
            offY = (a.Height() - b.Height()) / 2;
        }

        public override int Width()
        {
            return a.Width();
        }

        public override int Height()
        {
            return a.Height();
        }

        public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            if (a != null)
            {
                a.Render(r, X1, X2, Y1, Y2);
                b.Render(r, X1 + offX, X2 - offX, Y1 + offY, Y2 - offY);
            }
        }

        public override void RenderTextured(TextureCoords other, int X1, int X2, int Y1, int Y2)
        {
            if (a != null)
            {
                a.RenderTextured(other, X1, X2, Y1, Y2);
                b.RenderTextured(other, X1 + offX, X2 - offX, Y1 + offY, Y2 - offY);
            }
        }
    }
}