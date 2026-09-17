using settlement.main;
using settlement.trade;
using snake2d.util.sprite;
using util.gui.common;
using util.info;
using util.keymap;

namespace init.trade
{
    public abstract class TRADABLE : INFO, MAPPED, IconHaser
    {
        private readonly string key;
        private readonly int index;
        private readonly SPRITE icon;

        protected TRADABLE(string key, int index, INFO info, SPRITE icon) : base(info.name, info.names, info.desc, null)
        {
            this.key = key;
            this.index = index;
            this.icon = icon;
        }

        public int index()
        {
            return index;
        }

        public string key()
        {
            return key;
        }

        public SPRITE icon()
        {
            return icon;
        }

        public override string name()
        {
            return name;
        }

        public PBuyer pb()
        {
            return SETT.TRADE().buyer(this);
        }

        public PSeller ps()
        {
            return SETT.TRADE().seller(this);
        }

        public override string ToString()
        {
            return key + "[" + index + "]";
        }
    }
}