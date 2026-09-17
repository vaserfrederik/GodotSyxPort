using util.info;
using view.main;

namespace util.data
{
    public interface DOUBLE_O<T>
    {
        public double getD(T t);

        public interface DOUBLE_OE<T> : DOUBLE_O<T>
        {
            public default DOUBLE_OE<T> incD(T t, double d)
            {
                setD(t, getD(t) + d);
                return this;
            }

            public DOUBLE_OE<T> setD(T t, double d);
        }

        public default INFO info()
        {
            return null;
        }

        public abstract class DoubleOCached<T> : DOUBLE_O<T>
        {
            private int upI = -1;
            private T upR = default(T);
            private double cache;

            public override double getD(T t)
            {
                if (upI != VIEW.RI() || !object.Equals(upR, t))
                {
                    upI = VIEW.RI();
                    upR = t;
                    cache = getValue(t);
                }
                return cache;
            }

            public abstract double getValue(T t);
        }
    }
}