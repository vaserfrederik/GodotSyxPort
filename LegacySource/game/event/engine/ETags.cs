using System;
using System.Collections.Generic;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.event.engine
{
    internal sealed class ETags
    {
        public readonly string[] adds;
        public readonly string[] removes;
        public readonly string[] allows;
        public readonly string[] allows_not;

        public ETags(Json d)
        {
            if (d.Has("TAGS"))
            {
                d = d.Json("TAGS");
                adds = Read(d, "ADD");
                removes = Read(d, "REMOVE");
                allows = Read(d, "ALLOW");
                allows_not = Read(d, "ALLOW_NOT");
                d.CheckUnused();
            }
            else
            {
                adds = new string[0];
                removes = adds;
                allows = adds;
                allows_not = adds;
            }
        }

        private string[] Read(Json d, string key)
        {
            if (d.Has(key))
                return d.Values(key);
            else
                return new string[0];
        }

        public bool Can(KeyMap<bool> tags)
        {
            foreach (string k in allows)
            {
                if (!tags.ContainsKey(k) || tags.Get(k) == false)
                    return false;
            }

            foreach (string k in allows_not)
            {
                if (tags.ContainsKey(k) && tags.Get(k) == true)
                    return false;
            }
            return true;
        }
    }
}