using System;
using System.Collections.Generic;
using System.Linq;

namespace Settlement.Thing.Projectiles
{
    using GAME = Game.GAME;
    using Army = Game.Battle.Army;
    using BattleStatus = Game.Battle.Thread.Status.BattleStatus;
    using C = Init.Constant.C;
    using ENTITY = Settlement.Entity.ENTITY;
    using Humanoid = Settlement.Entity.Humanoid.Humanoid;
    using SETT = Settlement.Main.SETT;
    using STATS = Settlement.Stats.STATS;
    using Data = Settlement.Thing.Projectiles.PData.Data;
    using TerrainTile = Settlement.Tilemap.Terrain.TerrainTile;
    using GEO = Snake2D.Util.GEO;
    using DIR = Snake2D.Util.Datatypes.DIR;
    using Rec = Snake2D.Util.Datatypes.Rec;
    using VectorImp = Snake2D.Util.Datatypes.VectorImp;
    using CLAMP = Snake2D.Util.Misc.CLAMP;
    using RND = Snake2D.Util.Rnd.RND;

    internal sealed class Updater
    {
        private readonly SProjectiles p;
        private static readonly VectorImp vec = new VectorImp();
        private readonly double max = 1 / 64.0;

        public Updater(SProjectiles p)
        {
            this.p = p;
        }

        public void Update(int i, double dd)
        {
            float ds = (float)max;
            while (dd > 0)
            {
                dd -= max;
                Data d = p.data.data(i);

                d.DzSet(d.Dz() - Trajectory.G * ds);

                float z = d.Z() + d.Dz() * ds;
                if (z <= 0)
                {
                    ds *= d.Z() / (d.Z() - z);
                    z = d.Z() + d.Dz() * ds;
                }
                d.ZSet(z);

                double mag = d.DMagnitude();
                mag -= Trajectory.FRICTION * ds;
                if (mag < 0)
                    mag = 0;
                d.MagnitudeSet(mag);

                float x = (float)(d.X() + d.SpeedX() * ds);
                float y = (float)(d.Y() + d.SpeedY() * ds);

                if (d.Z() <= 0)
                {
                    if (Collide(i, d, x, y, p.data.type(i)))
                        return;
                    p.data.type(i).SoundHit().Rnd((int)x, (int)y, 1);
                    p.data.type(i).Impact(p.data.Ref(i), x, y, d.SpeedX(), d.SpeedY(), d.Dz());
                    p.data.Remove(i);

                    return;
                }
                if (Collide(i, d, x, y, p.data.type(i)))
                    return;

                if (!p.data.Move(i, x, y))
                    return;
            }
        }

        private readonly Rec rTile = new Rec(C.TILE_SIZE);

        private bool Collide(int e, Data d, float destX, float destY, Projectile type)
        {
            destY /= C.TILE_SIZE;
            destX /= C.TILE_SIZE;

            final double startX = d.X() / C.TILE_SIZE;
            final double startY = d.Y() / C.TILE_SIZE;

            if ((int)destX == (int)startX && (int)destY == (int)startY && d.Z() > 0)
                return false;

            double dx = destX - startX;
            double dy = destY - startY;
            double adx = Math.Abs(dx);
            double ady = Math.Abs(dy);
            double mag = 0;
            if (adx > ady)
            {
                mag = adx;
            }
            else
            {
                if (ady <= 0)
                    mag = 1;
                else
                    mag = ady;
            }

            dx /= mag;
            dy /= mag;

            double x = startX;
            double y = startY;

            while (mag > 0)
            {
                double dd = CLAMP.d(mag, 0, 1);
                double ox = x;
                double oy = y;
                x += dx * dd;
                y += dy * dd;
                mag -= 1;
                int tx = (int)x;
                int ty = (int)y;

                if (!SETT.IN_BOUNDS(tx, ty))
                {
                    p.data.Remove(e);
                    return true;
                }
                TerrainTile t = SETT.TERRAIN().Get(tx, ty);
                int min = t.HeightStart(tx, ty) * C.TILE_SIZE;
                int max = t.HeightEnd(tx, ty) * C.TILE_SIZE;

                bool tree = SETT.TERRAIN().TREES.IsTree(tx, ty);

                if (tree && (e & 1) == 0)
                {
                }
                else if (d.Z() <= 0 || (d.Z() > min && d.Z() < max))
                {
                    p.data.type(e).SoundHit().Rnd((int)(x * C.TILE_SIZE), (int)(y * C.TILE_SIZE), 1);
                    double mom = d.DMagnitude();
                    mom -= Trajectory.FRICTION * dd;
                    if (mom < 0)
                        mom = 0;
                    d.MagnitudeSet(mom);

                    double vz = d.Dz();
                    vz -= Trajectory.G * dd;
                    if (vz < 0)
                        vz = 0;
                    d.DzSet(vz);

                    p.data.type(i).Impact(p.data.Ref(i), x, y, d.SpeedX(), d.SpeedY(), d.Dz());
                    p.data.Remove(i);
                    return true;
                }

                int eh = t.HeightEnt(tx, ty) * C.TILE_SIZE;
                if (d.Z() >= eh && d.Z() <= eh + Trajectory.HIT_HEIGHT)
                {
                    if (BattleStatus.Map().HasAlly.Is(tx, ty, p.data.type(i).Ally))
                    {
                        return true;
                    }
                    if (BattleStatus.Map().HasEnemy.Is(tx, ty, p.data.type(i).Ally))
                        return true;
                    for (int di = 0; di < DIR.ORTHO.Size; di++)
                    {
                        DIR dir = DIR.ORTHO.Get(di);
                        if (BattleStatus.Map().HasAlly.Is(tx, ty, dir, p.data.type(i).Ally))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static string Test(Army ally, Trajectory traj, double height, double sx, double sy)
        {
            double time = traj.GetTime(height);
            float vz = (float)traj.Vz();
            double mag = vec.Set(traj.Vx(), traj.Vy());
            double length = Trajectory.GetLength(mag, time);

            length *= C.ITILE_SIZE;

            double ds = time / length;
            int imax = (int)Math.Ceiling(length);

            double x = sx;
            double y = sy;
            double z = height;

            for (int i = 0; i < imax; i++)
            {
                mag -= Trajectory.FRICTION * ds;
                if (mag < 0)
                    mag = 0;

                vz -= Trajectory.G * ds;

                x += vec.NX() * mag * ds;
                y += vec.NY() * mag * ds;
                z += vz * ds;

                float zz = (float)(z + vz * ds);
                if (zz <= 0)
                {
                    ds *= z / (vz - zz);
                }

                if (z <= 0)
                    return null;
                int tx = ((int)x) >> C.T_SCROLL;
                int ty = ((int)y) >> C.T_SCROLL;

                if (!SETT.IN_BOUNDS(tx, ty))
                {
                    return null;
                }
                TerrainTile t = SETT.TERRAIN().Get(tx, ty);
                int min = t.HeightStart(tx, ty) * C.TILE_SIZE;
                int max = t.HeightEnd(tx, ty) * C.TILE_SIZE;

                if (!SETT.TERRAIN().TREES.IsTree(tx, ty) && z > min && z < max)
                {
                    if (imax - i < 10)
                        return null;
                    return SProjectiles.¤¤TERRAIN;
                }
                int eh = t.HeightEnt(tx, ty) * C.TILE_SIZE;
                if (z >= eh && z <= eh + Trajectory.HIT_HEIGHT)
                {
                    if (BattleStatus.Map().HasAlly.Is(tx, ty, ally))
                    {
                        return SProjectiles.¤¤FRIENDLIES;
                    }
                    if (BattleStatus.Map().HasEnemy.Is(tx, ty, ally))
                        return null;
                    for (int di = 0; di < DIR.ORTHO.Size; di++)
                    {
                        DIR dir = DIR.ORTHO.Get(di);
                        if (BattleStatus.Map().HasAlly.Is(tx, ty, dir, ally))
                        {
                            return SProjectiles.¤¤FRIENDLIES;
                        }
                    }
                }
            }

            return null;
        }

        private bool Intesects(ENTITY e, double ox, double oy, double nx, double ny)
        {
            double x1 = ox;
            double y1 = oy;
            double x2 = nx;
            double y2 = ny;

            double w = e.Body().Width() * C.ITILE_SIZE;
            double x = e.Body().X1() * C.ITILE_SIZE;
            double y = e.Body().Y1() * C.ITILE_SIZE;
            if (GEO.Collides(x1, y1, x2, y2, x, y, x + w, y))
                return true;
            if (GEO.Collides(x1, y1, x2, y2, x + w, y, x + w, y + w))
                return true;
            if (GEO.Collides(x1, y1, x2, y2, x + w, y + w, x, y + w))
                return true;
            if (GEO.Collides(x1, y1, x2, y2, x, y + w, x, y))
                return true;
            return false;
        }
    }
}