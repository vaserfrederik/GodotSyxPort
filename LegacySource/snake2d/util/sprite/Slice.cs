using System;

namespace Snake2D.Util.Sprite
{
    public sealed class Slice : ITileSheet
    {
        private readonly ITileSheet combo;
        private readonly int start;
        private readonly int size;

        public Slice(ITileSheet combo, int start, int end)
        {
            this.combo = combo;
            this.start = start;
            this.size = end - this.start;
        }

        public void Render(ISpriteRenderer r, int tile, int x1, int x2, int y1, int y2)
        {
            combo.Render(r, tile + start, x1, x2, y1, y2);
        }

        public void RenderTextured(TextureCoords texture, int tile, int x1, int y1)
        {
            combo.RenderTextured(texture, tile + start, x1, y1);
        }

        public void RenderTextured(TextureCoords texture, int tile, int x1, int x2, int scale)
        {
            combo.RenderTextured(texture, tile + start, x1, x2, scale);
        }

        public TextureCoords GetTexture(int tile)
        {
            return combo.GetTexture(tile + start);
        }

        public int Size()
        {
            return combo.Size();
        }

        public int Tiles()
        {
            return size;
        }
    }
}