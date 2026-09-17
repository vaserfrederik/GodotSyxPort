using System;
using System.Collections.Generic;
using settlement.thing.projectiles;
using init.constant;
using settlement.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.data.INT;
using util.gui.slider;
using view.main;
using view.sett;
using view.tool;

public class Test
{
    private static readonly int vel = C.TILE_SIZE * 40;
    private static readonly double ang = 75;

    public Test()
    {
        IDebugPanelSett.Add(new Single());

        IDebugPanelSett.Add(new MASS());
    }

    private class Single : PlacableSimple
    {
        int sx, sy;
        readonly Trajectory t = new Trajectory();
        private readonly INTE type = new INTE()
        {
            int i = 0;

            public override int min() => 0;

            public override int max() => STATS.EQUIP().RANGED().Size - 1;

            public override int get() => i;

            public override void set(int t) => i = t;
        };
        private readonly ArrayList<CLICKABLE> extra = new ArrayList<CLICKABLE>(new GTarget(80, false, true, type));

        private readonly PlacableSimple next = new PlacableSimple(this.name())
        {
            public override void place(int x, int y)
            {
                if (t.calcLow(0, sx, sy, x, y, ang, vel))
                {
                    SETT.PROJS().launch(sx, sy, 0, t, Projectile.ALL.GetLast(), (byte)0, (byte)0, null);
                }
            }

            public override CharSequence isPlacable(int x, int y)
            {
                return t.calcLow(0, sx, sy, x, y, ang, vel) ? null : E;
            }

            public override PLACABLE getUndo()
            {
                return Single.this;
            }
        };

        public Single() : base("projectile")
        {
        }

        public override CharSequence isPlacable(int x, int y)
        {
            return SETT.PIXEL_IN_BOUNDS(x, y) ? null : E;
        }

        public override void place(int x, int y)
        {
            sx = x;
            sy = y;
            VIEW.s().tools.place(next);
        }

        public override LIST<CLICKABLE> getAdditionalButt()
        {
            return extra;
        }
    }

    private class MASS : PlacableSimple
    {
        readonly VectorImp vec = new VectorImp();
        int sx, sy;
        readonly Trajectory t = new Trajectory();

        private readonly PlacableSimple next = new PlacableSimple(this.name())
        {
            public override void place(int x, int y)
            {
                if (t.calcLow(0, sx, sy, x, y, ang, vel))
                {
                    vec.set(sx, sy, x, y);
                    vec.rotate90();
                    for (int i = -8; i <= 8; i++)
                    {
                        int xx = (int)(sx + vec.nX() * i * C.TILE_SIZEH);
                        int yy = (int)(sy + vec.nY() * i * C.TILE_SIZEH);
                        SETT.PROJS().launch(xx, yy, 0, t, Projectile.ALL.GetLast(), 0.05f, (short)0, null);
                    }
                }
            }

            public override CharSequence isPlacable(int x, int y)
            {
                return t.calcLow(0, sx, sy, x, y, ang, vel) ? null : E;
            }

            public override PLACABLE getUndo()
            {
                return MASS.this;
            }
        };

        public MASS() : base("projectile mass")
        {
        }

        public override CharSequence isPlacable(int x, int y)
        {
            return SETT.PIXEL_IN_BOUNDS(x, y) ? null : E;
        }

        public override void place(int x, int y)
        {
            sx = x;
            sy = y;
            VIEW.s().tools.place(next);
        }
    }
}