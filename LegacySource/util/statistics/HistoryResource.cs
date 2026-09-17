using System.Collections.Generic;
using game.time;
using init.resources;
using util.info;

namespace util.statistics
{
    public class HistoryResource : HistoryObject<RESOURCE>
    {
        public HistoryResource(int size, TIMECYCLE time, bool keep)
            : this(null, size, time, keep)
        {
        }

        public HistoryResource(INFO info, int size, TIMECYCLE time, bool keep)
            : base(info, size, time, keep, RESOURCES.Map())
        {
        }
    }
}