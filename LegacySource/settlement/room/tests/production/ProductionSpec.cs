using settlement.room.industry.module;

namespace settlement.room.tests.production
{
    public interface ProductionSpec
    {
        public double Bonus(Industry ins);
        public double ConsumptionBonus(Industry ins);
        public double WPerItemUsed();
        public double AddedW();
    }
}