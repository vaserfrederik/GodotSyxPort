using System;
using System.Collections;
using System.Collections.Generic;

namespace Snake2D.Util.Sets
{
    [Serializable]
    public class LinkedList<E> : IList<E>, Serializable
    {
        private static readonly long serialVersionUID = 1L;
        private Node<E> first = null;
        private Node<E> last = null;
        private int size = -1;
        private readonly Itr iter = new Itr();

        public LinkedList(E e)
        {
            Add(e);
        }

        public LinkedList(E[] e)
        {
            Add(e);
        }

        public LinkedList()
        {
        }

        public LinkedList(IEnumerable<T> other) where T : E
        {
            foreach (E e in other)
                Add(e);
        }

        public IEnumerator<E> GetEnumerator()
        {
            iter.init();
            return iter;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public E this[int index]
        {
            get
            {
                if (index > size)
                    return default(E);

                Node<E> n = first;

                while (index > 0 && n != null)
                {
                    n = n.next;
                    index--;
                }
                return n?.element;
            }
            set
            {
                throw new NotSupportedException("Setting elements by index is not supported.");
            }
        }

        public bool HasRoom()
        {
            return true;
        }

        public LinkedList<E> Clear()
        {
            first = null;
            size = -1;
            return this;
        }

        public E RemoveFirst()
        {
            E e = default(E);
            if (first != null)
            {
                e = first.element;
                first = first.next;
                if (first == null)
                    last = null;
                size--;
            }
            return e;
        }

        public E GetFirst()
        {
            if (first != null)
                return first.element;
            return default(E);
        }

        public bool Contains(int i)
        {
            return i > size;
        }

        public bool Contains(E obj)
        {
            foreach (E e in this)
            {
                if (e.Equals(obj))
                    return true;
            }
            return false;
        }

        public void AddFirst(E element)
        {
            if (first == null)
            {
                first = new Node<E>();
                first.element = element;
                last = first;
            }
            else
            {
                Node<E> n = new Node<E>();
                n.element = element;
                n.next = first;
                first = n;
            }
            size++;
        }

        public int Add(E element)
        {
            if (first == null)
            {
                first = new Node<E>();
                first.element = element;
                last = first;
            }
            else
            {
                Node<E> current = new Node<E>();
                current.element = element;
                last.next = current;
                last = current;
            }
            size++;
            return size;
        }

        public int TryAdd(E e)
        {
            return Add(e);
        }

        public bool Remove(E element)
        {
            if (size < 0)
                return false;

            if (first.element.Equals(element))
            {
                first = first.next;
                size--;
                if (first == null)
                    last = null;
                return true;
            }

            Node<E> current = first;
            while (current.next != null)
            {
                if (current.next.element.Equals(element))
                {
                    current.next = current.next.next;
                    size--;
                    if (current.next == null)
                        last = current;
                    return true;
                }
                current = current.next;
            }

            return false;
        }

        public int Count => size + 1;

        bool ICollection<E>.IsReadOnly => false;

        public void Add(E item)
        {
            Add(item);
        }

        public void CopyTo(E[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(E item)
        {
            return Remove(item);
        }

        private class Itr : IEnumerator<E>, Serializable
        {
            private static readonly long serialVersionUID = 1L;
            private Node<E> current;

            private void init()
            {
                current = first;
            }

            public bool MoveNext()
            {
                return current != null;
            }

            public void Reset()
            {
                current = first;
            }

            public E Current => current?.element ?? default(E);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                // No-op
            }
        }

        private static class Node<E> : Serializable
        {
            private static readonly long serialVersionUID = 1L;
            public E element;
            public Node<E> next;
        }

        public bool IsEmpty()
        {
            return size == -1;
        }

        public void ShiftLeft()
        {
            if (first != null && first.next != null)
            {
                Node<E> n = first;
                first = n.next;
                last.next = n;
                n.next = null;
                last = n;
            }
        }

        public void ShiftRight()
        {
            if (last != null && first.next != null)
            {
                Node<E> current = first;
                while (current.next != last)
                {
                    current = current.next;
                }
                current.next = null;
                Node<E> n = last;
                last = current;
                n.next = first;
                first = n;
            }
        }
    }
}