using System;
using System.Collections.Generic;

namespace Settlement.Path.Finders
{
    public sealed class SFinderPrey
    {
        private Animal a;

        public SFinderPrey()
        {
            new TestPath("Animal", Fin);
        }

        private readonly SFINDER Fin = new SFINDER()
        {
            IsInComponent = (c, distance) =>
            {
                return PATH().Comps.Data.ReservableAnimals[c] > 0;
            },

            IsTile = (tx, ty, tileNr) =>
            {
                foreach (ENTITY e in SETT.ENTITIES().GetAtTile(tx, ty))
                {
                    if (e is Animal)
                    {
                        a = (Animal)e;
                        if (a.HuntReservable())
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        };

        public bool Has(COORDINATE start)
        {
            SComponent c = PATH().Comps.SuperComp.Get(start);
            if (c == null)
                return false;

            if (PATH().Comps.Data.ReservableAnimals[c] <= 0)
                return false;
            return true;
        }

        public Animal FindAndReserve(COORDINATE start, SPath p, int radius)
        {
            if (!Has(start))
                return null;

            if (p.Request(start.X, start.Y, Fin, radius))
            {
                a.HuntReserve();
                return a;
            }

            return null;
        }
    }
}