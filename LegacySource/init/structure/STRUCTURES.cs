using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using snake2d.util.file;
using snake2d.util.sets;
using util.keymap;

namespace init.structure
{
    public class STRUCTURES : InitResource
    {
        private const string KEY = "STRUCTURE";
        private static Structure MUD;
        private static RMAPS<Structure> map;

        public STRUCTURES(INIT init) : base(init)
        {
            ResFolder f = path();

            LinkedList<string> keys = new LinkedList<string>();
            keys.Add("_MUD");
            keys.Add(f.init.GetFiles());

            ArrayList<Structure> all = new ArrayList<Structure>(keys.size);
            foreach (string key in keys)
            {
                Json d = new Json(f.init.Get(key));
                Json t = new Json(f.text.Get(key));
                new Structure(key, all, d, t);
            }
            map = new RMAPS<Structure>(KEY, all);
            MUD = all.Get(0);
        }

        public static LIST<Structure> all()
        {
            return map.all();
        }

        public static RMAPS<Structure> map()
        {
            return map;
        }

        public static Structure mud()
        {
            return MUD;
        }

        public static ResFolder path()
        {
            string f = KEY.ToLower(CultureInfo.InvariantCulture);
            return PATHS.SETT().folder(f);
        }
    }
}