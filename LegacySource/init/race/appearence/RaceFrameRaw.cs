using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.Misc;
using Snake2D.Util.Sprite;

namespace Init.Race.Appearance
{
    class RaceFrameRaw
    {
        public readonly TileSheet Sheet;
        public readonly int OffY;
        public const int WIDTH = RaceFrameMaker.TILES_X * RaceFrameMaker.TILE_SIZE;
        public const int HEIGHT = RaceFrameMaker.TILES_Y * RaceFrameMaker.TILE_SIZE;
        private readonly int hh;
        private readonly RaceFrameMaker f;

        public RaceFrameRaw(RaceFrameMaker f, TileSheet sheet, int offY)
        {
            this.Sheet = sheet;
            this.OffY = offY;
            this.f = f;
            hh = sheet.Tiles() / RaceFrameMaker.TILES_X;
        }

        public void Render(SpriteRenderer r, int X1, int Y1, int scale)
        {
            Y1 += OffY * scale;

            int d = scale * RaceFrameMaker.TILE_SIZE;

            int i = 0;
            for (int y = 0; y < hh; y++)
            {
                for (int x = 0; x < RaceFrameMaker.TILES_X; x++)
                {
                    Sheet.Render(r, i++, X1 + x * d, X1 + x * d + d, Y1 + y * d, Y1 + y * d + d);
                }
            }
        }

        public void RenderOverlay(SpriteRenderer r, int X1, int Y1, int scale, double blood, double grit, Color bloodC)
        {
            int bi = Clamp.I((int)(blood * 4), 0, 4) - 1;
            int gi = Clamp.I((int)(grit * 4), 0, 4) - 1;

            Y1 += OffY * scale;

            int d = scale * RaceFrameMaker.TILE_SIZE;
            Opacity.O99.Bind();
            if (gi >= 0)
            {
                int i = 0;
                for (int y = 0; y < hh; y++)
                {
                    for (int x = 0; x < RaceFrameMaker.TILES_X; x++)
                    {
                        Sheet.RenderTextured(f.Grit[gi].Sheet.GetTexture(i), i++, X1 + x * d, Y1 + y * d, scale);
                    }
                }
            }

            if (bi >= 0)
            {
                bloodC.Bind();
                int i = 0;
                for (int y = 0; y < hh; y++)
                {
                    for (int x = 0; x < RaceFrameMaker.TILES_X; x++)
                    {
                        Sheet.RenderTextured(f.Blood[bi].Sheet.GetTexture(i), i++, X1 + x * d, Y1 + y * d, scale);
                    }
                }
                Color.Unbind();
            }
            Opacity.Unbind();
        }
    }
}