using System;
using System.Collections.Generic;
using System.Linq;

namespace game.battle.thread.general.offence
{
    public class StepAttackEnemyNear
    {
        private readonly StrategosUtil util;
        private readonly Context context;
        private readonly int[] attacked;

        public StepAttackEnemyNear(StrategosUtil context, Context c)
        {
            this.util = context;
            this.context = c;
            this.attacked = new int[Config.battle().DIVISIONS_PER_ARMY];
        }

        public bool attackEnemies()
        {
            context.map.Clear();

            bool has = false;

            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = util.getArmy().divisions().Get(di);
                if (d.active() && !context.deployedToLine[di])
                {
                    BattleOrderTask.DIVTASK task = d.order().task.GetTask();
                    if (task == BattleOrderTask.DIVTASK.CHARGE)
                        continue;
                    if (d.status().engagements() > Math.Sqrt(d.menNrOf()) * 0.5)
                    {
                        React(d);
                        continue;
                    }
                    context.map.Add(d);
                    has = true;
                }
            }

            if (!has)
                return false;

            Flooder f = util.flooder.GetFlooder();
            f.Init(this);
            Array.Fill(attacked, 0);

            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = util.getArmy().enemy().divisions().Get(di);
                if (d.active())
                {
                    int tx = d.centre().ctX();
                    int ty = d.centre().ctY();
                    if (SETT.IN_BOUNDS(tx, ty))
                    {
                        f.PushSloppy(tx, ty, 0);
                        f.SetValue2(tx, ty, d.indexArmy());
                    }
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.PollSmallest();

                if (!context.blob.Is(t))
                    continue;

                Div enemy = util.getArmy().enemy().divisions().Get((int)t.getValue2());

                if (attacked[enemy.indexArmy()] >= enemy.menNrOf() * 4)
                    continue;

                if (t.getValue() > 48)
                    break;

                foreach (Div d in context.map.Get(t.x(), t.y()))
                {
                    Attack(enemy, d);
                }

                for (int di = 0; di < DIR.ALL.size(); di++)
                {
                    DIR dir = DIR.ALL.Get(di);
                    int dx = t.x() + dir.x();
                    int dy = t.y() + dir.y();
                    if (SETT.IN_BOUNDS(dx, dy))
                    {
                        double cost = Cost(util, dx, dy) * (1 + attacked[enemy.indexArmy()] / (1 + enemy.menNrOf()));

                        if (cost > 0)
                        {
                            if (!dir.isOrtho())
                            {
                                cost = Math.Max(cost, Cost(util, dx, t.y()));
                                cost = Math.Max(cost, Cost(util, t.x(), dy));
                            }
                            if (f.PushSmaller(dx, dy, t.getValue() + dir.tileDistance() * cost, t) != null)
                            {
                                f.SetValue2(dx, dy, t.getValue2());
                            }
                        }
                    }
                }
            }
            f.Done();
            return false;
        }

        private static double Cost(StrategosUtil context, int dx, int dy)
        {
            AVAILABILITY a = SETT.PATH().availability.Get(dx, dy);
            if (a.isSolid(context.getArmy()) || SETT.TERRAIN().Get(dx, dy) is TFortification.Tile)
            {
                return 3 + GAME.ARMIES().map.strength.Get(dx, dy) / (C.TILE_SIZE * 10);
            }
            else
            {
                double res = 1; // ArmyAIUtil.map().hasEnemy.is(dx, dy, c.army) ? 1 : 10;
                double s = SETT.ENV().map.SPACE.Get(dx, dy);
                if (s < 0.5)
                    return res + 2 + a.movementSpeedI;
                return res + a.movementSpeedI;
            }
        }

        private readonly int[] counts;

        private void React(Div d)
        {
            Array.Fill(counts, 0);

            Div ee = null;
            int max = 0;
            int tot = 0;

            context.deployedToLine[d.indexArmy()] = true;

            for (int i = 0; i < d.current().deployed(); i++)
            {
                int tx = d.current().tx(i);
                int ty = d.current().ty(i);
                Div e = BattleStatus.map().GetEnemySingle(tx, ty, util.getArmy());
                if (e != null)
                {
                    counts[e.indexArmy()]++;
                    max++;
                    if (ee == null || counts[e.indexArmy()] > max)
                    {
                        ee = e;
                    }
                }
            }

            if (ee != null && counts[ee.index()] > tot / 2)
            {
                d.settings().guard = false;
                d.settings().formation = DIV_FORMATION.TIGHT;
                Attack(ee, d);
            }
            else
            {
                d.settings().guard = true;
                d.settings().formation = DIV_FORMATION.TIGHT;
                d.order().task.Stop(d);
                d.order().task.Set(task);
            }
        }

        private void Attack(Div d, Div mDiv)
        {
            if (mDiv.settings().ammo() != null)
            {
                task.attackRanged(d, mDiv);
            }
            else
                task.attackMelee(d, mDiv);

            mDiv.order().task.Set(task);
            context.deployedToLine[mDiv.indexArmy()] = true;
            attacked[d.indexArmy()] += d.menNrOf();
            mDiv.settings().formation = d.settings().ammo() == null ? DIV_FORMATION.TIGHT : DIV_FORMATION.LOOSE;
        }

        private readonly BattleOrderTask task = new BattleOrderTask();
    }
}