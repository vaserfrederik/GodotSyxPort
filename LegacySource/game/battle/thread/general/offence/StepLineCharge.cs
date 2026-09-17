using game.battle.div;
using game.battle.formation;
using game.battle.thread.general;
using game.battle.thread.order;
using game.battle.thread.status;
using game.battle.thread.trajectory;
using init.constant;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.data.DOUBLE;
using util.data.INT;

namespace game.battle.thread.general.offence
{
    class StepLineCharge
    {
        private readonly Context context;
        private readonly StrategosUtil util;
        private readonly Bitmap2D blob;
        private readonly Bitmap2D block;
        private readonly DOUBLE_MUTABLE chargeReady;
        private readonly INTE divI;

        private readonly BattleOrderTask task = new BattleOrderTask();
        private readonly VectorImp vec = new VectorImp();

        public StepLineCharge(StrategosUtil context, Context c)
        {
            this.util = context;

            this.blob = c.blob;
            this.context = c;
            block = c.block;
            chargeReady = c.value;
            divI = c.checkI;
        }

        public void init()
        {
            divI.set(0);
            increaseBlob();
            markFighting();

            double tot = 0;
            double ready = 0;
            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = util.getArmy().divisions().get(di);
                if (context.deployedToLine.get(di))
                {
                    if (d.settings().ammo() != null && BattleTrajectories.trajectories(d) > d.men() / 2)
                        continue;
                    tot++;
                    d.order().task.get(task);
                    if (valid(d))
                        ready++;
                }
                else
                {
                    d.order().task.get(task);

                    if (task.task() == DIVTASK.CHARGE)
                        context.deployedToLine.set(di, true);
                }
            }
            if (tot > 0)
                ready /= tot;

            chargeReady.setD(ready);
        }


        public bool charge()
        {
            if (chargeReady.getD() == 0)
                return false;
            while (divI.get() < Config.battle().DIVISIONS_PER_ARMY)
            {
                Div d = util.getArmy().divisions().get(divI.get());
                divI.inc(1);
                if (!v(d))
                    continue;

                if (isCharger(d))
                {

                    if (!canCharge(d))
                    {
                        task.stop(d);
                        context.deployedToLine.set(d.indexArmy(), false);
                        d.order().task.set(task);
                    }
                    else
                    {
                        context.deployedToLine.set(d.indexArmy(), true);
                    }
                    return true;
                }
                else if (chargeReady.getD() > 0.75 || block.is(d.centre().ctX(), d.centre().ctY()))
                {
                    if (canCharge(d))
                    {
                        context.deployedToLine.set(d.indexArmy(), true);
                        task.charge(d);
                        d.order().task.set(task);
                    }
                    return true;
                }
            }
            return false;

        }


        private void increaseBlob()
        {
            Flooder f = util.flooder.getFlooder();
            f.init(this);
            for (int ty = 0; ty < SETT.THEIGHT; ty++)
            {
                for (int tx = 0; tx < SETT.TWIDTH; tx++)
                {
                    if (blob.is(tx, ty))
                        f.pushSloppy(tx, ty, 0);
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.getValue() >= 5)
                {
                    break;
                }
                blob.set(t, true);

                for (int di = 0; di < DIR.ALL.size(); di++)
                {
                    DIR d = DIR.ALL.get(di);
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (!SETT.IN_BOUNDS(dx, dy))
                        continue;
                    f.pushSmaller(dx, dy, d.tileDistance() + t.getValue(), t);

                }

            }
            f.done();
        }

        private void markFighting()
        {
            block.clear();
            Flooder f = util.flooder.getFlooder();
            f.init(this);
            for (int ty = 0; ty < SETT.THEIGHT; ty++)
            {
                for (int tx = 0; tx < SETT.TWIDTH; tx++)
                {
                    if (blob.is(tx, ty) && BattleStatus.map().hasAlly.is(tx, ty, util.getArmy()) && BattleStatus.map().hasEnemy.is(tx, ty, util.getArmy()))
                        f.pushSloppy(tx, ty, 0);
                }
            }

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();
                if (t.getValue() >= 64)
                {
                    break;
                }
                block.set(t, true);

                for (int di = 0; di < DIR.ALL.size(); di++)
                {
                    DIR d = DIR.ALL.get(di);
                    int dx = t.x() + d.x();
                    int dy = t.y() + d.y();
                    if (!SETT.IN_BOUNDS(dx, dy))
                        continue;
                    f.pushSmaller(dx, dy, d.tileDistance() + t.getValue(), t);

                }

            }
            f.done();
        }


        private bool v(Div d)
        {
            if (!d.active())
                return false;

            if (d.status().engagements() > d.menNrOf() / 20)
                return false;

            if (d.settings().fireAtWill && d.settings().ammo() != null && BattleTrajectories.trajectories(d) > d.men() / 2)
                return false;

            DivFormation f = d.position();

            int cx = (int)(f.start().x() + f.dx() * f.width() / 2) / C.TILE_SIZE;
            int cy = (int)(f.start().y() + f.dy() * f.width() / 2) / C.TILE_SIZE;

            if (!blob.is(cx, cy))
                return false;

            return true;


        }

        private bool valid(Div d)
        {
            if (!v(d))
                return false;
            d.order().task.get(task);

            if (task.task() != DIVTASK.STOP)
                return false;

            return true;


        }

        private bool isCharger(Div d)
        {
            if (!v(d))
                return false;

            d.order().task.get(task);

            if (task.task() != DIVTASK.CHARGE)
                return false;

            return true;


        }

        private bool canCharge(Div div)
        {

            DivFormation f = div.position();
            double l = f.width();
            double rx = f.dx();
            double ry = f.dy();

            int sx1 = f.start().x();
            int sy1 = f.start().y();

            int friends = 0;
            int enemies = 0;

            vec.set(rx, ry);
            vec.rotate90().rotate90().rotate90();

            for (int w = 0; w <= l; w += C.TILE_SIZE)
            {
                int sx = (int)(sx1 + rx * w);
                int sy = (int)(sy1 + ry * w);
                for (int i = C.TILE_SIZE; i < 32 * C.TILE_SIZE; i++)
                {


                    int x = (int)(sx + vec.nX() * i) / C.TILE_SIZE;
                    int y = (int)(sy + vec.nY() * i) / C.TILE_SIZE;

                    if (!DivPlacability.tileIsOK(x, y, util.getArmy()))
                        return false;

                    friends += BattleStatus.map().soldiers(util.getArmy()).get(x, y);
                    if (BattleStatus.map().hasEnemy.is(x, y, util.getArmy()))
                    {
                        enemies++;
                        break;
                    }
                }
            }

            int rows = (int)(l / C.TILE_SIZE);
            if (rows == 0)
                return enemies > 0;
            return enemies > rows / 2 && friends / rows < 5;

        }
    }
}