using snake2d.util.datatypes;
using snake2d.util.sets;

namespace settlement.room.main
{
    public interface ROOMA : AREA, INDEXED
    {
        public abstract int mX();
        public abstract int mY();
    }
}