using System;
using System.Collections.Generic;

namespace Settlement.Room.Industry.Module
{
    public abstract class IndustryRegion
    {
        private static List<IndustryRegion> all = new List<IndustryRegion>();
        static IndustryRegion()
        {
            GameDisposable.Add(new GameDisposable(() =>
            {
                all.Clear();
            }));
        }

        public readonly int index;
        public readonly Industry ins;
        public readonly double rarity;

        public IndustryRegion(Industry ins, double rarity)
        {
            this.ins = ins;
            this.index = all.Count;
            all.Add(this);
            this.rarity = rarity;
            ins.reg = this;
        }

        public abstract double Occurrence(Region reg);

        public static List<IndustryRegion> All()
        {
            return all;
        }
    }
}