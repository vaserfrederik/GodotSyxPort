using System;

namespace snake2d.util.sets
{
    public interface Tuple<A, B>
    {
        A a();
        B b();
    }

    public class TupleImp<A, B> : Tuple<A, B>
    {
        public A a;
        public B b;

        public TupleImp()
        {
        }

        public TupleImp(A a, B b)
        {
            this.a = a;
            this.b = b;
        }

        public A a()
        {
            return a;
        }

        public B b()
        {
            return b;
        }
    }

    public class TupleD<A>
    {
        public readonly A a;
        public double d;

        public TupleD(A a)
        {
            this.a = a;
        }
    }
}