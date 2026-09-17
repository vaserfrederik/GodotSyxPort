using System;
using System.IO;

namespace Init.Trade
{
    public class TR_STOCKPILE : INT_OE<TRADABLE>, SAVABLE
    {
        private int[] amounts;

        public TR_STOCKPILE()
        {
            amounts = new int[TR.ALL().Size];
        }

        public int Get(TRADABLE res)
        {
            return amounts[res.Index()];
        }

        public void Set(TRADABLE res, int amount)
        {
            amounts[res.Index()] = amount;
        }

        public void Add(TRADABLE res, int inc)
        {
            amounts[res.Index()] += inc;
        }

        public void Save(FilePutter file)
        {
            TR.MAP().Saver().Save(amounts, file);
        }

        public void Load(FileGetter file)
        {
            TR.MAP().Loader().Load(amounts, file, 0);
        }

        public void Clear()
        {
            Array.Fill(amounts, 0);
        }

        public int Get(int ri)
        {
            return amounts[ri];
        }

        public int Min(TRADABLE t)
        {
            return 0;
        }

        public int Max(TRADABLE t)
        {
            return int.MaxValue;
        }
    }
}