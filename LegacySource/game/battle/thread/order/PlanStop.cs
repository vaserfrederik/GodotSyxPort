using System;
using System.Collections.Generic;
using System.Linq;
using game.battle.formation;
using game.battle.thread.order;
using game.battle.thread.status;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;

namespace game.battle.thread.order
{
    internal sealed class PlanStop : Plan
    {
        private readonly VectorImp vec = new VectorImp();

        private readonly INT_OE<PlanData> timer;
        private readonly INT_OE<PlanData> timer2;

        public PlanStop(Tools tools, LISTE<Plan> all, Data data) : base(tools, all, data, DIVTASK.STOP)
        {
            timer = data.newDataInt();
            timer2 = data.newDataShort();
        }

        protected override void Init()
        {
            wait.Set();

            order.path.set(path);

            if (current.deployed() == 0)
                return;
            if (!div.active())
                return;

            DivFormationImp form = null;

            if (Plan.prev.deployed() == men && prev.isCoherent() && t.deployer.isValid(div.info, Plan.prev, a) && t.div.inPosition(current, prev, 1.5 * C.TILE_SIZE) > men / 2)
            {
                form = prev;
            }
            else if (Plan.dest.deployed() == men && t.deployer.isValid(div.info, Plan.dest, a) && t.div.inPosition(current, dest, 1.5 * C.TILE_SIZE) > men / 2)
            {
                form = dest;
            }
            else
            {
                DivPosition p = current;

                int cx = 0;
                int cy = 0;
                int ci = 0;

                for (int i = 0; i < p.deployed(); i++)
                {
                    if (!DivPlacability.pixelIsBlocked(p.px(i), p.px(i), C.TILE_SIZE, a))
                    {
                        if (div.reporter.reachable(i))
                        {
                            cx += p.px(i);
                            cy += p.py(i);
                            ci++;
                        }
                    }
                }

                if (ci == 0)
                {
                    for (int i = 0; i < p.deployed(); i++)
                    {
                        if (!DivPlacability.pixelIsBlocked(p.px(i), p.px(i), C.TILE_SIZE, a))
                        {
                            cx += p.px(i);
                            cy += p.py(i);
                            ci++;
                        }
                    }
                }

                if (ci == 0)
                {
                    return;
                }

                cx /= ci;
                cy /= ci;

                double dist = double.MaxValue;
                int bi = -1;

                for (int i = 0; i < p.deployed(); i++)
                {
                    if (DivPlacability.pixelIsBlocked(p.px(i), p.px(i), C.TILE_SIZE, a))
                    {
                        continue;
                    }

                    double d = COORDINATE.tileDistance(cx, cy, p.px(i), p.py(i));
                    if (!div.reporter.reachable(i))
                        d += 100000;

                    if (d < dist)
                    {
                        dist = d;
                        bi = i;
                    }
                }

                if (bi == -1)
                {
                    return;
                }

                int size = div.settings().formation.size(div);
                int width = (int)(Math.Sqrt(Plan.men)) * size;

                if (prev.width() / size > 0)
                {
                    if (men / (prev.width() / size) > 2)
                    {
                        width = prev.width();
                    }
                }

                double dx = prev.dx();
                double dy = prev.dy();
                if (dx == 0 && dy == 0)
                    dx = 1;

                //width /= size;

                form = t.deployer.deployArroundCentre(div.info, men, prev.formation(), cx, cy, dx, dy, width, a);

                if (form == null)
                {
                    LOG.err("nay1 " + cx / C.TILE_SIZE + " " + cy / C.TILE_SIZE + " " + current.deployed());
                }
                else
                {
                }
            }

            if (form != null)
            {
                form = t.mover.getFromMovedIntoTo(current, form);
                dest.copy(form);
                order.dest.set(dest);
                nextPos = dest;
            }
        }

        protected override bool ContinueWhenFighting()
        {
            return true;
        }

        public override void Update(int gamemillis)
        {
            state(m).update(gamemillis);
        }

        private STATE wait = new STATE("wait")
        {
            protected override void update(int gameMillis)
            {
                shouldBreak = true;
                timer.inc(m, gameMillis);
                timer2.inc(m, gameMillis);
                if (timer.get(m) < 1000)
                    return;
                timer.set(m, 0);
                nextPos = prev;

                if (t.div.fixIfNeeded(dest))
                {
                    order.dest.set(dest);
                    return;
                }

                int ddx = prev.start().x() - dest.start().x();
                int ddy = prev.start().y() - dest.start().y();
                double engagement = engagement(dest, ddx, ddy);

                if (engagement > 0)
                {
                    timer2.set(m, 0);

                    unfuckPrev();

                    if (!div.settings().guard)
                    {
                        if (engagement > currentEngagement)
                        {
                            nextPos = stepForward(currentEngagement);
                        }
                        else
                        {
                            nextPos = stepBack(currentEngagement);
                        }
                    }
                    else
                    {
                        if (engagement > currentEngagement)
                        {
                            nextPos = stepForward(currentEngagement);
                        }
                        else
                        {
                            nextPos = stepBack(currentEngagement);
                        }
                    }
                }
                else
                {
                    nextPos = stepForward(currentEngagement);
                }
            }

            protected override bool Set()
            {
                shouldBreak = false;
                return true;
            }
        };

        private void tryAdvance()
        {
            // Add your logic for advancing the plan here
        }

        private void tryRetreat()
        {
            // Add your logic for retreating here
        }

        private DivFormationImp stepForward(double currentEngagement)
        {
            vec.set(dest.dx(), dest.dy());
            vec.rotate90();
            vec.rotate90();
            vec.rotate90();
            for (int i = 1; i < 8; i++)
            {
                int dx = (int)(vec.nX() * 8 * i);
                int dy = (int)(vec.nY() * 8 * i);
                if (!t.deployer.canDeploy(dest.start().x() + dx, dest.start().y() + dy, dest.dx(), dest.dy(), dest.width(), dest.formation().size(div), a, div.race()))
                    break;
                if (engagement(dest, dx, dy) > currentEngagement)
                {
                    nextPos = t.deployer.move(div.info, dest, dx, dy, a);
                    return nextPos;
                }
            }

            return null;
        }

        private DivFormationImp stepBack(double currentEngagement)
        {
            vec.set(dest.dx(), dest.dy());
            vec.rotate90();

            for (int i = 1; i < 8; i++)
            {
                int dx = (int)(vec.nX() * 8 * i);
                int dy = (int)(vec.nY() * 8 * i);
                if (!t.deployer.canDeploy(prev.start().x() + dx, prev.start().y() + dy, dest.dx(), dest.dy(), dest.width(), dest.formation().size(div), a, div.race()))
                    break;
                if (engagement(prev, dx, dy) < currentEngagement)
                {
                    nextPos = t.deployer.move(div.info, prev, dx, dy, a);
                    //DivFormationImp f = t.mover.getFromMovedIntoTo(current, nextPos);
                    return nextPos;
                }
            }

            return null;
        }

        double engagement(DivFormationImp f, double dx, double dy)
        {
            double enemies = 0;

            if (f.deployed() == 0)
                return 0;

            for (int i = 0; i < f.deployed(); i++)
            {
                int x = (int)(f.px(i) + dx);
                int y = (int)(f.py(i) + dy);
                enemies += BattleStatus.map().soldiers(Plan.div.armyEnemy()).get(x >> C.T_SCROLL, y >> C.T_SCROLL);
            }

            return enemies / (f.width() / f.formation().size(Plan.div));
        }
    }
}