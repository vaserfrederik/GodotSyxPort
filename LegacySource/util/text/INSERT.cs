using System;
using game.faction;
using game.faction.royalty;
using init.race;
using settlement.entity.humanoid;
using settlement.stats;
using world.map.regions;

namespace util.text
{
    public static class INSERT
    {
        public static readonly Inserter<Induvidual> indu = new InsertIndu();
        public static readonly Inserter<Race> race = new InsertRace();
        public static readonly Inserter<Humanoid> human = new InsertHuman();
        public static readonly Inserter<Faction> faction = new InsertFaction();
        public static readonly Inserter<int> player = new InsertPlayer();
        public static readonly Inserter<Region> reg = new InsertRegion();
        public static readonly Inserter<Royalty> royalty = new InsertRoyalty();
    }
}