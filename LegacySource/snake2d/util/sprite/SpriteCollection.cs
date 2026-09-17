using System;
using System.Collections.Generic;

namespace snake2d.util.sprite
{
    /**
     * This disgusting class took me a day to make. I almost hit my wife because of it. I hate it.
     * @author mail__000
     *
     */
    public class SpriteCollection : SPRITE
    {
        private List<SpriteWCoo> sprites;
        private int width = 0;
        private int height = 0;

        public SpriteCollection(SPRITE s)
        {
            sprites = new List<SpriteWCoo>(500);
            Add(s, 0, 0);
        }

        public void Add(SPRITE s, int x, int y)
        {
            if (y < 0)
            {
                foreach (SpriteWCoo sc in sprites)
                {
                    sc.dY = sc.dY - y;
                }
                y = 0;
            }
            if (y + s.height() > height)
            {
                height = y + s.height();
            }

            if (x < 0)
            {
                foreach (SpriteWCoo sc in sprites)
                {
                    sc.dX = sc.dX - x;
                }
                x = 0;
            }
            if (x + s.width() > width)
            {
                width = x + s.width();
            }

            sprites.Add(new SpriteWCoo(s, x, y));
        }

        public SpriteCollection AddRight(SPRITE s)
        {
            Add(
                s,
                sprites[sprites.Count - 1].dX + sprites[sprites.Count - 1].s.width(),
                sprites[sprites.Count - 1].dY);
            return this;
        }

        public void AddRightC(int m, SPRITE s)
        {
            Add(
                s,
                sprites[sprites.Count - 1].dX + sprites[sprites.Count - 1].s.width() + m,
                sprites[sprites.Count - 1].dY + (sprites[sprites.Count - 1].s.height() - s.height()) / 2);
        }

        public void AddRightCAbs(int abs, SPRITE s)
        {
            Add(
                s,
                abs,
                sprites[sprites.Count - 1].dY + (sprites[sprites.Count - 1].s.height() - s.height()) / 2);
        }

        public void AddDown(SPRITE s)
        {
            Add(
                s,
                sprites[sprites.Count - 1].dX,
                sprites[sprites.Count - 1].dY + sprites[sprites.Count - 1].s.height());
        }

        public void AddLeft(SPRITE s)
        {
            Add(
                s,
                (sprites[sprites.Count - 1].dX - s.width()),
                sprites[sprites.Count - 1].dY);
        }

        public void AddUp(SPRITE s)
        {
            Add(
                s,
                sprites[sprites.Count - 1].dX,
                sprites[sprites.Count - 1].dY - s.height());
        }

        public void AddOnTopCentered(SPRITE s)
        {
            int x1 = sprites[sprites.Count - 1].dX + (sprites[sprites.Count - 1].s.width() - s.width()) / 2;
            int y1 = sprites[sprites.Count - 1].dY + (sprites[sprites.Count - 1].s.height() - s.height()) / 2;
            Add(s, x1, y1);
        }

        public int GetLastX1()
        {
            return sprites[sprites.Count - 1].dX;
        }

        public int GetLastX2()
        {
            return sprites[sprites.Count - 1].dX + sprites[sprites.Count - 1].s.width();
        }

        public int GetLastY1()
        {
            return sprites[sprites.Count - 1].dY;
        }

        public int GetLastY2()
        {
            return sprites[sprites.Count - 1].dY + sprites[sprites.Count - 1].s.height();
        }

        private class SpriteWCoo
        {
            private readonly SPRITE s;
            private int dY;
            private int dX;

            private SpriteWCoo(SPRITE s, int x, int y)
            {
                this.s = s;
                dY = y;
                dX = x;
            }
        }

        public int width()
        {
            return width;
        }

        public int height()
        {
            return height;
        }

        public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            float xScale = (X2 - X1) / (float)width;
            float yScale = (Y2 - Y1) / (float)height;

            int x1;
            int x2;
            int y1;
            int y2;
            foreach (SpriteWCoo s in sprites)
            {
                x1 = (int)(X1 + s.dX * xScale);
                y1 = (int)(Y1 + s.dY * yScale);
                x2 = (int)((X1 + s.dX + s.s.width()) * xScale);
                y2 = (int)((Y1 + s.dY + s.s.height()) * yScale);
                s.s.render(r, x1, x2, y1, y2);
            }
        }

        public void render(SPRITE_RENDERER r, int X1, int Y1)
        {
            foreach (SpriteWCoo s in sprites)
            {
                s.s.render(r, X1 + s.dX, X1 + s.dX + s.s.width(), Y1 + s.dY, Y1 + s.dY + s.s.height());
            }
        }

        public void renderTextured(TextureCoords other, int X1, int X2, int Y1, int Y2)
        {
            foreach (SpriteWCoo s in sprites)
            {
                s.s.renderTextured(other, X1 + s.dX, X1 + s.dX + s.s.width(), Y1 + s.dY, Y1 + s.dY + s.s.height());
            }
        }
    }
}