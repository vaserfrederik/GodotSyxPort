using System;
using System.IO;
using System.Linq;

namespace Init.Resources
{
    public interface IStockpile : INT_O<RESOURCE>
    {
        int Get(int ri);
    }

    public class StockpileImp : IStockpile, SAVABLE, INT_OE<RESOURCE>
    {
        private int[] amounts = new int[RESOURCES.ALL().Count];

        public int Get(RESOURCE res)
        {
            return amounts[res.BIndex()];
        }

        public void Set(RESOURCE res, int amount)
        {
            amounts[res.BIndex()] = amount;
        }

        public void Add(RESOURCE res, int inc)
        {
            amounts[res.BIndex()] += inc;
        }

        public void Save(FilePutter file)
        {
            file.IsE(amounts);
        }

        public void Load(FileGetter file)
        {
            file.IsE(amounts);
        }

        public void Clear()
        {
            amounts.Fill(0);
        }

        public int Get(int ri)
        {
            return amounts[ri];
        }

        public int Min(RESOURCE t)
        {
            return 0;
        }

        public int Max(RESOURCE t)
        {
            return int.MaxValue;
        }
    }
}