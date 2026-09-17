using init.sprite.UI;
using snake2d.util.sprite;
using util.data;

namespace init.value
{
    public class Value<T>
    {
        public readonly SPRITE icon;
        public readonly string key;
        public readonly ICharSequence name;
        public readonly DOUBLE_O<T> d;
        public readonly bool percentage;
        public readonly bool isBool;

        public Value(string key, SPRITE icon, ICharSequence name, DOUBLE_O<T> d, bool percentage, bool isBool)
        {
            this.icon = icon.Resized(Icon.S);
            this.name = name;
            this.d = d;
            this.percentage = percentage;
            this.isBool = isBool;
            this.key = key;
        }
    }
}