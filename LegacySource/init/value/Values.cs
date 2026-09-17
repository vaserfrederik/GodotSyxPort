using System;
using System.Collections.Generic;
using game;
using snake2d;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;

namespace init.value
{
    public class Values<T>
    {
        private readonly ArrayListGrower<Value<T>> all = new ArrayListGrower<Value<T>>();
        private readonly GValueCat<T> mommy;

        public Values(GValueCat<T> mommy)
        {
            this.mommy = mommy;
        }

        public void push(string targetKey, object path)
        {
            mommy.inits.add(new Promise(targetKey, path.ToString()));
        }

        public void push(Json json, params string[] notallowed)
        {
            push(GVALUES.KEY, json, notallowed);
        }

        public void push(string key, Json json, params string[] notallowed)
        {
            if (!json.has(key))
                return;

            string path = json.path() + ", line" + json.line(key);

            foreach (string k in json.values(key))
            {
                push(k, path);
            }
        }

        public void pushJson(string key, Json json, params string[] notallowed)
        {
            if (!json.has(key))
                return;

            string path = json.path() + ", line" + json.line(key);

            foreach (string k in json.values(key))
            {
                push(k, path);
            }
        }

        public LIST<Value<T>> all()
        {
            return all;
        }

        private class Promise : ACTION
        {
            public readonly string key;
            public readonly string path;

            public Promise(string key, string path)
            {
                this.key = key;
                this.path = path;
            }

            public void exe()
            {
                if (mommy.get(key) == null)
                {
                    if (!mommy.hasSpewed)
                    {
                        GAME.Warn(path + System.Environment.NewLine + "no value named : " + key + " available: " + System.Environment.NewLine + mommy.available());
                    }
                    else
                    {
                        LOG.ln("no value: " + key + "path: " + path);
                    }
                    mommy.hasSpewed = true;
                }
                else
                {
                    all.add(mommy.get(key));
                }
            }
        }
    }
}