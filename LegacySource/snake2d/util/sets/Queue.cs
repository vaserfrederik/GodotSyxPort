using System;
using System.Collections;
using System.Collections.Generic;

namespace snake2d.util.sets
{
    public sealed class Queue<T> : IEnumerable<T>
    {
        private readonly object[] queue;
        private int front;
        private int rear = -1;
        private int currentSize = 0;

        public Queue(int size)
        {
            this.queue = new object[size + 1];
            Clear();
        }

        public void Push(T item)
        {
            if (!HasRoom())
                throw new InvalidOperationException();
            rear++;
            if (rear == queue.Length - 1)
            {
                rear = 0;
            }
            queue[rear] = item;
            currentSize++;
        }

        public bool HasNext()
        {
            return currentSize > 0 && front >= 0;
        }

        public T Peek()
        {
            if (!HasNext())
                throw new InvalidOperationException();

            return (T)queue[front];
        }

        public T Poll()
        {
            if (!HasNext())
                throw new InvalidOperationException();

            object tmp = queue[front];
            currentSize--;
            front++;
            if (front == queue.Length - 1)
            {
                front = 0;
            }
            return (T)tmp;
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

        public bool Contains(T item)
        {
            int f = front;
            int c = currentSize;
            while (c > 0)
            {
                if (queue[f] == item)
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

        public IEnumerator<T> GetEnumerator()
        {
            iter.ii = 0;
            iter.f = front;
            return iter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly Iter iter = new Iter();

        private class Iter : IEnumerator<T>
        {
            public int ii = 0;
            public int f = 0;

            public bool MoveNext()
            {
                return ii < currentSize && f >= 0;
            }

            public void Reset()
            {
                ii = 0;
                f = 0;
            }

            public T Current
            {
                get
                {
                    object tmp = queue[f];
                    ii++;
                    f++;
                    if (f == queue.Length - 1)
                    {
                        f = 0;
                    }
                    return (T)tmp;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                // No-op
            }
        }
    }
}