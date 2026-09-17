using System;
using System.IO;

namespace snake2d.util.sets
{
    public class MapInt : SAVABLE
    {
        private int[] table;
        private int last;

        public MapInt()
        {
            this(20);
        }

        public MapInt(int initialCapacity)
        {
            table = Alloc.ii(initialCapacity);
            last = 0;
        }

        public void remove(int e)
        {
            int i = search(e);
            if (i < 0)
                throw new Exception();
            last--;
            for (; i < last; i++)
            {
                table[i] = table[i + 1];
            }
        }

        public void removeIndex(int index)
        {
            if (index < 0 || index >= last)
                throw new Exception();
            last--;
            for (; index < last; index++)
            {
                table[index] = table[index + 1];
            }
        }

        public bool contains(int i)
        {
            return search(i) >= 0;
        }

        public int size()
        {
            return last;
        }

        public int atIndex(int i)
        {
            return table[i];
        }

        public bool isEmpty()
        {
            return last == 0;
        }

        public int poll()
        {
            last--;
            return table[last];
        }

        public int peek()
        {
            return table[last - 1];
        }

        public int add(int e)
        {
            last++;
            if (last >= table.Length)
            {
                int[] table2 = Alloc.ii(table.Length * 2);
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
                if (e > table[i - 1])
                {
                    table[i] = e;
                    return i;
                }
                if (e == table[i - 1])
                    throw new Exception();
                table[i] = table[i - 1];

            }
            return -1;
        }

        public bool tryAdd(int e)
        {
            if (!contains(e))
            {
                add(e);
                return true;
            }
            return false;
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
                if (table[mid] < key)
                {
                    low = mid + 1;
                }
                else if (table[mid] > key)
                {
                    high = mid - 1;
                }
                else if (table[mid] == key)
                {
                    return mid;
                }
            }
            return -1;
        }

        public void clear()
        {
            last = 0;
        }

        public static void Main(string[] args)
        {
            int[] in = Alloc.ii(500);
            int st = RND.rInt();
            MapInt map = new MapInt();
            int a = 0;
            for (int k = 0; k < in.Length; k++)
            {
                in[k] = st + k;
                if ((k & 1) == 0)
                {
                    map.add(in[k]);
                    a++;
                }
            }

            foreach (int t in in)
                if (map.contains(t))
                    a--;

            Console.WriteLine(a + " " + in.Length + " " + map.contains(in[0]));
        }

        public void save(FilePutter file)
        {
            file.i(table.Length);
            file.is(table);
            file.i(last);
        }

        public void load(FileGetter file)
        {
            table = Alloc.ii(file.i());
            file.is(table);
            last = file.i();
        }
    }
}