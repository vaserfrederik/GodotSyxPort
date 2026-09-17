using System;
using System.IO;
using System.Numerics;
using Init.Constant;
using Settlement.Main;
using Settlement.Thing;
using Snake2D.Renderer;
using Snake2D.Util.Color;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Rnd;
using Util.Rendering;

namespace Settlement.Thing
{
    sealed class Gore_BloodDrops : Gore
    {
        private readonly Rec body = new Rec(C.TILE_SIZE * 3);
        private readonly byte[] data = Alloc.Bb(16 * 3);
        private int lim;
        private float timerLife;
        private readonly ColorImp col = new ColorImp();

        public Gore_BloodDrops(int index, Sprites s) : base(index)
        {
            for (int i = 0; i < data.Length; i += 3)
            {
                DEG.SetRandom();
                int d = RND.RInt(C.TILE_SIZE);
                data[i] = (byte)(d * DEG.GetCurrentX() + C.TILE_SIZE);
                data[i + 1] = (byte)(d * DEG.GetCurrentY() + C.TILE_SIZE);
                data[i + 2] = (byte)RND.RInt(s.bloodPool.Tiles());
            }
        }

        protected override void Save(FilePutter f)
        {
            body.Save(f);
            f.Bs(data);
            f.I(lim);
            f.F(timerLife);
            col.Save(f);
        }

        protected override void Load(FileGetter f)
        {
            body.Load(f);
            f.Bs(data);
            lim = f.I();
            timerLife = f.F();
            col.Load(f);
        }

        protected override void Init(int cx, int cy, double sx, double sy, COLOR col)
        {
            this.col.Set(col);

            Init(cx, cy, 1.0);
        }

        private void Init(int x, int y, double amount)
        {
            lim = (int)(16 * amount);
            if (lim == 0)
                lim = 1;
            if (lim > 16)
                lim = 16;
            lim *= 3;

            timerLife = 250;
            body.MoveC(x, y);
        }

        protected override bool Update(double ds)
        {
            timerLife -= ds;
            if (timerLife < 0)
                return false;
            return true;
        }

        public override void Render(Renderer r, ShadowBatch shadows, float ds, int offsetX, int offsetY)
        {
            for (int i = 0; i < lim; i += 3)
            {
                int x = body.X1() + offsetX + data[i];
                int y = body.Y1() + offsetY + data[i + 1];
                BindCol(col, i);
                SETT.THINGS().Sprites.BloodPool.Render(r, data[i + 2], x, y);
            }
            COLOR.Unbind();
        }

        public override RECTANGLE Body()
        {
            return body;
        }

        protected override int Z()
        {
            return 0;
        }

        public override ThingFactory<Gore_BloodDrops> Factory()
        {
            return SETT.THINGS().Gore.Drops;
        }
    }
}