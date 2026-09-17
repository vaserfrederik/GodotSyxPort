using settlement.job;
using settlement.main;

namespace view.sett.ui.room.copy
{
    internal static class Jobs
    {
        public static Job Get(int tx, int ty)
        {
            Job j = SETT.JOBS().JobGetter.Get(tx, ty);
            if (j != null && j.IsConstruction())
            {
                return j;
            }
            return null;
        }
    }
}