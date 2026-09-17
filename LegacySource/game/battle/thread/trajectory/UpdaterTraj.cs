using System;
using System.Collections.Generic;

namespace game.battle.thread.trajectory
{
    internal sealed class UpdaterTraj
    {
        private readonly DivTrajectory[] all;
        private readonly BattleOrderTask task = new BattleOrderTask();
        private readonly Trajectory trajLow = new Trajectory();
        private readonly VectorImp vec1 = new VectorImp();
        private readonly VectorImp vec2 = new VectorImp();
        private readonly FormationBody bodyArcher = new FormationBody();
        private readonly FormationBody bodyTarget = new FormationBody();
        private readonly List<Div> targets = new List<Div>(16);

        public UpdaterTraj()
        {
            all = new DivTrajectory[Config.battle().DIVISIONS_PER_BATTLE];
            for (int i = 0; i < all.Length; i++)
                all[i] = new DivTrajectory();
        }

        public DivTrajectory Update(Request req, Div div, DivTrajectory old)
        {
            DivTrajectory traj = all[div.Index()];
            traj.Clear();

            EquipRange ammo = div.Settings().ammo();

            if (div.Active() && ammo != null)
            {
                div.Order().task.Get(task);
                if (task.Task() == BattleOrderTask.DivTask.ATTACK_RANGED)
                {
                    SetTrajectory(req, div, task.TargetDiv(), traj);
                }
                else if (div.Settings().FireAtWill())
                {
                    targets.Clear();
                    div.Status().EnemiesClosest(targets);
                    foreach (Div d in targets)
                    {
                        if (SetTrajectory(req, div, d, traj))
                        {
                            break;
                        }
                    }
                }
            }

            all[div.Index()] = old;

            return traj;
        }

        private bool SetTrajectory(Request req, Div div, Div target, DivTrajectory traj)
        {
            if (target == null || !target.Active())
            {
                return false;
            }

            if (!bodyTarget.Init(target.Current()))
            {
                return false;
            }

            if (SProjectiles.Problem(trajLow, div, bodyTarget.CX(), bodyTarget.CY()) == SProjectiles.OutOfRange)
            {
                return false;
            }

            if (!bodyArcher.Init(div.Current()))
            {
                return false;
            }

            EquipRange a = div.Settings().ammo();
            if (a == null || a != req.Ammo())
            {
                return false;
            }

            traj.Potential = true;

            bool hasCounters = false;

        outerloop:
            for (int ui = 0; ui < div.MenNrOf(); ui++)
            {
                int i = ui;

                if (!req.Count(i))
                {
                    hasCounters = true;
                    continue;
                }

                float refValue = req.Ref(i);
                if (refValue < 0)
                {
                    continue;
                }

                double angle = a.Projectile.MaxAngle(refValue);
                double vel = a.Projectile.Velocity(refValue);

                int startX = req.X(i);
                int startY = req.Y(i);

                double ddx = (startX - bodyArcher.X1()) / bodyArcher.Width();

                double ddy = (startY - bodyArcher.Y1()) / bodyArcher.Height();

                int targetX = (int)(bodyTarget.X1() + ddx * bodyTarget.Width());
                int targetY = (int)(bodyTarget.Y1() + ddy * bodyTarget.Height());

                vec1.Set(startX, startY, targetX, targetY);

                vec2.Set(vec1);
                vec2.Rotate90();

                for (int vv1 = 0; vv1 > -5; vv1--)
                {
                    int dx = (int)(targetX + (vec1.NX()) * C.TILE_SIZE);
                    int dy = (int)(targetY + (vec1.NY()) * C.TILE_SIZE);
                    if (IsEnemy(div, dx, dy) && SProjectiles.Problem(div.Army(), trajLow, startX, startY, dx, dy, angle, vel) == null)
                    {
                        traj.Set(i, trajLow);
                        continue outerloop;
                    }
                }

                for (int vv1 = 1; vv1 <= 10; vv1++)
                {
                    double v1 = vv1 / 2 * ((vv1 & 1) == 1 ? 1 : -1);
                    for (int vv2 = 1; vv2 <= 10; vv2++)
                    {
                        double v2 = vv2 / 2 * ((vv2 & 1) == 1 ? 1 : -1);

                        int dx = (int)(targetX + (vec1.NX() * v1 + vec2.NX() * v2) * C.TILE_SIZE);
                        int dy = (int)(targetY + (vec1.NY() * v1 + vec2.NY() * v2) * C.TILE_SIZE);
                        if (IsEnemy(div, dx, dy) && SProjectiles.Problem(div.Army(), trajLow, startX, startY, dx, dy, angle, vel) == null)
                        {
                            traj.Set(i, trajLow);
                            break;
                        }
                    }
                }
            }

            if (traj.Targets > 0 || hasCounters)
                return true;
            return false;
        }

        private static bool IsEnemy(Div div, int x, int y)
        {
            int tx = x >> C.T_SCROLL;
            int ty = y >> C.T_SCROLL;
            if (SETT.IN_BOUNDS(tx, ty))
            {
                if (BattleStatus.Map().HasEnemy.Is(tx, ty, div.Army()))
                    return true;
            }
            return false;
        }
    }
}