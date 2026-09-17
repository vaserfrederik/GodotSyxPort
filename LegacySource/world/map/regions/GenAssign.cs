using System;
using System.Collections.Generic;
using System.Linq;

namespace World.Map.Regions
{
    using Game.Faction;
    using Init.Constant;
    using Init.Sprite;
    using Snake2D;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;
    using Util;
    using Util.Rendering;
    using World;
    using World.Map.Regions.Centre;
    using World.Map.Road;
    using World.Overlay;

    internal sealed class GenAssign
    {
        private readonly Polymap pmap = new Polymap(WORLD.TWIDTH(), WORLD.THEIGHT(), 8, 1);

        public GenAssign(ACTION lprinter)
        {
            if (!GenPlayer.Gen())
                return;

            WORLD.OVERLAY().debug = new WorldOverlays.OverlayTile(true, false)
            {
                RenderBelow = (r, s, it) =>
                {
                    COLOR.UNIQUE.Get(pmap.Get(it.tx(), it.ty())).Bind();
                    SPRITES.cons().BIG.outline.Render(r, 0, it.x(), it.y());
                    COLOR.Unbind();
                }
            };

            ArrayList<Tile> tiles = new ArrayList<Tile>(WORLD.TAREA());

            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                if (CanBeCentre(c))
                {
                    tiles.Add(new Tile(c));
                }
            }

            tiles.Sort((o1, o2) => o1.value > o2.value ? 1 : -1);

            int ri = 1;
            foreach (Tile t in tiles)
            {
                if (ri > WREGIONS.MAX)
                    return;
                if (CanBeCentre(t))
                {
                    Assign(t, WORLD.REGIONS().GetByIndex(ri));
                    ri++;
                }
            }

            lprinter.Exe();
            Expand();
        }

        private void Expand()
        {
            pmap.CheckInit();
            GUTIL.flooder().Init(this);
            foreach (COORDINATE c in WORLD.TBOUNDS())
            {
                Region reg = WORLD.REGIONS().map.Get(c);

                if (reg != null && reg != WORLD.REGIONS().player)
                {
                    GUTIL.flooder().PushSloppy(c, 0);
                    GUTIL.flooder().SetValue2(c, reg.Index());
                }
            }

            while (GUTIL.flooder().HasMore())
            {
                PathTile t = GUTIL.flooder().PollSmallest();
                Region dadda = WORLD.REGIONS().GetByIndex((int)Math.Round(t.GetValue2()));
                WORLD.REGIONS().pmap.Set(t, dadda);
                foreach (DIR d in DIR.ORTHO)
                {
                    if (IsExpandable(t.x(), t.y(), d, dadda))
                        if (GUTIL.flooder().PushSmaller(t, d, t.GetValue() + d.TileDistance() * ExpandValue(t.x(), t.y(), d)) != null)
                        {
                            GUTIL.flooder().SetValue2(t.x(), t.y(), d, dadda.Index());
                        }
                }
            }
            GUTIL.flooder().Done();
        }

        private void Assign(COORDINATE centre, Region reg)
        {
            GUTIL.flooder().Init(this);
            pmap.CheckInit();

            for (int y = -(WCentre.TILE_DIM / 2); y <= WCentre.TILE_DIM; y++)
            {
                for (int x = -(WCentre.TILE_DIM / 2); x <= WCentre.TILE_DIM; x++)
                {
                    int dx = centre.x() + x;
                    int dy = centre.y() + y;
                    GUTIL.flooder().PushSloppy(dx, dy, 0);
                    WORLD.REGIONS().pmap.Set(dx, dy, reg);
                }
            }

            int area = 0;

            double size = (WCentre.TILE_DIM + 2) * (WCentre.TILE_DIM + 2) + Config.world().REGION_SIZE;
            while (GUTIL.flooder().HasMore())
            {
                PathTile t = GUTIL.flooder().PollSmallest();
                if (WORLD.REGIONS().pmap.Get(t) != null && WORLD.REGIONS().pmap.Get(t) != reg)
                    continue;

                pmap.Checker.Set(t, true);

                area++;
                WORLD.REGIONS().pmap.Set(t, reg);
                size -= TileValue(t.x(), t.y());
                if (size < 0)
                {
                    GUTIL.flooder().Done();
                    return;
                }

                if (t.GetValue() > Math.Sqrt(area) * 2.0)
                    continue;

                foreach (DIR d in DIR.ORTHO)
                {
                    if (WTRAV.CanLand(t.x(), t.y(), d, false))
                        GUTIL.flooder().PushSmaller(t, d, t.GetValue() + d.TileDistance() * ExpandValue(t.x(), t.y(), d));
                }
            }
            GUTIL.flooder().Done();
        }

        private double ExpandValue(int fromX, int fromY, DIR dir)
        {
            if (Terrain(fromX, fromY) != Terrain(fromX + dir.x(), fromY + dir.y()))
                return 5;
            if (!pmap.Checker.Is(fromX + dir.x(), fromY + dir.y()))
                return 20;
            return 1;
        }

        public static int Terrain(int tx, int ty)
        {
            if (WORLD.MOUNTAIN().GetHeight(tx, ty) > 0)
                return 1;

            if (WORLD.WATER().isBig.Is(tx, ty))
            {
                return 2;
            }
            if (WORLD.FOREST().amount.Get(tx, ty) == 1)
                return 3;
            return 0;
        }

        private bool IsExpandable(int fromX, int fromY, DIR dir, Region reg)
        {
            int tx = fromX + dir.x();
            int ty = fromY + dir.y();
            if (!WORLD.IN_BOUNDS(tx, ty))
                return false;

            if (WORLD.REGIONS().pmap.Get(tx, ty) != null && WORLD.REGIONS().pmap.Get(tx, ty, dir) != reg)
                return false;

            if (WORLD.MOUNTAIN().CoversTile(fromX, fromY) && WORLD.MOUNTAIN().CoversTile(tx, ty))
                return false;

            if (WORLD.MOUNTAIN().CoversTile(tx, ty))
            {
                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR d = DIR.ALL.Get(di);
                    if (!WORLD.MOUNTAIN().CoversTile(tx + d.x(), ty + d.y()))
                        return true;
                }
                return false;
            }
            if (WORLD.WATER().has.Is(tx, ty))
            {
                if (!WORLD.WATER().CoversTile.Is(tx, ty))
                    return true;
                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR d = DIR.ALL.Get(di);
                    if (WORLD.IN_BOUNDS(tx, ty, d) && !WORLD.WATER().CoversTile.Is(tx + d.x(), ty + d.y()))
                        return true;
                }
                return false;
            }
            return true;
        }

        private bool CanBeCentre(COORDINATE c)
        {
            if (WorldCentrePlacablity.Terrain(c.x(), c.y()) == null)
            {
                for (int y = -(1 + WCentre.TILE_DIM / 2); y < WCentre.TILE_DIM + 2; y++)
                {
                    for (int x = -(1 + WCentre.TILE_DIM / 2); x < WCentre.TILE_DIM + 2; x++)
                    {
                        int dx = c.x() + x;
                        int dy = c.y() + y;
                        if (!WORLD.IN_BOUNDS(dx, dy))
                            return false;
                        if (WORLD.REGIONS().map.Get(dx, dy) != null)
                            return false;
                    }
                }
                return true;
            }
            return false;
        }

        private static double TileValue(int tx, int ty)
        {
            double v = CLAMP.d(WORLD.MOISTURE().Get(tx, ty), 0.05, 1);
            if (WORLD.WATER().has.Is(tx, ty))
            {
                v = 0.75 + 0.25 * v;
            }
            if (WORLD.MOUNTAIN().haser.Is(tx, ty))
                v = 0.5 + v * 0.5;
            if (WORLD.FOREST().is.Is(tx, ty))
                v = 0.2 + v * 0.8;
            return v;
        }

        public void Clear()
        {
            while (FACTIONS.NPCs().Size > 0)
            {
                FACTIONS.Remove(FACTIONS.NPCs().Get(0), false);
            }
            WORLD.REGIONS().saver().Clear();
        }
    }

    internal class Tile : CORD
    {
        public double value;

        public Tile(COORD c) : base(c.x(), c.y())
        {
            value = Value();
        }

        private double Value()
        {
            double v = WORLD.MOISTURE().Get(x(), y());
            foreach (DIR d in DIR.ALLC)
            {
                if (WORLD.WATER().has.Is(x(), y(), d))
                {
                    v += WORLD.MOISTURE().Get(x(), y());
                }
                if (WORLD.MOUNTAIN().haser.Is(x(), y(), d))
                    v += WORLD.MOISTURE().Get(x(), y());
            }
            v *= RND.rFloat1(0.25);
            return v + RND.rFloat();
        }
    }
}