using settlement.room.main;
using util.info;

namespace settlement.room.industry.module
{
    public interface RoomBoost
    {
        INFO Info();
        double Get(RoomInstance r);
        double Min() => 0;
        double Max() => 1.0;
    }
}