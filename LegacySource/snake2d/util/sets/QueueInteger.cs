using System;
using System.IO;

namespace snake2d.util.sets
{
    [Serializable]
    public sealed class QueueInteger : SAVABLE
    {
        private readonly int[] queue;
        private int front;
        private int rear = -1;
        private int currentSize = 0;

        public QueueInteger(int size)
        {
            this.queue = Alloc.Ii(size + 1);
            Clear();
        }

        public void Push(int i)
        {
            if (!HasRoom())
                throw new Exception();
            rear++;
            if (rear == queue.Length - 1)
            {
                rear = 0;
            }
            queue[rear] = i;
            currentSize++;
        }

        public bool HasNext()
        {
            return currentSize > 0 && front >= 0;
        }

        public int Peek()
        {
            if (!HasNext())
                throw new Exception();

            int tmp = queue[front];
            return tmp;
        }

        public int Poll()
        {
            if (!HasNext())
                throw new Exception();

            int tmp = queue[front];
            currentSize--;
            front++;
            if (front == queue.Length - 1)
            {
                front = 0;
            }
            return tmp;
        }

        public bool HasRoom()
        {
            return currentSize < queue.Length - 1;
        }

        public bool IsFull()
        {
            return Remaining() == 0;
        }

        public int Remaining()
        {
            return Capacity() - Size();
        }

        public int Capacity()
        {
            return queue.Length - 1;
        }

        public int Size()
        {
            return currentSize;
        }

        public bool Contains(int i)
        {
            int f = front;
            int c = currentSize;
            while (c > 0)
            {
                if (queue[f] == i)
                    return true;
                c--;
                f++;
                if (f == queue.Length - 1)
                {
                    f = 0;
                }
            }
            return false;
        }

        public void Clear()
        {
            front = 0;
            rear = -1;
            currentSize = 0;
        }

        public void Save(FilePutter file)
        {
            file.Is(queue);
            file.I(front);
            file.I(rear);
            file.I(currentSize);
        }

        public void Load(FileGetter file)
        {
            file.Is(queue);
            front = file.I();
            rear = file.I();
            currentSize = file.I();
        }
    }
}