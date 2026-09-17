using System;
using System.Collections.Generic;

namespace snake2d.util.iterators
{
    public class RECedgeIter : IEnumerator<COORDINATE>, IEnumerable<COORDINATE>
    {
        private int x1, x2, y1, y2;
        private int w;
        private int x, y;
        
        public static readonly RECedgeIter TMP = new RECedgeIter();
        private readonly Coo res = new Coo();
        
        public RECedgeIter Init(RECTANGLE r)
        {
            return Init(r.x1(), r.x2(), r.y1(), r.y2());
        }
        
        public RECedgeIter Init(int x1, int x2, int y1, int y2)
        {
            x = x1;
            y = y1;
            this.x1 = x1;
            this.x2 = x2;
            this.y1 = y1;
            this.y2 = y2;
            w = x2 - x1 - 1;
            if (w < 1)
                w = 1;
            return this;
        }
        
        public bool MoveNext()
        {
            return x < x2 && y < y2;
        }

        public COORDINATE Current => res;

        object IEnumerator.Current => Current;

        public void Reset()
        {
            x = x1;
            y = y1;
        }

        public void Dispose()
        {
            // No resources to dispose
        }

        public IEnumerator<COORDINATE> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}