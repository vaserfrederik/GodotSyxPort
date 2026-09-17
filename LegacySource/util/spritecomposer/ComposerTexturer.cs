using init.constant;
using snake2d.util.datatypes;
using util.spritecomposer;

namespace util.spritecomposer
{
    public sealed class ComposerTexturer : BODY_HOLDER
    {
        private readonly S source = new S();
        private readonly ComposerUtil c;
        private int destX, destY;
        public Rec body = new Rec();

        public ComposerTexturer(ComposerUtil c)
        {
            this.destX = 0;
            this.destY = 0;
            this.c = c;
        }

        public SpriteData Paste(int sourceX1, int sourceY1, int tilesX, int tilesY)
        {
            body.SetWidth(tilesX * C.T_PIXELS + 2 * 6).SetHeight(tilesY * C.T_PIXELS + 2 * 6);
            body.MoveX1Y1(sourceX1, sourceY1);

            int sx = sourceX1 + 6;
            int sy = sourceY1 + 6;

            //C
            Copy(sx, sy, 0, tilesX, 0, tilesY);
            Past(1, tilesX + 1, 1, tilesY + 1);

            //up
            Copy(sx, sy, 0, tilesX, tilesY - 1, tilesY);
            Past(1, tilesX + 1, 0, 1);

            //down
            Copy(sx, sy, 0, tilesX, 0, 1);
            Past(1, tilesX + 1, tilesY + 1, tilesY + 2);

            //left
            Copy(sx, sy, tilesX - 1, tilesX, 0, tilesY);
            Past(0, 1, 1, tilesY + 1);

            //left upper
            Copy(sx, sy, tilesX - 1, tilesX, tilesY - 1, tilesY);
            Past(0, 1, 0, 1);

            //right
            Copy(sx, sy, 0, 1, 0, tilesY);
            Past(tilesX + 1, tilesX + 2, 1, tilesY + 1);

            //up Right
            Copy(sx, sy, 0, 1, tilesY - 1, tilesY);
            Past(tilesX + 1, tilesX + 2, 0, 1);

            //down Right
            Copy(sx, sy, 0, 1, 0, 1);
            Past(tilesX + 1, tilesX + 2, tilesY + 1, tilesY + 2);

            //down Left
            Copy(sx, sy, tilesX - 1, tilesX, 0, 1);
            Past(0, 1, tilesY + 1, tilesY + 2);

            SpriteData s = SpriteData.Save(destX, destY, destX + C.T_PIXELS * (tilesX + 2), destY + C.T_PIXELS * (tilesY + 2), 24);
            destX += (tilesX + 2) * C.T_PIXELS;
            if (destX + (tilesX + 2) * C.T_PIXELS >= Resources.Dests.Chunk.DestWidth())
            {
                destX = 0;
                destY += 160;
            }
            return s;
        }

        private void Copy(int sx, int sy, int tx1, int tx2, int ty1, int ty2)
        {
            source.x1 = sx + tx1 * C.T_PIXELS;
            source.y1 = sy + ty1 * C.T_PIXELS;
            source.width = (tx2 - tx1) * C.T_PIXELS;
            source.height = (ty2 - ty1) * C.T_PIXELS;
            c.Copy(source);
        }

        private void Past(int tx1, int tx2, int ty1, int ty2)
        {
            DestChunk d = Resources.Dests.Chunk;
            d.rec.MoveX1Y1(destX + tx1 * C.T_PIXELS, destY + ty1 * C.T_PIXELS);
            d.rec.SetDim((tx2 - tx1) * C.T_PIXELS, (ty2 - ty1) * C.T_PIXELS);
            c.Paste(d);
        }

        private sealed class S : Source
        {
            private int x1, y1, width, height;

            public override RECTANGLE Body()
            {
                return null;
            }

            public override int Y1()
            {
                return y1;
            }

            public override int X1()
            {
                return x1;
            }

            public override int Width()
            {
                return width;
            }

            public override int Height()
            {
                return height;
            }
        }

        public override RECTANGLE Body()
        {
            return body;
        }
    }
}