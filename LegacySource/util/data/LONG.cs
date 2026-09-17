using System;

namespace Util.Data
{
    public interface LONG
    {
        long Get();
    }

    public interface LONGE : LONG
    {
        void Set(long i);
    }
}