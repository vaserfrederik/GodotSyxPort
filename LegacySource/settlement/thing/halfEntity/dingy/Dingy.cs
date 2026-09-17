using System;
using System.IO;

namespace Settlement.Thing.HalfEntity.Dingy
{
    public class Dingy : HalfEntity
    {
        private static VectorImp vec = new VectorImp();
        private const double SPEED = 2.0;

        private int hi = -1;
        private byte up = 0;
        private byte pointI;
        private byte pointM;
        private byte rCatch;
        private double mov;

        private readonly WayPoint[] points = new WayPoint[4];

        private static readonly int[] bumpOff = Alloc.Ii(128);

        static Dingy()
        {
            for (int i = 0; i < bumpOff.Length; i += 2)
            {
                bumpOff[i] = (int)(RND.RSign() * RND.RFloat() * 2);
                bumpOff[i + 1] = (int)(RND.RSign() * RND.RFloat() * 2);
            }
        }

        public Dingy() : base(C.TILE_SIZE * 2, C.TILE_SIZE * 2)
        {
            for (int i = 0; i < points.Length; i++)
                points[i] = new WayPoint();
        }

        protected override void Save(FilePutter file)
        {
            file.I(hi);
            file.B(pointI);
            file.B(pointM);
            RESOURCES.Map().Saver().Save(Catc(), file);
            file.B(rCatch);
            file.D(mov);
            file.B(up);
            foreach (WayPoint p in points)
                p.Save(file);
        }

        protected override HalfEntity Load(FileGetter file)
        {
            hi = file.I();
            pointI = file.B();
            pointM = file.B();
            rCatch = RESOURCES.Map().Loader().LoadB(file, null).BIndex();
            file.B();
            mov = file.D();
            up = file.B();
            foreach (WayPoint p in points)
                p.Load(file);
            return this;
        }

        public Humanoid Host()
        {
            ENTITY e = SETT.ENTITIES().GetByID(hi);
            if (e != null && e is Humanoid)
                return (Humanoid)e;
            return null;
        }

        private RESOURCE Catc()
        {
            return RESOURCES.ALL().Get(rCatch);
        }

        public bool Init(Humanoid h, int tx, int ty, RESOURCE cat, int upgrade, DIR dir)
        {
            hi = h.Id();
            mov = 0;
            RoomInstance ins = SETT.ROOMS().Map.Instance.Get(tx, ty);
            if (!SetWaypoint(tx + 0.5, ty + 0.5, 0, ins, dir.Id()))
                return false;
            pointI = 0;
            pointM = 1;
            up = (byte)(upgrade & 1);
            Body().MoveC(points[0].tx * C.TILE_SIZE, points[0].ty * C.TILE_SIZE);

            for (int i = 1; i < points.Length; i++)
            {
                WayPoint prev = points[i - 1];
                double dx = prev.tx + prev.dx * prev.distance;
                double dy = prev.ty + prev.dy * prev.distance;
                if (SetWaypoint(dx, dy, i, ins, prev.di))
                    pointM++;
                else
                    break;
            }
            rCatch = cat.BIndex();
            Add();
            return true;
        }

        private bool SetWaypoint(double tx, double ty, int pi, RoomInstance ins, int ri)
        {
            WayPoint p = points[pi];

            p.tx = (float)tx;
            p.ty = (float)ty;

            int mLength = 16 + RND.RInt(32);

            ri += RND.RInt0(1);

            for (int di = 0; di < DIR.ALL.Size; di++)
            {
                DIR d = DIR.ALL.GetC(ri + di);

                if (Passable(tx, ty, tx + d.XN(), ty + d.YN(), ins))
                {
                    vec.Set(d.XN(), d.YN());
                    p.dx = (float)vec.NX();
                    p.dy = (float)vec.NY();
                    p.di = (byte)d.Id();
                    p.distance = 0;
                    for (int i = 0; i < mLength; i++)
                    {
                        double nx = tx + p.dx;
                        double ny = ty + p.dy;
                        if (Passable(tx, ty, nx, ny, ins))
                        {
                            p.distance++;
                            tx += p.dx;
                            ty += p.dy;
                        }
                        else if (i > 5)
                        {
                            return true;
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        private bool Passable(double fromX, double fromY, double toX, double toY, RoomInstance ins)
        {
            int tx = (int)toX;
            int ty = (int)toY;
            if (!SETT.IN_BOUNDS(tx, ty))
                return false;
            if (ins != null && ins.Is(tx, ty))
            {
                if (!SETT.TERRAIN().WATER.Is.Is(tx, ty))
                    return false;
            }
            else if (!SETT.TERRAIN().WATER.Is.Is(tx, ty))
            {
                return false;
            }
            return true;
        }

        protected override void Update(float ds)
        {
            base.Update(ds);
            // TODO: Implement update logic
        }

        protected override void Render(Renderer r, ShadowBatch s, float ds, int x, int y)
        {
            // TODO: Implement rendering logic
        }

        protected override void RemoveAction()
        {
            if (Host() != null)
            {
                double time = 0;
                for (int i = 0; i < pointM; i++)
                {
                    time += points[i].distance;
                }
                time /= SPEED;
                time *= 2;
                time += 20;
                HEvent.Handler.FishingTripOver(Host(), time);
            }
        }

        protected override DingyFactory Constructor()
        {
            return SETT.HALFENTS().Dingy;
        }

        public override void HoverInfo(GBox box)
        {
            // TODO: Implement hover info logic
        }

        private class WayPoint : SAVABLE
        {
            public byte di;
            public float tx, ty;
            public float dx, dy;
            public float distance;

            public WayPoint()
            {
            }

            public void Save(FilePutter file)
            {
                file.B(di);
                file.F(tx);
                file.F(ty);
                file.F(dx);
                file.F(dy);
                file.F(distance);
            }

            public void Load(FileGetter file)
            {
                di = file.B();
                tx = file.F();
                ty = file.F();
                dx = file.F();
                dy = file.F();
                distance = file.F();
            }

            public void Clear()
            {
                // TODO: Implement clear logic
            }
        }
    }
}