using game;
using game.battle.div;
using game.battle.thread.general;
using game.battle.thread.status;
using init.constant;
using settlement.main;
using settlement.path;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    internal sealed class StepMoveToThrone
    {
        private readonly StrategosUtil util;
        private readonly Context context;
        private readonly Bitmap2D blocked;

        public StepMoveToThrone(StrategosUtil util, Context context)
        {
            this.util = util;
            this.context = context;
            blocked = context.block;
        }

        public void Init()
        {
            blocked.Clear();
        }

        public bool SetToThrone()
        {
            context.map.Clear();

            int am = 0;

            for (int di = 0; di < Config.battle().DIVISIONS_PER_ARMY; di++)
            {
                Div d = util.getArmy().divisions().Get(di);
                if (d.active() && !context.deployedToLine.Get(di) && !d.status().isFighting())
                {
                    context.map.Add(d);
                    am++;
                }
            }

            if (am == 0)
                return false;

            Flooder f = util.flooder.getFlooder();
            f.Init(this);
            f.PushSloppy(util.getDestCoo(), 0);

            while (f.hasMore())
            {
                PathTile t = f.pollSmallest();

                foreach (Div m in context.map.Get(t.x(), t.y()))
                {
                    if (context.deployedToLine.Get(m.indexArmy()))
                        continue;

                    context.deployedToLine.Set(m.indexArmy(), true);
                    f.Done();

                    PathTile dest = setDest(m, blocked, t);
                    block(t, dest, blocked);
                    util.divDeployer.deployTile(m, res.destX, res.destY, res.destDir);
                    return true;
                }

                for (int di = 0; di < DIR.ALL.size(); di++)
                {
                    DIR dir = DIR.ALL.Get(di);
                    int dx = t.x() + dir.x();
                    int dy = t.y() + dir.y();
                    if (SETT.IN_BOUNDS(dx, dy))
                    {
                        double cost = cost(util, dx, dy);

                        if (cost > 0)
                        {
                            if (!dir.isOrtho())
                            {
                                cost = Math.Max(cost, cost(util, dx, t.y()));
                                cost = Math.Max(cost, cost(util, t.x(), dy));
                            }
                            if (blocked.Is(dx, dy))
                            {
                                cost *= 4;
                            }
                            f.PushSmaller(dx, dy, t.GetValue() + dir.tileDistance() * cost, t);
                        }
                    }
                }
            }
            f.Done();
            return false;
        }

        public static double cost(StrategosUtil context, int dx, int dy)
        {
            AVAILABILITY a = SETT.PATH().availability.Get(dx, dy);
            if (a.isSolid(context.getArmy()) || SETT.TERRAIN().Get(dx, dy) is TFortification.Tile)
            {
                return 3 + GAME.ARMIES().map.strength.Get(dx, dy) / (C.TILE_SIZE * 10);
            }
            else
            {
                double res = 1;//ArmyAIUtil.map().hasEnemy.is(dx, dy, c.army) ? 1 : 10;
                double s = SETT.ENV().map.SPACE.Get(dx, dy);
                if (s < 0.5)
                    return res + 2 + a.movementSpeedI;
                return res + a.movementSpeedI;
            }
        }

        private static void block(PathTile t, PathTile dest, Bitmap2D toBlock)
        {
            while (t != dest)
            {
                for (int i = 0; i < DIR.ORTHO.size(); i++)
                {
                    toBlock.Set(t, DIR.ORTHO.Get(i), true);
                }
                t = t.getParent();
            }
        }

        private PathTile setDest(Div div, Bitmap2D blocked, PathTile t)
        {
            if (initBlocked(div, DIR.C, t.x(), t.y(), t))
                return t;

            if (t.getParent() == null)
            {
                res.destX = t.x();
                res.destY = t.y();
                res.destDir = DIR.C;
                return t;
            }

            PathTile prev = null;
            bool b = true;
            while (t.getParent() != null)
            {
                if (initBlocked(div, blocked, t))
                    return t.getParent();
                if (!b && blocked.Is(t.getParent()))
                {
                    res.destX = t.x();
                    res.destY = t.y();
                    res.destDir = DIR.Get(t, t.getParent());
                    return t;
                }
                prev = t;
                b = false;
                t = t.getParent();
            }
            res.destX = t.x();
            res.destY = t.y();
            res.destDir = DIR.Get(prev, t);
            return t;
        }

        private bool initBlocked(Div div, Bitmap2D blocked, PathTile block)
        {
            if (initBlocked(div, DIR.Get(block.getParent(), block), block.getParent().x(), block.getParent().y(), block))
                return true;
            if (initBlocked(div, DIR.Get(block.getParent(), block), block.getParent().x(), block.y(), block))
                return true;
            if (initBlocked(div, DIR.Get(block.getParent(), block), block.x(), block.getParent().y(), block))
                return true;
            return false;
        }

        private bool initBlocked(Div div, DIR d, int dx, int dy, PathTile block)
        {
            if (SETT.PATH().availability.Get(dx, dy).isSolid(div.army()) || BattleStatus.map().hasEnemy.Is(dx, dy, util.getArmy()))
            {
                res.destX = dx;
                res.destY = dy;
                res.destDir = d;
                return true;
            }
            return false;
        }

        private readonly Res res = new Res();

        private class Res
        {
            public int destX;
            public int destY;
            public DIR destDir;

        }
    }
}