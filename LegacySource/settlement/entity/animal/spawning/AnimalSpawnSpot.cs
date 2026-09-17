using System;
using System.IO;
using util;
using settlement.main;
using settlement.entity.animal;
using settlement.tilemap.terrain;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;

namespace settlement.entity.animal.spawning
{
    public sealed class AnimalSpawnSpot : COORDINATE, INDEXED
    {
        public const int MAX = 64;
        private readonly int index;
        private int cx, cy, am, max;
        private int si;
        private bool blocked;
        private double timer;

        public AnimalSpawnSpot(int index)
        {
            this.index = index;
            Clear();
        }

        public int x() => cx;

        public int y() => cy;

        public int max() => max;

        public int current() => am;

        public AnimalSpecies species() => SETT.ANIMALS().species.get(si);

        public bool active() => cx > 0 && cy > 0 && !blocked;

        public bool blocked() => blocked;

        public int index() => index;

        public void deregisterAnimal()
        {
            am--;
            am = CLAMP.i(am, 0, max);
        }

        void init(int cx, int cy, int max, AnimalSpecies s)
        {
            this.cy = cy;
            this.cx = cx;
            this.max = max;
            this.am = max;
            this.si = s.index();
        }

        void save(FilePutter f)
        {
            f.i(cx);
            f.i(cy);
            f.i(am);
            f.i(max);
            SETT.ANIMALS().map.saver().save(species(), f);
            f.d(timer);
            f.bool(blocked);
        }

        void load(FileGetter f)
        {
            this.cx = f.i();
            this.cy = f.i();
            this.am = f.i();
            this.max = f.i();
            si = SETT.ANIMALS().map.loader().loadB(f, null).index();
            this.timer = f.d();
            blocked = f.bool();
        }

        void Clear()
        {
            cx = -1;
            cy = -1;
            am = 0;
            max = 0;
            si = 0;
            timer = 0;
        }

        public override string ToString() => species().name + " " + max + " " + active() + " " + blocked();

        void update(double rate)
        {
            blocked = false;
            if (!active())
                return;
            blocked = checkBlocked();

            if (blocked)
                return;

            int m = GUTIL.coos().getI();
            GUTIL.coos().shuffle(m);

            timer += rate * max;
            for (int i = 0; i < m && timer >= 1 && am < max; i++)
            {
                GUTIL.coos().set(i);
                int ax = GUTIL.coos().get().x() * C.TILE_SIZE + C.TILE_SIZEH;
                int ay = GUTIL.coos().get().y() * C.TILE_SIZE + C.TILE_SIZEH;
                if (SETT.ANIMALS().isPlacable(species(), ax, ay))
                {
                    Animal a = new Animal(ax, ay, species(), this);
                    if (!a.isRemoved())
                    {
                        am++;
                        timer -= 1;
                    }
                }
            }

            timer -= (int)timer;
        }

        private bool checkBlocked()
        {
            blocked = true;
            int x1 = x() - 4;
            int y1 = y() - 4;
            int x2 = x1 + 8;
            int y2 = y1 + 8;

            GUTIL.coos().set(0);

            bool any = false;

            for (int y = y1; y < y2; y++)
            {
                for (int x = x1; x < x2; x++)
                {
                    if (!IN_BOUNDS(x, y))
                        continue;

                    if (SETT.ROOMS().map.is(x, y))
                        return true;
                    if (SETT.FLOOR().getter.is(x, y))
                        return true;
                    if (SETT.TERRAIN().get(x, y) is BuildingComponent)
                        return true;
                    any |= !PATH().solidity.is(x, y);
                    GUTIL.coos().get().set(x, y);
                    GUTIL.coos().inc();
                }
            }
            return !any;
        }
    }
}