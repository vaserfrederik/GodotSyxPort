using System;
using System.Collections.Generic;
using System.IO;

namespace util.keymap
{
    public class RMapInt<T> where T : MAPPED, new()
    {
        private readonly RMAPS<T> map;
        private readonly int min;
        private readonly int max;
        private readonly int[] data;
        private int total;
        private readonly int clearTo;

        public RMapInt(RMAPS<T> map)
            : this(map, int.MinValue, int.MaxValue)
        {
        }

        public RMapInt(RMAPS<T> map, int min, int max)
            : this(map, min, max, 0)
        {
        }

        public RMapInt(RMAPS<T> map, int min, int max, int clearTo)
        {
            this.map = map;
            this.min = min;
            this.max = max;
            data = Alloc.II(map.All().Count);
            this.clearTo = clearTo;
            Array.Fill(data, clearTo);
        }

        public void Save(FilePutter file)
        {
            map.Saver().Save(data, file);
        }

        public void Load(FileGetter file)
        {
            map.Loader().Load(data, file, 0);
            foreach (int i in data)
                total += i;
        }

        public void Clear()
        {
            Array.Fill(data, clearTo);
            total = 0;
        }

        public int Get(T t)
        {
            if (t == null)
                return total;
            return data[t.Index()];
        }

        public int Min(T t)
        {
            return min;
        }

        public int Max(T t)
        {
            return max;
        }

        private INFO info;

        public RMapInt<T> SetInfo(CharSequence name)
        {
            info = new INFO(name, name);
            return this;
        }

        public INFO Info()
        {
            return info;
        }

        public void Set(T t, int i)
        {
            total -= data[t.Index()];
            data[t.Index()] = i;
            total += data[t.Index()];
        }

        public void SetAll(int v)
        {
            Array.Fill(data, v);
            total = v * data.Length;
        }

        public class RMapIntTwo<A, B> : GETTER_TRANS<A, RMapInt<B>>, SAVABLE
            where A : MAPPED, new()
            where B : MAPPED, new()
        {
            private readonly ArrayList<RMapInt<B>> all;
            private readonly RMAPS<A> map;
            private readonly SAVABLE[] ss;

            public RMapIntTwo(RMAPS<A> map, RMAPS<B> map2)
                : this(map, map2, int.MinValue, int.MaxValue)
            {
            }

            public RMapIntTwo(RMAPS<A> map, RMAPS<B> map2, int min, int max)
            {
                this.map = map;
                all = new ArrayList<RMapInt<B>>(map.All().Count);
                ss = new SAVABLE[map.All().Count];
                for (int i = 0; i < ss.Length; i++)
                {
                    RMapInt<B> b = new RMapInt<B>(map2, min, max);
                    ss[i] = b;
                    all.Add(b);
                }
            }

            public void Save(FilePutter file)
            {
                map.Saver().Save(ss, file);
            }

            public void Load(FileGetter file)
            {
                map.Loader().Load(ss, file);
            }

            public void Clear()
            {
                foreach (RMapInt<B> b in all)
                {
                    b.Clear();
                }
            }

            public RMapInt<B> Get(A f)
            {
                return all.Get(f.Index());
            }

            public void SetAll(int v)
            {
                foreach (RMapInt<B> b in all)
                {
                    b.SetAll(v);
                }
            }
        }
    }
}