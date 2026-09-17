using System;
using System.IO;
using Newtonsoft.Json;

namespace Init.Race.Home
{
    public sealed class RaceHome
    {
        private readonly RaceHomeClass DUMMY;
        private readonly RaceHomeClass[] all;

        public RaceHome(string key)
        {
            string filePath = Path.Combine(PATHS.INIT().GetFolder("race").GetFolder("home").ToString(), key);
            string jsonContent = File.ReadAllText(filePath);
            var json = JsonConvert.DeserializeObject<Json>(jsonContent);

            DUMMY = new RaceHomeClass();
            all = new RaceHomeClass[HCLASSES.ALL().Size()];
            for (int i = 0; i < all.Length; i++)
                all[i] = DUMMY;

            all[HCLASSES.CITIZEN().Index()] = new RaceHomeClass(json.Json(HCLASSES.CITIZEN().Key));
            all[HCLASSES.NOBLE().Index()] = new RaceHomeClass(json.Json(HCLASSES.NOBLE().Key));
            all[HCLASSES.SLAVE().Index()] = new RaceHomeClass(json.Json(HCLASSES.SLAVE().Key));
        }

        public RaceHomeClass Clas(Humanoid h)
        {
            if (h == null)
                return DUMMY;
            return all[h.Indu().Clas().Index()];
        }

        public RaceHomeClass Clas(Induvidual h)
        {
            return all[h.Clas().Index()];
        }

        public RaceHomeClass Clas(HCLASS c)
        {
            return all[c.Index()];
        }
    }
}