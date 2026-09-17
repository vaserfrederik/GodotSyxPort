using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using game;
using settlement.stats;
using settlement.stats.muls;
using settlement.stats.stat;
using snake2d;
using snake2d.util.file;
using snake2d.util.sets;
using snake2d.util.sprite.text;

namespace settlement.stats.util
{
    public abstract class StatsJson
    {
        private static bool hasErrored = false;

        public StatsJson(Json json) : this("STATS", json)
        {
        }

        public StatsJson(string masterkey, Json json)
        {
            if (json.Has(masterkey))
                json = json.Json(masterkey);

            foreach (string k in json.Keys())
            {
                if (k.IndexOf('*') != -1)
                {
                    string kk = k.Substring(0, k.IndexOf('*'));
                    foreach (STAT s in STATS.All())
                    {
                        if (s.Key != null && Str.ContainsText(s.Key, kk))
                        {
                            DoWithTheJson(s, json, k);
                        }
                    }

                    foreach (StatMultiplier m in STATS.MULTIPLIERS().All())
                    {
                        if (m.Key != null && Str.ContainsText(m.Key, kk))
                        {
                            DoWithMultiplier(m, json, k);
                        }
                    }
                    continue;
                }

                if (STATS.STAT(k) != null && STATS.STAT(k).Key != null)
                {
                    DoWithTheJson(STATS.STAT(k), json, k);
                }
                else if (STATS.MULTIPLIERS().MAP.TryGet(k) != null)
                {
                    StatMultiplier m = STATS.MULTIPLIERS().MAP.TryGet(k);
                    DoWithMultiplier(m, json, k);
                }
                else
                {
                    HandleFault(json, k);
                }
            }
        }

        public abstract void DoWithMultiplier(StatMultiplier m, Json j, string key);

        public abstract void DoWithTheJson(STAT s, Json j, string key);

        public void HandleFault(Json j, string key)
        {
            string p = "No stat named: " + key + "  " + j.Path() + " line: " + j.Line(key);
            if (!hasErrored)
            {
                p += Environment.NewLine + "Available:" + Environment.NewLine;
                p += Available();
                GAME.Warn(p);
                hasErrored = true;
            }
            else
            {
                LOG.Ln(p);
            }
        }

        public string Available()
        {
            string k = "";
            LinkedList<string> ss = new LinkedList<string>();
            foreach (StatCollection c in STATS.COLLECTIONS())
                ss.AddLast(c.Key);
            foreach (StatMultiplier c in STATS.MULTIPLIERS().All())
                ss.AddLast(c.Key);
            foreach (STAT c in STATS.All())
                if (c.Key != null)
                    ss.AddLast(c.Key());
            ArrayList<string> asList = new ArrayList<string>(ss);
            asList.Sort();
            foreach (string s in asList)
                k += s + Environment.NewLine;
            return k;
        }
    }
}