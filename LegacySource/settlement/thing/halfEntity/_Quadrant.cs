using System;
using System.Collections.Generic;

namespace Settlement.Thing.HalfEntity
{
    class _Quadrant : IEnumerable<HalfEntity>
    {
        private HalfEntity first;
        private HalfEntity last;

        void Add(HalfEntity e)
        {
            e.renderNext = null;
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

        void Remove(HalfEntity e)
        {
            HalfEntity f = first;
            first = null;
            last = null;
            bool removed = false;

            while (f != null)
            {
                HalfEntity n = f.renderNext;
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
                throw new Exception();
        }

        void Clear()
        {
            first = null;
            last = null;
        }

        public IEnumerator<HalfEntity> GetEnumerator()
        {
            iter.current = first;
            return iter;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private readonly Iter iter = new Iter();

        private class Iter : IEnumerator<HalfEntity>
        {
            public HalfEntity current;

            public bool MoveNext()
            {
                return current != null;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public HalfEntity Current => current;

            object System.Collections.IEnumerator.Current => Current;

            public void Dispose()
            {
                // No resources to dispose
            }
        }
    }
}