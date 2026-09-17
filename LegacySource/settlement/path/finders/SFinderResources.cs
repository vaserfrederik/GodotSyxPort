using System;
using System.Collections.Generic;
using init.resources;
using settlement.main;
using settlement.path.components;
using settlement.path.path;
using settlement.room.main;
using settlement.thing;
using snake2d;
using util.gui.misc;
using view.sett;
using view.tool;

public static class SFinderResources
{
    public static readonly Normal normal = new Normal();
    public static readonly Scattered scattered = new Scattered();

    private static readonly RBITImp bscattered = new RBITImp();
    private static readonly RBITImp bstored = new RBITImp();
    private static readonly RBITImp bprio = new RBITImp();

    static SFinderResources()
    {
        IDebugPanelSett.Add("Unreserve everything", new ACTION
        {
            exe = () =>
            {
                foreach (COORDINATE c in new Rec(SETT.TILE_BOUNDS))
                {
                    while (unres(c))
                        ;

                    Room room = ROOMS().Map[c.x, c.y];
                    if (room != null)
                    {
                        RESOURCE_TILE res = room.ResourceTile(c.x, c.y);
                        while (res != null && res.FindableReservedIs() && res.Resource != null)
                        {
                            res.FindableReserveCancel();
                        }
                    }
                }
            },

            private bool unres(COORDINATE c)
            {
                foreach (Thing t in THINGS().Get(c.x, c.y))
                {
                    if (t is ScatteredResource)
                    {
                        ScatteredResource sc = (ScatteredResource)t;
                        if (sc.FindableReservedIs() && sc.Resource != null)
                        {
                            sc.FindableReserveCancel();
                            return true;
                        }
                    }
                }
                return false;
            }
        });

        IDebugPanelSett.Add(new PlacableSimpleTile("find resource")
        {
            RBITImp bits = new RBITImp();
            LIST<CLICKABLE> li;
            {
                GuiSection s = new GuiSection();
                foreach (RESOURCE r in RESOURCES.ALL())
                {
                    s.AddGrid(new GButt.ButtPanel(r.icon())
                    {
                        clickA = () => bits.Toggle(r),
                        renAction = () => selectedSet(bits.Has(r))
                    }, r.Index(), 10, 0, 0);
                }
                li = new ArrayList<CLICKABLE>(s);
            }

            public override void place(int tx, int ty)
            {
                SPath p = new SPath();
                RESOURCE res = find(bits, new Coo(tx, ty), p, 250);

                if (res == null)
                {
                    LOG.ln("nope");
                }
                else
                {
                    LOG.ln(p.DestX + " " + p.DestY);
                    RESOURCE_TILE.GETTER.Reserved(res, p.DestX, p.DestY);
                }
            }

            public override CharSequence isPlacable(int tx, int ty)
            {
                return null;
            }

            public override LIST<CLICKABLE> getAdditionalButt()
            {
                return li;
            }
        });
    }

    public static bool Has(int sx, int sy, RBIT bits)
    {
        return Has(sx, sy, bits, bits, bits);
    }

    public static bool Has(int sx, int sy, RBIT scattered, RBIT stored, RBIT prio)
    {
        return PATH().Comps.Data.ResScattered.Has(sx, sy, scattered) ||
               PATH().Comps.Data.ResCrate.Has(sx, sy, stored) || PATH().Comps.Data.ResPriority.Has(sx, sy, prio);
    }

    public static RESOURCE Find(RBIT scattered, RBIT stored, RBIT prio, COORDINATE start, SPath path, int maxdistance)
    {
        return Find(scattered, stored, prio, start.X, start.Y, path, maxdistance);
    }

    public static RESOURCE Find(RBIT bits, COORDINATE start, SPath path, int maxdistance)
    {
        return Find(bits, start.X, start.Y, path, maxdistance);
    }

    public static RESOURCE Find(RBIT bits, int sx, int sy, SPath path, int maxdistance)
    {
        return Find(bits, bits, bits, sx, sy, path, maxdistance);
    }

    public static RESOURCE Find(RBIT scattered, RBIT stored, RBIT prio, int sx, int sy, SPath path, int maxdistance)
    {
        if (Has(sx, sy, scattered, stored, prio))
        {
            bscattered.ClearSet(scattered);
            bstored.ClearSet(stored);
            bprio.ClearSet(prio);

            if (path.Request(sx, sy, finder, maxdistance))
            {
                RESOURCE_TILE t = RESOURCE_TILE.GETTER.Reservable(bscattered, bstored, bprio, path.DestX, path.DestY);
                t.FindableReserve();
                return t.Resource;
            }
        }

        return null;
    }

    public static RESOURCE_TILE Find(RBIT scattered, RBIT stored, RBIT prio, RoomInstance ins, int maxdistance)
    {
        if (Has(ins.MX, ins.MY, scattered, stored, prio))
        {
            bscattered.ClearSet(scattered);
            bstored.ClearSet(stored);
            bprio.ClearSet(prio);

            COORDINATE r = SETT.PATH().Finders.Finder().FindDest(ins, finder, maxdistance);
            if (r != null)
                return RESOURCE_TILE.GETTER.Reservable(bscattered, bstored, bprio, r.X, r.Y);
        }

        return null;
    }

    private static readonly SFINDER finder = new SFINDER
    {
        isInComponent = (SComponent c, double distance) =>
            PATH().Comps.Data.ResScattered.Has(c, bscattered) ||
            PATH().Comps.Data.ResCrate.Has(c, bstored) || PATH().Comps.Data.ResPriority.Has(c, bprio),

        isTile = (int tx, int ty, int tileNr) =>
            RESOURCE_TILE.GETTER.Reservable(bscattered, bstored, bprio, tx, ty) != null
    };

    public static int ReserveExtra(bool stored, bool fetch, RESOURCE r, int tx, int ty, int amount)
    {
        return RESOURCE_TILE.GETTER.Reserve(stored, fetch, r, tx, ty, amount);
    }

    public static bool IsReservedAndAvailable(RESOURCE r, int x, int y)
    {
        return RESOURCE_TILE.GETTER.Reserved(r, x, y) != null;
    }

    public static int Pickup(RESOURCE r, int tx, int ty, int amount)
    {
        return RESOURCE_TILE.GETTER.Pickup(r, tx, ty, amount);
    }

    public static void Unreserve(RESOURCE r, int tx, int ty, int amount)
    {
        RESOURCE_TILE.GETTER.Unreserve(r, tx, ty, amount);
    }

    public static void Report(RESOURCE r, int x, int y)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, COORDINATE c3)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, COORDINATE c2)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, COORDINATE c3, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, COORDINATE c3, COORDINATE c4)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, COORDINATE c3, COORDINATE c4, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, RESOURCE r, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, int amount)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11, COORDINATE c12)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11)
    {
        // Implementation for report
    }

    public static void Report(COORDINATE c, List<RESOURCE> resources, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11, COORDINATE c12, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<COORDINATE> coordinates, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11, int amount)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, List<RESOURCE> resources, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11, int amount)
    {
        // Implementation for report
    }

    public static void Report(ROOM room, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11, COORDINATE c12)
    {
        // Implementation for report
    }

    public static void Report(List<ROOM> rooms, RESOURCE r, COORDINATE c, COORDINATE c2, COORDINATE c3, COORDINATE c4, COORDINATE c5, COORDINATE c6, COORDINATE c7, COORDINATE c8, COORDINATE c9, COORDINATE c10, COORDINATE c11, COORDINATE c12)
    {
        // Implementation for report
    }

    // Additional methods can be added here for different numbers of coordinates and resources
}