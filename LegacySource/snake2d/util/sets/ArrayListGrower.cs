using System;
using System.Collections;
using System.Collections.Generic;

namespace Snake2d.Util.Sets
{
    [Serializable]
    public class ArrayListGrower<E> : LISTE<E>, Serializable
    {
        private static readonly long serialVersionUID = 1L;
        private object[] es = new object[0];
        private readonly Iter<E> iterator = new Iter<E>();

        public ArrayListGrower()
        {
        }

        public E Get(int index)
        {
            if (index < es.Length)
                return (E)es[index];
            throw new NoSuchElementException("no element at index: " + index);
        }

        public int Add(E e)
        {
            return TryAdd(e);
        }

        public int TryAdd(E e)
        {
            object[] nn = new object[es.Length + 1];
            for (int i = 0; i < es.Length; i++)
                nn[i] = es[i];

            nn[es.Length] = e;
            es = nn;
            return es.Length - 1;
        }

        public bool HasRoom()
        {
            return true;
        }

        public bool Contains(int i)
        {
            return i < es.Length;
        }

        public bool Contains(E obj)
        {
            return FirstIndexOf(obj) >= 0;
        }

        public void Clear()
        {
            es = new object[0];
        }

        public int Size()
        {
            return es.Length;
        }

        public Iter<E> Iterator()
        {
            iterator.Init();
            return iterator;
        }

        private class Iter<T> : IEnumerator<E>, Serializable
        {
            private static readonly long serialVersionUID = 1L;
            private int current;

            private void Init()
            {
                current = 0;
            }

            public bool MoveNext()
            {
                return current < es.Length;
            }

            public E Current
            {
                get
                {
                    return (E)es[current++];
                }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }

            public void Reset()
            {
                current = 0;
            }

            public void Dispose()
            {
                // No resources to dispose
            }
        }

        public bool IsEmpty()
        {
            return es.Length == 0;
        }

        public void Swap(int indexA, int indexB)
        {
            if (indexA < 0 || indexB >= Size() || indexB < 0 || indexB >= Size())
                throw new RuntimeException();
            object a = es[indexA];
            es[indexA] = es[indexB];
            es[indexB] = a;
        }

        public void ShiftRight()
        {
            if (IsEmpty())
                return;
            E e = Get(Size() - 1);
            for (int i = Size() - 1; i > 0; i--)
            {
                es[i] = es[i - 1];
            }
            es[0] = e;
        }

        public void Replace(int index, E e2)
        {
            es[index] = e2;
        }

        public bool Remove(E remove)
        {
            int i = FirstIndexOf(remove);
            if (i < 0)
                return false;
            Remove(i);
            return true;
        }

        public int FirstIndexOf(E e)
        {
            for (int i = 0; i < es.Length; i++)
            {
                if (es[i] == e)
                    return i;
            }
            return -1;
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= Size())
                throw new RuntimeException("" + index);
            object[] nn = new object[es.Length - 1];

            int oi = 0;
            for (int i = 0; i < es.Length; i++)
            {
                if (i == index)
                    continue;
                nn[oi] = es[i];
                oi++;
            }
            es = nn;
        }

        private transient IComparer<E> c;
        private transient IComparer<object> co;

        public void Sort(IComparer<E> c)
        {
            this.c = c;
            if (co == null)
            {
                co = new IComparer<object>
                {
                    Compare = (o1, o2) => this.c.Compare((E)o1, (E)o2)
                };
            }
            Sort.Sort(es, co);
        }
    }
}