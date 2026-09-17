using System;
using System.Collections.Generic;

namespace Snake2D.Util.Sets
{
    /**
     * A hashmap that works with indexed objects. It is slow at inserting objects, but does not take more space than its objects
     * @author Jake
     *
     * @param <T>
     */
    public sealed class MapIndexed<T> where T : INDEXED
    {
        private INDEXED[] table = new INDEXED[0];
        private int last = 0;

        public MapIndexed()
        {
        }

        public MapIndexed(T[] ts)
        {
            table = new INDEXED[ts.Length];
            last = table.Length;
            for (int i = 0; i < table.Length; i++)
                table[i] = ts[i];
            Array.Sort(table, new Comparison<INDEXED>((o1, o2) => o1.index() - o2.index()));
        }

        public T get(int hash)
        {
            return (T)table[search(hash)];
        }

        public T getTry(int index)
        {
            int ss = search(index);
            if (ss >= 0)
                return (T)table[ss];
            return null;
        }

        public void remove(int hash)
        {
            int i = search(hash);
            if (i < 0)
                throw new Exception();
            last--;
            for (; i < last; i++)
            {
                table[i] = table[i + 1];
            }
        }

        public bool contains(T @object)
        {
            return search(@object.index()) >= 0;
        }

        public bool contains(int hash)
        {
            return search(hash) >= 0;
        }

        public int size()
        {
            return last;
        }

        public bool isEmpty()
        {
            return last == 0;
        }

        public int add(T e)
        {
            last++;
            if (last >= table.Length)
            {
                INDEXED[] table2 = new INDEXED[table.Length + 1];
                for (int i = 0; i < table.Length; i++)
                {
                    table2[i] = table[i];
                }
                table = table2;
            }
            for (int i = last - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    table[i] = e;
                    return 0;
                }
                if (e.index() > table[i - 1].index())
                {
                    table[i] = e;
                    return i;
                }
                if (e.index() == table[i - 1].index())
                    throw new Exception();
                table[i] = table[i - 1];

            }
            return -1;
        }

        public bool hasRoom()
        {
            return true;
        }

        public int tryAdd(T e)
        {
            return add(e);
        }

        private int search(int value)
        {
            return runBinarySearchIteratively(value, 0, last - 1);
        }

        private int runBinarySearchIteratively(int key, int low, int high)
        {
            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                if (table[mid].index() < key)
                {
                    low = mid + 1;
                }
                else if (table[mid].index() > key)
                {
                    high = mid - 1;
                }
                else if (table[mid].index() == key)
                {
                    return mid;
                }
            }
            return -1;
        }

        public List<T> toList()
        {
            List<T> tt = new List<T>(last);
            for (int i = 0; i < last; i++)
                tt.Add((T)table[i]);
            return tt;
        }
    }
}