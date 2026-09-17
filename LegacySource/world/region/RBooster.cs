using game.boosting;
using game.faction;
using world.map.regions;

namespace world.region
{
    public abstract class RBooster : BoosterImp
    {
        public RBooster(BSourceInfo info, double from, double to, bool isMul) : base(info, from, to, isMul)
        {
        }

        public override double vGet(Faction f)
        {
            return 0;
        }

        protected abstract double get(Region reg);

        public override double vGet(Region reg)
        {
            return get(reg);
        }
    }
}