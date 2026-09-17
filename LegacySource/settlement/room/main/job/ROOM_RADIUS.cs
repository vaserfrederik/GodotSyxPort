using settlement.room.main;

namespace Settlement.Room.Main.Job
{
    public interface IRoomRadius
    {
        public interface IRoomRadiusInstance
        {
            public int Radius();
            public bool Searching();
            public byte RadiusRaw();
            public void RadiusRawSet(byte r);
            // public default void ReportUnavailability()
            // {
            // }
        }

        public interface IRoomRadiuse : IRoomRadius
        {
            public IRoomRadiusInstance RadiusInstance(Room t);
        }
    }
}