using System;
using System.IO;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using util.rendering;

namespace settlement.thing
{
    class Gore_BloodCloud : Gore
    {
        private static readonly int time = 60 * 2;
        private static readonly int amount = 175 * 2;
        private static readonly int[][] pos = Alloc.i2(time, amount);
        private ColorImp color = new ColorImp();

        static Gore_BloodCloud()
        {
            for (int k = 0; k < amount; k += 2)
            {
                DEG.setRandom();
                double x = RND.rInt0(5) + C.TILE_SIZE + C.TILE_SIZEH;
                double y = RND.rInt0(5) + C.TILE_SIZE + C.TILE_SIZEH;
                double speed = (C.TILE_SIZE * 2 + RND.rFloat() * C.TILE_SIZE * 12) / 60.0;
                for (int tick = 0; tick < time; tick++)
                {
                    pos[tick][k] = (int)x;
                    pos[tick][k + 1] = (int)y;
                    if (speed < 0)
                        speed = 0;
                    x += speed * DEG.getCurrentX();
                    y += speed * DEG.getCurrentY();
                    speed -= 0.4;
                }
            }
        }

        private readonly Rec body = new Rec(C.TILE_SIZE * 3);
        private readonly ESpeed.Imp speed = new ESpeed.Imp();
        private float timer = -70;
        private int tick;
        private int am;

        Gore_BloodCloud(int index) : base(index)
        {
            speed.magnitudeTargetSet(0);
        }

        protected override void save(FilePutter f)
        {
            body.save(f);
            speed.save(f);
            f.f(timer);
            f.i(tick);
            f.i(am);
            color.save(f);
        }

        protected override void load(FileGetter f)
        {
            body.load(f);
            speed.load(f);
            timer = f.f();
            tick = f.i();
            am = f.i();
            color.load(f);
        }

        protected override void init(int cx, int cy, double sx, double sy, COLOR col)
        {
            this.color.set(col);
            body.moveC(cx, cy);
            timer = 0;
            tick = 0;
            am = amount;
            speed.setRaw(sx, sy);
        }

        protected override bool update(double ds)
        {
            if (speed.isZero())
            {
                move(speed, ds, 0, body, false);
            }

            speed.magnitudeAdjust(ds, 10.0, 1);

            timer += (float)ds;
            tick = (int)(timer * 60);
            if (tick >= time)
            {
                am = (int)(amount - (timer - 2) * 10);
                if (am <= 0)
                    return false;
                tick = time - 1;
            }

            return true;
        }

        public override void render(Renderer r, ShadowBatch shadows, float ds, int offsetX, int offsetY)
        {
            int x = body().x1() + offsetX;
            int y = body().y1() + offsetY;

            for (int j = 0; j < am; j += 2)
            {
                bindCol(color, j, 0.7f);
                r.renderParticle(pos[tick][j] + x, pos[tick][j + 1] + y);
            }
            COLOR.unbind();
        }

        public override RECTANGLE body()
        {
            return body;
        }

        protected override int z()
        {
            return 1;
        }

        public override ThingFactory factory()
        {
            return SETT.THINGS().gore.clouds;
        }
    }
}