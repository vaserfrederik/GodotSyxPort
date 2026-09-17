using System;
using System.Collections.Generic;

namespace game.battle.thread.trajectory
{
    class UpdaterArtillery
    {
        private readonly List<Div> res = new List<Div>(32);
        private readonly CircleCooIterator cIt = new CircleCooIterator(4, GUTIL.flooder());
        private readonly ListResize<ArtilleryInstance> threadSafe = new ListResize<ArtilleryInstance>(256);

        void update()
        {
            threadSafe.ClearSoft();
            for (int bi = 0; bi < SETT.ROOMS().ARTILLERY.size(); bi++)
            {
                ROOM_ARTILLERY b = SETT.ROOMS().ARTILLERY.get(bi);

                b.threadInstances(threadSafe);
            }
            for (int i = 0; i < threadSafe.size(); i++)
            {
                ArtilleryInstance ins = threadSafe.get(i);

                if (!ins.mustered())
                    continue;
                if (!setCurrentTarget(ins))
                    setTarget(ins);
            }
        }

        bool setCurrentTarget(ArtilleryInstance ins)
        {
            COORDINATE tar = ins.targetCooGet();
            if (tar != null)
            {
                int x = tar.x();
                int y = tar.y();
                if (ins.testTarget(x, y, traj, false) == null)
                {
                    if (ins.bombarding())
                    {
                        ins.setTrajectory(traj);
                        return true;
                    }
                    int tx = x >> C.T_SCROLL;
                    int ty = y >> C.T_SCROLL;
                    if (!GAME.ARMIES().map.attackable.is(tx, ty, ins.army()))
                    {
                        int i = 0;
                        while (cIt.radius(i) <= 3)
                        {
                            int dx = tx + cIt.get(i).x();
                            int dy = ty + cIt.get(i).y();
                            if (GAME.ARMIES().map.attackable.is(dx, dy, ins.army()))
                            {
                                dx = dx << C.T_SCROLL + C.TILE_SIZEH;
                                dy = dy << C.T_SCROLL + C.TILE_SIZEH;
                                if (ins.testTarget(dx, dy, traj, false) == null)
                                {
                                    ins.targetCooSet(dx, dy, false, ins.targetIsUserSet());
                                    ins.setTrajectory(traj);
                                    return true;
                                }
                            }
                            i++;
                        }
                    }
                    else
                    {
                        ins.setTrajectory(traj);
                        return true;
                    }
                }
                ins.setTrajectory(null);
                if (ins.targetIsUserSet())
                {
                    return true;
                }
                ins.clearTarget();
                return false;
            }
            Div ddd = ins.targetDivGet();
            if (ddd != null)
            {
                if (!ddd.active())
                {
                    ins.clearTarget();
                    ins.setTrajectory(null);
                    return false;
                }
                else
                {
                    if (ins.testTarget(ddd.centre().cUnitX(), ddd.centre().cUnitY(), traj, true) == null)
                    {
                        ins.setTrajectory(traj);
                        return true;
                    }

                    if (ins.targetIsUserSet())
                    {
                        return true;
                    }
                    ins.clearTarget();
                    return false;
                }
            }
            return false;
        }

        private readonly int[] dirs = new int[] {
            0, 1, -1
        };

        void setTarget(ArtilleryInstance ins)
        {
            if (!ins.fireAtWill())
            {
                ins.setTrajectory(null);
                return;
            }

            ox = -1;
            oy = -1;

            DIR dir = ins.dir();
            int dimension = DivsQuadMap.size * C.TILE_SIZE;
            int startX = ins.body().x1() * C.TILE_SIZE + ins.body().width() * C.TILE_SIZE / 2 + ins.dir().x();
            int startY = ins.body().y1() * C.TILE_SIZE + ins.body().height() * C.TILE_SIZE / 2 + ins.dir().y();

            double minRange = ins.rangeMin() / C.SQR2;
            double maxRange = ins.rangeMax();
            double range = maxRange - minRange;
            range = CLAMP.d(range, 0, SETT.PWIDTH);
            double current = 0;

            while (current < range)
            {
                double cx = startX + (minRange + current) * dir.x();
                double cy = startY + (minRange + current) * dir.y();

                if (cx >= SETT.PWIDTH || cy >= SETT.PHEIGHT)
                {
                    break;
                }

                double sideAmount = 0;
                while (sideAmount < current)
                {
                    for (int di : dirs)
                    {
                        int tx = (int)(cx + dir.next(di).x() * sideAmount);
                        int ty = (int)(cy + dir.next(di).y() * sideAmount);
                        if (tx < 0 || ty < 0 || tx >= SETT.PWIDTH || ty >= SETT.PHEIGHT)
                        {
                            continue;
                        }
                        tx = tx >> C.T_SCROLL;
                        ty = ty >> C.T_SCROLL;

                        if (trySet(ins, tx, ty))
                            return;

                    }
                    sideAmount += dimension;
                }

                current += dimension;
            }
        }

        void setTarget2(ArtilleryInstance ins)
        {
            if (!ins.fireAtWill())
            {
                ins.setTrajectory(null);
                return;
            }

            ox = -1;
            oy = -1;
            int dddd = DivsQuadMap.size * C.TILE_SIZE;
            int fx = ins.body().x1() * C.TILE_SIZE + ins.body().width() * C.TILE_SIZE / 2;
            int fy = ins.body().y1() * C.TILE_SIZE + ins.body().height() * C.TILE_SIZE / 2;
            fx += C.TILE_SIZE * ins.dir().x();
            fy += C.TILE_SIZE * ins.dir().y();

            double min = ins.rangeMin() / C.SQR2;
            double max = ins.rangeMax();
            double dist = max - min;
            dist = CLAMP.d(dist, 0, SETT.PWIDTH);

            double wStart = min + dddd - 1;
            int steps = (int)Math.Ceiling((dist) / (dddd));
            double dStep = (dist) / steps;

            while (steps > 0)
            {
                double sx = fx + ins.dir().x() * min;
                double sy = fy + ins.dir().y() * min;

                for (int s = 0; s < wStart; s += dddd)
                {
                    for (int di = -2; di < 5; di += 4)
                    {
                        DIR d = ins.dir().next(di);
                        int tx = ((int)(sx + s * d.x()) >> C.T_SCROLL);
                        int ty = ((int)(sy + s * d.y()) >> C.T_SCROLL);
                        if (!SETT.IN_BOUNDS(tx, ty))
                            continue;

                        if (trySet(ins, tx, ty))
                            return;
                    }

                }

                dist += DivsQuadMap.size * C.TILE_SIZE;
                wStart += dStep;
                steps--;
            }
        }

        private int ox, oy;
        private readonly Rec bounds = new Rec(C.TILE_SIZE * DivsQuadMap.size);
        private readonly Trajectory traj = new Trajectory();

        private bool trySet(ArtilleryInstance ins, int tx, int ty)
        {
            Army enemy = ins.army().enemy();
            int qx = tx / DivsQuadMap.size;
            int qy = ty / DivsQuadMap.size;
            if (ox == qx && oy == qy)
                return false;
            ox = qx;
            oy = qy;

            {
                int fx = ins.body().x1() * C.TILE_SIZE + ins.body().width() * C.TILE_SIZE / 2;
                int fy = ins.body().y1() * C.TILE_SIZE + ins.body().height() * C.TILE_SIZE / 2;
                res.Clear();
                BattleStatus.quads().getDivsInBounds(bounds, res);
                foreach (Div div in res)
                {
                    if (div.army() == enemy)
                    {
                        int dist = (div.centre().x() - fx) * (div.centre().x() - fx) + (div.centre().y() - fy) * (div.centre().y() - fy);
                        if (ins.testTarget(div.centre().x(), div.centre().y(), traj, false) == null)
                        {
                            ins.targetDivSet(div, false);
                            ins.setTrajectory(traj);
                            return true;
                        }
                    }
                }
            }

            bounds.moveX1Y1(qx * C.TILE_SIZE * DivsQuadMap.size, qy * C.TILE_SIZE * DivsQuadMap.size);

            if (BattleStatus.quads().ART.is(tx, ty, enemy))
            {
                for (int i = 0; i < threadSafe.size(); i++)
                {
                    ArtilleryInstance other = threadSafe.get(i);
                    if (other.army() != ins.army())
                    {
                        int fx = other.body().x1() * C.TILE_SIZE + other.body().width() * C.TILE_SIZE / 2;
                        int fy = other.body().y1() * C.TILE_SIZE + other.body().height() * C.TILE_SIZE / 2;
                        if (bounds.touches(fx, fy))
                        {
                            if (ins.testTarget(fx, fy, traj, false) == null)
                            {
                                ins.targetCooSet(fx, fy, false, false);
                                ins.setTrajectory(traj);
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}