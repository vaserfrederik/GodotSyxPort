using System;

namespace Snake2D.Util.Sprite
{
    public class TileTexture : DIMENSION
    {
        private readonly int sx, sy;
        private readonly int width, height;
        private readonly int size;
        private readonly TextureCoords tex = new TextureCoords();

        public TileTexture(int tileSize, int tilesX, int tilesY, int px, int py)
        {
            this.sx = px;
            this.sy = py;
            this.width = tilesX;
            this.height = tilesY;
            this.size = tileSize;
        }

        public override int Width()
        {
            return width;
        }

        public override int Height()
        {
            return height;
        }

        public TextureCoords Get(int tx, int ty, double offX, double offY)
        {
            int px = (tx & (width - 1)) * size;
            px += size * offX;
            int py = (ty & (height - 1)) * size;
            py += size * offY;
            return tex.Get(sx + px, sy + py, size, size);
        }

        public TextureCoords Get(double tx, double ty)
        {
            int x = (int)tx;
            int y = (int)ty;

            x = MATH.Mod(x, (width) * size);
            y = MATH.Mod(y, (height) * size);
            return tex.Get(sx + x, sy + y, size, size);
        }

        public TileTextureScroller Scroller(double speedx, double speedy)
        {
            return new TileTextureScroller(this, speedx, speedy);
        }

        public class TileTextureScroller
        {
            private readonly TextureCoords tex = new TextureCoords();
            private readonly TileTexture scroller;
            private double speedx, speedy;
            public double dx;
            public double dy;
            private readonly int mx, my;

            public TileTextureScroller(TileTexture scroller, double speedx, double speedy)
            {
                this.scroller = scroller;
                this.speedx = speedx;
                this.speedy = speedy;
                mx = scroller.Width * scroller.size;
                my = scroller.Height * scroller.size;
                dx = RND.rFloat() * (scroller.Width) * scroller.size;
                dy = RND.rFloat() * (scroller.Height) * scroller.size;
            }

            public void Update(double ds)
            {
                Update(ds * speedx, ds * speedy);
            }

            public void Update(double x, double y)
            {
                dx += x;
                dy += y;

                dx = MATH.Mod(dx, mx);
                dy = MATH.Mod(dy, my);
            }

            public TextureCoords Get(int tileX, int tileY)
            {
                int px = (int)x1(tileX);
                int py = (int)y1(tileY);
                return tex.Get(px, py, scroller.size, scroller.size);
            }

            public float x1(int tileX)
            {
                double x = tileX * scroller.size + dx;
                x %= mx;
                return scroller.sx + (float)x;
            }

            public float y1(int tileY)
            {
                double y = tileY * scroller.size + dy;
                y %= my;
                return scroller.sy + (float)y;
            }
        }
    }
}