using System;
using snake2d;
using util.spritecomposer;

namespace util.spritecomposer
{
    class TIleSheetPow2 : TILE_SHEET
    {
        private readonly int startTile;
        private readonly int mask;
        private readonly int scrollY;
        private readonly int quadSize;
        private readonly int tiles;
        private readonly Tile t;
        private readonly int scale;

        private static readonly TextureCoords[] texs = new TextureCoords[]
        {
            new TextureCoords(),
            new TextureCoords()
        };
        private static int tI = 0;

        public TIleSheetPow2(int scale, int tileSize, int startTile, int tilesX, int tiles)
        {
            t = Optimizer.Get(tileSize);
            this.scale = scale;

            this.startTile = startTile;

            int m = 1;
            int scroll = 1;
            if (tilesX % 2 != 0)
                throw new RuntimeException();
            while ((tilesX /= 2) > 1)
            {
                m = m << 1;
                m |= 1;
                scroll++;
            }
            mask = m;
            scrollY = scroll;

            quadSize = tileSize * scale;
            this.tiles = tiles;
        }

        public override void Render(SPRITE_RENDERER r, int tile, int x1, int y1)
        {
            t.Render(r, tile + startTile, x1, y1, scale);
        }

        public override TextureCoords GetTexture(int tile)
        {
            tile += startTile;
            int tx = tile & mask;
            int ty = tile >> scrollY;
            tI++;
            return texs[tI & 1].Get(
                (tx * quadSize / scale),
                t.StartY + (ty * t.Size),
                t.Size,
                t.Size
            );
        }

        public override void RenderTextured(TextureCoords t, int tile, int x1, int y1)
        {
            if (tile < 0)
                return;

            this.t.RenderTextured(t, tile + startTile, x1, y1, scale);

            // tile += startTile;
            // int tx = tile & mask;
            // int ty = tile >> scrollY;

            // int px = startPixelX + (tx * tileSize);
            // int py = startPixelY + (ty * tileSize);

            // CORE.Renderer().RenderTextured(
            //     x1, x1 + quadSize, y1, y1 + quadSize, 
            //     t,
            //     TextureCoords.Normal.Get(px, py, tileSize, tileSize)
            // );
        }

        public override void RenderTextured(TextureCoords t, int tile, int x1, int y1, int scale)
        {
            this.t.RenderTextured(t, tile + startTile, x1, y1, scale);
        }

        public override int Size()
        {
            return quadSize;
        }

        public override int Tiles()
        {
            return tiles;
        }

        public override void Render(SPRITE_RENDERER r, int tile, int x1, int x2, int y1, int y2)
        {
            if (tile < 0)
                return;

            tile += startTile;
            int tx = tile & mask;
            int ty = tile >> scrollY;

            int px = (tx * t.Size);
            int py = t.StartY + (ty * t.Size);

            r.RenderSprite(x1, x2, y1, y2, TextureCoords.Normal.Get(px, py, t.Size, t.Size));
        }
    }
}