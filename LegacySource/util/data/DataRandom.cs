using System;
using System.Collections.Generic;
using snake2d;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.data;

namespace util.data
{
    public class DataRandom<T>
    {
        private readonly LIST<INT_O.INT_OE<T>> randomness;
        private const double rd = 1.0 / 0b1111_1111_1111_1111_1111;

        public DataRandom(DataOSimple<T> data, int ints)
        {
            ArrayListGrower<INT_O.INT_OE<T>> all = new ArrayListGrower<INT_O.INT_OE<T>>();
            for (int i = 0; i < ints; i++)
                all.add(data.new DataInt() {
                    public override int min(T t) {
                        return int.MinValue;
                    }
                });
            randomness = all;
        }

        public DataRandom(DataO<T> data, int ints)
        {
            ArrayListGrower<INT_O.INT_OE<T>> all = new ArrayListGrower<INT_O.INT_OE<T>>();
            for (int i = 0; i < ints; i++)
                all.add(data.new DataInt("RANDOM" + i) {
                    public override int min(T t) {
                        return int.MinValue;
                    }
                });
            randomness = all;
        }

        public double getD(T r, int startBit)
        {
            int ii = get(r, startBit, 20);
            return ii * rd;
        }

        public int get(T r, int startBit, int bits)
        {
            if (bits >= 32)
                throw new RuntimeException();

            startBit &= (32 * randomness.size()) - 1;
            int ii = startBit / 32;

            long a = randomness.get(ii).get(r);
            long b = randomness.getC(ii + 1).get(r);
            a = (a & 0xFFFFFFFFL) | ((b & 0xFFFFFFFFL) << 32);
            startBit &= 32 - 1;

            a = a >> startBit;
            a &= (1 << bits) - 1;

            return (int)a;
        }

        public void copyFrom(T dest, T source)
        {
            for (int i = 0; i < randomness.size(); i++)
            {
                randomness.get(i).set(dest, randomness.get(i).get(source));
            }
        }

        public int get(T r, int startBit)
        {
            return get(r, startBit, 31);
        }

        public long getL(T r, int startBit)
        {
            long res = get(r, startBit);
            res = res << 32;
            res |= get(r, startBit + 32);
            return res;
        }

        public void setLong(T r, int li, long ll)
        {
            randomness.get(li * 2).set(r, (int)(ll >> 32));
            randomness.get(li * 2 + 1).set(r, (int)(ll));
        }

        public LIST<INT_O.INT_OE<T>> all()
        {
            return randomness;
        }

        public void randomize(T r)
        {
            foreach (INT_O.INT_OE<T> i in randomness)
                i.set(r, RND.rInt());
        }

        static void Main(string[] args)
        {
            DataOSimple<Test> dataa = new DataOSimple<Test>()
            {
                protected long[] data(Test t)
                {
                    return t.data;
                }
            };

            DataRandom<Test> rr = new DataRandom<Test>(dataa, 4);

            double i8 = 0;
            double i12 = 0;
            double i16 = 0;
            double i20 = 0;
            double i24 = 0;
            double i28 = 0;
            double i31 = 0;

            for (int i = 0; i < 1000; i++)
            {
                Test t = new Test();
                t.data = new long[dataa.longCount()];
                rr.randomize(t);

                i8 += rr.get(t, 12, 8);
                i12 += rr.get(t, 12, 12);
                i16 += rr.get(t, 12, 16);
                i20 += rr.get(t, 12, 20);
                i24 += rr.get(t, 12, 24);
                i28 += rr.get(t, 12, 28);
                i31 += rr.get(t, 12, 31);
            }

            LOG.ln(i8 / (1000 * 0b1111_1111));
            LOG.ln(i12 / (1000.0 * 0b1111_1111_1111));
            LOG.ln(i16 / (1000.0 * 0b1111_1111_1111_1111));
            LOG.ln(i20 / (1000.0 * 0b1111_1111_1111_1111_1111));
            LOG.ln(i24 / (1000.0 * 0b1111_1111_1111_1111_1111_1111));
            LOG.ln(i28 / (1000.0 * 0b1111_1111_1111_1111_1111_1111_1111));
            LOG.ln(i31 / (1000.0 * 0b0111_1111_1111_1111_1111_1111_1111_1111));
        }

        private static class Test
        {
            public long[] data;
        }
    }
}