using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace init.race.appearence
{
    public class RaceFrameMaker
    {
        public static readonly int TILES_X = 5;
        public static readonly int TILES_Y = 5;
        public static readonly int TILE_SIZE = 8;
        private static readonly int FRAMES_X = 4;

        private Dictionary<string, List<RaceFrameRaw>> map = new Dictionary<string, List<RaceFrameRaw>>();

        public readonly List<RaceFrameRaw> grit;
        public readonly List<RaceFrameRaw> blood;

        public RaceFrameMaker()
        {
            List<RaceFrameRaw> ff = new List<RaceFrameRaw>(4);
            for (int i = 0; i < ff.Capacity; i++)
            {
                ff.Add(Frame("_Overlays", i, null));
            }
            grit = ff;
            ff = new List<RaceFrameRaw>(4);
            for (int i = 0; i < ff.Capacity; i++)
            {
                ff.Add(Frame("_Overlays", 4 + i, null));
            }
            blood = ff;
        }

        public List<RaceFrameRaw> Read(Json json) throws IOException
        {
            string[] vals = json.Values("FRAMES");

            List<RaceFrameRaw> frames = new List<RaceFrameRaw>();

            foreach (string val in vals)
            {
                if (!Read(frames, val, json))
                {
                    GAME.WarnLight("Unable to parse key " + val + " these keys should be FILE:INDEX, where FILE is a file in the portrait folder, and INDEX is the frame in that file (integer)");
                }
            }

            return frames;
        }

        private bool Read(List<RaceFrameRaw> frames, string val, Json json)
        {
            string[] ss = val.Split(':');
            if (ss.Length == 2)
            {
                string file = ss[0].Trim();
                try
                {
                    int row = int.Parse(ss[1].Trim());
                    if (row != null)
                    {
                        frames.Add(Frame(file, row, json));
                    }
                    return true;
                }
                catch (FormatException)
                {
                    return false;
                }
            }
            return false;
        }

        private RaceFrameRaw Frame(string file, int row, Json error)
        {
            if (!PATHS.RACE().sprite.GetFolder("face").Exists(file))
            {
                if (error != null)
                {
                    GAME.Warn("the face file: " + file + " does not exist");
                    return dFrame;
                }
                throw new Exception(file);
            }

            if (!map.ContainsKey(file))
                map[file] = Sheet(file);

            List<RaceFrameRaw> rows = map[file];

            if (row < 0 || row >= rows.Count)
            {
                if (error != null)
                {
                    GAME.Warn("the row number for face file: " + file + " is out of bounds " + row + " " + (rows.Count - 1));
                    return dFrame;
                }
                throw new Exception(file);
            }

            return rows[row];
        }

        private List<RaceFrameRaw> Sheet(string file)
        {
            Path pp = PATHS.RACE().sprite.GetFolder("face").Get(file);

            SnakeImage im = new SnakeImage(pp);
            int ww = 416;
            int hh = 60;
            if (im.width != ww || im.height < hh)
            {
                throw new Exception(pp.ToString() + " has wrong dimensions " + im.width + " " + im.height);
            }

            int framesY = im.height / hh;

            int[][] offYs = new int[framesY][FRAMES_X];
            int[][] startYs = new int[framesY][FRAMES_X];
            int[][] rowss = new int[framesY][FRAMES_X];

            for (int fy = 0; fy < framesY; fy++)
            {
                for (int fx = 0; fx < FRAMES_X; fx++)
                {
                    int sx = 6 + 52 * fx;
                    int sy = 6 + 60 * fy;

                    int y1 = sy;
                    int y2 = y1 + 48;

                    outer: for (int dy = 0; dy < 48; dy++)
                    {
                        for (int dx = 0; dx < 40; dx++)
                        {
                            int x = sx + dx;
                            if ((im.rgb[x, y1] & 0xFF) != 0)
                                break outer;
                        }
                        y1++;
                    }

                    outer: for (int dy = 0; dy < 48; dy++)
                    {
                        for (int dx = 0; dx < 40; dx++)
                        {
                            int x = sx + dx;
                            if ((im.rgb[x, y2 - 1] & 0xFF) != 0)
                                break outer;
                        }
                        y2--;
                    }

                    int offY = y1 - sy;

                    int rows = (int)Math.Ceiling((double)(y2 - y1) / TILE_SIZE);

                    if (y1 + rows * TILE_SIZE > sy + 48)
                    {
                        y1 = sy + 48 - rows * TILE_SIZE;
                        offY -= offY - (y1 - sy);
                    }

                    offYs[fy][fx] = offY;
                    startYs[fy][fx] = y1 - sy;
                    rowss[fy][fx] = rows;
                }
            }

            im.Dispose();

            new IInit(pp, ww, hh);

            List<RaceFrameRaw> res = new List<RaceFrameRaw>();
            for (int fy = 0; fy < framesY; fy++)
            {
                for (int fx = 0; fx < FRAMES_X; fx++)
                {
                    int offY = offYs[fy][fx];
                    int startY = startYs[fy][fx];
                    int rows = rowss[fy][fx];

                    int var = fx + fy * FRAMES_X;

                    TILE_SHEET sheet = new ITileSheet()
                    {
                        Init = (ComposerUtil c, ComposerSources s, ComposerDests d) =>
                        {
                            if (rows == 0)
                            {
                                return dFrame.sheet;
                            }
                            s.full.Init(0, startY, 4, framesY, TILES_X, 6, d.s8);
                            s.full.SetVar(var);
                            s.full.SetSkip(rows * TILES_X, 0);
                            s.full.Paste(true);
                            return d.s8.Save(1);
                        }
                    }.Get();
                    res.Add(new RaceFrameRaw(this, sheet, offY));
                }
            }

            return res;
        }

        private readonly TILE_SHEET DUMMY = new TILE_SHEET()
        {
            Tiles = () => TILES_X,
            Size = () => TILE_SIZE,
            RenderTextured = (TextureCoords texture, int tile, int x1, int x2, int scale) =>
            {
                // TODO Auto-generated method stub
            },
            RenderTextured = (TextureCoords texture, int tile, int x1, int y1) =>
            {
                // TODO Auto-generated method stub
            },
            Render = (SPRITE_RENDERER r, int tile, int x1, int x2, int y1, int y2) =>
            {
                // TODO Auto-generated method stub
            },
            GetTexture = (int tile) =>
            {
                // TODO Auto-generated method stub
                return null;
            }
        };

        private readonly RaceFrameRaw dFrame = new RaceFrameRaw(this, DUMMY, 0);
    }
}