using System;
using System.Collections.Generic;
using System.IO;
using Snake2D.Util.Color;
using Snake2D.Util.File;
using Snake2D.Util.Sets;

namespace Init.Resources
{
    public sealed class ResGDrink : ResG
    {
        public readonly COLOR Color;
        public readonly bool Serve;

        private ResGDrink(string key, int index, Json json) : base(index, key, RESOURCES.Map().Get(key, json))
        {
            Color = new ColorImp(json);
            Serve = !json.Bool("DONT_SERVE", false);
        }

        public static ResGroup<ResGDrink> Make(PATH pathData) throws IOException
        {
            string folder = "drinkable";
            PATH pd = pathData.GetFolder(folder);

            string[] files = pd.GetFiles();
            List<ResGDrink> res = new List<ResGDrink>(files.Length);

            foreach (string p in files)
            {
                Json j = new Json(pd.Get(p));
                ResGDrink g = new ResGDrink(p, res.Count, j);
                res.Add(g);
            }

            return new ResGroup<ResGDrink>("DRINKABLE", res);
        }
    }
}