using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Path.Finders
{
    public sealed class SFinderEntity
    {
        public SFinderEntity()
        {
            IDebugPanelSett.Add("find safety", new Action(() =>
            {
                long n = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                int sx = RND.rInt(SETT.TWIDTH);
                int sy = RND.rInt(SETT.THEIGHT);
                int max = 100;
                enemies = s().People(true);
                SComponent s = PATH().Comps.SuperComp.Get(sx, sy);
                if (s != null)
                {
                    SCompPath p = PATH().Comps.Pather.Find(sx, sy, findSafety, max, 16);
                    if (p != null)
                    {
                        SPathUtilResult r = SETT.PATH().Finders.Finder().Find(sx, sy, findSafety, max, p);
                        if (r != null)
                            LOG.Ln("yay " + sx + " " + sy + " " + r.DestX + " " + r.DestY);
                    }
                }
                LOG.Ln("" + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - n));
            }));
        }

        private FindableDataSingle enemies;

        public void Report(ENTITY e, int delta)
        {
            if (e is Humanoid)
            {
                FindableDataSingle s = s().People(!((Humanoid)e).Indu().Hostile());
                if (delta > 0)
                    s.ReportPresence(e.Ssx(), e.Ssy());
                else
                    s.ReportAbsence(e.Ssx(), e.Ssy());
            }
            else if (e is Animal)
            {
                if (((Animal)e).HuntReservable())
                {
                    if (delta == 1)
                        s().ReservableAnimals.ReportPresence(e.Ssx(), e.Ssy());
                    else
                        s().ReservableAnimals.ReportAbsence(e.Ssx(), e.Ssy());
                }
            }
        }

        private FindableDatas s()
        {
            return PATH().Comps.Data;
        }

        public int GetEnemies(Humanoid asker, int tx, int ty)
        {
            SComponent c = SETT.PATH().Comps.Zero.Get(tx, ty);
            if (c == null)
                return 0;
            return s().People(asker.Indu().Hostile()).Get(c);
        }

        public int GetFriendlies(Humanoid asker, int tx, int ty)
        {
            SComponent c = SETT.PATH().Comps.Zero.Get(tx, ty);
            if (c == null)
                return 0;
            return s().People(!asker.Indu().Hostile()).Get(c);
        }

        public int GetAny(int tx, int ty)
        {
            SComponent c = SETT.PATH().Comps.Zero.Get(tx, ty);
            if (c == null)
                return 0;
            return s().People(true).Get(c) + s().People(false).Get(c);
        }

        public bool FindExitNoEnemies(Humanoid asker, int sx, int sy, SPath path, int max)
        {
            enemies = s().People(asker.Indu().Hostile());
            SComponent s = PATH().Comps.SuperComp.Get(sx, sy);
            if (s != null && s.HasEdge())
            {
                SCompPath p = PATH().Comps.Pather.Find(sx, sy, rout, max, 16);
                if (p != null)
                {
                    SPathUtilResult r = SETT.PATH().Finders.Finder().Find(sx, sy, rout, max, p);
                    if (r != null)
                    {
                        path.SetDirect(sx, sy, r.DestX, r.DestY, r.T, false);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool FindSafety(Humanoid asker, int sx, int sy, SPath path, int max)
        {
            enemies = s().People(asker.Indu().Hostile());
            SComponent s = PATH().Comps.SuperComp.Get(sx, sy);
            if (s != null)
            {
                SCompPath p = PATH().Comps.Pather.Find(sx, sy, findSafety, max, 16);
                if (p != null)
                {
                    SPathUtilResult r = SETT.PATH().Finders.Finder().Find(sx, sy, findSafety, max, p);
                    if (r != null)
                    {
                        path.SetDirect(sx, sy, r.DestX, r.DestY, r.T, false);
                        return true;
                    }
                }
            }
            return false;
        }

        private readonly SFINDER findSafety = new SFINDER
        {
            private int tx, ty;

            public override bool IsInComponent(SComponent c, double distance)
            {
                if (enemies.Get(c) == 0)
                {
                    COORDINATE coo = c.RndCoo();
                    tx = coo.X();
                    ty = coo.Y();
                    return true;
                }
                return false;
            }

            public override bool IsTile(int tx, int ty, int tileNr)
            {
                return this.tx == tx && this.ty == ty;
            }

            public override bool CanCross(SComponent c)
            {
                return enemies.Get(c) == 0;
            }
        };

        private readonly SFINDER rout = new SFINDER
        {
            public override bool IsInComponent(SComponent c, double distance)
            {
                return c.HasEdge();
            }

            public override bool IsTile(int tx, int ty, int tileNr)
            {
                if (tx == 0 || tx == SETT.TWIDTH - 1 || ty == 0 || ty == SETT.THEIGHT - 1)
                    return !SETT.PATH().Solidity.Is(tx, ty);
                return false;
            }

            public override bool CanCross(SComponent c)
            {
                return enemies.Get(c) == 0;
            }
        };
    }
}