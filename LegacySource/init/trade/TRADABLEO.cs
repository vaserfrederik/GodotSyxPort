using snake2d.util.sprite;
using util.info;

namespace init.trade
{
    public class TRADABLEO<T> : TRADABLE
    {
        public readonly T t;

        public TRADABLEO(T t, string key, int index, INFO info, SPRITE icon) : base(key, index, info, icon)
        {
            this.t = t;
        }
    }
}