using System;

namespace Init.Value
{
    public sealed class Lock<T>
    {
        public readonly Lockable<T> Lockable;
        public readonly Locker<T> Unlocker;

        public Lock(Lockable<T> lockable, Locker<T> unlocker)
        {
            this.Lockable = lockable;
            this.Unlocker = unlocker;
        }
    }
}