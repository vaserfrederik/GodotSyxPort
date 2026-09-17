using util.info;

namespace util.data
{
    public interface GETTER_TRANS<F, T>
    {
        T get(F f);

        default INFO info()
        {
            return null;
        }

        public interface GETTER_TRANSE<F, T> : GETTER_TRANS<F, T>
        {
            void set(F f, T t);
        }
    }
}