using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Init.Race.Bio
{
    internal class BioOpinionData
    {
        private readonly string[] funnies;
        private readonly string[] full;
        private readonly Opinion[] all;
        private readonly Str stmp = new Str(256);

        private readonly string[][] titles;

        public BioOpinionData(JObject org, JObject spe) 
        {
            titles = new string[][]
            {
                tt("HAPPY", org, spe),
                tt("HAPPY_SOSO", org, spe),
                tt("HAPPY_NO", org, spe)
            };

            Opinion def = new Opinion();
            for (int i = 0; i < all.Length; i++)
                if (all[i] == null)
                    all[i] = def;
            foreach (var s in STATS.All())
            {
                all[s.Index()] = new Opinion().SetMore(s.Info().DefOpinion.More).SetLess(s.Info().DefOpinion.Less);
            }

            funnies = tt("FUNNY", org, spe);
            full = tt("NOTHING", org, spe);

            List<JObject> js = new List<JObject> { org };
            if (spe != null)
                js.Add(spe);

            foreach (var jj in js)
            {
                JObject j = jj["STATS_MORE"] as JObject;
                new StatsJson(j)
                {
                    DoWithTheJson = (STAT s, JObject j, string key) =>
                    {
                        string[] tt = BioLine.Insert.Check(j[key].ToObject<string[]>());
                        if (tt.Length == 0)
                            all[s.Index()].SetMore(BioLine.Insert.Check(j[key].ToObject<string[]>()));
                        all[s.Index()].SetMore(BioLine.Insert.Check(j[key].ToObject<string[]>()));
                    },
                    DoWithMultiplier = (StatMultiplier m, JObject j, string key) =>
                    {

                    }
                };

                j = jj["STATS_LESS"] as JObject;
                new StatsJson(j)
                {
                    DoWithTheJson = (STAT s, JObject j, string key) =>
                    {
                        all[s.Index()].SetLess(BioLine.Insert.Check(j[key].ToObject<string[]>()));
                    },
                    DoWithMultiplier = (StatMultiplier m, JObject j, string key) =>
                    {

                    }
                };
            }
        }

        private string[] tt(string key, JObject org, JObject spe)
        {
            if (spe != null && spe.ContainsKey(key))
                return BioLine.Insert.Check(spe[key].ToObject<string[]>());
            return BioLine.Insert.Check(org[key].ToObject<string[]>() ?? Array.Empty<string>());
        }

        public string Get(STAT s, Humanoid a, long ran)
        {
            if (s.Standing().Definition(a.Race()).Get(a.Indu().Clas()).From > s.Standing().Definition(a.Race()).Get(a.Indu().Clas()).To)
                return Less(s, ran, a);
            else
                return More(s, ran, a);
        }

        public string Title(Humanoid h, double value)
        {
            if (value > 0.95)
                return Get(h, titles[0], STATS.RAN().GetL(h.Indu(), 0));
            if (value > 0.8)
                return Get(h, titles[1], STATS.RAN().GetL(h.Indu(), 0));
            return Get(h, titles[2], STATS.RAN().GetL(h.Indu(), 0));
        }

        public string Funny(long ran)
        {
            return funnies[Math.Mod((int)ran, funnies.Length)];
        }

        public string Full(long ran)
        {
            return full[Math.Mod((int)ran, full.Length)];
        }

        private string Get(Humanoid i, string[] r, long ran)
        {
            if (r.Length == 0)
                return Dic.Empty;
            stmp.Clear().Add(r[Math.Mod((int)ran, r.Length)]);
            BioLine.Insert.Set(stmp, i);
            return stmp.ToString();
        }

        private string More(STAT stat, long ran, Humanoid a)
        {
            Opinion i = all[stat.Index()];
            if (i.More.Length == 0)
                return Dic.Empty;
            string s = i.More[Math.Mod((int)ran, i.More.Length)];
            stmp.Clear().Add(s);
            i.Insert(stmp, stat, a);
            return stmp.ToString();
        }

        private string Less(STAT stat, long ran, Humanoid a)
        {
            Opinion i = all[stat.Index()];
            if (i.Less.Length == 0)
                return Dic.Empty;
            string s = i.Less[Math.Mod((int)ran, i.Less.Length)];
            stmp.Clear().Add(s);
            i.Insert(stmp, stat, a);
            return stmp.ToString();
        }
    }
}