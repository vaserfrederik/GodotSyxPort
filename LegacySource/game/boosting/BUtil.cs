using System;
using System.Collections.Generic;

namespace Game.Boosting
{
    public static class BUtil
    {
        private BUtil()
        {
        }

        public static double Min<T>(IList<BoosterAbs<T>> all, double baseValue)
        {
            double m = 1;
            double a = baseValue;
            for (int i = 0; i < all.Count; i++)
            {
                BoosterAbs<T> bb = all[i];
                if (bb.IsMul)
                {
                    m *= bb.Min();
                }
                else
                {
                    a += bb.Min();
                }
            }
            return m * a;
        }

        public static double Max<T>(IList<BoosterAbs<T>> all, double baseValue)
        {
            double m = 1;
            double a = baseValue;
            for (int i = 0; i < all.Count; i++)
            {
                BoosterAbs<T> bb = all[i];
                if (bb.IsMul)
                {
                    m *= bb.Max();
                }
                else
                {
                    a += bb.Max();
                }
            }
            return m * a;
        }

        public static double Value(IList<BoosterAbs<object>> all, double input, double add, double mul, double minValue)
        {
            double padd = add > 0 ? add : 0;
            double sub = add < 0 ? add : 0;
            for (int si = 0; si < all.Count; si++)
            {
                BoosterAbs<object> s = all[si];
                if (s.IsMul)
                    mul *= s.GetValue(input);
                else
                {
                    double a = s.GetValue(input);
                    if (a < 0)
                        sub += a;
                    else
                        padd += a;
                }
            }
            return Math.Clamp(padd * mul + sub, minValue, double.MaxValue);
        }

        public static double Value<T>(IList<BoosterAbs<T>> all, T t, double add, double mul, double minValue)
        {
            double padd = add > 0 ? add : 0;
            double sub = add < 0 ? add : 0;
            for (int si = 0; si < all.Count; si++)
            {
                BoosterAbs<T> s = all[si];
                if (s.IsMul)
                    mul *= s.Get(t);
                else
                {
                    double a = s.Get(t);
                    if (a < 0)
                        sub += a;
                    else
                        padd += a;
                }
            }
            return Math.Clamp(padd * mul + sub, minValue, double.MaxValue);
        }

        public static double Value<T>(IList<BoosterAbs<T>> all, T t)
        {
            return Value(all, t, 1, 1, 0);
        }
    }
}