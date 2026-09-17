using System;

namespace snake2d.util.rnd
{
    public class RND
    {
        private static Random rnd;
        private static int seed;

        static RND()
        {
            rnd = new Random();
            seed = rnd.Next();
            rnd = new Random(seed);
            long seed = rnd.NextInt64();
            Console.WriteLine($"[RND] ---Initiating random, seed: {seed}");
            //rnd.Seed = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //seed stuff
        }

        public static int rInt()
        {
            return rnd.Next();
        }

        public static bool rBoolean()
        {
            return rnd.NextBoolean();
        }

        public static int rInt(int max)
        {
            return rnd.Next(max);
        }

        public static int rInt0(int dist)
        {
            if (dist == 0)
                return 0;
            return -dist + rnd.Next(dist * 2 + 1);
        }

        public static float rFloat()
        {
            return (float)rnd.NextDouble();
        }

        public static float rFloatP(float exponent)
        {
            float res = RND.rFloat();
            while (exponent > 1)
            {
                res *= res;
                exponent--;
            }
            return res;
        }

        public static float rFloat(double d)
        {
            return (float)(rnd.NextDouble() * d);
        }

        /**
         * 
         * @param d
         * @return get a float from 1-d to 1+d
         */
        public static float rFloat1(double d)
        {
            return (float)((1.0 - d) + rnd.NextDouble() * d * 2.0);
        }

        /**
         * 
         * @param d
         * @return a float f, f >= -d, f < d
         */
        public static float rFloat0(double d)
        {
            return (float)(-d + rnd.NextDouble() * d * 2.0);
        }

        public static bool oneIn(int what)
        {
            if (what <= 1)
                return true;
            return rnd.Next(what) == what - 1;
        }

        public static bool oneIn(double what)
        {
            return oneIn((int)what);
        }

        public static bool oneInD(double what)
        {
            return rFloat() * what < 1;
        }

        public static short rShort(int upperBound)
        {
            return (short)rInt(upperBound);
        }

        public static short rShort()
        {
            return rShort(short.MaxValue);
        }

        public static long rLong()
        {
            return rnd.NextInt64();
        }

        public static float rExpo()
        {
            float f = rFloat();
            return f * f;
        }

        public static double rSign()
        {
            if (rBoolean())
                return 1.0;
            return -1.0;
        }

        public static int seed()
        {
            return seed;
        }

        public static void setSeed(int seed)
        {
            RND.seed = seed;
            rnd = new Random(seed);
        }

        // public static final class UniqueGen {
        // 
        // private long state = System.nanoTime();  // or any seed
        //
        // public int nextUnique(int maxExclusive) {
        // state = (state * 1664525L + 1013904223L) & 0xFFFFFFFFL;
        // return (int)(state % maxExclusive);
        // }
        //
        // }
        //
        // public static void main(String[] args) {
        // UniqueGen g = new UniqueGen();
        // for (int i = 0; i < 20; i++) {
        // System.out.println(g.nextUnique(21));  // 0..10
        // }
        // }
    }
}