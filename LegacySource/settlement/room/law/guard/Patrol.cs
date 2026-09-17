using System;
using System.IO;

namespace Settlement.Room.Law.Guard
{
    public sealed class Patrol : SAVABLE
    {
        private const int WIDTH = 1;
        private const int DEPTH = 8;
        public const int MAX = (1 + WIDTH * 2) * DEPTH;
        private const int MAX_TILES = 120;

        public static double Speed = 0.5;

        private readonly PathSimple path = new PathSimple(MAX_TILES + 16);
        private double waitAtDest = RND.rFloat() * TIME.SecondsPerHour() * 3;
        private double progressTile = 0;
        private short[] txs = new short[DEPTH];
        private short[] tys = new short[DEPTH];
        private byte[] dirs = new byte[DEPTH];

        private Coo tmp = new Coo();

        public Patrol()
        {
        }

        public override void Save(FilePutter file)
        {
            file.D(waitAtDest);
            file.D(progressTile);
            file.SsE(txs);
            file.SsE(tys);
            file.BsE(dirs);
        }

        public override void Load(FileGetter file)
        {
            waitAtDest = file.D();
            progressTile = file.D();
            file.SsE(txs);
            file.SsE(tys);
            file.BsE(dirs);
        }

        public override void Clear()
        {
            waitAtDest = 0;
            progressTile = 0;
        }

        public void Update(double ds)
        {
            if (!path.HasNext())
            {
                waitAtDest -= ds;
                if (waitAtDest < 0)
                {
                    Find();

                    waitAtDest = TIME.SecondsPerHour() * (3 + RND.rFloat() * 3);
                }
            }
            else
            {
                progressTile += ds * Speed;

                if (progressTile > 1.0)
                {
                    progressTile -= 1.0;
                    SetNext();
                }
            }
        }

        private void SetNext()
        {
            if (!path.HasNext())
                return;
            path.SetNext();

            for (int i = DEPTH - 1; i > 0; i--)
            {
                txs[i] = txs[i - 1];
                tys[i] = tys[i - 1];
                dirs[i] = dirs[i - 1];
            }

            txs[0] = (short)path.X();
            tys[0] = (short)path.Y();

            int x = path.X();
            int y = path.Y();
            if (path.HasNext())
            {
                path.SetNext();
                DIR dirNext = DIR.Get(x, y, path.X(), path.Y());
                dirs[0] = (byte)dirNext.Id;
                path.SetPrev();
            }
        }

        private void Find()
        {
            int sx = path.X();
            int sy = path.Y();

            if (path.Length() == 0 || !SETT.PATH().Connectivity.Is(path.X(), path.Y()))
            {
                if (SETT.ROOMS().GUARD.InstancesSize() > 0)
                {
                    RoomInstance ins = SETT.ROOMS().GUARD.GetInstance(RND.rInt(SETT.ROOMS().GUARD.InstancesSize()));
                    sx = ins.MX();
                    sy = ins.MY();
                }
                else
                {
                    sx = THRONE.Coo().X;
                    sy = THRONE.Coo().Y;
                    DIR d = DIR.ORTHO.Rnd();
                    sx += d.X;
                    sy += d.Y;
                }
            }

            DIR dirDir = DIR.Get(THRONE.Coo().X, THRONE.Coo().Y, sx, sy);

            Flooder f = GUTIL.Flooder();
            f.Init(this);

            f.PushSloppy(sx, sy, 0);
            f.SetValue2(sx, sy, 0);

            double destDist = MAX_TILES + RND.rInt(MAX_TILES);

            while (f.HasMore())
            {
                PathTile t = f.PollSmallest();

                if (t.GetValue2() > destDist)
                {
                    GUTIL.Coos().Set(0);
                    GUTIL.Coos().Get().Set(t);

                    while (f.HasMore())
                    {
                        t = f.PollSmallest();
                        if (t.GetValue2() > destDist)
                        {
                            GUTIL.Coos().Inc();
                            GUTIL.Coos().Get().Set(t);
                        }
                    }
                    t = GUTIL.PathTools().GetTile(GUTIL.Coos().Get().X, GUTIL.Coos().Get().Y);

                    path.Set(t);

                    for (int i = 0; i < DEPTH; i++)
                        SetNext();
                    f.Done();
                    return;
                }

                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR d = DIR.ALL.Get(di);
                    int dx = t.X + d.X;
                    int dy = t.Y + d.Y;

                    if (!SETT.IN_BOUNDS(dx, dy))
                        continue;

                    double a = SETT.PATH().Coster.Player.GetCost(t.X, t.Y, dx, dy);
                    if (a < 0)
                        continue;

                    if (SETT.ENV().Map.URBAN.Get(dx, dy) == 0)
                        a *= 5;

                    a *= 10 - 9 * SETT.ENV().Map.SPACE.Get(dx, dy);
                    if (SETT.FLOOR().Getter.Get(dx, dy) == null)
                        a *= 4;
                    if (SETT.ROOMS().Map.Is(dx, dy))
                    {
                        a *= 4;
                    }

                    double dot = d.XN * dirDir.XN + d.YN * dirDir.YN;
                    a += 2 + dot;

                    a *= d.TileDistance();

                    if (f.PushSmaller(dx, dy, t.GetValue + a, t) != null)
                    {
                        f.SetValue2(dx, dy, t.GetValue2 + d.TileDistance());
                    }
                }
            }

            f.Done();
            return;
        }

        public Coo Pos(int pos)
        {
            int side = pos / DEPTH;
            pos = pos % DEPTH;
            DIR d = Dir(pos);

            double tx = txs[pos] + d.X * progressTile;
            double ty = tys[pos] + d.Y * progressTile;

            if (side != 0)
            {
                int depth = (int)Math.Ceiling(side / 2.0);
                d = d.Next(2 + (side % 2) * 4);
                tx += d.XN * (depth);
                ty += d.YN * (depth);
            }
            tmp.Set(tx * C.TILE_SIZE + C.TILE_SIZEH, ty * C.TILE_SIZE + C.TILE_SIZEH);
            return tmp;
        }

        public DIR Dir(int pos)
        {
            pos = pos % DEPTH;
            return DIR.ALL.Get(dirs[pos]);
        }

        public int Posses()
        {
            return MAX;
        }
    }
}