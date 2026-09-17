using settlement.entity.humanoid;
using settlement.main;

namespace settlement.room.home
{
    public interface HOME
    {
        /**
         * Only called by the stat
         * @param h
         */
        HOME vacate(Humanoid h);

        /**
         * Only called by the stat
         * @param h
         */
        HOME occupy(Humanoid h);

        Humanoid occupant(int oi);

        int occupants();
        int occupantsMax();
        int serviceX();
        int serviceY();
        int resourceAm(int ri);
        double isolation();
        bool canOccupy(Humanoid h);

        CharSequence typeName(int tx, int ty);

        static HOME get(int tx, int ty)
        {
            HOME h = SETT.ROOMS().HOME.getter.get(tx, ty);
            if (h != null)
                return h;
            return SETT.ROOMS().CHAMBER.get(tx, ty);
        }

        bool is(int tx, int ty);

        int area();
    }
}