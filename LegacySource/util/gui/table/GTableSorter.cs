using System;
using System.Collections.Generic;

namespace Util.Gui.Table
{
    public abstract class GTableSorter<T>
    {
        private readonly object[] pop;
        private readonly Tree<T> tree;

        private int popI;
        private int sortI;

        protected GTSort<T> sort;
        protected GTFilter<T> filter;

        public GTableSorter(int max)
        {
            pop = new object[max];
            tree = new Tree<T>(max)
            {
                IsGreaterThan = (current, cmp) =>
                {
                    if (sort == null)
                    {
                        return true;
                    }
                    int c = sort.Cmp(current, cmp);
                    return c <= 0;
                }
            };
        }

        public void Sort()
        {
            sortI--;
            if (sortI > 0)
                return;
            sortI = 64;
            popI = 0;
            tree.Clear();
            for (int i = 0; i < pop.Length; i++)
            {
                T t = GetUnsorted(i);
                if (t != null)
                {
                    if (filter == null || filter.Passes(t))
                        tree.Add(t);
                }
            }
            while (tree.HasMore())
            {
                pop[popI++] = tree.PollGreatest();
            }
        }

        public void SortForced()
        {
            sortI = 0;
            Sort();
        }

        protected abstract T GetUnsorted(int index);

        public int Size()
        {
            return popI;
        }

        public T Get(int index)
        {
            if (index >= 0 && index < popI)
            {
                return (T)pop[index];
            }
            return default;
        }

        public T Get(Func<int> getter)
        {
            return Get(getter());
        }

        public int GetIndex(T t)
        {
            for (int i = 0; i < popI; i++)
                if (t == pop[i])
                    return i;
            return 0;
        }

        public void SetFilter(GTFilter<T> filter)
        {
            this.filter = filter;
            sortI = 0;
            Sort();
        }

        public void SetSort(GTSort<T> sort)
        {
            this.sort = sort;
            sortI = 0;
            Sort();
        }

        public GTSort<T> CurrentSort()
        {
            return sort;
        }

        public GTFilter<T> CurrentFilter()
        {
            return filter;
        }

        public abstract class GTFilter<T>
        {
            public readonly string Name;

            public GTFilter(string name)
            {
                this.Name = name;
            }

            public abstract bool Passes(T h);
        }

        public abstract class GTSort<T>
        {
            public readonly string Name;

            public GTSort(string name)
            {
                this.Name = name;
            }

            public abstract int Cmp(T current, T cmp);

            public abstract void Format(T h, GText text);
        }
    }
}