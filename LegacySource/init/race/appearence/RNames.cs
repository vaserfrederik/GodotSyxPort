using System;
using System.Collections.Generic;
using init.paths;
using settlement.stats.colls;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.race.appearence
{
    public sealed class RNames
    {
        public readonly LIST<string> firstNames;
        public readonly LIST<string> lastNames;
        // public readonly LIST<string> firstNamesNoble;
        // public readonly LIST<string> lastNamesNoble;

        public RNames(Json json, KeyMap<string[]> names)
        {
            firstNames = names("NAMESET_FILE_FIRST", json, names);
            lastNames = names("NAMESET_FILE_SURNAME", json, names);
            // firstNamesNoble = names("NAMESET_FILE_FIRST_NOBLE", json, names);
            // lastNamesNoble = names("NAMESET_FILE_SURNAME_NOBLE", json, names);
        }

        static ArrayList<string> names(string key, Json json, KeyMap<string[]> names)
        {
            string v = json.value(key);
            if (names.ContainsKey(v))
                return new ArrayList<string>(names[v]);
            Json d = new Json(PATHS.TEXT().getFolder("names").getFolder("nameset").gets(v));
            string[] mm = d.texts("NAMES", 1, StatsAppearance.NAME_MAX);
            names.Put(v, mm);
            return new ArrayList<string>(mm);
        }
    }
}