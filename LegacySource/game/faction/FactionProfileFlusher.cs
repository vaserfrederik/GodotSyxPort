using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Game.Faction
{
    public class FactionProfileFlusher
    {
        private const string Name = "FACTION";

        public static void Flush(Player p)
        {
            try
            {
                var j = new JObject();
                j["RULER_NAME"] = p.RulerName.ToString();
                j["FACTION_NAME"] = p.Name.ToString();
                Color(j, p.Banner.ColorBG, "COLOR_BANNER_BACKGROUND");
                Color(j, p.Banner.ColorFG, "COLOR_BANNER_FOREGROUND");
                Color(j, p.Banner.ColorBorder, "COLOR_BANNER_BORDER");
                Color(j, p.Banner.ColorPole, "COLOR_BANNER_POLE");

                var cols = new JObject();

                foreach (var k in PlayerColors.Cats.Keys)
                {
                    foreach (var c in PlayerColors.Cats[k])
                    {
                        Color(cols, c.Color, c.Cat + "_" + c.Key);
                    }
                }

                string s = "";
                for (int i = 0; i < BitmapSprite.AREA; i++)
                {
                    s += p.Banner.Sprite.Is(i) ? "1" : "0";
                }
                j["BANNER_DATA"] = s;

                j["COLORS"] = cols;

                if (!Directory.Exists(Path.Combine(PATHS.Local.PROFILE, Name)))
                    Directory.CreateDirectory(Path.Combine(PATHS.Local.PROFILE, Name));
                File.WriteAllText(Path.Combine(PATHS.Local.PROFILE, Name, Name + ".json"), j.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Color(JObject j, COLOR c, string key)
        {
            string v = (2 * (c.Red & 0x0FF)) + "_" + 2 * (c.Green & 0x0FF) + "_" + 2 * (c.Blue & 0x0FF);
            j[key] = v;
        }

        public static bool CanLoad(Player p)
        {
            return Directory.Exists(Path.Combine(PATHS.Local.PROFILE, Name));
        }

        public static void Load(Player p)
        {
            if (!Directory.Exists(Path.Combine(PATHS.Local.PROFILE, Name)))
                return;
            try
            {
                var json = JObject.Parse(File.ReadAllText(Path.Combine(PATHS.Local.PROFILE, Name, Name + ".json")));
                p.RulerName.Clear().Add(json["RULER_NAME"].ToString());
                p.Name.Clear().Add(json["FACTION_NAME"].ToString());
                p.Banner.ColorBG.Set(new ColorImp(json["COLOR_BANNER_BACKGROUND"].ToString()));
                p.Banner.ColorFG.Set(new ColorImp(json["COLOR_BANNER_FOREGROUND"].ToString()));
                p.Banner.ColorBorder.Set(new ColorImp(json["COLOR_BANNER_BORDER"].ToString()));
                p.Banner.ColorPole.Set(new ColorImp(json["COLOR_BANNER_POLE"].ToString()));

                string s = json["BANNER_DATA"].ToString();
                for (int i = 0; i < s.Length; i++)
                {
                    p.Banner.Sprite.Set(i, s[i] == '1');
                }

                if (json.ContainsKey("COLORS"))
                {
                    var colorsJson = json["COLORS"] as JObject;
                    foreach (var k in PlayerColors.Cats.Keys)
                    {
                        foreach (var c in PlayerColors.Cats[k])
                        {
                            string kk = c.Cat + "_" + c.Key;
                            if (colorsJson.ContainsKey(kk))
                                c.Color.Set(new ColorImp(colorsJson[kk].ToString()));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}