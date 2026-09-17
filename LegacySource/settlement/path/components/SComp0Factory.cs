using System.Collections.Generic;

namespace Settlement.Path.Components
{
    internal class SComp0Factory
    {
        private List<SComp0> all = new List<SComp0>(SComp0Level.StartSize);
        private List<int> unused = new List<int>(1024);
        internal SComp0 NONE = Create();

        public SComp0 Create()
        {
            if (unused.Count == 0)
            {
                int i = all.Count;
                SComp0 c = new SComp0(i);
                all.Add(c);
                c.Retire(false);
                return c;
            }
            int i = unused[unused.Count - 1];
            unused.RemoveAt(unused.Count - 1);
            all[i].Retire(false);
            return all[i];
        }

        public void Clear()
        {
            all.Clear();
            unused.Clear();
            all.Add(NONE);
        }

        public SComp0 Get(int id)
        {
            return all[id];
        }

        public void Retire(SComp0 c)
        {
            unused.Add(c.Index());
            c.Retire();
        }

        public int MaxAmount()
        {
            return all.Count;
        }
    }
}