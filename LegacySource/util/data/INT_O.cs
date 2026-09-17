using System;

namespace Util.Data
{
    public interface INT_O<T> : DOUBLE_O<T>
    {
        public int Get(T t);
        public int Min(T t);
        public int Max(T t);

        public default double GetD(T t)
        {
            if (Max(t) == 0)
                return 0;
            return Get(t) / (double)Max(t);
        }

        public default bool IsMax(T t)
        {
            return Get(t) == Max(t);
        }

        public interface INT_OE<T> : INT_O<T>, DOUBLE_OE<T>
        {
            public default void SetMax(T t)
            {
                Set(t, Max(t));
            }

            public void Set(T t, int i);

            public default DOUBLE_OE<T> SetD(T t, double d)
            {
                Set(t, (int)(Max(t) * d));
                return this;
            }

            public default DOUBLE_OE<T> IncD(T t, double d)
            {
                int i = (int)(Max(t) * d);
                if (i == 0)
                    if (d < 0)
                        i = -1;
                    else
                        i = 1;
                Inc(t, i);
                return this;
            }

            public default void Inc(T t, int i)
            {
                Set(t, Math.Clamp(Get(t) + i, Min(t), Max(t)));
            }

            public default void IncFraction(T t, double d)
            {
                int am = (int)d;
                if (am != d)
                {
                    if (d < 0 && -(d + am) > RND.rFloat())
                        am--;
                    else if (d > 0 && d - am > RND.rFloat())
                        am++;
                }
                Set(t, Math.Clamp(Get(t) + am, Min(t), Max(t)));
            }

            public default DOUBLE_OE<T> MoveTo(T t, double d, int target)
            {
                int am = (int)d;
                if (am != d)
                {
                    if (d < 0 && -(d + am) > RND.rFloat())
                        am--;
                    else if (d > 0 && d - am > RND.rFloat())
                        am++;
                }
                int c = Get(t);
                if (c < target)
                {
                    c += am;
                    if (c > target)
                        c = target;
                }
                else
                {
                    c -= am;
                    if (c < target)
                        c = target;
                }

                Set(t, Math.Clamp(c, Min(t), Max(t)));
                return this;
            }

            public default void AndSet(T t, int i)
            {
                Set(t, Get(t) & i);
            }

            public default void OrSet(T t, int i)
            {
                Set(t, Get(t) | i);
            }

            public default INT.INTE CreateInt(T t)
            {
                return new INT.INTE()
                {
                    Min = () => INT_OE<T>.this.Min(t),
                    Max = () => INT_OE<T>.this.Max(t),
                    Get = () => INT_OE<T>.this.Get(t),
                    Set = k => INT_OE<T>.this.Set(t, k)
                };
            }

            public default INT.INTE CreateIntInverted(T t)
            {
                return new INT.INTE()
                {
                    Min = () => INT_OE<T>.this.Min(t),
                    Max = () => INT_OE<T>.this.Max(t),
                    Get = () => INT_OE<T>.this.Max(t) - INT_OE<T>.this.Get(t),
                    Set = k => INT_OE<T>.this.Set(t, INT_OE<T>.this.Max(t) - k)
                };
            }
        }

        public static class INTWRAP<T> : INT_OE<T>
        {
            private readonly Bits bits;
            private readonly INT_OE<T> data;

            public INTWRAP(int mask, INT_OE<T> data)
            {
                this.bits = new Bits(mask);
                this.data = data;
            }

            public int Get(T t)
            {
                return bits.Get(data.Get(t));
            }

            public int Min(T t)
            {
                return 0;
            }

            public int Max(T t)
            {
                return bits.mask;
            }

            public void Set(T t, int i)
            {
                int d = data.Get(t);
                d = bits.Set(d, i);
                data.Set(t, d);
            }
        }
    }
}