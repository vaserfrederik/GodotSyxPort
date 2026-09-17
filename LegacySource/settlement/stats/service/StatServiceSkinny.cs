using System;
using System.Collections.Generic;
using settlement.main;
using settlement.stats;
using snake2d.util.sets;
using util.text;

namespace settlement.stats.service
{
    internal class StatServiceSkinny : StatServiceSimple
    {
        private static readonly string ¤¤name = "Skinnydipping";
        private static readonly string ¤¤desc = "When the weather allows for it, subjects might want to have a dip in a pool of water.";

        static StatServiceSkinny()
        {
            D.ts(typeof(StatServiceSkinny));
        }

        public StatServiceSkinny(LISTE<StatServiceImp> all, StatsInit init) : base("MISC_SKINNYDIP", all, init, ¤¤name, ¤¤desc, SETT.ROOMS().POOLS.Get(0).icon, NEEDS.TYPES().SKINNYDIP)
        {
        }
    }
}