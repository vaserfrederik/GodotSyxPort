using System;
using System.IO;
using Init.Constant;
using Init.Resources;
using Settlement.Entity;
using Settlement.Entity.Humanoid;
using Settlement.Entity.Humanoid.AI.Work;
using Settlement.Main;
using Settlement.Room.Main.Furnisher;
using Settlement.Thing.HalfEntity;
using Snake2D;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Rnd;
using Util.Gui.Misc;
using Util.Rendering;

namespace Settlement.Thing.HalfEntity.Crate
{
    public sealed class TransportEntity : HalfEntity
    {
        private static int length = 3;
        private readonly int[] oo = Alloc.Ii(2 * 2 * length);
        private static VectorImp vec = new VectorImp();
        private int hi = -1;
        private int ri = -1;
        private byte ran;
        private double mov;
        private int ox, oy;
        private bool mil;
        private readonly int random = RND.RInt();
        private static readonly double moveMax = C.TILE_SIZE + C.TILE_SIZEH;
        private static readonly int[] bumpOff = Alloc.Ii(128);

        static TransportEntity()
        {
            for (int i = 0; i < bumpOff.Length; i += 2)
            {
                bumpOff[i] = (int)(RND.RSign() * RND.RFloat() * 2);
                bumpOff[i + 1] = (int)(RND.RSign() * RND.RFloat() * 2);
            }
        }

        public TransportEntity() : base(C.TILE_SIZE, C.TILE_SIZE)
        {
        }

        protected override void Save(FilePutter file)
        {
            file.I(hi);
            RESOURCES.Map().Saver().Save(Res(), file);
            file.B(ran);
            file.D(mov);
            file.I(ox);
            file.I(oy);
            file.Bool(mil);
            file.IsE(oo);
        }

        protected override HalfEntity Load(FileGetter file) => throw new NotImplementedException();

        public RESOURCE Res() => ri == -1 ? null : RESOURCES.ALL().Get(ri);

        public Humanoid Host()
        {
            ENTITY e = SETT.ENTITIES().GetByID(hi);
            return e != null && e is Humanoid ? (Humanoid)e : null;
        }

        public double CarryAmount()
        {
            Humanoid h = Host();
            return h == null ? -1 : AIModule_Work.GetTransportAmount(h);
        }

        bool Init(Humanoid h, int tx, int ty, RESOURCE res, byte ran, bool mil)
        {
            Body().MoveC(tx * C.TILE_SIZE + C.TILE_SIZEH, ty * C.TILE_SIZE + C.TILE_SIZEH);
            hi = h.Id();
            this.ran = ran;
            ri = res.BIndex();
            ox = (short)Body().CX();
            oy = (short)Body().CY();
            mov = 0;
            vec.Set(Body(), h.Body());

            DIR d = DIR.N;

            FurnisherItem it = SETT.ROOMS().FData.Item.Get(tx, ty);
            if (it != null)
                d = DIR.ORTHO.Get(it.Rotation).Perpendicular();

            for (int i = 0; i < oo.Length / 2; i++)
            {
                oo[i * 2] = (int)(Body().CX() + d.XN() * (i) * moveMax);
                oo[i * 2 + 1] = (int)(Body().CY() + d.YN() * (i) * moveMax);
            }

            this.mil = mil;
            Add();
            return true;
        }

        protected override void Update(double ds)
        {
            Humanoid a = Host();
            if (a == null)
            {
                Remove();
                return;
            }

            double am = CarryAmount();
            if (am <= 0)
            {
                Remove();
                return;
            }

            if (ox == a.Body().CX() && oy == a.Body().CY())
                return;

            mov = vec.Set(ox, oy, a.Body().CX(), a.Body().CY());

            if (mov >= moveMax)
            {
                mov = 0;
                Body().MoveC(ox, oy);
                ox = a.Body().CX();
                oy = a.Body().CY();

                for (int i = oo.Length / 2 - 1; i > 0; i--)
                {
                    oo[i * 2] = oo[(i - 1) * 2];
                    oo[i * 2 + 1] = oo[(i - 1) * 2 + 1];
                }
                oo[0] = a.Body().CX();
                oo[1] = a.Body().CY();
            }
        }

        public override void Render(Renderer r, ShadowBatch s, float ds, int x1, int y1)
        {
            double am = CarryAmount();
            if (am < 0)
                return;
            x1 += C.TILE_SIZEH;
            y1 += C.TILE_SIZEH;

            if (mil)
            {
                vec.Set(Body().CX(), Body().CY(), ox, oy);
                DIR dir = vec.Dir();
                int dx = (int)(vec.NX() * mov);
                int dy = (int)(vec.NY() * mov);
                int x = x1 + dx;
                int y = y1 + dy;

                int bi = (int)(mov * 2);
                bi %= bumpOff.Length;
                bi &= ~1;

                int cx = (int)(x);
                int cy = (int)(y);
                SETT.ANIMALS().RenderCaravan(r, s, mov / C.TILE_SIZE, cx, cy, null, 0, false, dir.Id(), ran);
                cx = (int)(x - dir.XN() * C.TILE_SIZE) + bumpOff[bi];
                cy = (int)(y - dir.YN() * C.TILE_SIZE) + bumpOff[bi + 1];
                RenderCart(r, s, dir.Id(), cx, cy, ran, Res(), am, mov / C.TILE_SIZE);
            }
            else
            {
                for (int i = 2; i < oo.Length / 2; i++)
                {
                    int xx = oo[i * 2] - (Body().X1() + C.TILE_SIZEH);
                    int yy = oo[i * 2 + 1] - (Body().Y1() + C.TILE_SIZEH);
                    vec.Set(oo[i * 2], oo[i * 2 + 1], oo[(i - 1) * 2], oo[(i - 1) * 2 + 1]);

                    DIR dir = vec.Dir();
                    int dx = (int)(vec.NX() * mov);
                    int dy = (int)(vec.NY() * mov);
                    int x = x1 + dx + xx;
                    int y = y1 + dy + yy;

                    int bi = (int)(mov * 2) + (random >> i * 2) & 0x0F;
                    bi %= bumpOff.Length;
                    bi &= ~1;

                    int cx = (int)(x) + bumpOff[bi];
                    int cy = (int)(y) + bumpOff[bi + 1];
                    if (i == 2)
                    {
                        int ax = (int)(x + dir.XN() * C.TILE_SIZE);
                        int ay = (int)(y + dir.YN() * C.TILE_SIZE);
                        SETT.ANIMALS().RenderCaravan(r, s, mov / C.TILE_SIZE, ax, ay, null, 0, false, dir.Id(), ran);
                        Constructor().Sprite.RenderHarness(r, s, dir.Id(), cx, cy);
                    }

                    RenderCart(r, s, dir.Id(), cx, cy, ran, Res(), am, mov / C.TILE_SIZE);
                }
            }
        }

        public void RenderCart(SPRITE_RENDERER r, ShadowBatch s, int rot, int cx, int cy, int ran, RESOURCE res, double resamount, double mov)
        {
            Constructor().Sprite.RenderBelow(r, s, rot, cx, cy, mov, ran, 0, res, resamount);
            Constructor().Sprite.Render(r, s, rot, cx, cy, 0, mil);
        }

        protected override void RemoveAction()
        {
        }

        protected override TransportFactory Constructor() => null;

        public override void HoverInfo(GBox box)
        {
        }
    }
}