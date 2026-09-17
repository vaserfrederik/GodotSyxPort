using System;
using System.Collections.Generic;
using game.boosting;
using init.paths.PATHS;
using settlement.stats;
using settlement.stats.colls;
using snake2d.util.sets;

namespace init.type
{
    public class NEED_E : NEED
    {
        private readonly int indexE;

        public NEED_E(string key, ResFolder f, LISTE<NEED> all, LISTE<NEED_E> alle, BoostableCat cat)
            : base(key, f, all, cat, null, true)
        {
            indexE = alle.Add(this);
        }

        public StatNeedNormal Stat()
        {
            return STATS.NEEDS().SNEEDS.Get(indexE);
        }
    }
}