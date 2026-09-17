using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Recipe
{
    public class RecipeRatesVanilla
    {
        private readonly double[] best;
        private readonly double[] bestTotal;
        private readonly Recipe[] bestR;
        private readonly double[] rateTot;

        public RecipeRatesVanilla(Recipes recp)
        {
            rateTot = new double[recp.All().Count];

            best = new double[TR.ALL().Count];
            bestTotal = new double[TR.ALL().Count];
            bestR = new Recipe[TR.ALL().Count];

            best.Fill(double.MaxValue);
            bestTotal.Fill(double.MaxValue);

            for (int iii = 0; iii < recp.All().Count; iii++)
            {
                Recipe ii = recp.All()[iii];
                double r = 1.0 / ii.aiRate;
                if (r < best[ii.Out.Index()])
                    best[ii.Out.Index()] = r;
            }

            for (int i = 0; i < 10; i++)
            {
                for (int iii = 0; iii < recp.All().Count; iii++)
                {
                    Recipe ii = recp.All()[iii];
                    Init(ii);
                }
            }
        }

        private bool Init(Recipe ii)
        {
            double mm = 1.0 / ii.aiRate;

            foreach (RecipeInput i in ii.Ins)
            {
                if (bestTotal[i.Res.Index()] == double.MaxValue)
                    return true;
                mm += i.Rate * bestTotal[i.Res.Index()] / ii.aiRate;
            }

            rateTot[ii.Index] = mm;

            if (mm < bestTotal[ii.Out.Index()])
            {
                bestTotal[ii.Out.Index()] = mm;
                bestR[ii.Out.Index()] = ii;
                return true;
            }
            return false;
        }

        public double Vanilla(TRADABLE res)
        {
            return best[res.Index()];
        }

        public double VanillaRate(TRADABLE res)
        {
            return bestTotal[res.Index()];
        }

        public Recipe BestRecipe(TRADABLE res)
        {
            return bestR[res.Index()];
        }
    }
}