using System;
using System.IO;

namespace snake2d.util.sets
{
    [Serializable]
    public class IntegerStack
    {
        private readonly int[] ints;
        private int current = -1;

        public IntegerStack(int maxSize)
        {
            ints = new int[maxSize];
        }

        public void Save(FilePutter fp)
        {
            fp.Is(ints);
            fp.WriteInt(current);
        }

        public void Load(FileGetter fp)
        {
            fp.Is(ints);
            current = fp.I();
        }

        public IntegerStack Fill()
        {
            if (current != -1)
                throw new Exception("has elements");
            for (int i = ints.Length - 1; i >= 0; i--)
                Push(i);
            return this;
        }

        public int Pop()
        {
            if (current >= 0)
                return ints[current--];
            throw new Exception("I'm empty!");
        }

        public bool Push(int i)
        {
            if (!IsFull())
            {
                ints[++current] = i;
                return true;
            }
            return false;
        }

        public bool IsFull()
        {
            return current >= ints.Length - 1;
        }

        public bool IsEmpty()
        {
            return current == -1;
        }

        public IntegerStack Clear()
        {
            current = -1;
            return this;
        }

        public int Size()
        {
            return current + 1;
        }

        public int Get(int index)
        {
            return ints[index];
        }
    }
}