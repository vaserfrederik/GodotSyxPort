using System.Collections.Generic;
using snake2d.util.sets;

namespace init.trade
{
    public sealed class TRADE_TYPE
    {
        private static readonly ArrayListGrower<TRADE_TYPE> pall = new ArrayListGrower<TRADE_TYPE>();

        public static readonly LIST<TRADE_TYPE> all = pall;

        public static readonly TRADE_TYPE tax = new TRADE_TYPE(RTYPE.TAX, CTYPE.TAX);
        public static readonly TRADE_TYPE trade = new TRADE_TYPE(RTYPE.TRADE, CTYPE.TRADE);
        public static readonly TRADE_TYPE spoils = new TRADE_TYPE(RTYPE.SPOILS, CTYPE.TRIBUTE);
        public static readonly TRADE_TYPE diplomacy = new TRADE_TYPE(RTYPE.DIPLOMACY, CTYPE.DIPLOMACY);

        public readonly CharSequence name;
        public readonly int index;
        public readonly RTYPE rtype;
        public readonly CTYPE ctype;

        private TRADE_TYPE(RTYPE rtype, CTYPE ctype)
        {
            this.name = rtype.name;
            this.index = pall.Add(this);
            this.rtype = rtype;
            this.ctype = ctype;
        }
    }
}