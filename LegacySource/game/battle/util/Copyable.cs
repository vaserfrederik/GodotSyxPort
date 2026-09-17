using System;

namespace Game.Battle.Util
{
    public interface Copyable<T> : SAVABLE
    {
        void Copy(T toBeCopied);
    }
}