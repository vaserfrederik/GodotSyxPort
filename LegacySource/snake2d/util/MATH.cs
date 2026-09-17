using System;

namespace Snake2D.Util
{
    public static class MATH
    {
        public static readonly QuickPOW pow15 = new QuickPOW(1.5, 64);

        private MATH()
        {
        }

        public static int Mod(int a, int m)
        {
            int remainder = (a % m);
            a = ((remainder >> 31) & m) + remainder;
            return a;
        }

        public static double Mod(double a, double b)
        {
            if (a < 0)
            {
                a = -a;
                a %= b;
                a = b - a;
                return a;
            }
            else if (a > b)
                return a % b;
            return a;
        }

        public static double Distance(double from, double to, double max)
        {
            if (to < from)
                return max - from + to;
            return to - from;
        }

        public static int Distance(int start, int current, int max)
        {
            if (current < start)
                return max - start + current;
            return current - start;
        }

        public static double DistanceC(double from, double to, double max)
        {
            if (max <= 0)
                return 0;

            from = Mod(from, max);
            to = Mod(to, max);

            if (to < from)
            {
                return from - to;
            }
            return to - from;
        }

        public static int DistanceC(int from, int to, int max)
        {
            from = Mod(from, max);
            to = Mod(to, max);

            if (to < from)
            {
                return from - to;
            }
            return to - from;
        }

        public static bool IsWithin(double point, double from, double to)
        {
            if (from < to)
                return point >= from && point < to;
            return point >= from || point < to;
        }

        public static int ETA(int now, int target, int timeCycle)
        {
            if (now <= target)
            {
                return target - now;
            }
            else
            {
                return timeCycle - now + target;
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine(MATH.Mod(-1.0, 10));
            Console.WriteLine(MATH.DistanceC(-1, 5, 10));
            Console.WriteLine(MATH.DistanceC(5, -1, 10));

            Console.WriteLine(MATH.Distance(5, 4, 16));
            Console.WriteLine(MATH.Distance(5, -1, 10));

            Console.WriteLine(MATH.ETA(12, 9, 16));
        }

        /**
         * Math.Pow(x,2) is quicker than this fix
         * Math.Sqrt is almost as quick
         * @param d
         * @param pow2
         * @return
         */
        public sealed class QuickPOW
        {
            public readonly double Pow;

            private readonly double[] Pows;

            public QuickPOW(double pow, int precision)
            {
                Pows = new double[precision];
                this.Pow = pow;
                for (int i = 0; i < precision; i++)
                {
                    double d = (double)i / precision;
                    Pows[i] = Math.Pow(d, pow);
                }
            }

            public double Pow(double d)
            {
                int prec = Pows.Length - 1;
                double ii = (d * prec);
                int i = (int)ii;
                if (i < 0)
                    return 0;
                if (i >= prec)
                    return 1.0;

                ii -= i;
                double res = Pows[i] * (1 - ii);
                res += Pows[i + 1] * ii;

                return res;
            }
        }
    }
}