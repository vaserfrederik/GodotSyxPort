using System;
using System.Collections.Generic;
using game.battle.div;
using game.battle.formation;
using game.battle.thread.order;
using game.battle.thread.trajectory;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data.INT_O;

namespace game.battle.thread.order
{
    internal sealed class PlanFireDiv : PlanWalkAbs
    {
        private readonly INT_OE<PlanData> pathI;
        private readonly INT_OE<PlanData> pathd;
        private readonly INT_OE<PlanData> timer;
        private readonly VectorImp vec = new VectorImp();

        public PlanFireDiv(Tools tools, LISTE<Plan> all, Data data) : base(tools, all, data, DIVTASK.ATTACK_RANGED)
        {
            pathI = data.new DataShort();
            timer = data.new DataShort();
            pathd = data.new DataBit();
        }

        protected override void Init()
        {
            if (CheckTarget())
            {
                if (!div.settings().shouldNotMoveToFire || BattleTrajectories.trajectories(div) > 0)
                    wait.Set();
                else
                {
                    SetDest();
                    SetWalkToDest();
                }
            }
            else
            {
                fail.Set();
            }
        }

        private bool CheckTarget()
        {
            if (div.settings().ammo() == null)
                return false;
            Div target = task.targetDiv();
            if (target == null || !target.active())
            {
                fail.Set();
                return false;
            }
            return true;
        }

        private void SetDest()
        {
            Div target = task.targetDiv();

            int sx = div.centre().cUnitX();
            int sy = div.centre().cUnitY();

            int dx = target.centre().cUnitX();
            int dy = target.centre().cUnitY();

            double nx = 1;
            double ny = 0;

            if (sx != dx || sy != dy)
            {
                vec.Set(sx, sy, dx, dy);
                vec.rotate90();
                nx = vec.nX();
                ny = vec.nY();
            }

            int w = (int)Math.Sqrt(men * 2);

            DivFormationImp f = t.deployer.deployCentre(div.info, men, div.settings().formation, dx, dy, nx, ny, w, a);

            if (f == null)
            {
                fail.Set();
                return;
            }

            dest.Copy(f);
            SetWalkToDest();
            pathd.Set(m, path.currentI() & 1);
        }

        protected override void Update(int gamemillis)
        {
            if (!CheckTarget())
            {
                fail.Set();
                return;
            }

            if (state(m) == wait)
            {
                if (!div.settings().shouldNotMoveToFire)
                    return;

                if (BattleTrajectories.trajectories(div) > 0)
                    return;

                timer.inc(m, gamemillis);

                if (!BattleTrajectories.hasPotential(div) && timer.get(m) > 1000)
                {
                    SetDest();
                    SetWalkToDest();
                }

                if (timer.get(m) > 5000)
                {
                    SetDest();
                    SetWalkToDest();
                }
                return;
            }

            if (div.status().engagements() > 0)
            {
                return;
            }

            if ((path.currentI() & 1) != pathd.get(m))
            {
                pathd.Set(m, path.currentI() & 1);
                pathI.inc(m, 1);

                if (pathI.get(m) > 5 && BattleTrajectories.hasPotential(div))
                {
                    dest.Copy(prev);
                    path.Clear();
                    order.dest.Set(dest);
                    order.path.Set(path);
                    wait.Set();
                    return;
                }

                int tres = 50;
                if (path.isComplete())
                {
                    tres = CLAMP.i(path.length() - path.currentI(), 1, 50);
                }

                if (pathI.get(m) > tres)
                {
                    SetDest();
                }
            }
        }

        protected override void Finished()
        {
            fail.Set();
        }

        protected override bool ContinueWhenFighting()
        {
            return false;
        }

        private readonly STATE wait = new STATE("wait")
        {
            protected override void Update(int gameMillis) { }

            protected override bool SetAction()
            {
                timer.set(m, 0);
                return true;
            }
        };

        private readonly STATE fail = new STATE("fail")
        {
            protected override void Update(int gameMillis) { }

            protected override bool SetAction()
            {
                task.stop(div);
                order.task.Set(task);
                return true;
            }
        };
    }
}