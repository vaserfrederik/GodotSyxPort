using System;

namespace snake2d.util.sets
{
    [Serializable]
    public class Stack<T> : ISerializable
    {
        private static readonly long serialVersionUID = 1L;
        private readonly object[] ints;
        private int current = -1;

        public Stack(int maxSize)
        {
            ints = new object[maxSize];
        }

        public T Pop()
        {
            if (current >= 0)
                return (T)ints[current--];
            throw new RuntimeException("I'm empty!");
        }

        public bool Push(T i)
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

        public Stack<T> Clear()
        {
            current = -1;
            return this;
        }

        public int Size()
        {
            return current + 1;
        }

        // Serialization constructor (required for ISerializable)
        protected Stack(SerializationInfo info, StreamingContext context)
        {
            // Implement deserialization logic here if needed
        }

        // Implement GetObjectData method if needed
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Implement serialization logic here if needed
        }
    }
}