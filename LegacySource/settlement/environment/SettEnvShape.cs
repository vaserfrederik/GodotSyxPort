using System;
using System.Collections.Generic;
using snake2d;
using util;

namespace settlement.environment
{
    public sealed class SettEnvShape : SettEnvMap.Updatable
    {
        public readonly LIST<Type> all;
        public readonly Type round;
        public readonly Type square;
        public readonly double radius = 10;

        public static CharSequence ¤¤name = "Shape";
        private static CharSequence ¤¤square = "Squareness";
        private static CharSequence ¤¤round = "Roundness";

        static SettEnvShape()
        {
            D.ts(typeof(SettEnvShape));
        }

        public SettEnvShape(LISTE<Updatable> all) : base(all)
        {
            round = new Type("ROUND", ¤¤round, UI.icons().l.dia)
            {
                protected override bool isBase(int tx, int ty, DIR dd)
                {
                    if (dd.isOrtho())
                        return false;
                    DIR d = dd.next(2);
                    DIR t = getWallDIR(tx + d.x(), ty + d.y());
                    if (t != null && !t.isOrtho())
                        return true;
                    d = dd.next(-2);
                    t = getWallDIR(tx + d.x(), ty + d.y());
                    if (t != null && !t.isOrtho())
                        return true;
                    return false;
                }
            };

            square = new Type("SQUARE", ¤¤square, UI.icons().l.square)
            {
                private readonly DIR[] dir = new DIR[] { DIR.N, DIR.E };

                protected override bool isBase(int tx, int ty, DIR dd)
                {
                    for (int i = 0; i < dir.Length; i++)
                    {
                        if (test(tx, ty, dir[i], dd) && test(tx, ty, dir[i].perpendicular(), dd))
                            return true;
                    }

                    return false;
                }

                private bool test(int tx, int ty, DIR d, DIR dd)
                {
                    for (int i = 1; i < 3; i++)
                    {
                        if (getWallDIR(tx + d.x() * i, ty + d.y() * i) != dd)
                            return false;
                    }
                    return true;
                }
            };

            this.all = new ArrayList<Type>(round, square);
        }

        protected override void update(RECTANGLE bounds, RECTANGLE area)
        {
            foreach (COORDINATE c in area)
            {
                foreach (Type t in all)
                    t.set(c, false);
            }

            foreach (COORDINATE c in bounds)
                GUTIL.flooder().setValue2(c, -1);

            for (int ti = 0; ti < all.size(); ti++)
            {
                GUTIL.flooder().init(this);

                foreach (COORDINATE c in bounds)
                {
                    DIR d = getWallDIR(c.x(), c.y());
                    if (d != null && all.get(ti).isBase(c.x(), c.y(), d) && GUTIL.flooder().getValue2(c.x(), c.y()) == -1)
                        GUTIL.flooder().pushSloppy(c, 0);
                }

                while (GUTIL.flooder().hasMore())
                {
                    PathTile t = GUTIL.flooder().pollSmallest();
                    if (!bounds.holdsPoint(t))
                        continue;
                    if (t.getValue() > 10)
                        continue;
                    if (wall.is(t))
                        continue;
                    if (t.getValue2() != -1)
                        continue;
                    t.setValue2(ti);
                    if (area.holdsPoint(t))
                        all.get(ti).set(t, true);

                    foreach (DIR d in DIR.ALL)
                    {
                        int dx = t.x() + d.x();
                        int dy = t.y() + d.y();
                        if (SETT.IN_BOUNDS(dx, dy))
                            GUTIL.flooder().pushSmaller(dx, dy, t.getValue() + d.tileDistance());
                    }
                }

                GUTIL.flooder().done();
            }
        }

        public bool isBase(int tx, int ty)
        {
            DIR d = getWallDIR(tx, ty);
            if (d != null)
                foreach (Type t in all)
                    if (t.isBase(tx, ty, d))
                        return true;
            return false;
        }

        public DIR getWallDIR(int tx, int ty)
        {
            if (wall.is(tx, ty))
                return null;

            if (!SETT.IN_BOUNDS(tx, ty))
                return null;

            DIR res = null;
            for (int i = 0; i < DIR.ORTHO.size(); i++)
            {
                DIR d = DIR.ORTHO.get(i);
                if (isWallMask(tx, ty, d))
                {
                    if (!isWallMask(tx, ty, d.next(2)) && !isWallMask(tx, ty, d.next(-2)))
                        return d;

                    if (isWallMask(tx, ty, d.next(-2)) && isWallMask(tx, ty, d.next(-1)))
                        return d.next(-1);

                    if (isWallMask(tx, ty, d.next(2)) && isWallMask(tx, ty, d.next(1)))
                        return d.next(1);

                    return null;
                }
            }

            return res;
        }

        private bool isWallMask(int tx, int ty, DIR d)
        {
            int dx = tx + d.x();
            int dy = ty + d.y();
            if (!SETT.IN_BOUNDS(dx, dy))
                return false;
            TerrainTile t = SETT.TERRAIN().get(dx, dy);

            if (t.clearing().isStructure() && t.getAvailability(dx, dy) != null && t.getAvailability(dx, dy).player < 0)
                return true;

            return false;
        }

        private MAP_BOOLEAN wall = new MAP_BOOLEAN()
        {
            public override bool is(int tx, int ty)
            {
                TerrainTile t = SETT.TERRAIN().get(tx, ty);

                if (t.clearing().isStructure() && t.getAvailability(tx, ty) != null && t.getAvailability(tx, ty).player < 0)
                    return true;

                return false;
            }

            public override bool is(int tile)
            {
                int tx = tile % SETT.TWIDTH;
                int ty = tile / SETT.TWIDTH;
                return is(tx, ty);
            }
        };

        public abstract class Type : Bitmap2D
        {
            public readonly CharSequence name;
            public readonly SPRITE icon;
            public readonly string key;

            public Type(string key, CharSequence name, SPRITE icon) : base(SETT.TILE_BOUNDS, false)
            {
                this.key = key;
                this.name = name;
                this.icon = icon;
            }

            protected abstract bool isBase(int tx, int ty, DIR dd);
        }

        public override double getBaseValue(int tx, int ty)
        {
            return isBase(tx, ty) ? 1 : 0;
        }

        protected override bool has(int tx, int ty)
        {
            return round.is(tx, ty) || square.is(tx, ty);
        }

        protected override void clear()
        {
            foreach (Type t in all)
                t.clear();
        }
    }
}