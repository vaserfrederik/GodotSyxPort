using System;
using System.Collections.Generic;

namespace Settlement.Path.Components
{
    internal sealed class SCompNFactory
    {
        private readonly List<SCompN> all;
        private readonly List<int> unused;
        private readonly byte level;

        public SCompNFactory(int level, int size)
        {
            this.level = (byte)level;
            all = new List<SCompN>(2048 / level);
            unused = new List<int>(512 / level);
        }

        public SCompN Create()
        {
            if (unused.Count == 0)
            {
                int i = all.Count;
                SCompN c = new SCompN(i, level);
                all.Add(c);
                c.Retired = false;
                return c;
            }
            int i = unused[unused.Count - 1];
            unused.RemoveAt(unused.Count - 1);
            all[i].Retired = false;
            return all[i];
        }

        public void Clear()
        {
            all.Clear();
            unused.Clear();
        }

        public SCompN Get(int id)
        {
            return all[id];
        }

        public void Retire(SCompN c)
        {
            if (c.Retired)
                return;
            unused.Add(c.Index());
            c.Retire();
        }

        public int MaxAmount()
        {
            return all.Count;
        }
    }
}