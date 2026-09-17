using System;
using System.Collections.Generic;
using System.Linq;
using util.data;
using util.sets;
using snake2d.util.file;
using snake2d.util.sprite;
using init.sprite.UI;

namespace init.value
{
    public class GValueCat<T>
    {
        private readonly KeyMap<Value<T>> map = new KeyMap<Value<T>>();
        private LIST<Value<T>> all;
        private LinkedList<ACTION> inits = new LinkedList<ACTION>();
        private bool hasSpewed = false;
        public readonly string key;
        public Locks LOCK = new Locks();

        public GValueCat(string key)
        {
            this.key = key;
        }

        public void clear()
        {
            map.clear();
            inits.clear();
            hasSpewed = false;
            LOCK.clear();
        }

        public KeyMap<Value<T>> map()
        {
            return map;
        }

        private void init()
        {
            foreach (ACTION a in inits)
                a.exe();
            inits.clear();
            LOCK.init();
            all = map.allSorted();
        }

        public LIST<Value<T>> all()
        {
            return all;
        }

        public void push(Value<T> value)
        {
            if (map.containsKey(value.key))
                throw new RuntimeException("Another value has the same key: " + value.key);
            map.put(value.key, value);
        }

        public void push(string key, CharSequence name, SPRITE icon, DOUBLE_O<T> value, bool isPercentage)
        {
            Value<T> v = new Value<T>(key, icon, name, value, isPercentage, false);
            push(v);
        }

        public void push(string key, CharSequence name, SPRITE icon, DOUBLE_O<T> value, bool isPercentage, bool isBool)
        {
            Value<T> v = new Value<T>(key, icon, name, value, isPercentage, isBool);
            push(v);
        }

        public void push(string key, CharSequence name, SPRITE icon, BOOLEANO<T> value)
        {
            DOUBLE_O<T> v = new DOUBLE_O<T>()
            {
                getD = t => value.is(t) ? 1 : 0
            };

            push(key, name, icon, v, false, true);
        }

        public void push(string key, CharSequence name, SPRITE icon, DOUBLE_O<T> value)
        {
            push(key, name, icon, value, true);
        }

        public void pushI(string key, CharSequence name, SPRITE icon, INT_O<T> value)
        {
            DOUBLE_O<T> v = new DOUBLE_O<T>()
            {
                getD = t => value.get(t)
            };

            push(key, name, icon, v, false);
        }

        public Value<T> get(string key)
        {
            return map.get(key);
        }

        private bool eee = false;
        public LIST<Value<T>> get(string key, Json error)
        {
            ArrayListGrower<Value<T>> res = new ArrayListGrower<Value<T>>();
            if (key.IndexOf('*') > 0)
            {
                string s = key.Substring(0, key.IndexOf('*'));
                foreach (Value<T> v in all)
                {
                    if (v.key.StartsWith(s))
                        res.add(v);
                }
            }
            else
            {
                Value<T> v = map.get(key);
                if (v == null)
                {
                    string e = error.errorGet("No " + this.key + " named: " + key, key);
                    if (!eee)
                    {
                        eee = true;
                        e += " Available:";
                        e += Environment.NewLine;
                        e += map.keysString();
                    }
                    LOG.err(e);
                }
                else
                {
                    res.add(v);
                }
            }
            return res;
        }

        public string available()
        {
            return map.keysString();
        }

        public sealed class Locks
        {
            private readonly KeyMap<Lockable<T>> map = new KeyMap<Lockable<T>>();
            private LinkedList<ACTION> inits = new LinkedList<ACTION>();
            private bool hasSpewed = false;
            public readonly Lockable<T> empty = new Lockable<T>("", "", "", UI.icons().s.DUMMY, GValueCat<T>.this);
            private Locks() { }

            public void init()
            {
                foreach (ACTION a in inits)
                    a.exe();
                inits.clear();
            }

            private void clear()
            {
                map.clear();
                inits.clear();
                hasSpewed = false;
            }

            public Lockable<T> get(string key)
            {
                return map.get(key);
            }

            public string available()
            {
                return map.keysString();
            }

            public Lockable<T> push(string key, CharSequence name, CharSequence desc, SPRITE icon)
            {
                key = key.Replace("__", "_");
                Lockable<T> t = new Lockable<T>(key, name, desc, icon, GValueCat<T>.this);
                map.put(key, t);
                return t;
            }

            public Lockable<T> push()
            {
                Lockable<T> t = new Lockable<T>("", "", "", UI.icons().s.DUMMY, GValueCat<T>.this);
                return t;
            }
        }

        public abstract class LockJson
        {
            private readonly Json json;

            public LockJson(string key, Json j)
            {
                this.json = j.json(key);
                inits.add(new ACTION()
                {
                    exe = () =>
                    {
                        foreach (string keyComp in json.keys())
                        {
                            COMPARATOR comp = COMPARATOR.map.get(keyComp, json);
                            if (comp != null)
                            {
                                Json j = json.json(keyComp);
                                foreach (string k in j.keys())
                                {
                                    Value<T> v = get(k);
                                    if (v == null)
                                        continue;
                                    callback(comp, v, k, j);
                                }
                            }
                        }
                    }
                });
            }

            public abstract void callback(COMPARATOR comp, Value<T> value, string key, Json json);
        }
    }
}