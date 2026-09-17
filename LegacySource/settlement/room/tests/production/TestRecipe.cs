using System.Collections.Generic;
using init.resources;
using settlement.room.industry.module;

namespace settlement.room.tests.production
{
    public class TestRecipe
    {
        private List<Input> inputs = new List<Input>();
        public readonly Industry ins;
        public readonly RESOURCE res;
        public readonly double rate;
        public readonly double wPerItem;
        public readonly int index;

        public TestRecipe(int index, Industry ins, IndustryResource outResource)
        {
            this.ins = ins;
            res = outResource.resource;
            rate = outResource.rate;
            wPerItem = 1.0 / outResource.rate;
            this.index = index;
        }

        public TestRecipe(int index, TestRecipe r)
        {
            this.ins = r.ins;
            res = r.res;
            wPerItem = r.wPerItem;
            rate = r.rate;
            this.index = index;
        }

        public List<Input> inputs()
        {
            return inputs;
        }

        public double wPerItem(ProductionSpec ibonuses)
        {
            return (wPerItem + ibonuses.wPerItemUsed()) / ibonuses.bonus(ins);
        }

        public double wTotPerItem(ProductionSpec ibonuses)
        {
            double w = wPerItem(ibonuses);
            for (int i = 0; i < inputs.Count; i++)
            {
                Input ii = inputs[i];
                w += (ii.amount * ii.producer.wTotPerItem(ibonuses) / (rate * ibonuses.consumptionBonus(ins)));
            }
            return w;
        }

        public double amountPerW(ProductionSpec ibonuses)
        {
            return 1.0 / wPerItem(ibonuses);
        }

        public double amountPerWTot(ProductionSpec ibonuses)
        {
            return 1.0 / wTotPerItem(ibonuses);
        }

        public double pricePerItem(ProductionSpec ibonuses)
        {
            double p = wPerItem(ibonuses);
            for (int i = 0; i < inputs.Count; i++)
            {
                Input ii = inputs[i];
                p += ii.producer.pricePerItem(ibonuses) * ii.amount / (rate * ibonuses.consumptionBonus(ins));
            }
            return p + ibonuses.addedW();
        }
    }
}