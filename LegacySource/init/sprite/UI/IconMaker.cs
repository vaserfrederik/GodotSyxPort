using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.spritecomposer;

namespace init.sprite.UI
{
    public class IconMaker
    {
        public static readonly string split = "->";

        private readonly PATH path;
        private readonly KeyMap<TILE_SHEET> map = new KeyMap<TILE_SHEET>();
        public readonly int DIM;
        public readonly Icon DUMMY;

        public IconMaker(string root, int dim)
        {
            path = PATHS.SPRITE().getFolder("icon").getFolder(root);
            this.DIM = dim;
            DUMMY = new Icon(DIM, COLOR.ORANGE100);
        }

        public Icon get(string relPath, int nr) => get(relPath + PathParser.split + nr, null, null);

        public Icon get(string relPath, Json json, string jsonKey)
        {
            var p = PathParser.get(path, relPath, json, jsonKey, 1);

            if (p == null)
                return DUMMY;

            var ss = relPath.Split(PathParser.split);

            if (ss.Length <= 0)
            {
                string ne = "The row of the icon file is missing at the end of the path. Sytax is folder->folder->file->0, where 0 is the row of the icon file.";
                PathParser.error(ne, json, jsonKey);
                return DUMMY;
            }

            int nr = -1;

            try
            {
                nr = int.Parse(ss[ss.Length - 1]);
            }
            catch (FormatException)
            {
                string ne = "The final element in the path must be a number. Sytax is folder->folder->file->0, where 0 is the row of the icon file.";
                PathParser.error(ne, json, jsonKey);
                return DUMMY;
            }

            string ne = "The selected row of the file is out of bounds for the file. Sytax is folder->folder->file->0, where 0 is the row of the icon file.";

            if (nr < 0)
            {
                PathParser.error(ne, json, jsonKey);
            }

            var sheet = sheet(p, json, jsonKey);
            if (sheet == null)
                return DUMMY;

            if (nr >= sheet.tiles())
            {
                PathParser.error(ne, json, jsonKey);
                return DUMMY;
            }

            return new IconSheet(DIM, sheet, nr);
        }

        private TILE_SHEET sheet(Path path, Json json, string jsonKey) throws IOException
        {
            string kk = "" + path.toAbsolutePath();

            if (!map.ContainsKey(kk))
            {
                var im = new SnakeImage(path);
                final int iwidth = im.width / 2;
                final int iheight = im.height;
                im.dispose();

                if ((iwidth - 6) % (DIM + 6) != 0 || (iheight - 6) % (DIM + 6) != 0)
                {
                    PathParser.error(path.toAbsolutePath() + " does not have the right dimensions: Should be a multiple of " + DIM + " squares. Look at other file for reference.", json, jsonKey);
                    return null;
                }

                int xs = (iwidth - 6) / (DIM + 6);
                int ys = (iheight - 6) / (DIM + 6);

                var s = new ITileSheet(path, iwidth * 2, iheight)
                {
                    protected override TILE_SHEET init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        Tile t = d.s16;
                        if (DIM == 24)
                            t = d.s24;
                        if (DIM == 32)
                            t = d.s32;
                        s.singles.init(0, 0, 1, 1, xs, ys, t);
                        s.singles.paste(true);
                        return t.saveGame();
                    }
                }.get();
                map.put(kk, s);
            }
            return map.get(kk);
        }
    }
}