using System;
using init.sprite.UI;

namespace game.boosting
{
    public class BSourceInfo
    {
        public readonly string name;
        public readonly string desc;
        public readonly SPRITE icon;

        public BSourceInfo(ICharSequence name, SPRITE icon)
            : this(name, null, icon)
        {
        }

        public BSourceInfo(ICharSequence name, ICharSequence append, SPRITE icon)
            : this(name, null, append, icon)
        {
        }

        public BSourceInfo(ICharSequence name, ICharSequence desc, ICharSequence append, SPRITE icon)
        {
            if (append != null)
                name = name + " (" + append + ")";
            this.name = name.ToString();
            this.desc = desc?.ToString();
            if (icon == null)
                icon = UI.icons().s.DUMMY;
            this.icon = new SPRITE.Resized(icon, Icon.S);
        }
    }
}