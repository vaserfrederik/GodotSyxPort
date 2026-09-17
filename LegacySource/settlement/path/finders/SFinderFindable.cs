using System;
using System.Collections.Generic;

namespace Settlement.Path.Finders
{
    public abstract class SFinderFindable : SFINDER
    {
        public static List<SFinderFindable> all = new List<SFinderFindable>(256);
        static SFinderFindable()
        {
            new GameDisposable
            {
                Dispose = () =>
                {
                    all.Clear();
                }
            };
        }

        public readonly short index = (short)(all.Count);
        protected FINDABLE result;
        private double distance;
        public readonly string name;
        public readonly SFinderFindableMap map = new SFinderFindableMap();

        public SFinderFindable(string name)
        {
            this.name = name;
            all.Add(this);
        }

        public double GetDistance()
        {
            return distance;
        }

        public static List<SFinderFindable> All()
        {
            return all;
        }

        public static SFinderFindable Get(short index)
        {
            return all[index & 0x0FF];
        }

        public override bool IsInComponent(SComponent c, double distance)
        {
            if (Fin().Get(c) > 0)
            {
                this.distance = distance;
                return true;
            }
            return false;
        }

        public override bool IsTile(int tx, int ty, int tileNr)
        {
            result = GetReservable(tx, ty);
            if (result != null)
            {
                return true;
            }
            return false;
        }

        public bool Has(int sx, int sy)
        {
            return Fin().Has(sx, sy);
        }

        public bool Has(COORDINATE c)
        {
            return Fin().Has(c.x(), c.y());
        }

        private FindableDataSingle Fin()
        {
            return SETT.PATH().comps.data.SINGLES.Get(index);
        }

        public abstract FINDABLE GetReservable(int tx, int ty);
        public abstract FINDABLE GetReserved(int tx, int ty);

        public bool Reserve(COORDINATE start, SPath path, int maxdistance)
        {
            if (path.Request(start.x(), start.y(), this, maxdistance))
            {
                result.FindableReserve();
                map.Report(start, true);
                return true;
            }
            map.Report(start, false);
            return false;
        }

        public COORDINATE Reserve(COORDINATE start, int maxdistance)
        {
            if (SETT.PATH().finders.finder().FindDest(start.x(), start.y(), this, maxdistance) != null)
            {
                result.FindableReserve();
                map.Report(start, true);
                return result;
            }
            if (this == SETT.ROOMS().BENCH.finder())
                GAME.Notify("here");
            map.Report(start, false);
            return null;
        }

        public COORDINATE Reserve(int sx, int sy, int maxdistance)
        {
            if (Has(sx, sy) && SETT.PATH().finders.finder().FindDest(sx, sy, this, maxdistance) != null)
            {
                result.FindableReserve();
                map.Report(sx, sy, true);
                return result;
            }
            map.Report(sx, sy, false);
            return null;
        }

        public void Report(FINDABLE coo, int delta)
        {
            Report(coo.x(), coo.y(), delta);
        }

        public void Report(int x, int y, int delta)
        {
            if (delta == 1)
                Fin().ReportPresence(x, y);
            else if (delta == -1)
                Fin().ReportAbsence(x, y);
            else
                throw new InvalidOperationException(delta.ToString());
        }

        public abstract class FinderThing<T> : SFinderFindable where T : ThingFindable
        {
            FinderThing(string name) : base(name) { }

            public abstract T GetReservable(int tx, int ty);
            public abstract T GetReserved(int tx, int ty);

            public T GetResult()
            {
                return (T)result;
            }
        }
    }
}