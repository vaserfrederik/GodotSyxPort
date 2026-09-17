using game.faction;
using settlement.entity.humanoid;
using settlement.room.main;

namespace settlement.room.industry.module
{
    public interface IROOM_PRODUCER_INSTANCE : IROOM_IDATA_INSTANCE
    {
        Industry Industry();
        int IndustryI();
        void SetIndustry(int i);

        void UpdateIndustryLocks()
        {
            Industry inInd = Industry();
            if (!inInd.Lockable.Passes(FACTIONS.Player()))
            {
                SetIndustry(0);
            }
        }

        double ProductionRate(RoomInstance ins, Humanoid h, Industry inInd, IndustryResource oo)
        {
            return IndustryUtil.CalcProductionRate(oo.Rate, h, inInd, ins) * ins.Employees().TotEfficiency();
        }
    }
}