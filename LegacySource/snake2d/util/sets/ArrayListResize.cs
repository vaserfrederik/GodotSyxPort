using System;
using System.Collections;
using System.Collections.Generic;

namespace snake2d.util.sets
{
    [Serializable]
    public class ArrayListResize<E> : LISTE<E>, Serializable
    {
        private static readonly long serialVersionUID = 1L;
        private object[] es;
        private readonly Iter<E> iterator = new Iter<E>();
        private readonly IterReverse<E> iteratorReverse = new IterReverse<E>();
        private readonly int maxSize;
        private readonly int minSize;
        private int last = 0;

        public ArrayListResize(int minSize, int maxSize)
        {
            this.maxSize = maxSize;
            this.minSize = minSize;
            es = new object[minSize];
        }

        public ArrayListResize(int minSize)
        {
            this.maxSize = int.MaxValue;
            this.minSize = minSize;
            es = new object[minSize];
        }

        private void increase()
        {
            if (last == es.Length - 1 && es.Length != maxSize)
            {
                int size = es.Length * 2;
                if (size > maxSize)
                    size = maxSize;
                object[] esNew = new object[size];
                for (int i = 0; i < last; i++)
                {
                    esNew[i] = es[i];
                }
                es = esNew;
            }
        }

        private void decrease()
        {
            if (es.Length != minSize && last < es.Length / 2)
            {
                int size = es.Length / 2;
                if (size < minSize)
                    size = minSize;
                object[] esNew = new object[size];
                for (int i = 0; i < last; i++)
                {
                    esNew[i] = es[i];
                }
                es = esNew;
            }
        }

        public E get(int index)
        {
            if (index < this.last)
                return (E)es[index];
            throw new NoSuchElementException("no element at index: " + index);
        }

        public int add(E e)
        {
            int i = tryAdd(e);
            if (i == -1)
                throw new RuntimeException("" + hasRoom());
            return i;
        }

        public int tryAdd(E e)
        {
            increase();
            if (!hasRoom())
                return -1;
            es[last] = e;
            last++;
            return last - 1;
        }

        public E remove(int i)
        {
            if (i >= last)
                return null;

            E e = (E)es[i];

            if (i == last - 1)
            {
                es[last - 1] = null;
                last--;
                decrease();
                return e;
            }

            es[i] = es[last - 1];
            es[last - 1] = null;
            last--;
            if (iterator.current == i + 1)
                iterator.current--;
            decrease();
            return e;
        }

        public int removeOrdered(E object)
        {
            for (int i = 0; i < last; i++)
            {
                if (es[i] == object)
                {
                    removeOrdered(i);
                    return i;
                }
            }
            return -1;
        }

        public E removeOrdered(int i)
        {
            if (i >= last)
                return null;

            if (i == last - 1)
            {
                E e = (E)es[last - 1];
                es[last - 1] = null;
                last--;
                return e;
            }

            E e = (E)es[i];

            for (int k = i; k < last - 1; k++)
            {
                es[k] = es[k + 1];
            }

            es[last - 1] = null;
            last--;
            if (iterator.current >= i)
                iterator.current--;
            return e;
        }

        public bool remove(E object)
        {
            for (int i = 0; i < last; i++)
            {
                if (es[i] == object)
                    return remove(i) != null;
            }
            return false;
        }

        public int remainingSlots()
        {
            return maxSize - last;
        }

        public bool hasRoom()
        {
            return remainingSlots() > 0;
        }

        public bool contains(int i)
        {
            return i < last;
        }

        public bool contains(E object)
        {
            for (int i = 0; i < last; i++)
            {
                if (es[i] == object)
                    return true;
            }
            return false;
        }

        public void clear()
        {
            if (es.Length != minSize)
                es = new object[minSize];
            last = 0;
        }

        public void clearSoft()
        {
            last = 0;
        }

        public int size()
        {
            return last;
        }

        public int max()
        {
            return maxSize;
        }

        public Iter<E> iterator()
        {
            iterator.init();
            return iterator;
        }

        public IEnumerator<E> iteratorReverse()
        {
            iteratorReverse.init();
            return iteratorReverse;
        }

        public void iteratorRemoveCurrent()
        {
            remove(iterator.current - 1);
        }

        [Serializable]
        private class Iter<T> : IEnumerator<E>, Serializable
        {
            private static readonly long serialVersionUID = 1L;
            private int current;

            private void init()
            {
                current = 0;
            }

            public bool MoveNext()
            {
                return current < last;
            }

            public E Current => (E)es[current++];

            object IEnumerator.Current => Current;

            public void Reset()
            {
                current = 0;
            }

            public void Dispose()
            {
            }
        }

        [Serializable]
        private class IterReverse<T> : IEnumerator<E>, Serializable
        {
            private static readonly long serialVersionUID = 1L;
            private int current;

            private void init()
            {
                current = last - 1;
            }

            public bool MoveNext()
            {
                return current >= 0;
            }

            public E Current => (E)es[current--];

            object IEnumerator.Current => Current;

            public void Reset()
            {
                current = last - 1;
            }

            public void Dispose()
            {
            }
        }

        public bool IsEmpty()
        {
            return last == 0;
        }

        public void trim()
        {
            object[] no = new object[last];
            for (int i = 0; i < last; i++)
                no[i] = es[i];
            es = no;
        }

        public E getLast()
        {
            return get(last - 1);
        }

        private transient IComparer<E> c;
        private transient IComparer<object> co;

        public void sort(IComparer<E> c)
        {
            if (last <= 1)
                return;
            if (co == null)
            {
                co = new IComparer<object>
                {
                    Compare = (o1, o2) => this.c.Compare((E)o1, (E)o2)
                };
            }

            this.c = c;
            Sort.sort(es, 0, last - 1, co);
        }

        public void swap(int indexA, int indexB)
        {
            if (indexA < 0 || indexB >= size() || indexB < 0 || indexB >= size())
                throw new RuntimeException();
            object a = es[indexA];
            es[indexA] = es[indexB];
            es[indexB] = a;
        }

        public void shiftRight()
        {
            if (IsEmpty())
                return;
            E e = get(size() - 1);
            for (int i = size() - 1; i > 0; i--)
            {
                es[i] = es[i - 1];
            }
            es[0] = e;
        }

        public void replace(int index, E e2)
        {
            es[index] = e2;
        }
    }
}