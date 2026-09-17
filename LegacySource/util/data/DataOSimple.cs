using System;
using System.Collections.Generic;

namespace Util.Data
{
    public abstract class DataOSimple<T>
    {
        public DataOSimple()
        {
        }

        protected abstract long[] Data(T t);

        public int LongCount()
        {
            return countLong + 1;
        }

        private int countLong = -1;

        private readonly Count cInt = new Count(32, null);
        private readonly Count cShort = new Count(16, cInt);
        private readonly Count cByte = new Count(8, cShort);
        private readonly Count cNibble = new Count(4, cByte);
        private readonly Count cCrumb = new Count(2, cNibble);
        private readonly Count cBit = new Count(1, cCrumb);

        private class Count
        {
            private readonly int size;
            private int pScroll = 0;
            private int longI;
            private int count = 1;
            private readonly Count next;

            public Count(int size, Count next)
            {
                this.size = size;
                this.next = next;
            }

            public Count Count()
            {
                if (next == null)
                {
                    count++;
                    if (count > 1)
                    {
                        countLong++;
                        count = 0;
                        longI = countLong;
                    }

                    return this;
                }

                count++;
                if (count > 1)
                {
                    next.Count();
                    pScroll = next.Scroll();
                    count = 0;
                    longI = next.longI;
                }

                return this;
            }

            public int Scroll()
            {
                return pScroll + count * size;
            }
        }

        private class DataAbs : INT_OE<T>
        {
            private readonly int iLong;
            private readonly int scroll;
            private readonly long mask;
            private readonly INFO info;

            public DataAbs(INFO info, Count c)
            {
                c.Count();
                this.scroll = c.Scroll();
                this.mask = ((1L << (c.size)) - 1);
                iLong = c.longI;
                this.info = info;

                long cc = mask;
                cc = cc << scroll;
            }

            public INFO Info()
            {
                return info;
            }

            public int Get(T t)
            {
                return (int)((Data(t)[iLong] >>> scroll) & mask);
            }

            public int Min(T t)
            {
                return 0;
            }

            public int Max(T t)
            {
                return (int)mask;
            }

            public void Set(T t, int s)
            {
                if (s < Min(t) || s > Max(t))
                    throw new RuntimeException(s + " " + Min(t) + " " + Max(t));
                long c = mask;
                s &= mask;
                Data(t)[iLong] &= ~(mask << scroll);
                c = s & 0x0FFFFFFFFL;
                c = c << scroll;
                Data(t)[iLong] |= c;
            }
        }

        public class DataBit : DataAbs, BOOLEAN_OE<T>
        {
            public DataBit(INFO info) : base(info, cBit)
            {
            }

            public DataBit() : this(null)
            {
            }

            public DataBit(string name, string desc) : this(new INFO(name, desc))
            {
            }

            public bool Is(T t)
            {
                return Get(t) == 1;
            }

            public BOOLEAN_OE<T> Set(T t, bool b)
            {
                Set(t, b ? 1 : 0);
                return this;
            }
        }

        public class DataNibble : DataAbs, INT_OE<T>
        {
            private readonly int max;

            public DataNibble(INFO info, int max) : base(info, cNibble)
            {
                this.max = max;
            }

            public DataNibble() : this(null, 0x0F)
            {
            }

            public DataNibble(int max) : this(null, max)
            {
            }

            public DataNibble(string name, string desc) : this(new INFO(name, desc), 0x0F)
            {
            }

            public DataNibble(string name, string desc, int max) : this(new INFO(name, desc), max)
            {
            }

            public override int Max(T t)
            {
                return max;
            }
        }

        public class DataByte : DataAbs, INT_OE<T>
        {
            private readonly int max;

            public DataByte(INFO info, int max) : base(info, cByte)
            {
                this.max = max;
            }

            public DataByte(INFO info) : this(info, 255)
            {
            }

            public DataByte(int max) : this(null, max)
            {
            }

            public DataByte() : this(null)
            {
            }

            public DataByte(string name, string desc) : this(new INFO(name, desc))
            {
            }

            public override int Max(T t)
            {
                return max;
            }

            public override void Set(T t, int s)
            {
                if (s < Min(t) || s > max)
                    throw new RuntimeException(s.ToString());
                base.Set(t, s);
            }
        }

        public class DataShort : DataAbs, INT_OE<T>
        {
            private readonly int max;

            public DataShort(INFO info, int max) : base(info, cShort)
            {
                this.max = max;
            }

            public DataShort(INFO info) : this(info, 0x0FFFF)
            {
            }

            public DataShort() : this(null)
            {
            }

            public DataShort(string name, string desc) : this(new INFO(name, desc))
            {
            }

            public DataShort(string name, string desc, int max) : this(new INFO(name, desc), max)
            {
            }

            public override int Min(T t)
            {
                return 0;
            }

            public override int Max(T t)
            {
                return max;
            }

            public override void Set(T t, int s)
            {
                if (s < Min(t) || s > max)
                    throw new RuntimeException(s.ToString());
                base.Set(t, s);
            }
        }

        public class DataInt : DataAbs, INT_OE<T>
        {
            private readonly int max;

            public DataInt(INFO info, int max) : base(info, cInt)
            {
                this.max = max;
            }

            public DataInt() : this(null, int.MaxValue)
            {
            }

            public DataInt(INFO info) : this(info, int.MaxValue)
            {
            }

            public DataInt(string name, string desc) : this(new INFO(name, desc), int.MaxValue)
            {
            }

            public override int Max(T t)
            {
                return max;
            }

            public override void Set(T t, int s)
            {
                if (s < Min(t) || s > max)
                    throw new RuntimeException(s.ToString());
                base.Set(t, s);
            }
        }

        public class DataFloat : DOUBLE_OE<T>
        {
            private readonly DataInt dd = new DataInt();
            private INFO info;

            public DataFloat(INFO info)
            {
                this.info = info;
            }

            public DataFloat() : this(null)
            {
            }

            public double GetD(T t)
            {
                return BitConverter.ToSingle(BitConverter.GetBytes(dd.Get(t)), 0);
            }

            public DOUBLE_OE<T> SetD(T t, double d)
            {
                int i = BitConverter.ToInt32(BitConverter.GetBytes((float)d), 0);
                dd.Set(t, i);
                return this;
            }

            public INFO Info()
            {
                return info;
            }
        }

        public class DataLong : LONG_OE<T>
        {
            private readonly int longI;

            public DataLong()
            {
                countLong++;
                this.longI = countLong;
            }

            public long Get(T t)
            {
                return Data(t)[longI];
            }

            public void Set(T t, long i)
            {
                Data(t)[longI] = i;
            }
        }

        public class DataDouble : DOUBLE_OE<T>
        {
            private readonly DataLong dd = new DataLong();
            private INFO info;

            public DataDouble(INFO info)
            {
                this.info = info;
            }

            public DataDouble() : this(null)
            {
            }

            public double GetD(T t)
            {
                return BitConverter.Int64BitsToDouble(dd.Get(t));
            }

            public DOUBLE_OE<T> SetD(T t, double d)
            {
                long i = BitConverter.DoubleToInt64Bits(d);
                dd.Set(t, i);
                return this;
            }

            public INFO Info()
            {
                return info;
            }
        }
    }
}