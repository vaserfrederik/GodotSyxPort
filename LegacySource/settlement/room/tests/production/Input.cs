using init.resources;

namespace settlement.room.tests.production
{
    public class Input
    {
        public readonly RESOURCE res;
        public readonly double amount;
        public readonly TestRecipe producer;

        public Input(double amount, TestRecipe producer)
        {
            this.res = producer.res;
            this.amount = amount;
            this.producer = producer;
        }

        public double wTot(ProductionSpec ibonuses)
        {
            return amount * producer.wTotPerItem(ibonuses);
        }
    }
}