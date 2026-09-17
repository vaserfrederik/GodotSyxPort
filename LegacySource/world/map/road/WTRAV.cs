using System;
using game;
using snake2d;
using snake2d.util.datatypes;
using world;

namespace world.map.road
{
    public class WTRAV
    {
        public const int PORT_PENALTY = 10;

        public static bool Can(int fromX, int fromY, DIR d, bool roaded)
        {
            if (!d.IsOrtho() && WORLD.REGIONS().map.Get(fromX, fromY) != WORLD.REGIONS().map.Get(fromX + d.x(), fromY + d.y()))
                return false;

            TravTile from = Get(fromX, fromY, roaded);
            int toX = fromX + d.x();
            int toY = fromY + d.y();
            TravTile to = Get(toX, toY, roaded);
            return from.IsPossible(fromX, fromY, toX, toY, to, d, roaded) && to.IsPossible(toX, toY, fromX, fromY, from, d.Perpendicular(), roaded);
        }

        public static bool CanLand(int fromX, int fromY, DIR d, bool roaded)
        {
            if (!Can(fromX, fromY, d, roaded))
                return false;
            int toX = fromX + d.x();
            int toY = fromY + d.y();

            if (WORLD.WATER().IsBig.Is(fromX, fromY) && WORLD.WATER().IsBig.Is(toX, toY))
                return false;
            return true;
        }

        public static int Cost(int fromX, int fromY, DIR d)
        {
            if (WORLD.WATER().IsBig.Is(fromX, fromY))
            {
                return 1;
            }
            int toX = fromX + d.x();
            int toY = fromY + d.y();
            if (WORLD.WATER().IsBig.Is(toX, toY))
                return PORT_PENALTY * 2;

            if (WORLD.MOUNTAIN().CoversTile(fromX, fromY))
                return 12;
            if (WORLD.FOREST().Amount.Get(fromX, fromY) == 1.0)
                return 4;
            return 3;
        }

        public static bool IsHarbour(int tx, int ty)
        {
            return Get(tx, ty, false) == HARBOUR;
        }

        public static bool IsGoodLandTile(int tx, int ty)
        {
            return LAND.IsPossible(tx, ty, false);
        }

        public abstract class TravTile
        {
            public readonly string Name;

            public TravTile(string name)
            {
                this.Name = name;
            }

            public abstract bool IsPossible(int tx, int ty, bool roaded);
            public abstract bool IsPossible(int fromX, int fromY, int tx, int ty, TravTile to, DIR d, bool roaded);
            public abstract void Road(int fromX, int fromY, int tx, int ty, TravTile to, DIR d);
            public virtual int ExtraCost(TravTile to)
            {
                return 0;
            }
        }

        private static void MakeRoad(PathTile dest)
        {
            PathTile t = dest;
            PathTile from = null;

            while (t != null)
            {
                if (WORLD.ROADS().Placable.Is(t))
                {
                    if (WORLD.WATER().IsBig.Is(t))
                    {
                        if ((from != null && !WORLD.WATER().IsBig.Is(from)) || (t.Parent != null && !WORLD.WATER().IsBig.Is(t.Parent)))
                        {
                            bool bridge = false;
                            if (!WORLD.ROADS().Is(t) && WORLD.ROADS().CanBridge.Is(t) && from != null && t.Parent != null)
                            {
                                if (!WORLD.WATER().IsBig.Is(from) && !WORLD.WATER().IsBig.Is(t.Parent))
                                    bridge = true;
                            }

                            WORLD.ROADS().Set(t, true);
                            if (bridge)
                            {
                                WORLD.ROADS().Bridge.Set(t, true);
                            }
                        }
                    }
                    else
                    {
                        WORLD.ROADS().Set(t, true);
                        if (from != null)
                        {
                            DIR d = DIR.Get(from, t);

                            if (!d.IsOrtho())
                            {
                                DIR d1 = d.Next(-1);
                                DIR d2 = d.Next(1);
                                if (LAND.IsPossible(from.x() + d1.x(), from.y() + d1.y(), false) && WORLD.ROADS().Is(from, d1))
                                {
                                }
                                else if (LAND.IsPossible(from.x() + d2.x(), from.y() + d2.y(), false) && WORLD.ROADS().Is(from, d2))
                                {
                                }
                                else if (LAND.IsPossible(from.x() + d1.x(), from.y() + d1.y(), false) && WORLD.REGIONS().map.Get(from.x() + d1.x(), from.y() + d1.y()) == WORLD.REGIONS().map.Get(from.x(), from.y()))
                                {
                                    WORLD.ROADS().Set(from.x() + d1.x(), from.y() + d1.y(), true);
                                }
                                else if (LAND.IsPossible(from.x() + d2.x(), from.y() + d2.y(), false))
                                {
                                    WORLD.ROADS().Set(from.x() + d2.x(), from.y() + d2.y(), true);
                                }
                            }
                        }
                    }
                }

                from = t;
                t = t.Parent;
            }
        }

        private static void CheckRoads(PathTile dest)
        {
            PathTile t = dest;
            PathTile from = null;

            while (t != null)
            {
                if (WORLD.ROADS().Placable.Is(t))
                {
                    if (WORLD.WATER().IsBig.Is(t))
                    {
                        if ((from != null && !WORLD.WATER().IsBig.Is(from)) || (t.Parent != null && !WORLD.WATER().IsBig.Is(t.Parent)))
                        {
                            bool bridge = false;
                            if (!WORLD.ROADS().Is(t) && WORLD.ROADS().CanBridge.Is(t) && from != null && t.Parent != null)
                            {
                                if (!WORLD.WATER().IsBig.Is(from) && !WORLD.WATER().IsBig.Is(t.Parent))
                                    bridge = true;
                            }

                            WORLD.ROADS().Set(t, true);
                            if (bridge)
                            {
                                WORLD.ROADS().Bridge.Set(t, true);
                            }
                        }
                    }
                    else
                    {
                        WORLD.ROADS().Set(t, true);
                        if (from != null)
                        {
                            DIR d = DIR.Get(from, t);

                            if (!d.IsOrtho())
                            {
                                DIR d1 = d.Next(-1);
                                DIR d2 = d.Next(1);
                                if (LAND.IsPossible(from.x() + d1.x(), from.y() + d1.y(), false) && WORLD.ROADS().Is(from, d1))
                                {
                                }
                                else if (LAND.IsPossible(from.x() + d2.x(), from.y() + d2.y(), false) && WORLD.ROADS().Is(from, d2))
                                {
                                }
                                else if (LAND.IsPossible(from.x() + d1.x(), from.y() + d1.y(), false) && WORLD.REGIONS().map.Get(from.x() + d1.x(), from.y() + d1.y()) == WORLD.REGIONS().map.Get(from.x(), from.y()))
                                {
                                    WORLD.ROADS().Set(from.x() + d1.x(), from.y() + d1.y(), true);
                                }
                                else if (LAND.IsPossible(from.x() + d2.x(), from.y() + d2.y(), false))
                                {
                                    WORLD.ROADS().Set(from.x() + d2.x(), from.y() + d2.y(), true);
                                }
                            }
                        }
                    }
                }

                from = t;
                t = t.Parent;
            }
        }

        private static TravTile Get(int tx, int ty, bool roaded)
        {
            if (WORLD.WATER().IsBig.Is(tx, ty))
                return WATER;
            else if (WORLD.MOUNTAIN().CoversTile(tx, ty))
                return NOTHING;
            else
                return LAND;
        }

        private static readonly TravTile LAND = new TravTile("Land")
        {
            IsPossible = (fromX, fromY, tx, ty, to, d, roaded) =>
            {
                if (!IsPossible(tx, ty, roaded))
                    return false;
                if (to == WATER)
                {
                    if (!d.IsOrtho())
                        return false;
                    return WORLD.WATER().IsBig.Is(tx, ty);
                }
                else if (to == HARBOUR)
                {
                    if (!d.IsOrtho())
                        return false;
                    return WATER.IsPossible(tx, ty, roaded);
                }
                return true;
            },
            IsPossible = (tx, ty, roaded) =>
            {
                if (WORLD.MOUNTAIN().CoversTile(tx, ty))
                    return false;
                return true;
            },
            Road = (fromX, fromY, tx, ty, to, d) =>
            {
                if (to == WATER)
                {
                    if (!d.IsOrtho())
                    {
                        if (WORLD.WATER().IsBig.Is(fromX, ty) && WORLD.WATER().IsBig.Is(tx, fromY))
                        {
                            WORLD.ROADS().Set(fromX, ty, true);
                            WORLD.ROADS().Set(tx, fromY, true);
                        }
                    }
                }
                else if (to == HARBOUR)
                {
                    if (!d.IsOrtho())
                    {
                        if (WORLD.WATER().IsBig.Is(fromX, ty) && WORLD.WATER().IsBig.Is(tx, fromY))
                        {
                            WORLD.ROADS().Set(fromX, ty, true);
                            WORLD.ROADS().Set(tx, fromY, true);
                        }
                    }
                }
            }
        };

        private static readonly TravTile HARBOUR = new TravTile("Harbour")
        {
            IsPossible = (fromX, fromY, tx, ty, to, d, roaded) =>
            {
                if (!IsPossible(tx, ty, roaded))
                    return false;
                if (to == LAND)
                {
                    if (!d.IsOrtho())
                        return false;
                    return LAND.IsPossible(tx, ty, roaded);
                }
                else if (to == WATER || to == this)
                {
                    return true;
                }
                else if (roaded && WORLD.ROADS().Is(fromX, fromY))
                {
                    return to == LAND;
                }
                return false;
            },
            IsPossible = (tx, ty, roaded) =>
            {
                if (WORLD.WATER().IsBig.Is(tx, ty) && CanBe(tx, ty))
                {
                    if (roaded)
                        return WORLD.ROADS().Is(tx, ty);
                    return true;
                }
                return false;
            },
            CanBe = (tx, ty) =>
            {
                if (WORLD.MOUNTAIN().CoversTile(tx, ty))
                    return false;
                if (WORLD.WATER().IsBig.Is(tx, ty))
                {
                    return Ok(tx, ty, DIR.N) || Ok(tx, ty, DIR.E);
                }
                return false;
            },
            Ok = (tx, ty, d) =>
            {
                return WORLD.WATER().IsBig.Is(tx, ty, d) && WORLD.WATER().IsBig.Is(tx, ty, d.Perpendicular()) && (!WORLD.WATER().IsBig.Is(tx, ty, d.Next(2)) || !WORLD.WATER().IsBig.Is(tx, ty, d.Perpendicular().Next(2)));
            },
            Road = (fromX, fromY, tx, int ty, TravTile to, DIR d) =>
            {
                if (to == LAND)
                    WORLD.ROADS().Set(fromX, fromY, true);
            }
        };

        private static readonly TravTile WATER = new TravTile("Water")
        {
            IsPossible = (fromX, fromY, tx, ty, to, d, roaded) =>
            {
                if (!IsPossible(tx, ty, roaded))
                    return false;
                else if (to == HARBOUR)
                {
                    if (!HARBOUR.IsPossible(tx, ty, roaded))
                        return false;
                    if (!d.IsOrtho())
                    {
                        return WORLD.WATER().IsBig.Is(fromX, ty) && WORLD.WATER().IsBig.Is(tx, fromY);
                    }
                    return true;
                }
                else if (to == WATER)
                {
                    if (!WATER.IsPossible(tx, ty, roaded))
                        return false;
                    if (!d.IsOrtho())
                    {
                        return WORLD.WATER().IsBig.Is(fromX, ty) && WORLD.WATER().IsBig.Is(tx, fromY);
                    }
                    return true;
                }
                return false;
            },
            IsPossible = (tx, ty, roaded) =>
            {
                return WORLD.WATER().IsBig.Is(tx, ty);
            },
            Road = (fromX, fromY, tx, ty, TravTile to, DIR d) =>
            {
                // TODO Auto-generated method stub
            }
        };

        private static readonly TravTile NOTHING = new TravTile("Nothing")
        {
            IsPossible = (fromX, fromY, tx, ty, TravTile to, DIR d, bool roaded) =>
            {
                return false;
            },
            IsPossible = (tx, ty, roaded) =>
            {
                return false;
            },
            Road = (fromX, fromY, tx, ty, TravTile to, DIR d) =>
            {
                // TODO Auto-generated method stub
            }
        };
    }
}