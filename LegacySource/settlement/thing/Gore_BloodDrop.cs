using System;
using System.IO;
using settlement.main;
using settlement.thing;
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
    class Gore_BloodDrop : Gore
    {
        private Rec body = new Rec();
        private int ran;
        private float timerLife;
        private readonly ColorImp color = new ColorImp();

        public Gore_BloodDrop(int index) : base(index)
        {
        }

        protected override void Save(FilePutter f)
        {
            body.Save(f);
            f.I(ran);
            f.F(timerLife);
            color.Save(f);
        }

        protected override void Load(FileGetter f)
        {
            body.Load(f);
            ran = f.I();
            timerLife = f.F();
            color.Load(f);
        }

        protected override void Init(int cx, int cy, double sx, double sy, COLOR c)
        {
            int x = cx;
            int y = cy;
            int w = RND.RInt(16);
            color.Set(c);
            Init(x, y, w);
        }

        private void Init(int x, int y, int dim)
        {
            DEG.SetRandom();
            x += dim * DEG.GetCurrentX();
            y += dim * DEG.GetCurrentY();

            timerLife = 120;
            ran = RND.RInt();

            body.SetDim(Sprite().Size());
            body.MoveX1Y1(x, y);
        }

        private TILE_SHEET Sprite()
        {
            return SETT.THINGS().Sprites.bloodPool;
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
            BindCol(color, ran >> 8);
            Sprite().Render(r, ran % Sprite().Tiles(), body.X1() + offsetX, body.Y1() + offsetY);
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

        public override ThingFactory Factory()
        {
            return SETT.THINGS().Gore.Drop;
        }
    }
}