using game.time;

namespace settlement.room.food.farm
{
    class Util
    {
        private Util()
        {
        }

        public static double Prospect(FarmInstance ins)
        {
            double baseValue = Base(ins) * ins.Industry().Outs()[0].Rate;

            double skill = ins.TData.Skill();
            double work = ins.TData.Work();
            return baseValue * skill * work * TIME.Years().BitConversion(TIME.Days());
        }

        public static double Base(FarmInstance ins)
        {
            double area = ins.Area() / ROOM_FARM.WORKERPERTILE;
            return area;
        }

        public static int PrevHarvest(FarmInstance ins)
        {
            ROOM_FARM b = ins.BlueprintI();
            Time t = b.Time;
            if (t.DayI() < t.DayDeath)
                return (int)b.Industries()[0].Outs()[0].YearPrev[ins];
            else
                return (int)b.Industries()[0].Outs()[0].Year[ins];
        }
    }
}