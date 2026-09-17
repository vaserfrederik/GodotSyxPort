using System;
using System.IO;

namespace Snake2D.Util.Sets
{
    [Serializable]
    public class ArrayListInt : ISAVABLE
    {
        private static readonly long serialVersionUID = 1L;
        private readonly int[] es;
        private readonly int size;
        private int last = 0;

        /**
         * 
         * @param size the fixed size of the list. Connot be changed.
         */
        public ArrayListInt(int size)
        {
            this.size = size;
            es = new int[size];
        }

        public int Get(int index)
        {
            if (index < this.last)
                return es[index];
            return -1;
        }

        /**
         * 
         * @param e
         * @return index if successful, -1 if not (set full)
         */
        public int Add(int e)
        {
            if (!HasRoom())
                throw new Exception("I'm full!");
            es[last] = e;
            last++;
            return last - 1;
        }

        public void Set(int e, int index)
        {
            if (index >= last)
                throw new Exception();
            es[index] = e;
        }

        /**
         * removes and messes up the order.
         * @param i
         * @return if success
         */
        public int Remove(int index)
        {
            if (index >= last)
                throw new Exception();

            int res = es[index];

            if (index == last - 1)
            {
                es[last - 1] = -1;
                last--;
            }
            else
            {
                es[index] = es[last - 1];
                es[last - 1] = -1;
                last--;
            }
            return res;
        }

        public void RemoveShort(short s)
        {
            for (int i = 0; i < Size(); i++)
            {
                if (Get(i) == s)
                {
                    Remove(i);
                    return;
                }
            }
            throw new Exception();
        }

        public int RemainingSlots()
        {
            return size - last;
        }

        public bool HasRoom()
        {
            return RemainingSlots() > 0;
        }

        public void Clear()
        {
            last = 0;
        }

        public int Size()
        {
            return last;
        }

        public int Max()
        {
            return size;
        }

        public bool IsEmpty()
        {
            return last <= 0;
        }

        public void Save(FilePutter file)
        {
            file.Is(es);
            file.I(last);
        }

        public void Load(FileGetter file)
        {
            file.Is(es);
            last = file.I();
        }

        public void Reverse()
        {
            int s = Size() / 2;
            for (int i = 0; i < s; i++)
            {
                int o = es[i];
                es[i] = es[Size() - 1 - i];
                es[Size() - 1 - i] = o;
            }
        }

        public bool Contains(int s)
        {
            for (int i = 0; i < Size(); i++)
            {
                if (Get(i) == s)
                {
                    return true;
                }
            }
            return false;
        }
    }
}