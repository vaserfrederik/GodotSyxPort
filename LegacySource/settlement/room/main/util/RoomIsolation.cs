using System;
using System.Collections.Generic;
using System.Linq;

namespace settlement.room.main.util
{
    public class RoomIsolation
    {
        private const int CHUNKSIZE = 32;
        private const int CHUNCS = (int)Math.Log(CHUNKSIZE, 2);
        private readonly Chunk[,] chunks;
        private readonly List<Chunk> toUpdate = new List<Chunk>();

        private readonly Rec rec = new Rec();
        public readonly INFO info;
        private readonly RoomAreaWrapper wrap = new RoomAreaWrapper();

        public RoomIsolation(ROOMS r)
        {
            D.gInit(this);
            info = new INFO(Dic.¤¤Isolation, D.g("desc", "Insulation prevents room degradation and sound pollution. Surrounding walls increase insulation, while doors and gaps decrease it. Poorly insulated rooms need more maintenance. Poorly insulated homes degrade furniture faster."));

            chunks = new Chunk[(int)Math.Ceiling((double)SETT.THEIGHT / CHUNKSIZE), (int)Math.Ceiling((double)SETT.TWIDTH / CHUNKSIZE)];
            for (int y = 0; y < chunks.GetLength(0); y++)
            {
                for (int x = 0; x < chunks.GetLength(1); x++)
                {
                    chunks[y, x] = new Chunk(x, y);
                }
            }

            new AvailabilityListener
            {
                Changed = (tx, ty, a, old, playerChange) => setChanged(tx, ty, a, old)
            };
        }

        private void setChanged(int tx, int ty, AVAILABILITY a, AVAILABILITY old)
        {
            if (a.player < 0 != old.player < 0)
            {
                foreach (var d in DIR.ALL)
                {
                    Room r = SETT.ROOMS().map.get(tx, ty, d);
                    setChanged(r, tx + d.x(), ty + d.y());
                }
            }
        }

        private void setChanged(Room r, int x, int y)
        {
            if (r != null && r.constructor() != null && r.constructor().needsIsolation())
            {
                int mx = r.mX(x, y) >> CHUNCS;
                int my = r.mY(x, y) >> CHUNCS;
                Chunk c = chunks[my, mx];
                if (!c.added)
                {
                    toUpdate.Add(c);
                    c.added = true;
                }
            }
        }

        public void update()
        {
            while (toUpdate.Any())
            {
                Chunk ch = toUpdate.Last();
                toUpdate.RemoveAt(toUpdate.Count - 1);

                foreach (var c in ch.body)
                {
                    Room r = SETT.ROOMS().map.get(c);
                    if (r != null && r.mX(c.x(), c.y()) == c.x() && r.mY(c.x(), c.y()) == c.y())
                    {
                        r.isolationSet(c.x(), c.y(), getProspect(r.blueprint(), wrap.init(r, c.x(), c.y()), null));
                        wrap.done();
                    }
                }
                ch.added = false;
            }
        }

        public double getProspect(RoomBlueprint blue, AREA r, MAP_BOOLEAN isWall)
        {
            this.isWall = isWall;

            double unwalled = 0;
            double total = 0;
            rec.set(r.body());
            GUTIL.marker().init(this);

            foreach (var c in rec)
            {
                if (!r.is(c) || !isEdge(r, c))
                    continue;

                total++;
                foreach (var d in DIR.ALL)
                {
                    if (!SETT.IN_BOUNDS(c, d))
                    {
                        unwalled++;
                        continue;
                    }
                    if (r.is(c, d))
                        continue;

                    if (!wall.is(c, d) && !GUTIL.marker().is(c, d))
                    {
                        if (blue == SETT.ROOMS().HOME)
                        {
                            if (blue != SETT.ROOMS().map.blueprintImp.get(c, d))
                            {
                                GUTIL.marker().set(c, d, true);
                                unwalled++;
                            }
                        }
                        else
                        {
                            GUTIL.marker().set(c, d, true);
                            unwalled++;
                        }
                        continue;
                    }
                }
            }

            GUTIL.filler().done();

            int bonus = (int)Math.Ceiling(total / 10.0);
            double v = total - unwalled + bonus;
            v /= total;
            v = Math.Clamp(v, 0, 1);
            v = MATH.pow15.pow(v);
            return v;
        }

        private MAP_BOOLEAN isWall;

        private readonly MAP_BOOLEAN wall = new MAP_BOOLEAN
        {
            is = (tx, ty) =>
            {
                if (!SETT.IN_BOUNDS(tx, ty))
                    return false;
                if (isWall != null && isWall.is(tx, ty))
                    return true;
                TerrainTile t = SETT.TERRAIN().get(tx, ty);
                if (t is TFortification.Normal)
                    return true;
                return t.clearing().isStructure() && t.getAvailability(tx, ty) != null && t.getAvailability(tx, ty).player < 0;
            }
        };

        private bool isEdge(AREA r, COORDINATE c)
        {
            foreach (var d in DIR.ALL)
                if (!r.is(c, d))
                    return true;
            return false;
        }

        public readonly SAVABLE saver = new SAVABLE
        {
            save = file => { },
            load = file =>
            {
                throw new NotImplementedException();
            },
            clear = () => { }
        };

        private class Chunk
        {
            public readonly RecShort body;
            public bool added;

            public Chunk(int x1, int y1)
            {
                x1 *= CHUNKSIZE;
                y1 *= CHUNKSIZE;
                int x2 = x1 + CHUNKSIZE;
                x2 = Math.Min(x2, SETT.TWIDTH);
                int y2 = y1 + CHUNKSIZE;
                y2 = Math.Min(y2, SETT.THEIGHT);
                body = new RecShort(x1, x2, y1, y2);
            }
        }
    }
}