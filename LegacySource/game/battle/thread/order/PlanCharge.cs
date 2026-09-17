using System;
using System.Collections.Generic;
using game.battle.formation;
using game.battle.thread.order;
using init.constant;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.data;

namespace game.battle.thread.order
{
    class PlanCharge : Plan
    {
        private readonly VectorImp vec = new VectorImp();
        private readonly INT_OE<PlanData> timer;
        private readonly INT_OE<PlanData> timer2;

        public PlanCharge(Tools tools, LISTE<Plan> all, Data data) : base(tools, all, data, DIVTASK.CHARGE)
        {
            timer = data.new DataInt();
            timer2 = data.new DataInt();
        }

        override void init()
        {
            wait.set();
        }

        private STATE wait = new STATE("wait")
        {
            override void update(int gameMillis)
            {
                if (t.div.fixIfNeeded(prev))
                {
                    nextPos = prev;
                }
                timer2.set(m, 0);
                timer.set(m, 0);
                if (inPosition() < (prev.deployed() - unreachable) / 2)
                    return;

                charge.set();
                return;
            }

            override bool setAction()
            {
                if (prev.deployed() > 0)
                {
                    dest.copy(prev);
                    order.dest.set(dest);
                }
                else if (current.deployed() > 0)
                {
                    int xx = 0;
                    int yy = 0;
                    for (int i = 0; i < current.deployed(); i++)
                    {
                        xx += current.tile(i).x();
                        yy += current.tile(i).y();
                    }

                    xx /= current.deployed();
                    yy /= current.deployed();
                    int min = int.MaxValue;
                    int f = -1;
                    for (int i = 0; i < current.deployed(); i++)
                    {
                        int d = Math.Abs(current.tile(i).x() - xx) + Math.Abs(current.tile(i).y() - yy);
                        if (d < min)
                        {
                            min = d;
                            f = i;
                        }
                    }

                    if (f == -1)
                    {
                        task.stop(div);
                        order.task.set(task);
                        return false;
                    }

                    DivFormationImp d = t.deployer.deployCentre(div.info, current.deployed(), div.settings().formation, current.pixel(f).x(), current.pixel(f).y(), 1.0, 0.0, 5, a);
                    if (d == null)
                    {
                        task.stop(div);
                        order.task.set(task);
                        return false;
                    }
                    dest.copy(d);
                }

                return false;
            }
        };

        private STATE charge = new STATE("charge")
        {
            override void update(int gameMillis)
            {
                if (div.status().engagements() > 0)
                {
                    timer.inc(m, gameMillis);
                    if (timer.get(m) > 2000)
                    {
                        stop();
                        return;
                    }
                }
                else
                {
                    timer.set(m, 0);
                }

                timer2.inc(m, gameMillis);

                if (timer2.get(m) > 15000 && !div.army().player())
                {
                    stop();
                    return;
                }

                if (inPosition() == 0)
                    return;

                vec.set(prev.dx(), prev.dy());
                vec.rotate90().rotate90().rotate90();
                int sx = (int)(prev.start().x() + vec.nX() * C.TILE_SIZE);
                int sy = (int)(prev.start().y() + vec.nY() * C.TILE_SIZE);

                if (!SETT.PIXEL_IN_BOUNDS(sx, sy) || !SETT.PIXEL_IN_BOUNDS((int)(sx + prev.dx() * prev.width()), (int)(sy + prev.dy() * prev.width())))
                {
                    task.stop(div);
                    order.task.set(task);
                    return;
                }

                vec.set(prev.dx(), prev.dy());

                DivFormationImp f = t.deployer.deploy(div.info, men, div.settings().formation, sx, sy, prev.dx(), prev.dy(), prev.width(), a);
                if (f != null && f.deployed() > 0)
                {
                    prev.copy(t.mover.getFromMovedIntoTo(prev, f));
                    nextPos = prev;
                    return;
                }

                double largestGap = -1;
                int largestI1 = -1;

                int size = div.settings().formation.size(div);

                for (int d = 0; d <= prev.width(); d += size)
                {
                    int x1 = (int)(sx + d * vec.nX()) + size / 2;
                    int y1 = (int)(sy + d * vec.nY()) + size / 2;

                    if (!DivPlacability.pixelIsBlocked(x1, y1, size, a))
                    {
                        int am = 1;
                        int di = d;
                        for (; d <= prev.width(); d += size)
                        {
                            int x = (int)(sx + d * vec.nX());
                            int y = (int)(sy + d * vec.nY());
                            if (!DivPlacability.pixelIsBlocked(x, y, size, a))
                            {
                                am++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (am > largestGap)
                        {
                            largestGap = am;
                            largestI1 = di;
                        }
                    }
                }

                if (largestI1 == -1)
                {
                    stop();
                    return;
                }
                int cx = (int)(sx + (largestI1) * vec.nX());
                int cy = (int)(sy + (largestI1) * vec.nY());

                f = t.deployer.deploy(div.info, men, div.settings().formation, cx, cy, prev.dx(), prev.dy(), (int)(largestGap * div.settings().formation.size(div)), a);

                if (f != null && f.deployed() > 0)
                {
                    prev.copy(t.mover.getFromMovedIntoTo(prev, f));
                }
                else
                {
                    stop();
                    return;
                }
            }

            override bool setAction()
            {
                return false;
            }
        };

        private void stop()
        {
            task.stop(div);
            order.task.set(task);
        }

        private int inPosition()
        {
            int am = 0;
            for (int i = 0; i < current.deployed() && i < prev.deployed(); i++)
            {
                if (current.pixel(i).tileDistanceTo(prev.pixel(i)) < C.TILE_SIZE * 3)
                    am++;
            }

            return am;
        }

        override void update(int gamemillis)
        {
            charging = true;
            state(m).update(gamemillis);
        }

        override bool continueWhenFighting()
        {
            return true;
        }
    }
}