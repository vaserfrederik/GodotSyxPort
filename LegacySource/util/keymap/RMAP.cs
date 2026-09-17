using System;
using System.Collections.Generic;
using System.Linq;

namespace Util.Keymap
{
    public class RMAP<T> where T : MAPPED, new()
    {
        public static readonly string WILDCARD = "*";
        public readonly string key;
        protected readonly KeyMap<T> map;
        private readonly LIST<T> all;

        public RMAP(string key, LIST<T> all)
        {
            this.key = key;
            this.map = new KeyMap<T>();
            foreach (T t in all)
                map.Put(t.Key(), t);
            this.all = all;
            map.Expand();
        }

        public T Read(Json reader)
        {
            return Read(this.key(), reader);
        }

        public T Read(string key, Json reader)
        {
            string value = reader.Value(key);
            T t = TryGet(value);
            if (t != null)
            {
                return t;
            }
            string k = "   Available: ";
            foreach (string s in Available())
                k += s + ", ";
            reader.Error("no " + this.key() + " named: " + value + k, key);
            return null;
        }

        public T ReadTry(string key, Json reader)
        {
            if (reader.Has(key))
            {
                string value = reader.Value(key);
                T t = TryGet(value);
                if (t != null)
                {
                    return t;
                }
                string k = "   Available: ";
                foreach (string s in Available())
                    k += s + ", ";
                GAME.WarnLight(reader.ErrorGet("no " + this.key() + " named: " + value + k, key));
                return null;
            }
            return null;
        }

        public T ReadTry(Json reader)
        {
            return ReadTry(key(), reader);
        }

        public T Get(string key, Json error)
        {
            T t = TryGet(key);
            if (t != null)
            {
                return t;
            }
            string k = "   Available: ";
            foreach (string s in Available())
                k += s + ", ";
            if (key.EndsWith(" "))
            {
            }
            if (error == null)
                throw new RuntimeException("no " + this.key() + " named: " + key + (key.EndsWith(" ") ? "It ends with space!" : "") + k);
            error.Error("no " + this.key() + " named: " + key + (key.EndsWith(" ") ? "It ends with space!" : "") + k, key);
            return null;
        }

        public LIST<T> Get(string s)
        {
            ArrayListGrower<T> res = new ArrayListGrower<T>();

            if (s.IndexOf(WILDCARD) >= 0)
            {
                string beg = s.Substring(0, s.IndexOf(WILDCARD));
                foreach (string k in map.Keys())
                {
                    if (k.StartsWith(beg))
                    {
                        T t = map.Get(k);
                        if (!res.Contains(t))
                            res.Add(t);
                    }
                }
            }
            else
            {
                T t = TryGet(s);
                if (t != null)
                {
                    res.Add(t);
                }
            }

            return res;
        }

        public T GetWarn(string key, Json reader)
        {
            T t = TryGet(key);
            if (t != null)
            {
                return t;
            }
            string k = "   Available: ";
            foreach (string s in Available())
                k += s + ", ";
            GAME.WarnLight(reader.ErrorGet("no " + this.key() + " named: " + key + k, key));
            return null;
        }

        public LIST<T> ReadMany(string key, Json reader)
        {
            if (!reader.Has(key))
                return new ArrayList<T>();

            string[] values = reader.Values(key);
            ArrayListGrower<T> res = new ArrayListGrower<T>();
            foreach (string s in values)
            {
                if (s.IndexOf(WILDCARD) >= 0)
                {
                    string beg = s.Substring(0, s.IndexOf(WILDCARD));
                    foreach (string k in map.Keys())
                    {
                        if (k.StartsWith(beg))
                        {
                            T t = map.Get(k);
                            if (!res.Contains(t))
                                res.Add(t);
                        }
                    }
                }
                else
                {
                    T t = TryGet(s);
                    if (t != null)
                    {
                        res.Add(t);
                    }
                    else
                    {
                        string k = "   Available: ";
                        foreach (string ss in Available())
                            k += ss + ", ";
                        reader.Error("no " + this.key() + " named: " + s + k, key);
                    }
                }
            }

            return res;
        }

        public LIST<T> ReadMany(Json reader)
        {
            return ReadMany(this.key() + "S", reader);
        }

        public LIST<T> ReadManyWarn(Json reader)
        {
            return ReadManyWarn(this.key() + "S", reader);
        }

        public LIST<T> ReadManyWarn(string value, Json reader)
        {
            if (!reader.Has(value))
                return new ArrayList<T>();

            string[] values = reader.Values(value);
            foreach (string v in values)
            {
                if (v.Equals("*"))
                {
                    return new ArrayList<T>(All());
                }
            }

            ArrayList<T> res = new ArrayList<T>(values.Length);
            foreach (string v in values)
            {
                LIST<T> t = TryGetMany(v);
                if (t.Size() != 0)
                {
                    res.Add(t);
                }
                else
                {
                    string k = "   Available: ";
                    foreach (string s in Available())
                        k += s + ", \n";
                    GAME.WarnLight(reader.ErrorGet("no " + this.key() + " named: " + v + k, v));
                }
            }
            return res;
        }

        public void ReadFill(double[] res, Json j, double min, double max)
        {
            ReadFill(key(), res, j, min, max);
        }

        public void ReadFill(string key, double[] res, Json j, double min, double max)
        {
            new KJson(key, j)
            {
                protected override void Process(T s, Json j, string key, bool isWeak)
                {
                    res[s.Index()] = j.D(key, min, max);
                }
            };
        }

        public double[] ReadFill(Json j, double max)
        {
            double[] res = new double[All().Size()];
            ReadFill(res, j, max);
            return res;
        }

        public void ReadFill(double[] res, Json j, double max)
        {
            ReadFill(res, j, 0, max);
        }

        public bool[] ReadIs(Json j)
        {
            bool[] res = new bool[All().Size()];
            foreach (T t in ReadMany(j))
            {
                res[t.Index()] = true;
            }
            return res;
        }

        public LIST<T> All()
        {
            return all;
        }

        public T GetAt(int index)
        {
            return All().Get(index);
        }

        public T TryGet(string value)
        {
            if (map.ContainsKey(value))
            {
                return map.Get(value);
            }
            return null;
        }

        public LIST<T> TryGetMany(string s)
        {
            ArrayListGrower<T> res = new ArrayListGrower<T>();
            if (s.IndexOf(WILDCARD) >= 0)
            {
                string beg = s.Substring(0, s.IndexOf(WILDCARD));
                foreach (string k in map.Keys())
                {
                    if (k.StartsWith(beg))
                    {
                        T t = map.Get(k);
                        if (!res.Contains(t))
                            res.Add(t);
                    }
                }
            }
            else
            {
                T t = TryGet(s);
                if (t != null)
                {
                    res.Add(t);
                }
            }

            return res;
        }

        public string Key()
        {
            return key;
        }

        public HashSet<string> Available()
        {
            return map.Keys();
        }

        private bool hasErr = false;

        public abstract class KJson : MAPJson<T>
        {
            public KJson(Json json) : this(key, json)
            {
            }

            public KJson(string key, Json json) : base(key, json, map, RMAP<T>.this.hasErr)
            {
                RMAP<T>.this.hasErr = hasErr;
            }
        }

        public static abstract class MAPJson<T> where T : MAPPED, new()
        {
            public bool hasErr;

            public MAPJson(string key, Json json, KeyMap<T> map, bool hasErr)
            {
                if (json.Has(key))
                {
                    json = json.Json(key);
                    foreach (string s in json.Keys())
                    {
                        if (s.IndexOf(WILDCARD) >= 0)
                        {
                            string beg = s.Substring(0, s.IndexOf(WILDCARD));
                            foreach (string k in map.Keys())
                            {
                                if (k.StartsWith(beg))
                                {
                                    T t = map.Get(k);
                                    Process(t, json, s, true);
                                }
                            }
                        }
                        else
                        {
                            if (map.ContainsKey(s))
                            {
                                T t = map.Get(s);
                                Process(t, json, s, false);
                            }
                            else
                            {
                                string p = "No " + key + " named " + s + " " + json.Path() + " line: " + json.Line(s);
                                if (!hasErr)
                                {
                                    p += Environment.NewLine + "Available:" + Environment.NewLine;
                                    p += map.KeysString();
                                    GAME.Warn(p);
                                    hasErr = true;
                                }
                                else
                                {
                                    LOG.Ln(p);
                                }
                            }
                        }
                    }
                }
                this.hasErr = hasErr;
            }

            protected abstract void Process(T s, Json j, string key, bool isWeak);
        }
    }
}