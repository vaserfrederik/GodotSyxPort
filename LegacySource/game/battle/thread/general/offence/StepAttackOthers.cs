using System;
using System.Collections.Generic;
using game.battle.div;
using game.battle.formation;
using game.battle.thread.general;
using game.battle.thread.order;
using init.constant;
using settlement.main;
using settlement.path;
using settlement.tilemap.terrain;
using snake2d.PathTile;
using snake2d.PathUtilOnline;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    final class StepAttackOthers
    {
        private readonly StrategosUtil util;
        private readonly Context context;
        private readonly Bitmap1D attacked = new Bitmap1D(Config.battle().DIVISIONS_PER_ARMY, false);

        public StepAttackOthers(StrategosUtil context, Context c)
        {
            this.util = context;
            this.context = c;
        }

        public void Init()
        {
            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = util.getArmy().divisions().get(di);
                if (!Valid(d))
                {
                    continue;
                }
                context.trickedDivs[di]++;
                context.deployedToLine.set(di, false);
            }
        }

        public bool Attack()
        {
            context.map.clear();

            int am = 0;
            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = util.getArmy().divisions().get(di);
                if (!Valid(d))
                {
                    continue;
                }
                if (context.deployedToLine.get(di))
                    continue;
                if (context.trickedDivs[di] < 4)
                    continue;
                context.map.add(d);
                am++;
            }

            if (am == 0)
                return false;

            Flooder f = util.flooder.getFlooder();
            f.init(this);
            f.pushSloppy(util.getDestCoo(), 0);
            attacked.clear();
            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = util.getArmy().enemy().divisions().get(di);
                if (!d.active())
                {
                    continue;
                }
                int tx = d.centre().ctX();
                int ty = d.centre().ctY();
                if (SETT.IN_BOUNDS(tx, ty))
                {
                    f.pushSloppy(tx, ty, 0);
                    f.setValue2(tx, ty, d.indexArmy());
                }
            }

            am = 0;
            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();

                if (t.getValue() > 100)
                    break;

                if (t.getParent() != null)
                    t.setValue2(t.getParent().getValue2());

                Div enemy = util.getArmy().enemy().divisions().get((int)t.getValue2());

                if (enemy == null)
                    continue;

                if (attacked.get(enemy.indexArmy()))
                    continue;

                foreach (Div d in context.map.get(t.x(), t.y()))
                {
                    Attack(d, enemy);
                    am++;
                    break;
                }

                for (int di = 0; di < DIR.ALL.size(); di++)
                {
                    DIR dir = DIR.ALL.get(di);
                    int dx = t.x() + dir.x();
                    int dy = t.y() + dir.y();
                    if (SETT.IN_BOUNDS(dx, dy))
                    {
                        double cost = Cost(util, dx, dy);

                        if (cost > 0)
                        {
                            if (!dir.isOrtho())
                            {
                                cost = Math.Min(cost, Cost(util, dx, t.y()));
                                cost = Math.Min(cost, Cost(util, t.x(), dy));
                            }
                            f.pushSmaller(dx, dy, t.getValue() + dir.tileDistance() * cost, t);
                        }
                    }
                }
            }
            f.done();
            return am > 0;
        }

        private readonly BattleOrderTask task = new BattleOrderTask();

        private void Attack(Div mDiv, Div enemy)
        {
            if (mDiv.settings().ammo() != null)
            {
                task.attackRanged(enemy, mDiv);
            }
            else
                task.attackMelee(enemy, mDiv);

            mDiv.order().task.set(task);
            context.deployedToLine.set(mDiv.indexArmy(), true);
            attacked.set(enemy.indexArmy(), true);
            mDiv.settings().running = true;
            mDiv.settings().formation = enemy.settings().ammo() == null ? DIV_FORMATION.TIGHT : DIV_FORMATION.LOOSE;
        }

        public static double Cost(StrategosUtil context, int dx, int dy)
        {
            AVAILABILITY a = SETT.PATH().availability.get(dx, dy);
            if (a.isSolid(context.getArmy()) || SETT.TERRAIN().get(dx, dy) is TFortification.Tile)
            {
                return 1;
            }
            else
            {
                double res = 1; // ArmyAIUtil.map().hasEnemy.is(dx, dy, c.army) ? 1 : 10;
                double s = SETT.ENV().map.SPACE.get(dx, dy);
                if (s < 0.5)
                    return res + 2 + a.movementSpeedI;
                return res + a.movementSpeedI;
            }
        }

        private bool Valid(Div d)
        {
            if (!d.active())
                return false;
            if (d.status().isFighting())
                return false;
            if (context.distsToLine[d.indexArmy()] + context.distsFromLineToBlob[d.indexArmy()] > 16)
                return false;

            d.order().task.get(task);

            if (task.task() != DIVTASK.STOP)
                return false;

            return true;
        }
    }
}