using System;
using System.Collections.Generic;
using System.Linq;

namespace util.text
{
    public class Inserter<T>
    {
        public readonly List<II> all = new List<II>();
        private readonly Dictionary<char, II> map = new Dictionary<char, II>();

        public Inserter()
        {
        }

        public Inserter(Inserter<T> ii, string prefix)
        {
            foreach (var i in ii.all)
            {
                new II(prefix + i.key)
                {
                    set = (t, str) =>
                    {
                        if (t == null)
                            return;
                        i.set(t, str);
                    }
                };
            }
        }

        public abstract class II : StrInserter<T>
        {
            public II(string key) : base(key)
            {
                all.Add(this);
                map[key] = this;
            }

            public abstract void set(T t, Str str);
        }

        private static int ranI;
        private static long ran = RND.rInt();
        public static void setRandom(long ran)
        {
            ranI = 0;
            Inserter<T>.ran = ran;
        }

        public int ran()
        {
            ran = ran >> ranI;

            ranI += 4;
            if (ranI >= 64)
                ranI = 0;
            return (int)(ran & 0x0F);
        }

        private static bool ww = false;

        public void check(IReadOnlyList<char> str)
        {
            for (int ii = 0; ii < 100; ii++)
            {
                var s = Str.getInsert(str, ii);
                if (s == null)
                    break;
                if (map.ContainsKey(s))
                    continue;
                GAME.WarnLight("missing insert: " + s + ", in text: " + str);
                if (!ww)
                {
                    ww = true;
                    string v = "Available:" + Environment.NewLine;
                    v += string.Join(", ", map.Keys);
                    GAME.Warn(v);
                }
            }
        }

        public IReadOnlyList<char>[] check(IReadOnlyList<char>[] str)
        {
            foreach (var cc in str)
                check(cc);
            return str;
        }

        public Inserter<T> join(Inserter<K> inOther, GETTER_TRANS<T, K> trans)
        {
            foreach (var ii in inOther.all)
            {
                join(ii, trans);
            }
            return this;
        }

        public Inserter<T> join(II ii, GETTER_TRANS<T, K> trans)
        {
            new II(ii.key)
            {
                set = (t, str) => ii.set(trans.get(t), str)
            };
            return this;
        }

        public Inserter<T> join(Inserter<T> inOther)
        {
            foreach (var ii in inOther.all)
            {
                join(ii);
            }
            return this;
        }

        public Inserter<T> join(II ii)
        {
            map[ii.key] = ii;
            all.Add(ii);
            return this;
        }

        public void set(Str str, T i)
        {
            if (i == null)
                return;

            for (int ii = 0; ii < 100; ii++)
            {
                var s = Str.getInsert(str, ii);
                if (s == null)
                    break;
                var iii = map[s];
                if (iii == null)
                    continue;
                try
                {
                    iii.insert(i, str);
                }
                catch (Exception e)
                {
                    throw new Exception("problems with insert " + iii.key, e);
                }

                ii--;
            }
        }
    }
}