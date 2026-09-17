using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using Snake2D;
using Snake2D.Util.Color;
using Snake2D.Util.File;
using Snake2D.Util.Sets;
using Snake2D.Util.Sprite;
using Util.SpriteComposer;

namespace Init.Sprite.UI
{
    public static class UIImageMaker
    {
        public const string split = "->";

        private readonly PATH path;
        private readonly KeyMap<SPRITE> map = new KeyMap<SPRITE>();
        public const int DIM = 32;
        public readonly SPRITE DUMMY;

        public UIImageMaker()
        {
            path = PATHS.SPRITE().GetFolder("image");
            DUMMY = new SPRITE.Imp(100, 100)
            {
                Render = (r, X1, X2, Y1, Y2) =>
                {
                    COLOR.ORANGE100.Render(r, X1, X2, Y1, Y2);
                }
            };
        }

        public SPRITE Get(string relPath)
        {
            return Get(relPath, null, null);
        }

        public SPRITE Get(Json json)
        {
            return Get(json.Value<string>("IMAGE"), json, "IMAGE");
        }

        public SPRITE Get(string relPath, Json json, string jsonKey)
        {
            Path p = PathParser.Get(path, relPath, json, jsonKey, 0);

            if (p == null)
                return DUMMY;

            string kk = "" + p.FullName;

            if (!map.ContainsKey(kk))
            {
                SnakeImage im = new SnakeImage(p);
                int iwidth = im.width / 2;
                int iheight = im.height;
                im.Dispose();

                if ((iwidth - 12) % DIM != 0 || (iheight - 12) % DIM != 0)
                {
                    PathParser.Error(p.FullName + " does not have the right dimensions: Should be a multiple of " + DIM + " squares. Look at other file for reference.", json, jsonKey);
                    return DUMMY;
                }

                int xs = (iwidth - 12) / DIM;
                int ys = (iheight - 12) / DIM;

                TILE_SHEET s = new ITileSheet(p, iwidth * 2, iheight)
                {
                    Init = (c, s, d) =>
                    {
                        s.Full.Init(0, 0, 1, 1, xs, ys, d.S32);
                        s.Full.Paste(true);
                        return d.S32.SaveNormal();
                    }
                }.Get();

                UIImage m = new UIImage(s, xs, ys);

                map.Put(kk, m);
            }

            return map.Get(kk);
        }

        private class UIImage : SPRITE
        {
            private const int TILE_SIZE = 32 * C.SCALE_NORMAL;
            private readonly int tilesX;
            private readonly int tilesY;
            private readonly int width;
            private readonly int height;

            private readonly TILE_SHEET sheet;

            public UIImage(TILE_SHEET sheet, int tilesX, int tilesY)
            {
                this.sheet = sheet;
                this.tilesX = tilesX;
                this.tilesY = tilesY;
                width = tilesX * TILE_SIZE;
                height = tilesY * TILE_SIZE;
            }

            public override int Width => width;

            public override int Height => height;

            public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                int startX = X1;
                int tile = 0;
                for (int ty = 0; ty < tilesY; ty++)
                {
                    X1 = startX;
                    for (int tx = 0; tx < tilesX; tx++)
                    {
                        sheet.Render(r, tile, X1, Y1);
                        X1 += TILE_SIZE;
                        tile++;
                    }
                    Y1 += TILE_SIZE;
                }
            }

            public override void RenderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
            {
                throw new NotImplementedException();
            }
        }
    }
}