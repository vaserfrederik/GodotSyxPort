using System;

namespace Init.Tech
{
    public sealed class TechCost
    {
        public readonly TechCurrency Cu;
        public readonly double Amount;

        public TechCost(TechCurrency cu, double amount)
        {
            this.Cu = cu;
            this.Amount = amount;
        }
    }
}