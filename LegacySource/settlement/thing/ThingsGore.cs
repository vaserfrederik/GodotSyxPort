using System;
using System.Collections.Generic;

namespace settlement.thing
{
    using init.constant;
    using settlement.entity;
    using settlement.thing.THINGS;
    using snake2d.util.color;
    using snake2d.util.rnd;
    using snake2d.util.sets;

    public class ThingsGore
    {
        private static readonly float[][] colRan = new float[64][];
        static ThingsGore()
        {
            for (int i = 0; i < colRan.Length; i++)
            {
                colRan[i] = new float[3];
                for (int k = 0; k < colRan[i].Length; k++)
                    colRan[i][k] = (float)(0.5 + 0.5 * RND.rFloat());
            }
        }

        private const int amount = 5000;

        private readonly GoreHolder flesh;
        private readonly GoreHolder drop;
        private readonly GoreHolder clouds;
        private readonly GoreHolder drops;

        private readonly COLOR cDebris = new ColorImp(80, 80, 80);

        public ThingsGore(LISTE<ThingFactory<?>> all, float goreamount, Sprites s)
        {
            Gore_Flesh[] flesht = new Gore_Flesh[amount];
            Gore_BloodDrop[] dropst = new Gore_BloodDrop[amount];
            Gore_BloodDrops[] dropss = new Gore_BloodDrops[amount];
            for (int i = 0; i < flesht.Length; i++)
            {
                flesht[i] = new Gore_Flesh(i);
                dropst[i] = new Gore_BloodDrop(i);
                dropss[i] = new Gore_BloodDrops(i, s);
            }
            Gore_BloodCloud[] clouds = new Gore_BloodCloud[amount / 4];
            for (int i = 0; i < clouds.Length; i++)
            {
                clouds[i] = new Gore_BloodCloud(i);
            }

            this.flesh = new GoreHolder(all, flesht, false);
            this.drop = new GoreHolder(all, dropst, true);
            this.clouds = new GoreHolder(all, clouds, false);
            this.drops = new GoreHolder(all, dropss, true);
        }

        public void update(float ds)
        {
            flesh.update(ds);
            drop.update(ds);
            clouds.update(ds);
            drops.update(ds);
        }

        public void explode(ENTITY e, COLOR col)
        {
            for (int i = 0; i < 15; i++)
            {
                flesh(e, col);
            }
            drops(e, col);
            cloud(e, col);
        }

        public void gore(int cx, int cy, COLOR col)
        {
            flesh.make(cx, cy, 0, 0, col);
        }

        public void bleed(ENTITY e, COLOR col)
        {
            drop.make(e, col);
        }

        public void bleed(int cx, int cy, double sx, double sy, COLOR col)
        {
            drop.make(
                cx + RND.rInt0(C.TILE_SIZEH),
                cy + RND.rInt0(C.TILE_SIZEH),
                sx, sy, col);
        }

        public void flesh(ENTITY e, COLOR col)
        {
            flesh.make(e, col);
        }

        public void debris(ENTITY e)
        {
            Gore_Flesh.debr = true;
            flesh.make(e, cDebris);
            Gore_Flesh.debr = false;
        }

        public void debris(int cx, int cy, double sx, double sy)
        {
            Gore_Flesh.debr = true;
            flesh.make(cx, cy, sx, sy, cDebris);
            flesh.make(cx, cy, sx, sy, cDebris);
            Gore_Flesh.debr = false;
        }

        public void cloud(ENTITY e, COLOR col)
        {
            clouds.make(e, col);
        }

        public void drops(ENTITY e, COLOR col)
        {
            drops.make(e, col);
        }

        public abstract class Gore : Thing
        {
            private static readonly ColorImp colTmp = new ColorImp();

            public Gore(int index) : base(index)
            {
            }

            protected abstract bool update(double ds);

            protected abstract void init(int cx, int cy, double sx, double sy, COLOR color);

            protected void bindCol(COLOR color, int am)
            {
                bindCol(color, am, 1.0f);
            }

            protected void bindCol(COLOR color, int am, float mul)
            {
                float[] tt = colRan[am & 63];
                colTmp.set((int)(color.red() * tt[0] * mul) & 0x0FF, (int)(color.green() * tt[1] * mul) & 0x0FF, (int)(color.blue() * tt[2] * mul) & 0x0FF).bind();
            }
        }

        public sealed class GoreHolder : ThingFactory<Gore>
        {
            private readonly Gore[] gore;
            private readonly bool slow;
            private double t = 10;

            public GoreHolder(LISTE<ThingFactory<?>> all, Gore[] gore, bool slow) : base(all, gore.Length)
            {
                this.gore = gore;
                this.slow = slow;
            }

            public void make(ENTITY e, COLOR col)
            {
                make(
                    e.body().cX() + RND.rInt0(C.TILE_SIZEH),
                    e.body().cY() + RND.rInt0(C.TILE_SIZEH),
                    e.speed.x(), e.speed.y(), col);
            }

            public void make(int cx, int cy, double sx, double sy, COLOR col)
            {
                Gore f = nextInLine();
                f.init(cx, cy, sx, sy, col);
                f.add();
            }

            public override void update(double ds)
            {
                if (slow)
                {
                    t -= ds;
                    if (t > 0)
                        return;
                    t = 10;
                    ds = 10;
                }

                Gore g = first();
                Gore drop = null;
                while (g != null)
                {
                    if (drop == null)
                        drop = g;
                    else if (drop == g)
                        break;

                    Gore next = next(g);

                    if (!g.update(ds))
                    {
                        g.remove();
                    }
                    g = next;
                }
            }

            protected override Gore[] all()
            {
                return gore;
            }
        }
    }
}