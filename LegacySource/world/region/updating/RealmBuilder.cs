using init.race;
using init.religion;
using init.trade;
using world.map.regions;

namespace world.region.updating
{
    public interface RealmBuilder
    {
        double policy(Race race, Region reg);
        double priority(TRADABLE res, Region reg);
        double priority(Religion religion, Region reg);
        double military(Region reg);
        double size();
    }
}