using System;
using game.time;
using init.trade;
using util.info;

namespace util.statistics
{
    public class HistoryTradable : HistoryObject<TRADABLE>
    {
        public HistoryTradable(int size, TIMECYCLE time, bool keep)
            : this(null, size, time, keep)
        {
        }

        public HistoryTradable(INFO info, int size, TIMECYCLE time, bool keep)
            : base(info, size, time, keep, TR.MAP())
        {
        }
    }
}