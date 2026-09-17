using System;

namespace World.Map.Regions
{
    class Test
    {
        public static void Main(string[] args)
        {
            const int tot = 500;
            Trans tr = null;
            T[] all = new T[tot];
            for (int i = 0; i < all.Length; i++)
                all[i] = new T(RND.rFloatP(2));
            {
                double ave = 0;
                double max = 0;
                double mi = double.MaxValue;
                foreach (T t in all)
                {
                    ave += t.value;
                    max = Math.Max(max, t.value);
                    mi = Math.Min(t.value, mi);
                }
                ave /= tot;

                tr = new Trans(ave, mi, max);
            }
            {
                double a = 0;
                double mi = double.MaxValue;
                double ma = 0;
                foreach (T t in all)
                {
                    double v = tr.D(t.value);
                    a += v;
                    mi = Math.Min(mi, v);
                    ma = Math.Max(v, ma);
                }
                Console.WriteLine(a / tot + " " + mi + " " + ma);
            }
        }

        private static class T
        {
            public double value;

            public T(double value)
            {
                this.value = value;
            }
        }

        private static class Trans
        {
            public readonly double weight;
            public readonly double ave;
            public readonly double max;

            public Trans(double ave, double min, double max)
            {
                this.ave = ave;
                this.max = max;

                double w = 1 - ave / max;
                this.weight = 1.0 / w;
            }

            private double D(double v)
            {
                double m = weight * ave / max;
                double d = (1 - m + weight * v / max) / 2.0;
                return CLAMP.d(d, 0, 1);
            }
        }
    }

    static class RND
    {
        private static readonly Random random = new Random();

        public static double rFloatP(int precision)
        {
            return Math.Round(random.NextDouble(), precision);
        }
    }

    static class CLAMP
    {
        public static double d(double value, double min, double max)
        {
            return Math.Min(max, Math.Max(min, value));
        }
    }
}