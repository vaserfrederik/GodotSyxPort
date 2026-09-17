using System;
using init.race;
using util.data.BOOLEANO;

namespace settlement.room.law
{
    public interface PUNISHMENT_SERVICE
    {
        public int punishTotal();
        public int punishUsed();

        public default BOOLEAN_OE<Race> punishEnabled()
        {
            return null;
        }
    }
}