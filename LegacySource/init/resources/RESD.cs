using System;

namespace Init.Resources
{
    public class RESD
    {
        public readonly RESOURCE res;
        public readonly double amount;

        public RESD(RESOURCE r, double amount)
        {
            this.res = r;
            this.amount = amount;
        }
    }
}