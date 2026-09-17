using System;
using util.data;

namespace util.statistics
{
    public interface HISTORY_COLLECTION<T> : INT_O<T>
    {
        public HISTORY_INT history(T r);
        public HISTORY_INT total();
    }
}