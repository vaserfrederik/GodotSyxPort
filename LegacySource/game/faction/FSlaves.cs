using System;
using init.race;

namespace game.faction
{
    public abstract class FSlaves : FactionResource
    {
        public abstract int available(Race race);
        public abstract void trade(Race race, int am, int credits);
        public abstract int price(Race race, int am);

        public static int B22ASE_PRICE(Race race)
        {
            int days = 20;
            days *= FACTIONS.PRICE().edibleLow();
            return days;
        }
    }
}