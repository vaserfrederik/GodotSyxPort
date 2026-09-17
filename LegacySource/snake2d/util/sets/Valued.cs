using System;

namespace Snake2D.Util.Sets
{
    public interface IValued<T>
    {
        T T { get; }
        double Value { get; }

        public class ValuedImp<T> : IValued<T>
        {
            public readonly T T;
            public double Value;

            public ValuedImp(T t)
            {
                T = t;
            }

            public T T => T;

            public double Value => Value;

            public ValuedImp<T> Set(double d)
            {
                Value = d;
                return this;
            }
        }
    }
}