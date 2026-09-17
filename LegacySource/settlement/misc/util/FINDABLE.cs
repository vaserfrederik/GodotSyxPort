using snake2d.util.datatypes;

namespace settlement.misc.util
{
    public interface FINDABLE : COORDINATE
    {
        bool FindableReservedCanBe();
        void FindableReserve();
        bool FindableReservedIs();
        void FindableReserveCancel();
    }
}