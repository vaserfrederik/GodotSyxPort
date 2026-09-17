using System;

namespace snake2d.util.datatypes
{
    public class DEG
    {
        private static double[][] PRE = new double[360][];
        static
        {
            for (int i = 0; i < PRE.Length; i++)
            {
                PRE[i] = new double[2];
                PRE[i][0] = Math.Cos(Math.PI * i / 180);
                PRE[i][1] = Math.Sin(Math.PI * i / 180);
            }
        }

        private static int currentI;
        private static int tmpI;

        private DEG()
        {
        }

        public static void Set(COORDINATE coo)
        {
            Set(coo.x(), coo.y());
        }

        public static void Set(VECTOR vec)
        {
            Set(vec.x(), vec.y());
        }

        public static void Set(double x, double y)
        {
            currentI = (int)Math.Round(Math.Atan2(y, x) * (180 / Math.PI));
            if (currentI < 0)
                currentI += 360;
        }

        public static double GetCurrentX()
        {
            return PRE[currentI][0];
        }

        public static double GetCurrentY()
        {
            return PRE[currentI][1];
        }

        public static double GetTmpX()
        {
            return PRE[tmpI][0];
        }

        public static double GetTmpY()
        {
            return PRE[tmpI][1];
        }

        public static void MoveTmp(int deg)
        {
            tmpI = currentI + deg;
            if (tmpI < 0)
                tmpI += 360;
            else if (tmpI >= 360)
                tmpI -= 360;
        }

        public static void SetRandom()
        {
            currentI = RND.rInt(360);
        }
    }
}