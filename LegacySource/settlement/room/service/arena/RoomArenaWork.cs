using settlement.room.main;
using snake2d.util.datatypes;

namespace settlement.room.service.arena
{
    public interface RoomArenaWork
    {
        COORDINATE GladiatorsGetSpot(RoomInstance ins);
        RECTANGLE GladiatorsArea(int tx, int ty);
        bool GladiatorsInArena(int tx, int ty);
        void GladiatorsDrawMakeSheer(COORDINATE coo);
        RoomInstance ReserveDeath(COORDINATE coo);
        void UnreserveDeath(int tx, int ty);
        
        int Executions();
        int ExecutionsMax();
        int Executions(RoomInstance ins);
        int ExecutionsMax(RoomInstance ins);
    }
}