using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace settlement.thing.halfEntity.transport
{
    public class TransportEntity : HalfEntity
    {
        private static int length = 3;

        private SPath path = new SPath();
        private DIR moveDir = DIR.N;
        private int amount;

        private int ox, oy;
        private short startTx, startTy;
        private readonly int[] oo = new int[2 * 2 * length];

        private RESOURCE res;
        private byte ran;
        private double mov;

        private bool mil;
        private readonly int random = RND.rInt();
        private static readonly VectorImp vec = new VectorImp();
        private static readonly int[] bumpOff = new int[128];

        static TransportEntity()
        {
            for (int i = 0; i < bumpOff.Length; i += 2)
            {
                bumpOff[i] = (int)(RND.rSign() * RND.rFloat() * 2);
                bumpOff[i + 1] = (int)(RND.rSign() * RND.rFloat() * 2);
            }
        }

        public TransportEntity() : base(C.TILE_SIZE, C.TILE_SIZE)
        {
        }

        protected override void Save(FilePutter file)
        {
            RESOURCES.map().saver().Save(res, file);
            file.s(startTx);
            file.s(startTy);
            file.i(moveDir.id());
            file.i(amount);
            file.b(ran);
            file.d(mov);
            file.i(ox);
            file.i(oy);
            file.bool(mil);
            file.isE(oo);
            path.Save(file);
        }

        protected override HalfEntity Load(FileGetter file)
        {
            res = RESOURCES.map().loader().LoadB(file, null);
            startTx = file.s();
            startTy = file.s();
            moveDir = DIR.ALL.GetC(file.i());
            amount = file.i();
            ran = file.b();
            mov = file.d();
            ox = file.i();
            oy = file.i();
            mil = file.bool();
            file.isE(oo);
            if (!VERSION.versionIsBefore(71, 43))
                path.Load(file);
            return this;
        }

        public bool InitMilitary(int tx, int ty, byte ran, RESOURCE res, int ramount, DIR d)
        {
            if (!PATH().finders.entryPoints.Find(tx, ty, path, int.MaxValue))
                return false;
            mil = true;
            Init(tx, ty, ran, res, ramount, d);
            return true;
        }

        public bool InitStation(int tx, int ty, byte ran, RESOURCE res, int ramount, DIR d, COORDINATE station)
        {
            if (!path.Request(tx, ty, station))
                return false;
            mil = false;
            Init(tx, ty, ran, res, ramount, d);
            return true;
        }

        private void Init(int tx, int ty, byte ran, RESOURCE res, int amount, DIR d)
        {
            moveDir = d;
            this.amount = amount;
            this.res = res;
            startTx = (short)tx;
            startTy = (short)ty;
            body().moveC(tx * C.TILE_SIZE + C.TILE_SIZEH, ty * C.TILE_SIZE + C.TILE_SIZEH);
            this.ran = ran;

            ox = (short)body().cX();
            oy = (short)body().cY();
            mov = 0;

            for (int i = 0; i < oo.Length / 2; i++)
            {
                oo[i * 2] = (int)(body().cX() + d.xN() * (i) * moveDir.tileDistance() * C.TILE_SIZE);
                oo[i * 2 + 1] = (int)(body().cY() + d.yN() * (i) * moveDir.tileDistance() * C.TILE_SIZE);
            }
            Add();
        }

        protected override void Update(double ds)
        {
            mov += ds * 2;

            if (!path.IsSuccess())
            {
                Remove();
                return;
            }

            if (mov >= moveDir.tileDistance())
            {
                if (path.IsDest())
                {
                    mov = moveDir.tileDistance();
                    if (!mil)
                    {
                        SETT.ROOMS().TRANSPORT.EndDelivery(startTx, startTy, res, amount, path.LengthTotal());
                        SETT.ROOMS().STATION.Deliver(res, amount, path.destX(), path.destY());
                        amount = 0;
                    }
                    else
                    {
                    }

                    Remove();
                }
                else
                {
                    mov -= moveDir.tileDistance();
                    body().moveC(ox, oy);
                    path.SetNext();
                    ox = path.GetSettCX();
                    oy = path.GetSettCY();
                    moveDir = DIR.Get(body().cX(), body().cY(), ox, oy);
                    for (int i = oo.Length / 2 - 1; i > 0; i--)
                    {
                        oo[i * 2] = oo[(i - 1) * 2];
                        oo[i * 2 + 1] = oo[(i - 1) * 2 + 1];
                    }
                    oo[0] = path.GetSettCX();
                    oo[1] = path.GetSettCY();
                }
            }
        }

        public override void Render(Renderer r, ShadowBatch s, float ds, int x1, int y1)
        {
            if (amount < 0)
                return;
            x1 += C.TILE_SIZEH;
            y1 += C.TILE_SIZEH;

            if (mil)
            {
                vec.Set(body().cX(), body().cY(), ox, oy);
                DIR dir = vec.Dir();
                int dx = (int)(vec.nX() * mov * C.TILE_SIZE);
                int dy = (int)(vec.nY() * mov * C.TILE_SIZE);
                int x = x1 + dx;
                int y = y1 + dy;

                int bi = (int)(mov * 22);
                bi %= bumpOff.Length;
                bi &= ~1;

                int cx = (int)(x);
                int cy = (int)(y);
                SETT.ANIMALS().RenderCaravan(r, s, mov, cx, cy, null, 0, false, dir.id(), ran);
                cx = (int)(x - dir.xN() * C.TILE_SIZE) + bumpOff[bi];
                cy = (int)(y - dir.yN() * C.TILE_SIZE) + bumpOff[bi + 1];
                RenderCart(r, s, dir.id(), cx, cy, ran, res, amount, mov);
            }
            else
            {
                for (int i = 2; i < oo.Length / 2; i++)
                {
                    int xx = oo[i * 2] - (body().x1() + C.TILE_SIZEH);
                    int yy = oo[i * 2 + 1] - (body().y1() + C.TILE_SIZEH);
                    vec.Set(oo[i * 2], oo[i * 2 + 1], oo[(i - 1) * 2], oo[(i - 1) * 2 + 1]);

                    DIR dir = vec.Dir();
                    int dx = (int)(vec.nX() * mov * C.TILE_SIZE);
                    int dy = (int)(vec.nY() * mov * C.TILE_SIZE);
                    int x = x1 + dx + xx;
                    int y = y1 + dy + yy;

                    int bi = (int)(mov * 22) + (random >> i * 2) & 0x0F;
                    bi %= bumpOff.Length;
                    bi &= ~1;

                    int cx = (int)(x) + bumpOff[bi];
                    int cy = (int)(y) + bumpOff[bi + 1];
                    if (i == 2)
                    {
                        int ax = (int)(x + dir.xN() * C.TILE_SIZE);
                        int ay = (int)(y + dir.yN() * C.TILE_SIZE);
                        constructor().sprite.RenderBelow(r, s, rot, cx, cy, mov, ran, 0, res, resamount);
                        constructor().sprite.Render(r, s, rot, cx, cy, 0, mil);
                    }
                }
            }
        }

        public void RenderCart(SPRITE_RENDERER r, ShadowBatch s, int rot, int cx, int cy, int ran, RESOURCE res, double resamount, double mov)
        {
            constructor().sprite.RenderBelow(r, s, rot, cx, cy, mov, ran, 0, res, resamount);
            constructor().sprite.Render(r, s, rot, cx, cy, 0, mil);
        }

        protected override void RemoveAction()
        {
            if (amount > 0)
            {
                if (mil)
                {
                }
                else
                {
                    SETT.ROOMS().TRANSPORT.EndDelivery(startTx, startTy, res, amount, path.LengthTotal());
                    SETT.THINGS().resources.Create(body().cX() / C.TILE_SIZE, body().cY() / C.TILE_SIZE, res, amount);
                    SETT.ROOMS().STATION.ReserveCancel(res, path.destX(), path.destY());
                }
            }
        }

        protected override TransportFactory Constructor()
        {
            return SETT.HALFENTS().transports;
        }

        public override void HoverInfo(GBox box)
        {
        }
    }
}