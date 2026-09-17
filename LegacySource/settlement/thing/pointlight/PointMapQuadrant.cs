using System;
using System.IO;

namespace Settlement.Thing.Pointlight
{
    class PointMapQuadrant : SAVABLE
    {
        private long[] added = new long[16];
        private byte last = 0;

        void Add(long d)
        {
            if (last >= byte.MaxValue)
                return;

            if (last == added.Length)
            {
                long[] n = new long[added.Length + 16];
                for (int i = 0; i < added.Length; i++)
                    n[i] = added[i];
                added = n;
            }

            added[last] = d;
            last++;
        }

        void Remove(int tx, int ty)
        {
            for (int i = 0; i < last; i++)
            {
                Light q = Light.Init(added[i]);
                if (q.Tx() == tx && q.Ty() == ty)
                {
                    if (i < last)
                        added[i] = added[last - 1];
                    last--;
                    i--;
                }
            }
        }

        bool Is(int tx, int ty)
        {
            for (int i = 0; i < last; i++)
            {
                Light q = Light.Init(added[i]);
                if (q.Tx() == tx && q.Ty() == ty)
                {
                    return true;
                }
            }
            return false;
        }

        public override void Clear()
        {
            last = 0;
        }

        public override void Save(FilePutter file)
        {
            file.I(last);
            for (int i = 0; i < last; i++)
                file.L(added[i]);
        }

        public override void Load(FileGetter file)
        {
            last = 0;
            int k = file.I();
            for (int i = 0; i < k; i++)
                Add(file.L());
        }

        public int Last()
        {
            return last;
        }

        public long Get(int i)
        {
            return added[i];
        }

        public void Set(int i, long d)
        {
            added[i] = d;
        }
    }
}