using System;

namespace Util.Data
{
    public interface GETTER<T>
    {
        T Get();
    }

    public class GETTER_IMP<T> : GETTERE<T>
    {
        public T A { get; set; }

        public GETTER_IMP()
        {
            // TODO Auto-generated constructor stub
        }

        public GETTER_IMP(T t)
        {
            this.A = t;
        }

        public void Set(T t)
        {
            this.A = t;
        }

        public T Get()
        {
            return A;
        }
    }

    public interface GETTERE<T> : GETTER<T>
    {
        void Set(T t);
    }
}