using game.boosting;
using init.sprite.UI;
using init.value;
using snake2d.util.sets;
using world.map.regions;

namespace world.region.building
{
    public sealed class RDBuildingLevel : INDEXED
    {
        public readonly BoostSpecs local;
        private BoostSpecs global;
        public readonly Lockable<Region> reqs;
        public readonly Icon icon;
        public readonly ICharSequence name;
        public int index;
        public int cost = 0;

        public RDBuildingLevel(ICharSequence name, Icon icon, Lockable<Region> needs)
        {
            local = new BoostSpecs(name, icon, false);
            global = new BoostSpecs(name, icon, false);

            this.name = name;
            this.icon = icon;
            this.reqs = needs;
        }

        public int Index()
        {
            return index;
        }
    }
}