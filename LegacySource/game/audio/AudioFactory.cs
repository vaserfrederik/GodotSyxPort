using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using init.paths;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.audio
{
    public abstract class AudioFactory<T>
    {
        private readonly KeyMap<T> map = new KeyMap<T>();
        protected readonly LinkedList<T> all = new LinkedList<T>();
        private readonly PATH path;

        private readonly T DUMMY;
        private readonly ArrayList<T> LDUMMY;
        private readonly string key;
        private readonly string split;

        protected AudioFactory(string key, PATH path, T DUMMY)
        {
            this.path = path;
            LDUMMY = new ArrayList<T>(DUMMY);
            this.key = key;

            string pp = path.Get().ToAbsolutePath().ToString();
            split = pp.Substring(pp.LastIndexOf("audio"));
            this.DUMMY = DUMMY;
        }

        public LIST<T> Create(string[] paths, Json json, string jsonKey)
        {
            LinkedList<T> res = new LinkedList<T>();

            foreach (string relPath in paths)
            {
                if (relPath.Equals("DUMMY"))
                    res.Add(LDUMMY);
                else
                {
                    LIST<Path> pps = PathParser.GetMany(path, relPath, json, jsonKey);
                    if (pps == null || pps.Count == 0)
                    {
                        res.Add(LDUMMY);
                    }
                    else
                    {
                        foreach (Path p in pps)
                        {
                            string pn = p.ToAbsolutePath().ToString();
                            string kk = pn.Substring(pn.LastIndexOf(split) + split.Length + 1);
                            if (!map.ContainsKey(kk))
                            {
                                T e = Create(all, p, kk);
                                all.Add(e);
                                map.Put(kk, e);
                            }
                            res.Add(map.Get(kk));
                        }
                    }
                }
            }

            return new ArrayList<T>(res);
        }

        public LIST<T> Read(Json json)
        {
            return Read(key, json);
        }

        public LIST<T> Read(string key, Json json)
        {
            if (!json.Has(key))
                return LDUMMY;
            if (json.ArrayIs(key))
                return Create(json.Values(key), json, key);
            return Create(new string[] { json.Value(key) }, json, key);
        }

        public LIST<T> All()
        {
            return all;
        }

        public KeyMap<T> Map()
        {
            return map;
        }

        public T DUMMY()
        {
            return DUMMY;
        }

        public LIST<T> LDUMMY()
        {
            return LDUMMY;
        }

        protected abstract T Create(LinkedList<T> all, Path p, string key);
    }
}