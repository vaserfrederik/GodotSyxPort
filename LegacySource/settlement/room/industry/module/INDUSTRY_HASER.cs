using System.Collections.Generic;
using settlement.room.main;
using util.gui.misc;
using util.info;

namespace settlement.room.industry.module
{
    public interface INDUSTRY_HASER
    {
        IList<Industry> Industries();

        double IndustryFormatProductionRate(GText text, IndustryResource i, RoomInstance ins);
        double IndustryFormatConsumptionRate(GText text, IndustryResource i, RoomInstance ins);
        double IndustryFormatProductionRateEmpl(GText text, IndustryResource i, RoomInstance ins);
        void IndustryHoverProductionRate(GBox b, IndustryResource i, RoomInstance ins);
        void IndustryHoverConsumptionRate(GBox b, IndustryResource i, RoomInstance ins);
        bool IndustryIgnoreUI();
    }

    public static class INDUSTRY_HASERExtensions
    {
        public static double IndustryFormatProductionRate(this INDUSTRY_HASER instance, GText text, IndustryResource i, RoomInstance ins)
        {
            double n = IndustryUtil.CalcProductionRate(i.rate, ((ROOM_PRODUCER_INSTANCE)ins).Industry(), ins);
            n *= ins.Employees().Employed();
            double nn = i.rate * ins.Employees().Employed();

            text.Add('+');
            GFORMAT.FRel(text, n, nn);
            return n * ins.Employees().Efficiency();
        }

        public static double IndustryFormatConsumptionRate(this INDUSTRY_HASER instance, GText text, IndustryResource i, RoomInstance ins)
        {
            ROOM_PRODUCER_INSTANCE pp = (ROOM_PRODUCER_INSTANCE)ins;
            double n = IndustryUtil.CalcConsumptionRate(i.rate, ins, pp.Industry());

            n *= ins.Employees().Employed();

            GFORMAT.F0(text, -n);
            return n;
        }

        public static double IndustryFormatProductionRateEmpl(this INDUSTRY_HASER instance, GText text, IndustryResource i, RoomInstance ins)
        {
            double n = IndustryUtil.CalcProductionRate(i.rate, ((ROOM_PRODUCER_INSTANCE)ins).Industry(), ins);
            GFORMAT.FRel(text, n * ins.Employees().TotEfficiency(), i.rate);
            return n * ins.Employees().Efficiency();
        }

        public static void IndustryHoverProductionRate(this INDUSTRY_HASER instance, GBox b, IndustryResource i, RoomInstance ins)
        {
            IndustryUtil.HoverProductionRate(b, i.rate, ((ROOM_PRODUCER_INSTANCE)ins).Industry(), ins);
        }

        public static void IndustryHoverConsumptionRate(this INDUSTRY_HASER instance, GBox b, IndustryResource i, RoomInstance ins)
        {
            IndustryUtil.HoverConsumptionRate(b, i.rate, ins, ((ROOM_PRODUCER_INSTANCE)ins).Industry());
        }

        public static bool IndustryIgnoreUI(this INDUSTRY_HASER instance)
        {
            return false;
        }
    }
}