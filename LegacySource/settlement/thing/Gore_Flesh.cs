using System;
using System.IO;
using settlement.entity;
using settlement.main;
using settlement.thing.THINGS;
using settlement.thing.ThingsGore;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sprite;
using util.rendering;

namespace settlement.thing
{
    public class Gore_Flesh : Gore
    {
        private static COLOR[] cols = new COLOR[64];
        static Gore_Flesh()
        {
            for (int i = 0; i < cols.Length; i++)
            {
                double shade = 0.4 + 0.6 * RND.rFloat();
                int r = (int)(127 * (shade - RND.rFloat() * 0.05));
                int g = (int)(127 * (shade - RND.rFloat() * 0.05));
                int b = (int)(127 * (shade - RND.rFloat() * 0.05));
                cols[i] = new ColorImp(r, g, b);
            }
        }

        private readonly Rec body = new Rec();
        private readonly ESpeed.Imp speed = new ESpeed.Imp();
        private readonly ColorImp col = new ColorImp();

        private int ran;
        float timer;
        bool debris = false;
        static bool debr = false;

        public Gore_Flesh(int index) : base(index)
        {
            speed.magnitudeTargetSet(0);
            speed.accelerationInit(C.TILE_SIZE * 5);
        }

        protected override void save(FilePutter f)
        {
            body.save(f);
            speed.save(f);
            f.i(ran);
            f.f(timer);
            f.bool(debris);
            col.save(f);
        }

        protected override void load(FileGetter f)
        {
            body.load(f);
            speed.load(f);
            ran = f.i();
            timer = f.f();
            debris = f.bool();
            col.load(f);
        }

        protected override void init(int cx, int cy, double sx, double sy, COLOR col)
        {
            body.setDim(sprite().size(), sprite().size());
            body.moveC(cx, cy);
            this.col.set(col);
            ran = RND.rInt();
            double m = C.TILE_SIZE * 2 + RND.rFloatP(2) * (C.TILE_SIZE * 15);
            DEG.setRandom();
            speed.setRaw(sx + DEG.getCurrentX() * m, sy + DEG.getCurrentY() * m);
            timer = 120 + RND.rFloat(100);
            this.debris = debr;
        }

        void setDebris()
        {
            this.debris = true;
        }

        protected override bool update(double ds)
        {
            if (speed.isZero())
            {
                timer -= ds;
                if (timer < 0)
                {
                    return false;
                }
                return true;
            }

            speed.magnitudeAdjust(ds, 2.0, 1);
            move(speed, ds, 0.5f, body, true);
            return true;
        }

        public override void render(Renderer r, ShadowBatch shadows, float ds, int offsetX, int offsetY)
        {
            int spriteI = ran & (sprite().tiles() - 1);
            if (!debris)
                bindCol(col, ran >> 8);
            else
                cols[(ran >> 8) & 63].bind();
            sprite().render(r, spriteI, body().x1() + offsetX, body().y1() + offsetY);
            if (spriteI < 32)
            {
                shadows.setDistance2Ground(2).setHeight(0);
                sprite().render(shadows, spriteI, body().x1() + offsetX, body().y1() + offsetY);
            }
            COLOR.unbind();
        }

        private TILE_SHEET sprite()
        {
            if (debris)
                return SETT.THINGS().sprites.debris;
            return SETT.THINGS().sprites.flesh;
        }

        public override RECTANGLE body()
        {
            return body;
        }

        protected override int z()
        {
            return 100;
        }

        public override ThingFactory<?> factory()
        {
            return SETT.THINGS().gore.flesh;
        }
    }
}