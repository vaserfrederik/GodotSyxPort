using System;
using System.IO;
using System.Collections.Generic;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.resources
{
    public sealed class ResGEat : ResG
    {
        public readonly bool Serve;

        private ResGEat(string key, int index, Json json) : base(index, key, RESOURCES.map().get(key, json))
        {
            Serve = !json.bool("DONT_SERVE", false);
        }

        public static ResGroup<ResGEat> make(final PATH pathData) throws IOException
        {
            string folder = "edible";
            final PATH pd = pathData.getFolder(folder);

            string[] files = pd.getFiles();
            final ArrayList<ResGEat> res = new ArrayList<ResGEat>(files.length);

            foreach (string p in files)
            {
                Json j = new Json(pd.gets(p));
                ResGEat g = new ResGEat(p, res.size(), j);
                res.add(g);
            }

            return new ResGroup<ResGEat>("EDIBLE", res);
        }
    }
}