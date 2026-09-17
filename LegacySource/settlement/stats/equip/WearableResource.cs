using init.resources;
using settlement.stats;

namespace settlement.stats.equip
{
    public interface WearableResource
    {
        RESOURCE resource(Induvidual i);
        void wearOut(Induvidual i);
        int max(Induvidual i);
        int target(Induvidual i);
        double wearPerYear(Induvidual i);
        void set(Induvidual i, int am);
        int get(Induvidual i);
        int needed(Induvidual i);

        void inc(Induvidual i, int am)
        {
            set(i, get(i) + am);
        }
    }
}