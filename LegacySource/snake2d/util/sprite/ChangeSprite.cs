using System;
using snake2d;

namespace snake2d.util.sprite
{
    public class ChangeSprite : SPRITE
    {
        private SPRITE s;
        private readonly ColorImp c = new ColorImp(COLOR.WHITE100);

        public ChangeSprite()
        {
        }

        public ChangeSprite(SPRITE s)
        {
            Set(s);
        }

        public ChangeSprite(SPRITE s, COLOR c)
        {
            Set(s);
            GetColor().Set(c);
        }

        public void Set(SPRITE s)
        {
            this.s = s;
        }

        public ColorImp GetColor()
        {
            return c;
        }

        public override int Width()
        {
            return s != null ? s.Width() : 0;
        }

        public override int Height()
        {
            return s != null ? s.Height() : 0;
        }

        public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            if (s != null)
            {
                c.Bind();
                s.Render(r, X1, X2, Y1, Y2);
                COLOR.Unbind();
            }
        }

        public override void Render(SPRITE_RENDERER r, RECTANGLE rec)
        {
            if (s != null)
            {
                c.Bind();
                s.Render(r, rec);
                COLOR.Unbind();
            }
        }

        public override void Render(SPRITE_RENDERER r, int X1, int Y1)
        {
            if (s != null)
            {
                c.Bind();
                s.Render(r, X1, Y1);
                COLOR.Unbind();
            }
        }

        public override void RenderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
        {
            s.RenderTextured(texture, X1, X2, Y1, Y2);
        }
    }
}