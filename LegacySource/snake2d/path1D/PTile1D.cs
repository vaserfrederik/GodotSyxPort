using System;

namespace snake2d.path1D
{
    public class PTile1D
    {
        public readonly int index; // 2
        public float value2; // 4
        float value; // 4
        public PTile1D pathParent; // 4
        int pathId = 0; // 4
        public bool closed; // 1
        PTile1D left; // 4
        PTile1D right; // 4
        PTile1D parent; // 4
        public bool color; // 1

        public PTile1D(int index)
        {
            this.index = index;
        }

        public float GetValue()
        {
            return value;
        }

        public PTile1D GetParent()
        {
            return pathParent;
        }

        public int Parents()
        {
            int p = 0;
            PTile1D pa = pathParent;
            while (pa != null)
            {
                p++;
                pa = pa.pathParent;
            }
            return p;
        }
        
        public void ParentSet(PTile1D p)
        {
            this.pathParent = p;
        }
        
        public override string ToString()
        {
            return "PathTile: (" + index + ")";
        }
    }
}