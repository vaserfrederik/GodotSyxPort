using System;
using System.IO;

namespace snake2d.util.sets
{
    [Serializable]
    public class ShortStack
    {
        private static readonly long serialVersionUID = 1L;
        private readonly short[] ints;
        private int current = -1;

        public ShortStack(int maxSize)
        {
            ints = new short[maxSize];
        }

        public void Save(FilePutter fp)
        {
            fp.Ss(ints);
            fp.WriteInt(current);
        }

        public void Load(FileGetter fp)
        {
            fp.Ss(ints);
            current = fp.I();
        }

        public ShortStack Fill()
        {
            if (current != -1)
                throw new Exception("has elements");
            for (int i = ints.Length - 1; i >= 0; i--)
                Push((short)i);
            return this;
        }

        public short Pop()
        {
            if (current >= 0)
                return ints[current--];
            throw new Exception("I'm empty!");
        }

        public bool Push(short i)
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

        public ShortStack Clear()
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

        public int Capacity()
        {
            return ints.Length;
        }
    }
}