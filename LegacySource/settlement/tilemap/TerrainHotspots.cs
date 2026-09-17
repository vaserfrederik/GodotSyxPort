using System.Collections.Generic;
using snake2d.util.datatypes;
using init.resources;

namespace settlement.tilemap
{
    public class TerrainHotspots
    {
        private List<TerrainHotSpot> all = new List<TerrainHotspots.TerrainHotSpot>();

        public TerrainHotspots()
        {
        }

        public static class TerrainHotSpot : BODY_HOLDER
        {
            public readonly RESOURCE res;
            public readonly int type;
            private readonly int subType;
            public readonly SPRITE icon;
            private readonly Rec rec;

            private TerrainHotSpot(RESOURCE res, int type, int subType, SPRITE icon, Rec rec)
            {
                this.type = type;
                this.subType = subType;
                this.icon = icon;
                this.rec = rec;
                this.res = res;
                rec.incrW(6);
                rec.incrH(6);
                rec.incrX(-3);
                rec.incrY(-3);
            }

            public RECTANGLE body()
            {
                return rec;
            }
        }

        public List<TerrainHotSpot> All()
        {
            return all;
        }

        public void Init()
        {
            Bitmap1D checked = new Bitmap1D(SETT.TAREA, false);
            LinkedList<TerrainHotSpot> all = new LinkedList<TerrainHotSpot>();

            foreach (COORDINATE c in new Rec(SETT.TILE_BOUNDS))
            {
                TerrainHotSpot sp = Make(c.x(), c.y(), checked);
                if (sp != null)
                {
                    all.Add(sp);
                }
            }

            LinkedList<TerrainHotSpot> all2 = new LinkedList<TerrainHotSpot>();

            while (!all.IsEmpty())
            {
                TerrainHotSpot h = all.RemoveFirst();

                bool fuck = false;
                foreach (TerrainHotSpot s2 in all2)
                {
                    if (h.type == s2.type && h.subType == s2.subType && h.rec.touches(s2))
                    {
                        s2.rec.unify(h.rec);
                        fuck = true;
                    }
                }
                if (!fuck)
                    all2.Add(h);
            }

            this.all = new List<TerrainHotSpot>(all2);
        }

        private TerrainHotSpot Make(int tx, int ty, Bitmap1D checked)
        {
            if (checked.Get(tx + ty * SETT.TWIDTH))
                return null;

            int d = 8;

            TerrainTile t = SETT.TERRAIN().Get(tx, ty);
            if (t != null && t is TGrowable)
            {
                Rec r = new Rec(1);
                r.MoveX1Y1(tx, ty);
                checked.Set(tx + ty * SETT.TWIDTH, true);
                while (true)
                {
                    if (Join(r, t, r.x1() - d, r.x1(), r.y1(), r.y2(), checked))
                        continue;
                    if (Join(r, t, r.x2(), r.x2() + d, r.y1(), r.y2(), checked))
                        continue;
                    if (Join(r, t, r.x1() - d, r.x2() + d, r.y1() - d, r.y1(), checked))
                        continue;
                    if (Join(r, t, r.x1() - d, r.x2() + d, r.y2(), r.y2() + d, checked))
                        continue;
                    break;
                }
                if (r.width() * r.height() < 9)
                    return null;
                return new TerrainHotSpot(((TGrowable)t).Growable.resource, 0, ((TGrowable)t).GIndex, t.GetIcon(), r);
            }
            else if (SETT.MINERALS().getter.Get(tx, ty) != null)
            {
                Rec r = new Rec();
                r.MoveX1Y1(tx, ty);
                checked.Set(tx + ty * SETT.TWIDTH, true);
                Minable m = SETT.MINERALS().getter.Get(tx, ty);
                while (true)
                {
                    if (Join(r, m, r.x1() - d, r.x1(), r.y1(), r.y2(), checked))
                        continue;
                    if (Join(r, m, r.x2(), r.x2() + d, r.y1(), r.y2(), checked))
                        continue;
                    if (Join(r, m, r.x1() - d, r.x2() + d, r.y1() - d, r.y1(), checked))
                        continue;
                    if (Join(r, m, r.x1() - d, r.x2() + d, r.y2(), r.y2() + d, checked))
                        continue;
                    break;
                }
                if (r.width() * r.height() < 9)
                    return null;
                return new TerrainHotSpot(m.resource, 1, m.index, m.resource.icon(), r);
            }
            return null;
        }

        private bool Join(Rec r, TerrainTile t, int x1, int x2, int y1, int y2, Bitmap1D checked)
        {
            bool j = false;
            for (int y = y1; y < y2; y++)
            {
                for (int x = x1; x < x2; x++)
                {
                    if (!SETT.IN_BOUNDS(x, y))
                        continue;
                    if (t.Is(x, y))
                    {
                        checked.Set(x + y * SETT.TWIDTH, true);
                        r.unify(x, y);
                        j = true;
                    }
                }
            }
            return j;
        }

        private bool Join(Rec r, Minable t, int x1, int x2, int y1, int y2, Bitmap1D checked)
        {
            bool j = false;
            for (int y = y1; y < y2; y++)
            {
                for (int x = x1; x < x2; x++)
                {
                    if (!SETT.IN_BOUNDS(x, y))
                        continue;
                    if (SETT.MINERALS().getter.Get(x, y) == t)
                    {
                        checked.Set(x + y * SETT.TWIDTH, true);
                        r.unify(x, y);
                        j = true;
                    }
                }
            }
            return j;
        }

        public List<TerrainHotSpot> ALL()
        {
            return all;
        }
    }
}