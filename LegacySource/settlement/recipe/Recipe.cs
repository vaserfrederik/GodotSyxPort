using System;
using System.Collections.Generic;
using game.boosting;
using init.trade;
using settlement.main;
using snake2d.util.sets;
using snake2d.util.sprite;

namespace settlement.recipe
{
    public class Recipe : INDEXED
    {
        public readonly TRADABLE Out;
        public readonly Boostable Bo;
        public readonly double Rate;
        public readonly double AiRate;
        public readonly double AiRecovery;
        public readonly string Name;
        public readonly SPRITE Icon;
        public readonly int Index;
        public readonly int IndustryIndex;
        public readonly RecipeInput[] Ins;

        public Recipe(int index, int industryIndex, TRADABLE outt, double rate, double aiRate, double aiRecovery, Boostable bo, Boostable consumption, string name, SPRITE icon, LIST<RecipeInput> inss)
        {
            this.Index = index;
            this.IndustryIndex = industryIndex;
            this.Bo = bo;
            this.Out = outt;
            this.Rate = rate;

            // lets say logistics costs 0.05W
            // {
            //     double d = 1.0 / aiRate;
            //     d += 0.05;
            //     aiRate = 1.0 / d;
            // }

            this.AiRate = aiRate;
            this.AiRecovery = aiRecovery;
            this.Name = name;
            this.Icon = icon;
            Ins = new RecipeInput[inss.Size()];
            for (int i = 0; i < inss.Size(); i++)
                Ins[i] = inss.Get(i);
        }

        public override int Index()
        {
            return Index;
        }

        public double Manpower()
        {
            return 1.0 / AiRate;
        }

        public double ManpowerTotal()
        {
            return SETT.RECIPES().RatesV.VanillaRate(Out);
        }

        public double Manpower(BOOSTABLE_O b)
        {
            return 1.0 / (AiRate * Bo.Get(b));
        }

        public double ManpowerTotal(BOOSTABLE_O b)
        {
            return SETT.RECIPES().Rates.RateTotal(b, this);
        }
    }
}