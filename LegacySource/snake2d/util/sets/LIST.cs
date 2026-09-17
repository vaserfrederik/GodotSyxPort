using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake2D.Util.Sets
{
    /**
     * An unmodifiable "arraylist"
     * @author mail__000
     *
     * @param <E>
     */
    public interface IList<E> : IEnumerable<E>
    {

        /**
         * if there exists something at this index, you'll get it.
         * @param index
         * @return
         */
        E Get(int index);

        default E GetC(int index)
        {
            if (Size() == 0)
                return default(E);
            int remainder = (index % Size());
            index = ((remainder >> 31) & Size()) + remainder;
            return Get(index);
        }

        default E Rnd()
        {
            return Get(RND.RInt(Size()));
        }

        /**
         * is there an object at given index? 1 complexity.
         * @param i
         * @return
         */
        bool Contains(int i);

        /**
         * Does this contain this object? N complexity.
         * @param object
         * @return
         */
        bool Contains(E obj);

        /**
         * 
         * @return the nr of elements this list contains
         */
        int Size();

        /**
         * 
         * @return
         */
        bool IsEmpty();

        default E Last()
        {
            return Get(Size() - 1);
        }

        default IList<E> Join(IList<E> other)
        {
            List<E> n = new List<E>(Size() + other.Size());
            for (int i = 0; i < Size(); i++)
                n.Add(Get(i));
            for (int i = 0; i < other.Size(); i++)
                n.Add(other.Get(i));
            return n as IList<E>;
        }

        default IList<E> Join(params E[] others)
        {
            List<E> n = new List<E>(Size() + others.Length);
            for (int i = 0; i < Size(); i++)
                n.Add(Get(i));
            for (int i = 0; i < others.Length; i++)
                n.Add(others[i]);
            return n as IList<E>;
        }
    }
}