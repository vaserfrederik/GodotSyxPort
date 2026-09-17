using System;
using System.Collections.Generic;

namespace Settlement.Recipe
{
    public class RecipeRates
    {
        public static readonly double logisticsW = 0.1;
        private readonly double[] best;
        private readonly double[] bestTotal;
        private readonly Recipe[] bestR;
        private readonly double[] rateTot;

        private int rateI = -1;
        private BOOSTABLE_O rateII = null;
        private readonly Recipes recp;

        public RecipeRates(Recipes recp)
        {
            rateTot = new double[recp.All().Count];
            this.recp = recp;

            best = new double[TR.ALL().Count];
            bestTotal = new double[TR.ALL().Count];
            bestR = new Recipe[TR.ALL().Count];
        }

        public double RateTotal(BOOSTABLE_O cl, TRADABLE res)
        {
            Calc(cl);
            return bestTotal[res.Index()];
        }

        public double Rate(BOOSTABLE_O cl, TRADABLE res)
        {
            Calc(cl);
            return best[res.Index()];
        }

        private double RateTotal(BOOSTABLE_O cl, Recipe res)
        {
            Calc(cl);
            return rateTot[res.Index()];
        }

        public Recipe BestRecipe(BOOSTABLE_O bo, TRADABLE res)
        {
            Calc(bo);
            return bestR[res.Index()];
        }

        private void Calc(BOOSTABLE_O cl)
        {
            if (rateI == GAME.UpdateI() && rateII == cl)
                return;
            rateI = GAME.UpdateI();
            rateII = cl;

            Array.Fill(best, double.MaxValue);
            Array.Fill(bestTotal, double.MaxValue);

            for (int iii = 0; iii < recp.All().Count; iii++)
            {
                Recipe ii = recp.All()[iii];
                double r = 1.0 / (Rate(ii.AiRate) * ii.Bo.Get(cl));
                if (r < best[ii.Out.Index()])
                    best[ii.Out.Index()] = r;
            }

            for (int i = 0; i < 10; i++)
            {
                for (int iii = 0; iii < recp.All().Count; iii++)
                {
                    Recipe ii = recp.All()[iii];
                    Set2(cl, ii);
                }
            }
        }

        private double Rate(double rate)
        {
            rate = 1.0 / rate;
            rate += logisticsW;
            rate = 1.0 / rate;
            return rate;
        }

        private void Set2(BOOSTABLE_O cl, Recipe ii)
        {
            double mm = 1.0 / (Rate(ii.AiRate) * ii.Bo.Get(cl));

            if (bestR[ii.Out.Index()] == null)
                bestR[ii.Out.Index()] = ii;

            foreach (RecipeInput i in ii.Ins)
            {
                if (bestTotal[i.Res.Index()] == double.MaxValue)
                    return;
                mm += (i.Rate) * bestTotal[i.Res.Index()] / (ii.AiRate * i.Boost.Get(cl));
            }

            rateTot[ii.Index] = mm;

            if (mm < bestTotal[ii.Out.Index()])
            {
                bestTotal[ii.Out.Index()] = mm;
                bestR[ii.Out.Index()] = ii;
            }
        }
    }
}