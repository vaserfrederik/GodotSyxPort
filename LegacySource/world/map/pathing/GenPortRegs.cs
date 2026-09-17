using System;
using System.Collections.Generic;

namespace World.Map.Pathing
{
    using static World.WORLD;

    using Init.Sprite;
    using Snake2D;
    using Snake2D.PathUtilOnline;
    using Snake2D.Util.Color;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Map;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;
    using Util;
    using Util.Rendering;
    using World;
    using World.Map.Road;
    using World.Overlay;

    internal sealed class GenPortRegs : Bitsmap2D
    {
        public GenPortRegs(Action u)
            : base(-1, 6, TBOUNDS())
        {
            WORLD.OVERLAY().debug = new WorldOverlays.OverlayTile(true, false)
            {
                protected override void renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderIterator it)
                {
                    if (WORLD.WATER().isBig.Is(it.Tile()) && Get(it.Tile()) >= 0)
                    {
                        COLOR.UNIQUE.Get(Get(it.Tile())).Bind();
                        SPRITES.cons().BIG.outline.Render(r, 0, it.X(), it.Y());
                        COLOR.Unbind();
                    }
                }
            };

            Flooder f = GUTIL.flooder();
            f.Init(f);

            Polymap p = new Polymap(TBOUNDS(), 16, 1);
            p.CheckInit();
            int ma = 0;

            foreach (COORDINATE c in TBOUNDS())
            {
                if (WORLD.WATER().isBig.Is(c) && WORLD.ROADS().harbour.Is(c))
                {
                    if (p.Checker.Is(c))
                    {
                        continue;
                    }
                    p.Checker.Set(c, true);
                    f.PushSloppy(c.X(), c.Y(), 0, null);
                    ma = Math.Max(ma, p.Getter.Get(c));
                    f.SetValue2(c, p.Getter.Get(c));
                }
            }

            TmpReg[] regs = new TmpReg[ma + 1];

            int id = 0;

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();
                if (t.Parent != null)
                    t.SetValue2(t.Parent.GetValue2());

                int pi = (int)t.GetValue2();
                if (regs[pi] == null)
                {
                    regs[pi] = new TmpReg(id++, t);
                }
                map.Set(t, regs[pi]);
                regs[pi].area++;

                foreach (DIR d in DIR.ALL)
                {
                    if (!WORLD.WATER().isBig.Is(t, d))
                        continue;
                    if (!WTRAV.Can(t.X(), t.Y(), d, false))
                        continue;

                    double v = 1;
                    if (pi != p.Getter.Get(t, d))
                        v += 100;
                    f.PushSmaller(t, d, t.GetValue() + v * d.TileDistance(), t);
                }

            }

            f.Done();

            Bitmap1D check = new Bitmap1D(id, false);
            id = 0;
            foreach (COORDINATE c in TBOUNDS())
            {
                if (Process(c, id % Max(), check))
                {
                    id++;
                    if (id % 5 == 0)
                        u.Invoke();
                }
            }
        }

        private bool Process(COORDINATE start, int id, Bitmap1D check)
        {

            TmpReg home = map.Get(start);
            if (home == null)
                return false;

            if (home.done)
                return false;
            check.Clear();
            check.Set(home.id, true);
            Flooder f = GUTIL.flooder();
            f.Init(f);
            f.PushSloppy(start.X(), start.Y(), 0);

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();

                TmpReg r = map.Get(t);
                if (r.done)
                    continue;
                if (r != home && !check.Get(r.id))
                {
                    if (r.area < 64)
                    {
                        check.Set(r.id, true);
                        home.area += r.area;
                        r.done = true;
                        r.area = 0;
                    }
                    else if (home.area < 64)
                    {
                        check.Set(r.id, true);
                        home.area += r.area;
                        r.done = true;
                        r.area = 0;
                    }
                }


                if (!check.Get(r.id))
                {
                    continue;
                }

                map.Set(t, home);
                Set(t, id);

                foreach (DIR d in DIR.ALL)
                {
                    if (!WORLD.WATER().isBig.Is(t, d))
                        continue;
                    if (!WTRAV.Can(t.X(), t.Y(), d, false))
                        continue;

                    double v = d.TileDistance();
                    TmpReg to = map.Get(t, d);
                    if (!check.Get(to.id))
                    {
                        v += to.area * 100;
                    }
                    f.PushSmaller(t, d, t.GetValue() + v, t);
                }

            }

            f.Done();
            home.done = true;

            return true;
        }


        private static class TmpReg
        {

            private readonly int id;
            public int area = 0;
            public bool done = false;

            public TmpReg(int id, COORDINATE c)
            {
                this.id = id;
            }

        }

        private readonly MAP_OBJECTE<TmpReg> map = new MAP_OBJECTE<GenPortRegs.TmpReg>()
        {

            private readonly TmpReg[] rmap = new TmpReg[WORLD.TAREA()];

            public override TmpReg Get(int tx, int ty)
            {
                if (!WORLD.IN_BOUNDS(tx, ty))
                    return null;
                return Get(tx + ty * WORLD.TWIDTH());
            }

            public override TmpReg Get(int tile)
            {
                return rmap[tile];
            }

            public override void Set(int tx, int ty, TmpReg obj)
            {
                if (WORLD.IN_BOUNDS(tx, ty))
                {
                    Set(tx + ty * WORLD.TWIDTH(), obj);
                }

            }

            public override void Set(int tile, TmpReg obj)
            {
                rmap[tile] = obj;
            }
        };
    }
}