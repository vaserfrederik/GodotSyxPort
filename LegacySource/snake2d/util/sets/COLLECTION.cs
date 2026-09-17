using System;

namespace Snake2D.Util.Sets
{
    public interface ICollection<T>
    {
        T GetAt(int index);
    }
}