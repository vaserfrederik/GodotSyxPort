using System;
using System.Collections.Generic;

namespace snake2d.util.sets
{
    public interface ADDABLE<E>
    {
        int add(E e);
        void add(IEnumerable<E> es);
        void add(E[] es);
        E addReturn(E element);
        int tryAdd(E e);
        bool hasRoom();
    }
}