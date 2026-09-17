using System;

namespace snake2d
{
    public class PathTile : IComparable<PathTile>, COORDINATE
    {
        // 12
        private readonly short x; // 2
        private readonly short y; // 2
        public float accCost; // 4
        public float value; // 4
        public PathTile pathParent; // 4
        public int pathId = 0; // 4
        public bool closed; // 1
                            // 33
        public PathTile left; // 4
        public PathTile right; // 4
        public PathTile parent; // 4
        public bool color; // 1
                            // 34 + 12 = 46 = 64
                          // possible: // 2+2+4+4+1+4+4+4+4 = 29

        public PathTile(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public int CompareTo(PathTile o)
        {
            if (o == this)
                return 0;
            return value < o.value ? -1 : 1;
        }

        public float GetValue()
        {
            return value;
        }

        public float GetValue2()
        {
            return accCost;
        }

        public void SetValue2(double v)
        {
            accCost = (float)v;
        }

        public int X()
        {
            return x;
        }

        public int Y()
        {
            return y;
        }

        public PathTile GetParent()
        {
            return pathParent;
        }

        public int Parents()
        {
            int p = 0;
            PathTile pa = pathParent;
            while (pa != null)
            {
                p++;
                pa = pa.pathParent;
            }
            return p;
        }

        public void ParentSet(PathTile p)
        {
            this.pathParent = p;
        }

        public override string ToString()
        {
            return "PathTile: (" + x + ", " + y + ")";
        }
    }
}