using System;
using System.IO;

namespace Snake2D.Util.Sets
{
    public class ArrayListIntegerResize : SAVABLE
    {
        private int[] es;
        private readonly int maxSize;
        private readonly int minSize;
        private int last = 0;

        /**
         * 
         * @param maxSize the fixed size of the list. Connot be changed.
         */
        public ArrayListIntegerResize(int minSize, int maxSize)
        {
            this.maxSize = maxSize;
            this.minSize = minSize;
            es = Alloc.Ii(minSize);
        }

        private void Increase()
        {
            if (last == es.Length - 1 && es.Length != maxSize)
            {
                int size = es.Length * 2;
                if (size > maxSize)
                    size = maxSize;
                int[] esNew = Alloc.Ii(size);
                for (int i = 0; i < last; i++)
                {
                    esNew[i] = es[i];
                }
                es = esNew;
            }
        }

        private void Decrease()
        {
            if (es.Length != minSize && last < es.Length / 2)
            {
                int size = es.Length / 2;
                if (size < minSize)
                    size = minSize;
                int[] esNew = Alloc.Ii(size);
                for (int i = 0; i < last; i++)
                {
                    esNew[i] = es[i];
                }
                es = esNew;
            }
        }

        public int Get(int index)
        {
            if (index < this.last)
                return es[index];
            throw new IndexOutOfRangeException("no element at index: " + index);
        }

        public int Add(int e)
        {
            Increase();
            if (!HasRoom())
                return -1;
            es[last] = e;
            last++;
            return last - 1;
        }

        /**
         * removes and messes up the order.
         * @param i
         * @return if success
         */
        public bool Remove(int i)
        {
            if (i >= last)
                return false;

            if (i == last - 1)
            {
                last--;
                Decrease();
                return true;
            }

            es[i] = es[last - 1];
            last--;
            Decrease();
            return true;
        }

        public int RemainingSlots()
        {
            return maxSize - last;
        }

        public bool HasRoom()
        {
            return RemainingSlots() > 0;
        }

        /**
         * Start anew!
         */
        public override void Clear()
        {
            if (es.Length != minSize)
                es = Alloc.Ii(minSize);
            last = 0;
        }

        public int Size()
        {
            return last;
        }

        public int Max()
        {
            return maxSize;
        }

        public bool IsEmpty()
        {
            return last == 0;
        }

        public void Trim()
        {
            int[] no = Alloc.Ii(last);
            for (int i = 0; i < last; i++)
                no[i] = es[i];
            es = no;
        }

        public int GetLast()
        {
            return Get(last - 1);
        }

        public void Swap(int indexA, int indexB)
        {
            if (indexA < 0 || indexB >= Size() || indexB < 0 || indexB >= Size())
                throw new Exception();
            int a = es[indexA];
            es[indexA] = es[indexB];
            es[indexB] = a;
        }

        public override void Save(FilePutter file)
        {
            file.I(last);
            for (int i = 0; i < Size(); i++)
                file.I(Get(i));

        }

        public override void Load(FileGetter file)
        {
            Clear();
            int m = file.I();
            for (int i = 0; i < m; i++)
                Add(file.I());

        }
    }
}