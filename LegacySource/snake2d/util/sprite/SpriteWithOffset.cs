using System;

namespace Snake2D.Util.Sprite
{
    public class SpriteWithOffset : Coo, SPRITE
    {
        private readonly SPRITE a;

        public SpriteWithOffset(SPRITE s) : base()
        {
            a = s;
        }

        public SpriteWithOffset(SPRITE s, int x, int y) : base(x, y)
        {
            a = s;
        }

        public int Width()
        {
            return a.Width();
        }

        public int Height()
        {
            return a.Height();
        }

        public void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            X1 += X();
            Y1 += Y();
            X2 += X();
            Y2 += Y();
            a.Render(r, X1, X2, Y1, Y2);
        }

        public void RenderTextured(TextureCoords other, int X1, int X2, int Y1, int Y2)
        {
            X1 += X();
            Y1 += Y();
            X2 += X();
            Y2 += Y();
            a.RenderTextured(other, X1, X2, Y1, Y2);
        }
    }
}