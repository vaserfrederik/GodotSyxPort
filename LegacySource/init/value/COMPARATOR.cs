using System.Collections.Generic;
using util.keymap;

namespace init.value
{
    public abstract class COMPARATOR : MAPPED
    {
        private static LinkedList<COMPARATOR> all = new LinkedList<COMPARATOR>();

        public static readonly COMPARATOR LESS = new COMPARATOR("LESS", "<")
        {
            public override bool passes(double a, double b)
            {
                return a < b;
            }

            public override double progress(double a, double b)
            {
                return b / a;
            }
        };

        public static readonly COMPARATOR GREATER = new COMPARATOR("GREATER", ">")
        {
            public override bool passes(double a, double b)
            {
                return a > b;
            }

            public override double progress(double a, double b)
            {
                return a / b;
            }
        };

        public static readonly COMPARATOR GREATERE = new COMPARATOR("GREATERE", ">=")
        {
            public override bool passes(double a, double b)
            {
                return a >= b;
            }

            public override double progress(double a, double b)
            {
                return a / b;
            }
        };

        public static readonly COMPARATOR EQUAL = new COMPARATOR("EQUAL", "=")
        {
            public override bool passes(double a, double b)
            {
                return a == b;
            }

            public override double progress(double a, double b)
            {
                return 1.0 - Math.Abs(a - b);
            }
        };

        public static readonly COMPARATOR NEQUAL = new COMPARATOR("NEQUAL", "!=")
        {
            public override bool passes(double a, double b)
            {
                return a != b;
            }

            public override double progress(double a, double b)
            {
                return a == b ? 0 : 1;
            }
        };

        public static readonly RMAP<COMPARATOR> map = new RMAP<COMPARATOR>("COMPARATOR", all);

        public readonly string KEY;
        public readonly string rep;
        private readonly int index;

        private COMPARATOR(string key, string rep)
        {
            this.rep = rep;
            this.KEY = key;
            this.index = all.Add(this);
        }

        public override int index()
        {
            return index;
        }

        public static LinkedList<COMPARATOR> ALL()
        {
            return all;
        }

        public abstract bool passes(double a, double b);

        public abstract double progress(double a, double b);

        public override string key()
        {
            return KEY;
        }
    }
}