using System;
using System.Collections.Generic;

namespace World.Entity
{
    class _QuadrantArray : IEnumerable<WEntity>
    {
        private WEntity first;
        private WEntity last;

        public void Add(WEntity e)
        {
            if (first == null)
            {
                first = e;
                last = e;
            }
            else
            {
                last.renderNext = e;
                last = e;
            }
        }

        public void Remove(WEntity e)
        {
            WEntity f = first;
            first = null;
            last = null;
            bool removed = false;

            while (f != null)
            {
                WEntity n = f.renderNext;
                f.renderNext = null;
                if (f == e)
                {
                    removed = true;
                }
                else
                {
                    Add(f);
                }

                f = n;
            }
            if (!removed)
                throw new RuntimeException();
        }

        public void Clear()
        {
            first = null;
            last = null;
        }

        public IEnumerator<WEntity> GetEnumerator()
        {
            iter.current = first;
            return iter;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly Iter iter = new Iter();

        private sealed class Iter : IEnumerator<WEntity>
        {
            public WEntity current;

            public bool MoveNext()
            {
                return current != null;
            }

            public WEntity Current => current;

            object System.Collections.IEnumerator.Current => Current;

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                // No resources to dispose
            }
        }
    }
}