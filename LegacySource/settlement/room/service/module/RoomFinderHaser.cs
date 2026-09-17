using settlement.path.finders;

namespace settlement.room.service.module
{
    public interface RoomFinderHaser
    {
        SFinderFindable Finder();
        int Radius();
    }
}