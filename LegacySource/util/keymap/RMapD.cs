using System;
using System.Collections.Generic;
using System.IO;

namespace util.keymap
{
    public class RMapD<T> where T : MAPPED, new()
    {
        private readonly RMAPS<T> map;
        private readonly double[] data;

        public RMapD(RMAPS<T> map)
            : this(map, double.MinValue, double.MaxValue)
        {
        }

        public RMapD(RMAPS<T> map, double min, double max)
        {
            this.map = map;
            data = new double[map.all().Count];
        }

        public void save(FilePutter file)
        {
            map.saver().save(data, file);
        }

        public void load(FileGetter file)
        {
            map.loader().load(data, file, 0);
        }

        public void clear()
        {
            Array.Fill(data, 0);
        }

        public double getD(T t)
        {
            return data[t.index()];
        }

        public RMapD<T> setD(T t, double d)
        {
            data[t.index()] = d;
            return this;
        }

        public static class RMapDTwo<A, B> where A : MAPPED, new() where B : MAPPED, new()
        {
            private readonly List<RMapD<B>> all;
            private readonly RMAPS<A> map;
            private readonly SAVABLE[] ss;

            public RMapDTwo(RMAPS<A> map, RMAPS<B> map2)
                : this(map, map2, int.MinValue, int.MaxValue)
            {
            }

            public RMapDTwo(RMAPS<A> map, RMAPS<B> map2, int min, int max)
            {
                this.map = map;
                all = new List<RMapD<B>>(map.all().Count);
                ss = new SAVABLE[map.all().Count];
                for (int i = 0; i < ss.Length; i++)
                {
                    RMapD<B> b = new RMapD<B>(map2);
                    ss[i] = b;
                    all.Add(b);
                }
            }

            public void save(FilePutter file)
            {
                map.saver().save(ss, file);
            }

            public void load(FileGetter file)
            {
                map.loader().load(ss, file);
            }

            public void clear()
            {
                // TODO Auto-generated method stub
            }

            public RMapD<B> get(A f)
            {
                return all[f.index()];
            }
        }
    }
}