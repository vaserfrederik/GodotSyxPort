using System;
using System.IO;

namespace Snake2D.Util.Sets
{
    [Serializable]
    public class ArrayCooShort : ISerializable
    {
        private const long serialVersionUID = 1L;
        private readonly short[] coos;
        private readonly int size;
        private readonly Coord coo = new Coord();
        private int i;

        public ArrayCooShort(int size)
        {
            this.size = size;
            this.coos = new short[size * 2];
        }

        public void Save(FilePutter fp)
        {
            fp.Ss(coos);
            fp.I(i);
        }

        public void Load(FileGetter fp)
        {
            fp.Ss(coos);
            i = fp.I();
            Set(i);
        }

        public COORDINATEE Get()
        {
            return coo;
        }

        public int GetI()
        {
            return i;
        }

        public COORDINATEE Set(int i)
        {
            if (i < 0 || i >= size)
                throw new InvalidOperationException(i + " " + size);
            this.i = i;
            return coo;
        }

        public bool HasNext()
        {
            return i < size - 1;
        }

        public COORDINATEE Next()
        {
            return Set(i + 1);
        }

        public int Size()
        {
            return size;
        }

        public void Copy(ArrayCooShort other)
        {
            if (size != other.size)
                throw new InvalidOperationException();

            for (int i = 0; i < coos.Length; i++)
            {
                coos[i] = other.coos[i];
            }
            Set(0);
        }

        public int X(int i)
        {
            return coos[i];
        }

        public int Y(int i)
        {
            return coos[i + size];
        }

        [Serializable]
        private class Coord : COORDINATEE.Abs, ISerializable
        {
            private const long serialVersionUID = 1L;

            public override int X()
            {
                return coos[i];
            }

            public override int Y()
            {
                return coos[i + size];
            }

            public override void XSet(double x)
            {
                coos[i] = (short)x;
            }

            public override void YSet(double y)
            {
                coos[i + size] = (short)y;
            }

            public override string ToString()
            {
                return "COORD " + X() + " " + Y();
            }
        }

        public void Swap(int i1, int i2)
        {
            short x = coos[i2];
            short y = coos[i2 + size];

            coos[i2] = coos[i1];
            coos[i2 + size] = coos[i1 + size];

            coos[i1] = x;
            coos[i1 + size] = y;
        }

        public void Shuffle(int max)
        {
            for (int i = 0; i < max; i++)
            {
                Swap(RND.RInt(max), RND.RInt(max));
            }
        }

        public void Shuffle(int from, int to)
        {
            int d = to - from;
            for (int i = from; i < to; i++)
            {
                Swap(from + RND.RInt(d), from + RND.RInt(d));
            }
        }

        public void Inc()
        {
            i++;
            i %= (size);
        }

        public void Dec()
        {
            i--;
            if (i <= 0)
                i = 0;
        }
    }
}