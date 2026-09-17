using game.time;
using snake2d.util;

namespace settlement.room.food.farm
{
    internal sealed class Time
    {
        public readonly int days;
        public readonly int daysPlanting;
        public readonly int dayPlant;
        public readonly int dayEvent;
        public readonly int dayHarvest;
        public readonly int dayOffWork;
        public readonly int dayDeath;
        public readonly int daysWorking;
        public readonly double daysWorkingI;

        public Time(ROOM_FARM b)
        {
            days = (int)TIME.years().bitConversion(TIME.days());

            daysPlanting = (int)Math.Ceiling(days * 3.0 / 8.0);
            dayHarvest = (int)Math.Round(b.crop.seasonalOffset * days);
            dayOffWork = MATH.mod(dayHarvest + 1, days);

            dayDeath = MATH.mod(dayHarvest + 2, days);

            dayPlant = MATH.mod(dayHarvest - daysPlanting, days);
            dayEvent = dayPlant + 1;
            daysWorking = days - 2;
            daysWorkingI = 1.0 / daysWorking;
        }

        public double day()
        {
            return TIME.years().bitPartOf() * days;
        }

        public int dayI()
        {
            return (int)day();
        }

        public bool isHarvest()
        {
            return dayI() == dayHarvest || dayI() == MATH.mod(dayHarvest + 1, days);
        }

        public double daysToHarvest()
        {
            if (isHarvest())
                return 0;
            return MATH.distance(day(), dayHarvest, days);
        }
    }
}