using System;
using System.IO;

namespace Snake2D.Util.Sets
{
    public class ArrayInt : SAVABLE
    {
        private readonly int[] data;

        public ArrayInt(int size)
        {
            this.data = Alloc.Ii(size);
        }

        public ArrayInt(LIST<object> li)
        {
            this.data = Alloc.Ii(li.Size());
        }

        public override void Save(FilePutter file)
        {
            file.IsE(data);
        }

        public override void Load(FileGetter file)
        {
            file.IsE(data);
        }

        public override void Clear()
        {
            SetAll(0);
        }

        public int Get(int i)
        {
            return data[i];
        }

        public int Get(INDEXED i)
        {
            return Get(i.Index());
        }

        public ArrayInt SetAll(int v)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = v;
            return this;
        }

        public ArrayInt Set(int i, int v)
        {
            data[i] = v;
            return this;
        }

        public ArrayInt Set(INDEXED i, int v)
        {
            return Set(i.Index(), v);
        }

        public ArrayInt Inc(int i, int d)
        {
            data[i] += d;
            return this;
        }

        public ArrayInt Inc(INDEXED i, int d)
        {
            return Inc(i.Index(), d);
        }

        public class ArrayInt2D : SAVABLE
        {
            private readonly ArrayInt[] ints;

            public ArrayInt2D(int h, int w)
            {
                ints = new ArrayInt[h];
                for (int i = 0; i < h; i++)
                {
                    ints[i] = new ArrayInt(w);
                }
            }

            public override void Save(FilePutter file)
            {
                foreach (ArrayInt i in ints)
                    i.Save(file);
            }

            public override void Load(FileGetter file)
            {
                foreach (ArrayInt i in ints)
                    i.Load(file);
            }

            public override void Clear()
            {
                foreach (ArrayInt i in ints)
                    i.Clear();
            }

            public ArrayInt2D SetAll(int v)
            {
                foreach (ArrayInt i in ints)
                    i.SetAll(v);
                return this;
            }

            public ArrayInt Get(int i)
            {
                return ints[i];
            }

            public ArrayInt Get(INDEXED i)
            {
                return ints[i.Index()];
            }
        }
    }
}