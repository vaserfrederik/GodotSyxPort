using System;
using System.Collections.Generic;

namespace Snake2D.Util.Iterators
{
    [Serializable]
    public class RECIter : IEnumerator<COORDINATE>, IEnumerable<COORDINATE>, COORDINATE
    {
        private static readonly long SerialVersionUID = 1L;
        private readonly RECTANGLE rec;
        private int ix, iy;

        public RECIter(RECTANGLE rec)
        {
            this.rec = rec;
        }

        public RECIter Init()
        {
            ix = rec.X1 - 1;
            iy = rec.Y1;
            return this;
        }

        public int X()
        {
            return ix;
        }

        public int Y()
        {
            return iy;
        }

        public bool MoveNext()
        {
            ix++;
            if (ix >= rec.X2)
            {
                if (iy >= rec.Y2)
                    throw new InvalidOperationException();
                iy++;
                ix = rec.X1;
            }
            return ix < rec.X2 || iy < rec.Y2;
        }

        public void Reset()
        {
            ix = rec.X1 - 1;
            iy = rec.Y1;
        }

        public COORDINATE Current => this;

        object IEnumerator.Current => this;

        public void Dispose()
        {
            // Implement IDisposable if necessary
        }

        public IEnumerator<COORDINATE> GetEnumerator()
        {
            ix = rec.X1 - 1;
            iy = rec.Y1;
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return $"{GetType().Name} {ix} {iy}";
        }
    }
}