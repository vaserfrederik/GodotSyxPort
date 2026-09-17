using System;
using System.Collections.Generic;

namespace snake2d.util.sets
{
    public interface TRANSFORMER<F, T>
    {
        T Transform(F f);

        default List<T> ToArrayList(params F[] fs)
        {
            List<T> res = new List<T>(fs.Length);
            foreach (F f in fs)
                res.Add(Transform(f));
            return res;
        }
    }
}