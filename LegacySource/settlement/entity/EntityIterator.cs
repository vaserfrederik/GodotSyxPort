using System;

namespace Settlement.Entity
{
    public abstract class EntityIterator
    {
        /**
         * 
         * @param e
         * @return if to break
         */
        protected abstract bool ProcessAndShouldBreak(ENTITY e, int ie);

        public final void Iterate()
        {
            ENTITY[] es = SETT.ENTITIES().GetAllEnts();
            int m = SETT.ENTITIES().Imax();
            for (int i = 0; i <= m; i++)
            {
                if (es[i] != null)
                    if (ProcessAndShouldBreak(es[i], i))
                        return;
            }
        }

        public final void Iterate(int off)
        {
            ENTITY[] es = SETT.ENTITIES().GetAllEnts();
            int m = SETT.ENTITIES().Imax() + 1;
            for (int i = 0; i < m; i++)
            {
                int k = MATH.Mod(i + off, m);
                if (es[k] != null)
                    if (ProcessAndShouldBreak(es[k], k))
                        return;
            }
        }

        public static abstract class Humans : EntityIterator
        {
            @Override
            public final bool ProcessAndShouldBreak(ENTITY e, int ie)
            {
                if (e is Humanoid)
                {
                    return ProcessAndShouldBreakH((Humanoid)e, ie);
                }
                return false;
            }

            protected abstract bool ProcessAndShouldBreakH(Humanoid h, int ie);
        }
    }
}