using System;
using game;
using game.debug;
using snake2d.util.file;
using snake2d.util.rnd;

namespace game.time
{
    public sealed class Intervals : GameResource
    {
        private double i20 = 0;
        private double i15 = 0;
        private double i10 = 0;
        private double i08 = 0;
        private double i05 = 0;

        private double i04 = 0;
        private double i02 = 0;
        private double i01 = 0;
        private double i005 = 0;
        private double ran = 0;
        private double[] rans = new double[128];

        public Intervals() : base("INTER", true)
        {
            for (int i = 0; i < rans.Length; i++)
            {
                rans[i] = 0.1 + RND.rFloat();
            }
        }

        protected override void Update(double ds, Profiler prof)
        {
            i20 += ds * 20.0;
            i15 += ds * 15;
            i10 += ds * 10;
            i08 += ds * 8;
            i05 += ds * 5;

            i04 += ds * 4;
            i02 += ds * 2;
            i01 += ds;
            i005 += ds * 0.5;

            int ri = (int)ran & 127;
            ran += rans[ri] * ds;
        }

        public int Get20()
        {
            return (int)i20;
        }

        public int Get15()
        {
            return (int)i15;
        }

        public int Get05()
        {
            return (int)i05;
        }

        public int Get08()
        {
            return (int)i08;
        }

        public int Get04()
        {
            return (int)i04;
        }

        public int Get02()
        {
            return (int)i02;
        }

        public int Get01()
        {
            return (int)i01;
        }

        public int Get005()
        {
            return (int)i005;
        }

        public int Get10()
        {
            return (int)i10;
        }

        public double Circle(double speed)
        {
            return Circle(i01, speed);
        }

        public static double Circle(double second, double speed)
        {
            double d = speed * second;
            d -= (int)d;

            if (d < 0.5)
            {
                return d * 2;
            }
            else
            {
                return 1 - (d - 0.5) * 2;
            }
        }

        public static double CirclePow(double second, double speed)
        {
            double d = speed * second;
            d -= (int)d;

            if (d < 0.5)
            {
                return Math.Pow(d * 2, 2);
            }
            else
            {
                return 1 - Math.Pow((d - 0.5) * 2, 2);
            }
        }

        public int Ran(double speed, int ran)
        {
            int t = (int)(((ran & 0x0FF) / 16.0 + this.ran) * speed);
            return t;
        }

        public int RanC(double speed, int ran, int max)
        {
            int t = Ran(speed, ran);
            t %= max * 2;
            if (t > max)
                t = max * 2 - t;

            return t;
        }

        public static int Get(double speed)
        {
            return (int)(GAME.Intervals().Get01() * speed);
        }

        protected override void Save(FilePutter file)
        {
            // TODO Auto-generated method stub
        }

        protected override void Load(FileGetter file)
        {
            // TODO Auto-generated method stub
        }
    }
}