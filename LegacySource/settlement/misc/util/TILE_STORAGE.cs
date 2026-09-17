using init.resources;
using snake2d.util.datatypes;

namespace settlement.misc.util
{
    public interface TILE_STORAGE : COORDINATE
    {
        RESOURCE resource();
        void storageDeposit(int amount);
        int storageReservable();
        int storageReserved();
        void storageReserve(int amount);
        void storageUnreserve(int amount);

        bool storageIsFindable() => true;
    }
}