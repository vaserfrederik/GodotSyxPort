using System;
using System.IO;
using System.Linq;

namespace settlement.trade
{
    public class TradableData : INT_OE<TRADE_TYPE>, SAVABLE
    {
        private int[] data = Alloc.ii(TRADE_TYPE.all.size());
        private int total = 0;

        public int get(TRADE_TYPE t)
        {
            if (t == null)
                return total;
            return data[t.index];
        }

        public int min(TRADE_TYPE t)
        {
            return 0;
        }

        public int max(TRADE_TYPE t)
        {
            return int.MaxValue;
        }

        public void set(TRADE_TYPE t, int i)
        {
            total -= data[t.index];
            data[t.index] = i;
            total += data[t.index];
        }

        public void save(FilePutter file)
        {
            file.isE(data);
        }

        public void load(FileGetter file)
        {
            file.isE(data);
            total = 0;
            foreach (int i in data)
                total += i;
        }

        public void clear()
        {
            data.Fill(0);
            total = 0;
        }
    }
}