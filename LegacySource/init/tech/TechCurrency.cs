using System;
using System.Collections.Generic;
using snake2d.util.file;
using snake2d.util.sets;

namespace init.tech
{
    public sealed class TechCurrency
    {
        public readonly Boostable Bo;
        public readonly int Index;

        public TechCurrency(Boostable bo, int index)
        {
            this.Bo = bo;
            this.Index = index;
        }
    }

    public class TechCurrencies
    {
        public readonly ArrayListGrower<TechCurrency> All = new ArrayListGrower<TechCurrency>();

        public LIST<TechCost> Read(Json json)
        {
            ArrayListGrower<TechCost> cc = new ArrayListGrower<TechCost>();
            json = json.Json("COSTS");

            foreach (string k in json.Keys())
            {
                Boostable bo = BOOSTING.MAP().TryGet(k);
                if (bo == null)
                {
                    json.Error("The boostable: " + k + " does not exist in this context. The boostable in question must be predefined in the game and cannot be dynamic.", k);
                }
                double am = json.D(k, 0, 10000000);
                cc.Add(Add(bo, am));
            }
            return cc;
        }

        private TechCost Add(Boostable bo, double am)
        {
            foreach (TechCurrency c in All)
            {
                if (c.Bo == bo)
                {
                    return new TechCost(c, am);
                }
            }
            TechCurrency c = new TechCurrency(bo, All.Size);
            All.Add(c);
            return new TechCost(c, am);
        }
    }
}