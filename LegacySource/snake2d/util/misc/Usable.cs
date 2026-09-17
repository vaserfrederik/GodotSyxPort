using System;

namespace snake2d.util.misc
{
    public class Usable<T>
    {
        private readonly T t;
        private object user;

        public Usable(T t)
        {
            this.t = t;
        }

        public T Use(object user)
        {
            if (this.user != null)
                throw new Exception("In use by: " + this.user);
            this.user = user;
            return t;
        }

        public void Done()
        {
            user = null;
        }
    }
}