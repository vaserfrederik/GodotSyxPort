using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Snake2d.Util.Sets
{
    /**
     * I got pissed that java doesn't have a inheritance structure that allows for
     * unmodifiable collections, so I made my own arraylist. this list is fixed in size.
     * the order of the elemets put in will not be the order of the content. It is not threadable.
     * A neat thing is that you can remove/add while iterating. It is serilizable.
     * @author mail__000
     *
     * @param <E>
     */
    [Serializable]
    public class ArrayList<E> : IList<E>, ISAVABLE, ISerializable
    {
        private static readonly long serialVersionUID = 1L;
        private readonly object[] es;
        private readonly Iter<E> iterator = new Iter<E>();
        private readonly int size;
        private int last = 0;

        /**
         * 
         * @param size the fixed size of the list. Connot be changed.
         */
        public ArrayList(int size)
        {
            this.size = size;
            es = new object[size];
        }

        public ArrayList(E first)
        {
            this.size = 1;
            es = new object[1];
            last = size;
            es[0] = first;
        }

        public ArrayList(IEnumerable<E> other)
        {
            int i = 0;
            foreach (E e in other)
                i++;
            es = new object[i];
            i = 0;
            foreach (E e in other)
            {
                es[i] = e;
                i++;
            }
            size = i;
            last = size;
        }

        public ArrayList(params E[] es)
        {
            this.es = es;
            this.size = es.Length;
            last = size;
        }

        public int IndexOf(E t)
        {
            for (int i = 0; i < last; i++)
            {
                if (es[i] == t)
                    return i;
            }
            return -1;
        }

        public E Get(int index)
        {
            if (index < this.last)
                return (E)es[index];
            return default(E);
        }

        public E Last()
        {
            if (Size() > 0)
                return (E)es[last - 1];
            return default(E);
        }

        /**
         * 
         * @param e
         * @return index if successful, -1 if not (set full)
         */
        public int Add(E e)
        {
            if (!HasRoom())
                throw new RuntimeException("I'm full!" + " " + last + " " + size);
            es[last] = e;
            last++;
            return last - 1;
        }

        public void AddNull()
        {
            if (!HasRoom())
                throw new RuntimeException("I'm full!");
            es[last] = null;
            last++;
        }

        public int TryAdd(E e)
        {
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
        public E Remove(int i)
        {
            if (i >= last)
                return default(E);

            if (i == last - 1)
            {
                E e = (E)es[last - 1];
                es[last - 1] = null;
                last--;
                return e;
            }

            E e = (E)es[i];
            es[i] = es[last - 1];
            es[last - 1] = null;
            last--;
            if (iterator.current == i + 1)
                iterator.current--;
            return e;
        }

        public E RemoveOrdered(int i)
        {
            if (i >= last)
                return default(E);

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

        /**
         * removes and messes up the order.
         * @param object
         * @return if success
         */
        public E Remove(E obj)
        {
            for (int i = 0; i < last; i++)
            {
                if (es[i] == obj)
                    return Remove(i);
            }
            return default(E);
        }

        public int RemoveOrdered(E obj)
        {
            for (int i = 0; i < last; i++)
            {
                if (es[i] == obj)
                {
                    RemoveOrdered(i);
                    return i;
                }
            }
            return -1;
        }

        public int RemainingSlots()
        {
            return size - last;
        }

        public bool HasRoom()
        {
            return RemainingSlots() > 0;
        }

        public bool Contains(int i)
        {
            return i < size && es[i] != null;
        }

        public bool Contains(E obj)
        {
            for (int i = 0; i < last; i++)
            {
                if (es[i] == obj)
                    return true;
            }
            return false;
        }

        /**
         * Start anew!
         */
        public void Clear()
        {
            if (last > es.Length)
                last = es.Length;
            for (int i = 0; i < last; i++)
            {
                es[i] = null;
            }
            last = 0;
        }

        public void ClearSloppy()
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

        public Iter<E> Iterator()
        {
            iterator.Init();
            return iterator;
        }

        public void IteratorRemoveCurrent()
        {
            Remove(iterator.current - 1);
        }

        public class Iter<T> : IEnumerator<E>, ISerializable
        {
            private static readonly long serialVersionUID = 1L;
            private int current;

            private void Init()
            {
                current = 0;
            }

            public bool HasNext()
            {
                return current < last;
            }

            public E Next()
            {
                return (E)es[current++];
            }

            public void Dispose()
            {
                // Not required for this implementation
            }

            object IEnumerator.Current => Next();

            public bool MoveNext()
            {
                return HasNext();
            }

            void IEnumerator.Reset()
            {
                current = 0;
            }
        }

        public bool IsEmpty()
        {
            return last == 0;
        }

        public E RemoveLast()
        {
            return Remove(Size() - 1);
        }

        private transient IComparer<E> c;
        private transient IComparer<object> co;

        public void Sort(IComparer<E> c)
        {
            if (last <= 1)
                return;
            this.c = c;
            if (co == null)
            {
                co = new AnonymousComparer(c, this);
            }
            Sort.Sort(es, 0, last, co);
        }

        private class AnonymousComparer : IComparer<object>
        {
            private readonly IComparer<E> _c;
            private readonly ArrayList<E> _list;

            public AnonymousComparer(IComparer<E> c, ArrayList<E> list)
            {
                _c = c;
                _list = list;
            }

            public int Compare(object o1, object o2)
            {
                return _c.Compare((E)o1, (E)o2);
            }
        }

        public void ShiftLeft()
        {
            for (int i = 1; i < last; i++)
            {
                es[i - 1] = es[i];
            }
            es[last - 1] = null;
            last--;
        }

        public void ShiftLeft(int index)
        {
            for (int i = index + 1; i < last; i++)
            {
                es[i - 1] = es[i];
            }
            es[last - 1] = null;
            last--;
        }

        public void Save(FilePutter file)
        {
            file.i(Size());
            foreach (E e in this)
                file.@object(e);
        }

        public void Load(FileGetter file) throws IOException
        {
            last = file.i();
            for (int i = 0; i < last; i++)
                es[i] = file.@object();
        }

        public void ReplaceObjectData(SerializationInfo info, StreamingContext context)
        {
            // Implement custom deserialization logic if needed
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Implement custom serialization logic if needed
        }
    }
}