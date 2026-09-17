using System;
using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data.INT_O;

abstract class PlanWalkAbs : Plan
{
    private readonly INT_OE<PlanData> inPosition;
    private readonly INT_OE<PlanData> timer;
    internal readonly INT_OE<PlanData> colTimer;
    private readonly INT_OE<PlanData> tilesDestCheck;
    private readonly INT_OE<PlanData> destId;
    static int amountOfPaths = 0;

    public PlanWalkAbs(Tools tools, LISTE<Plan> all, Data data, DIVTASK task) : base(tools, all, data, task)
    {
        inPosition = data.newDataByte();
        timer = data.newDataInt();
        colTimer = data.newDataShort();
        tilesDestCheck = data.newDataByte();
        destId = data.newDataNibble();
    }

    void SetWalkToDest()
    {
        path.Clear();
        order.path.Set(path);

        wait.Set();
    }

    private bool CheckNextDest()
    {
        int di = order.dest.SetI() & 0x0F;

        if (destId.Get(m) != di)
        {
            destId.Set(m, di);
            return true;
        }

        if (t.div.NeedsFixing(dest, men, a, div.Settings().Formation))
        {
            int w = dest.Width();
            int destX = dest.Start().X();
            int destY = dest.Start().Y();

            if (t.deployer.FixFormation(div.Info, dest, div.Settings().Formation, men, a))
            {
                order.dest.Set(dest);
                di = order.dest.SetI() & 0x0F;
                destId.Set(m, di);
                if (!dest.Start().IsSameAs(destX, destY) || Math.Abs(w - dest.Width()) > dest.Formation().Size(div))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private readonly STATE wait = new STATE("wait")
    {
        public override void Update(int gameMillis)
        {
            if (!div.Active() || men <= 0)
                return;

            if (amountOfPaths > 1)
                return;
            amountOfPaths++;
            destId.Set(m, order.dest.SetI() & 0x0F);
            setStart.Set();
        }

        public override bool SetAction()
        {
            return true;
        }
    };

    private readonly STATE setStart = new STATE("setStart")
    {
        public override bool SetAction()
        {
            if (!t.walk.SetStart(ToolsWalk.destMoveStart))
                return moveIntoDest.Set();

            if (t.div.IntersectsSomewhat(prev, dest))
                return moveIntoDest.Set();

            timer.Set(m, 0);
            inPosition.Set(m, t.walk.CountPosition());
            return true;
        }

        public override void Update(int gamemillis)
        {
            if (men == 0)
                return;

            if (CheckNextDest())
            {
                wait.Set();
                return;
            }

            if (prev.Deployed() == 0)
            {
                task.Stop(div);
                div.Order().Task.Set(task);
                return;
            }

            if (prev.Deployed() > 0 && t.div.FixIfNeeded(prev))
                nextPos = prev;

            int pos = t.walk.CountPosition();
            if (pos > 0)
            {
                if (pos == 1)
                    timer.Set(m, 0);

                if (pos >= men - unreachable || Running())
                {
                    followPath.Set();
                    return;
                }

                timer.Inc(m, gamemillis);
                if (pos > inPosition.Get(m))
                    timer.Set(m, 0);
                inPosition.Set(m, pos);

                if (Running() || t.div.IsCloseToFighting() || timer.Get(m) >= 1500)
                {
                    followPath.Set();
                    return;
                }
            }
            else
            {
                timer.Inc(m, gamemillis);
                if (timer.Get(m) > 1000)
                {
                    SetAction();
                    return;
                }
            }

            if (path.IsDest())
                return;

            if (path.CurrentI() < path.Length() - 1)
            {
                COORDINATE cc = t.div.CurrentCentre();
                double d1 = cc.TileDistanceTo(path.X(), path.Y());
                path.CurrentIInc(1);
                double d2 = cc.TileDistanceTo(path.X(), path.Y());
                path.CurrentIInc(-1);

                if (d1 <= d2 + 3)
                    return;
                t.walk.SetNextPosition(ToolsWalk.destMoveStart, gamemillis);
                timer.Set(m, 0);
            }
        }
    };

    private readonly STATE followPath = new STATE("follow path")
    {
        public override bool SetAction()
        {
            tilesDestCheck.Set(m, ToolsWalk.destMoveStart - ToolsWalk.destMoveResume);
            inPosition.Set(m, t.walk.CountPosition());
            timer.Set(m, 0);
            colTimer.Set(m, 0);
            return true;
        }

        public override void Update(int gameMillis)
        {
            double sp = speed(gameMillis);

            if (sp == 0)
                return;
            if (path.IsDest())
            {
                resume();
                return;
            }
            order.path.Get(path);
            tilesDestCheck.Inc(m, -1);
            int pi = path.CurrentI();
            if (!t.walk.SetNextPosition(ToolsWalk.destMoveResume + tilesDestCheck.Get(m), (int)Math.Ceiling(gameMillis * sp)))
            {
                init();
                return;
            }
            if (pi != path.CurrentI())
            {
                if (CheckNextDest())
                    wait.Set();
            }

            // double dist = t.div.DistanceMaxFromCurrentToNext(tmp, next);
            // timer.Set(m, 0);
            // double ma = C.TILE_SIZE + C.TILE_SIZEH;
            // if (dist > ma) {
            // timer.inc(m, (int) (1000.0*(dist-ma)/ma));
            // }
        }

        private double speed(int gameMillis)
        {
            int in = t.walk.CountPosition();

            if (in == 0)
            {
                colTimer.Inc(m, gameMillis);
                if (colTimer.Get(m) > 3000)
                {
                    setStart.Set();
                }
                else if (path.CurrentI() < path.Length() - 1)
                {
                    COORDINATE cc = t.div.CurrentCentre();
                    double d1 = cc.TileDistanceTo(path.X(), path.Y());
                    path.CurrentIInc(1);
                    double d2 = cc.TileDistanceTo(path.X(), path.Y());
                    path.CurrentIInc(-1);

                    if (d1 > d2 + 3)
                    {
                        if (!t.walk.SetNextPosition(ToolsWalk.destMoveResume + tilesDestCheck.Get(m), gameMillis))
                        {
                            init();
                            return 0;
                        }

                        timer.Set(m, 0);
                        colTimer.Set(m, 0);
                        return 0;
                    }
                }

                return 0;
            }
            colTimer.Set(m, 0);

            if (Running())
                return 1.0;

            in += unreachable;

            if (in >= prev.Deployed())
                return 1.0;

            double d = (men - in) / (men);
            return CLAMP.d(0.25 + d, 0, 1);
        }

        void resume()
        {
            if (path.IsDest() && path.IsComplete())
            {
                moveIntoDest.Set();
                return;
            }
            if (!t.walk.SetStart(ToolsWalk.destMoveStart))
            {
                init();
                return;
            }
            if (t.div.IntersectsSomewhat(prev, dest))
                moveIntoDest.Set();
            else
                inPosition.Set(m, CLAMP.i(t.walk.CountPosition(), 0, men));
            Update(0);
        }
    };

    private readonly STATE moveIntoDest = new STATE("move into dest")
    {
        public override bool SetAction()
        {
            nextPos = dest;

            return true;
        }

        public override void Update(int gamemillis)
        {
            if (CheckNextDest())
            {
                wait.Set();
                return;
            }
            if (wait(m, gamemillis))
                return;
            if (!t.mover.Merge(prev, dest))
            {
                stayInDest.Set();
                return;
            }

            nextPos = prev;
        }

        private bool wait(PlanData m, int gamemillis)
        {
            timer.Inc(m, -gamemillis);
            int in = t.walk.CountPosition();

            if (in == 0)
                return true;

            if (in < inPosition.Get(m))
            {
                timer.Inc(m, -gamemillis);
                if (timer.Get(m) <= 0)
                {
                    inPosition.Inc(m, -1);
                    timer.Set(m, 100);
                }
                return true;
            }
            else
            {
                inPosition.Set(m, in);
            }
            return false;
        }
    };

    private readonly STATE stayInDest = new STATE("stay in dest")
    {
        public override bool SetAction()
        {
            nextPos = dest;

            return true;
        }

        public override void Update(int gamemillis)
        {
            if (CheckNextDest())
            {
                wait.Set();
                return;
            }

            if (!t.walk.HasReachedPrev())
                return;

            finished();
        }
    };

    abstract void finished();
}