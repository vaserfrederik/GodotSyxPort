using System;
using System.Collections.Generic;
using System.IO;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.spritecomposer;

namespace init.race.appearence
{
    public sealed class RCrown
    {
        private ArrayListGrower<SPRITE> crowns = new ArrayListGrower<SPRITE>();
        private ArrayListGrower<SPRITE> raiders = new ArrayListGrower<SPRITE>();
        private ArrayListGrower<SPRITE> merc = new ArrayListGrower<SPRITE>();

        public RCrown(ExpandInit init, Json data)
        {
            Make(init, data, "CROWN", crowns);
            Make(init, data, "RAIDER", raiders);
            Make(init, data, "MERC", merc);
        }

        private static void Make(ExpandInit init, Json data, string key, ArrayListGrower<SPRITE> crowns)
        {
            if (!data.Has(key))
                data.Error("Not declared", key);
            if (data.Has(key))
            {
                if (data.ArrayIs(key))
                {
                    foreach (Json j in data.Jsons(key))
                    {
                        Make(j, init, crowns);
                    }
                }
                else
                {
                    data = data.Json(key);
                    Make(data, init, crowns);
                }
            }
        }

        private static void Make(Json json, ExpandInit init, ArrayListGrower<SPRITE> crowns)
        {
            int offX = json.I("OFFX", -48, 48);
            int offY = json.I("OFFY", -48, 48);

            string she = json.Value("FILE");
            if (!init.crowns.ContainsKey(she))
            {
                ResFolder f = PATHS.RACE().Folder("face").Folder("addon");
                if (!f.Sprite.Exists(she))
                {
                    LOG.Err(json.ErrorGet(f.Sprite.Get() + " No file named this.", she));
                    return;
                }

                TILE_SHEET sheet = new ITileSheet(f.Sprite.Get(she), 104, 36)
                {
                    protected override TILE_SHEET Init(ComposerUtil c, ComposerSources s, ComposerDests d)
                    {
                        int FRAMES = c.GetSource().Height / 36;
                        s.Full.Init(0, 0, 1, FRAMES, 5, 3, d.S8);
                        for (int i = 0; i < FRAMES; i++)
                            s.Full.SetVar(i).Paste(true);
                        return d.S8.SaveGame();
                    }
                }.Get();
                init.crowns[she] = sheet;
            }

            int tot = 5 * 3;
            TILE_SHEET sheet = init.crowns[she];
            int am = sheet.Tiles() / tot;

            for (int i = 0; i < am; i++)
            {
                int k = i * tot;
                SPRITE sprite = new SPRITE.Imp(40, 24)
                {
                    public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        int t = k;

                        int w = (X2 - X1) / 5;
                        int h = (Y2 - Y1) / 3;

                        int ox = offX * (X2 - X1) / 48;
                        int oy = offY * (Y2 - Y1) / 24;

                        for (int y = 0; y < 3; y++)
                        {
                            for (int x = 0; x < 5; x++)
                            {
                                sheet.Render(r, t, X1 + ox + x * w, X1 + ox + x * w + w, Y1 + oy + y * h, Y1 + oy + y * h + h);
                                t++;
                            }
                        }
                    }
                };
                crowns.Add(sprite);
            }
        }

        public LIST<SPRITE> Crowns()
        {
            return crowns;
        }

        public LIST<SPRITE> Raiders()
        {
            return raiders;
        }

        public LIST<SPRITE> Merc()
        {
            return merc;
        }
    }
}