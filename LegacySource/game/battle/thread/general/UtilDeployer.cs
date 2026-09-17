using System;
using game.battle.div;
using game.battle.formation;
using game.battle.thread.order;
using init.constant;
using settlement.main;
using snake2d.util.datatypes;

namespace game.battle.thread.general
{
    /**
     * uses the prelines, and sets them to divs.
     * @author Jake
     *
     */
    public class UtilDeployer
    {
        private readonly StrategosUtil context;

        public UtilDeployer(StrategosUtil context)
        {
            this.context = context;
        }

        private readonly BattleOrderTask task = new BattleOrderTask();

        DivFormationImp old = new DivFormationImp();

        public DivFormation deploy(Div d, int x1, int y1, int wi, double dx, double dy)
        {
            int m = d.menNrOf();
            if (m == 0)
                return null;
            int w = (int)Math.Sqrt(m);
            if (w == 0)
                return null;

            DivFormationImp f = context.deployer.deploy(d.info, m, d.settings().formation, x1, y1, dx, dy, wi, context.getArmy());

            if (f != null)
            {
                d.order().task.get(task);
                if (task.task() == DIVTASK.MOVE || task.task() == DIVTASK.STOP)
                {
                    d.order().dest.get(old);
                    if (old.isSameAs(f, d))
                    {
                        return old;
                    }
                }

                d.order().dest.set(f);
                task.move(d);
                d.order().task.set(task);
            }
            else
            {
                task.stop(d);
                d.order().task.set(task);
            }
            return f;
        }

        public DivFormationImp deployTile(Div d, int tx, int ty, DIR dir)
        {
            if (!attackTile(tx, ty, d))
            {
                return moveToDest(d, tx, ty, dir);
            }
            return null;
        }

        DivFormationImp oldDest = new DivFormationImp();

        private DivFormationImp moveToDest(Div d, int tx, int ty, DIR dir)
        {
            int rm = (int)(1 + Math.Sqrt(d.menNrOf() / 2.0)) * d.settings().formation.size(d);
            DivFormationImp f = context.deployer.deployArroundCentre(d.info, d.menNrOf(), d.settings().formation, (tx << C.T_SCROLL) + C.TILE_SIZEH, (ty << C.T_SCROLL) + C.TILE_SIZEH, dir.xN(), dir.yN(), rm, d.army());

            if (f == null)
            {
                task.stop(d);
                d.order().task.set(task);
                return null;
            }

            d.order().dest.set(f);
            task.move(d);
            d.order().task.set(task);
            return f;
        }

        private bool attackTile(int tx, int ty, Div d)
        {
            if (SETT.IN_BOUNDS(tx, ty) && SETT.PATH().availability.get(tx, ty).isSolid(context.getArmy()))
            {
                d.order().task.get(task);

                if (task.targetTileX() != -1)
                {
                    if (COORDINATE.tileDistance(task.targetTileX(), task.targetTileY(), tx, ty) < 3)
                        return true;
                }

                task.attack(tx, ty, d);
                d.order().task.set(task);
                return true;
            }
            return false;
        }
    }
}