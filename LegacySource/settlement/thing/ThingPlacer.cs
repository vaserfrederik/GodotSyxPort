using System;
using System.Collections.Generic;
using settlement.main;
using init.resources;
using settlement.misc.util;
using settlement.room.main;
using settlement.thing;
using settlement.thing.ThingsResources;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sets;
using view.sett;
using view.tool;

class ThingPlacer
{
    static void Init()
    {
        LinkedList<PLACABLE> pl = new LinkedList<PLACABLE>();
        foreach (RESOURCE r in RESOURCES.ALL())
        {
            pl.Add(new PlacableMulti(r.name)
            {
                public void Place(int tx, int ty, AREA a, PLACER_TYPE t)
                {
                    SETT.THINGS().resources.createPrecise(tx, ty, r, 1 + RND.rInt(29));
                }

                public CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
                {
                    return null;
                }
            });
        }

        pl.Add(new PlacableMulti("resources all")
        {
            int ri = 0;

            public void Place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                SETT.THINGS().resources.createPrecise(tx, ty, RESOURCES.ALL().get(ri), 64);
                ri++;
                ri %= RESOURCES.ALL().size();
            }

            public CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
            {
                return null;
            }
        });

        pl.Add(new PlacableMulti("resource increase")
        {
            public void Place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                foreach (Thing t in THINGS().get(tx, ty))
                {
                    if (t is ScatteredResource)
                    {
                        int a = ((ScatteredResource)t).amount();
                        a -= (a / 10) * 10;
                        a = 10 - a;
                        if (((ScatteredResource)t).amount() + a < ThingsResources.MAX_AMOUNT)
                            THINGS().resources.createPrecise(tx, ty, ((ScatteredResource)t).resource(), a);
                        return;
                    }
                }
            }

            public CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
            {
                foreach (Thing t in THINGS().get(tx, ty))
                {
                    if (t is ScatteredResource)
                        return null;
                }
                return "";
            }
        });

        pl.Add(new PlacableMulti("increase crate")
        {
            public void Place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                Room r = SETT.ROOMS().map.get(tx, ty);
                if (r != null)
                {
                    TILE_STORAGE c = SETT.MAPS().STORAGE.get(tx, ty);
                    for (int i = 0; i < 16 && c.resource() != null && c != null && c.storageReservable() > 0; i++)
                    {
                        c.storageReserve(1);
                        c.storageDeposit(1);
                    }
                }
            }

            public CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
            {
                if (SETT.MAPS().STORAGE.get(tx, ty) != null)
                {
                    return null;
                }
                return "";
            }
        });

        pl.Add(new PlacableMulti("remove things")
        {
            public void Place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                LIST<Thing> l = THINGS().get(tx, ty);
                if (!l.isEmpty())
                    l.get(0).remove();
            }

            public CharSequence IsPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
            {
                LIST<Thing> l = THINGS().get(tx, ty);
                if (l.isEmpty())
                    return "";
                return null;
            }
        });

        IDebugPanelSett.Add("resources", pl);
    }
}