using System;
using System.Collections.Generic;
using System.IO;

namespace Init.Sprite.Game
{
    public class GameSheets
    {
        private readonly Sheet[] overlays = new Sheet[SheetType.ALL.Count];
        private readonly List<Dictionary<string, List<Sheet>>> gsheets;
        private readonly List<Dictionary<string, List<TILE_SHEET>>> raws;
        public readonly Dictionary<string, SheetType> imap = new Dictionary<string, SheetType>();

        // public readonly Textures textures;

        public GameSheets() 
        {
            List<Dictionary<string, List<Sheet>>> m = new List<Dictionary<string, List<Sheet>>>(SheetType.ALL.Count);
            List<Dictionary<string, List<TILE_SHEET>>> r = new List<Dictionary<string, List<TILE_SHEET>>>(SheetType.ALL.Count);
            gsheets = m;
            raws = r;
            foreach (SheetType t in SheetType.ALL)
            {
                imap[t.path] = t;
                m.Add(new Dictionary<string, List<Sheet>>());
                r.Add(new Dictionary<string, List<TILE_SHEET>>());
            }
            foreach (SheetType t in SheetType.ALL)
            {
                if (t == SheetType.sCombo)
                    continue;
                if (t == SheetType.sBox)
                    continue;
                if (t == SheetType.sTex)
                    continue;
                this.overlays[t.index()] = sheets(t, "_OVERLAY", null)[0];
            }
        }

        public List<TILE_SHEET> raws(SheetType t, string file, Json error) 
        {
            if (raws[t.index()].ContainsKey(file))
            {
                return raws[t.index()][file];
            }
            List<TILE_SHEET> sh = t.make(file, error);
            raws[t.index()][file] = sh;

            List<Sheet> res = new List<Sheet>(sh.Count);
            foreach (TILE_SHEET s in sh)
                res.Add(new Sheet.Imp(t, s, true));

            gsheets[t.index()][file] = res;

            return sh;
        }

        public TILE_SHEET raw(SheetType t, Json json) 
        {
            json = json.json("GAME_TEXTURE");
            string file = json.value("FILE");
            int row = json.i("ROW");
            return raw(t, file, row, json);
        }

        public TILE_SHEET raw(SheetType t, string key, Json json) 
        {
            json = json.json(key);
            string file = json.value("FILE");
            int row = json.i("ROW");
            return raw(t, file, row, json);
        }

        public TILE_SHEET raw(SheetType t, string file, int row, Json error)
        {
            List<TILE_SHEET> li = raws(t, file, error);

            if (row >= li.Count)
            {
                if (error != null)
                    GAME.WarnLight(row + " is outside of the available sprites. In + file " + error.path());
                return SheetType.DUMMY;
            }
            return li[row];
        }

        public Sheet overlay(SheetType t)
        {
            return overlays[t.index()];
        }

        public List<Sheet> sheets(SheetType t, string file, Json error) 
        {
            if (gsheets[t.index()].ContainsKey(file))
                return gsheets[t.index()][file];
            raws(t, file, error);
            List<Sheet> ss = gsheets[t.index()][file];

            return ss;
        }

        public void add(SheetType t, List<Sheet> sh, string key)
        {
            if (gsheets[t.index()].ContainsKey(key))
            {
                throw new Exception(key);
            }
            gsheets[t.index()][key] = sh;
        }

        private bool[] adump = new bool[SheetType.ALL.Count];

        public List<SheetPair> sheets(SheetType type, Json json) 
        {
            string[] ss = json.values("FRAMES");

            SheetData[] datas = new SheetData[ss.Length];
            {
                SheetData odata = new SheetData(json);
                for (int i = 0; i < datas.Length; i++)
                {
                    datas[i] = odata;
                }
                if (json.has("OVERWRITE"))
                {
                    Json[] js = json.jsons("OVERWRITE");
                    for (int i = 0; i < datas.Length && i < js.Length; i++)
                    {
                        datas[i] = new SheetData(odata, js[i]);
                    }
                }
            }

            int i = 0;
            List<SheetPair> sheets = new List<SheetPair>(ss.Length);

            foreach (string k in ss)
            {
                SheetData data = datas[i++];

                if (k.Equals("-"))
                {
                    sheets.Add(new SheetPair(type.dummy(), data));
                    continue;
                }

                string[] chops = k.Split(':');

                if (chops.Length != 2)
                {
                    json.error("malformatted frame. Format is FILENAME:ROW Current is: " + k, k);
                }

                string file = chops[0].Trim();
                string snr = chops[1].Trim();

                PATH p = PATHS.SPRITE_GAME().getFolder(type.path);

                if (!gsheets[type.index()].ContainsKey(file))
                {
                    if (!p.exists(file))
                    {
                        string a = "";
                        if (!adump[type.index()])
                        {
                            a = Environment.NewLine + "Available: ";
                            adump[type.index()] = true;
                            foreach (string ke in gsheets[type.index()].Keys)
                            {
                                a += Environment.NewLine + ke;
                            }
                            foreach (string ke in p.getFiles())
                            {
                                if (!gsheets[type.index()].ContainsKey(ke))
                                {
                                    a += Environment.NewLine + ke;
                                }
                            }
                        }

                        GAME.WarnLight(p.get() + "/" + file + " does not exist and will be ignored. Referenced from: " + json.path() + a);
                        continue;
                    }
                }
                List<Sheet> shs = sheets(type, file, json);

                int nr = 0;
                try
                {
                    nr = int.Parse(snr);
                }
                catch (Exception e)
                {
                    json.error("malformatted Row. Format is FOLDER:FILENAME:ROW", k);
                    continue;
                }

                if (nr < 0 || nr >= shs.Count)
                {
                    GAME.WarnLight("ROW: " + nr + " in file: " + p.get(file) + " is out of bounds. Must specify a row of the image: " + json.path());
                    continue;
                }

                sheets.Add(new SheetPair(shs[nr], data));
            }

            return new List<SheetPair>(sheets);
        }
    }
}