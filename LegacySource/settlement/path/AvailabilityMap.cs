using System;
using System.Collections.Generic;
using System.Linq;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using settlement.main;
using settlement.path.components;

namespace settlement.path
{
    public sealed class AvailabilityMap : MAP_OBJECTE<AVAILABILITY>
    {
        private readonly byte[] costs = Alloc.bb(TAREA);
        public int state { get; private set; } = 0;
        private readonly SCOMPONENTS comps;

        public AvailabilityMap(SCOMPONENTS comps)
        {
            this.comps = comps;
            for (int i = SETT.TWIDTH * SETT.TWIDTH - 1; i >= 0; i--)
            {
                costs[i] = (byte)AVAILABILITY.NORMAL.ordinal();
            }
        }

        public override AVAILABILITY get(int tile)
        {
            return AVAILABILITY.values[costs[tile]];
        }

        public override AVAILABILITY get(int tx, int ty)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return null;
            return get(tx + ty * SETT.TWIDTH);
        }

        public override void set(int tile, AVAILABILITY c)
        {
            AVAILABILITY old = get(tile);
            costs[tile] = (byte)c.ordinal();
            if (old.player == c.player)
            {
                int x = tile % SETT.TWIDTH;
                int y = tile / SETT.THEIGHT;
                if (SETT.MAINTENANCE().reservable.is(tile))
                {
                    comps.updateAvailability(x, y);
                }
                return;
            }

            state++;
            int x = tile % SETT.TWIDTH;
            int y = tile / SETT.THEIGHT;
            comps.updateAvailability(x, y);
            bool change = old.player * c.player < 0;
            AvailabilityListener.notify(x, y, c, old, change);
        }

        public override void set(int tx, int ty, AVAILABILITY object)
        {
            if (SETT.IN_BOUNDS(tx, ty))
                set(tx + ty * SETT.TWIDTH, object);
        }

        void init()
        {
            foreach (COORDINATE c in SETT.TILE_BOUNDS)
            {
                costs[c.x() + c.y() * SETT.TWIDTH] = (byte)pget(c.x(), c.y()).ordinal();
            }
        }

        private AVAILABILITY pget(int tx, int ty)
        {
            AVAILABILITY a;

            a = TERRAIN().get(tx, ty).getAvailability(tx, ty);
            if (a != null)
            {
                return a;
            }
            a = ROOMS().getAvailability(tx, ty);
            if (a != null)
            {
                return a;
            }
            a = FLOOR().getAvailability(tx, ty);
            if (a != null)
            {
                return a;
            }
            return AVAILABILITY.NORMAL;
        }

        public void updateAvailability(int tx, int ty)
        {
            set(tx, ty, pget(tx, ty));
        }

        public int state()
        {
            return state;
        }

        public void updateService(int x, int y)
        {
            comps.updateService(x, y);
        }
    }
}