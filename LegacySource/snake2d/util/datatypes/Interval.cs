using System;

namespace Snake2D.Util.DataTypes
{
    public class Interval
    {
        private int[] x1;
        private int[] x2;

        public Interval()
        {
        }

        public void Add(int a, int b)
        {
            if (x1 == null)
            {
                x1 = new int[] { a };
                x2 = new int[] { b };
                return;
            }

            int[] newX1 = Alloc.Ii(x1.Length + 1);
            int[] newX2 = Alloc.Ii(x2.Length + 1);
            int count = 0;

            for (int i = 0; i < newX1.Length - 1; i++)
            {
                if ((a <= x2[i] && b >= x1[i]))
                {
                    a = a < x1[i] ? a : x1[i];
                    b = b > x2[i] ? b : x2[i];
                    continue;
                }
                newX1[count] = x1[i];
                newX2[count] = x2[i];
                count++;
            }

            newX1[count] = a;
            newX2[count] = b;

            x1 = Alloc.Ii(count + 1);
            x2 = Alloc.Ii(count + 1);

            for (int i = 0; i < x1.Length; i++)
            {
                x1[i] = newX1[i];
                x2[i] = newX2[i];
            }
        }

        public bool Holds(int x)
        {
            if (x1 == null)
                return false;

            for (int i = 0; i < x1.Length; i++)
            {
                if (x >= x1[i] && x < x2[i])
                    return true;
            }
            return false;
        }

        public bool IsEmpty()
        {
            return x1 == null;
        }

        public void Clear()
        {
            x1 = null;
        }
    }
}