using System;
using System.Collections.Generic;
using game;
using game.battle.div;
using game.battle.formation;
using game.battle.thread.general;
using game.battle.thread.general.offence.ContextLines;
using game.battle.thread.order;
using init.constant;
using settlement.main;
using settlement.path;
using snake2d.PathUtilOnline;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    class StepLinesMoveTo
    {
        private readonly Bitmap1D deployed;
        private readonly int[] dists;
        private readonly int[] distsToBlob;

        private readonly StrategosUtil util;
        private readonly int maxRange = 1000;
        private readonly Context c;
        private readonly ContextLines lines;
        private readonly Bitmap2D blob;
        private readonly Bitmap2D penalty;
        private readonly UtilDivMap map;
        private readonly DivDeploment[] dall = new DivDeploment[Config.battle().DIVISIONS_PER_ARMY];
        private readonly List<DivDeploment> toDeploy = new List<DivDeploment>(dall.Length);
        private readonly VectorImp vec = new VectorImp();

        public StepLinesMoveTo(StrategosUtil context, Context c)
        {
            this.c = c;
            this.util = context;
            this.lines = c.lines;
            this.map = c.map;
            this.blob = c.blob;
            for (int i = 0; i < dall.Length; i++)
                dall[i] = new DivDeploment();
            this.deployed = c.deployedToLine;
            this.dists = c.distsToLine;
            this.distsToBlob = c.distsFromLineToBlob;
            penalty = c.block;
        }

        public void init()
        {
            for (int i = 0; i < lines.lines(); i++)
            {
                Line l = lines.get(i);
                l.back = 0;
            }
            map.clear();

            penalty.clear();
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
                if (t.getValue() >= 24)
                {
                    break;
                }
                penalty.set(t, true);

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

        public bool deployDivsToLine()
        {
            fillLines(false);
            return deploy();
        }

        public bool deployDivsToLineRanged()
        {
            fillLines(true);
            return deploy();
        }

        public bool setSpeedAndFormation()
        {
            double dist = 0;
            int am = 0;
            int fighters = 0;
            double tot = 0;
            for (int i = 0; i < Config.battle().DIVISIONS_PER_ARMY; i++)
            {
                if (deployed.get(i))
                {
                    dist += dists[i];
                    am++;
                }
                Div d = util.getArmy().divisions().get(i);
                int m = d.menNrOf();
                if (d.status().engagements() > 1)
                    fighters += m;
                tot += m;
            }

            if (am == 0)
                return false;

            dist /= am;

            dist += 16;

            dist -= dist * (fighters / tot);

            for (int i = 0; i < Config.battle().DIVISIONS_PER_ARMY; i++)
            {
                if (deployed.get(i))
                {
                    Div d = util.getArmy().divisions().get(i);
                    d.settings().running = dists[i] > dist;
                }
            }

            return true;
        }

        private bool deploy()
        {
            if (toDeploy.Count == 0)
                return false;

            foreach (DivDeploment d in toDeploy)
            {
                if (d.dir == null)
                {
                    if (d.l.deploy(util, d.div) != null)
                    {
                        deployed.set(d.div.indexArmy(), true);

                    }
                    else
                    {
                        d.l.back = -1;
                    }
                }
                else
                {
                    deployed.set(d.div.indexArmy(), true);
                    DivFormationImp f = util.divDeployer.deployTile(d.div, d.cx, d.cy, d.dir);
                    if (f != null)
                    {
                        d.div.settings().formation = f;
                    }
                }
            }
            return true;
        }

        private void fillLines(bool ranged)
        {
            toDeploy.Clear();
            for (int i = 0; i < lines.lines(); i++)
            {
                Line line = lines.get(i);
                if (line.back >= 0)
                {
                    foreach (Div div in util.getArmy().divisions())
                    {
                        if (valid(div, ranged))
                        {
                            DivDeploment dd = new DivDeploment();
                            init(div, line, line.backTile, dd, ranged);
                            toDeploy.Add(dd);
                        }
                    }
                }
            }
        }

        private int penalty(Line line, bool ranged)
        {
            return (int)(line.blobID * C.ITILE_SIZE * (ranged ? 2 * c.flanking : c.flanking));
        }

        private void init(Div div, Line line, PathTile t, DivDeploment dd, bool ranged)
        {
            dd.dir = null;
            dd.div = div;
            dd.l = line;

            double dist = t.getValue() - penalty(line, ranged);

            PathTile p = t;
            while (p.getParent() != null)
            {
                if (penalty.is(p))
                    dist -= 10 * DIR.get(p, p.getParent()).tileDistance();
                p = p.getParent();
            }
            dists[div.indexArmy()] = (int)dist;
            distsToBlob[div.indexArmy()] = (int)(line.blobID * C.ITILE_SIZE);
            DIV_FORMATION f = DIV_FORMATION.LOOSE;

            PathTile start = t;
            t = setDest(t);

            if (t.getParent() == null)
            {
                t = avoidWalkingThroughBlobDest(div, start);
                if (t != null && t.getParent() != null)
                {
                    dd.cx = t.x();
                    dd.cy = t.y();
                    dd.dir = DIR.get(t, t.getParent());
                }
                else if (dists[div.indexArmy()] + line.blobID * C.ITILE_SIZE < 32)
                    f = DIV_FORMATION.TIGHT;
            }
            else
            {
                dd.cx = t.x();
                dd.cy = t.y();
                dd.dir = DIR.get(t, t.getParent());
            }

            div.settings().formation = f;
        }

        private bool valid(Div d, bool ranged)
        {
            if (!d.active())
                return false;
            if (deployed.get(d.indexArmy()))
                return false;
            if (ranged && d.settings().ammo() == null)
                return false;
            if (!ranged && d.settings().ammo() != null)
                return false;

            d.order().task.get(task);

            if (task.task() != DIVTASK.MOVE && task.task() != DIVTASK.STOP)
                return false;

            return true;
        }

        private PathTile avoidWalkingThroughBlobDest(Div d, PathTile start)
        {
            int rewind = 1;
            PathTile t = start;

            outer:
            while (t != null)
            {
                if (rewind++ % 14 == 0)
                {
                    double l = vec.set(t, start);

                    for (int i = 0; i < l; i++)
                    {
                        int tx = (int)(t.x() + vec.nX() * i);
                        int ty = (int)(t.y() + vec.nY() * i);
                        if (blob.is(tx, ty))
                            break outer;
                    }

                }

                t = t.getParent();
            }

            if (t == null)
                return null;

            t = start;

            rewind -= 14;

            while (t != null)
            {
                if (rewind-- <= 0)
                    return t;

                t = t.getParent();
            }

            return t;

        }

        private double cost(int dx, int dy)
        {
            if (blob.is(dx, dy))
                return -1;
            if (penalty.is(dx, dy))
                return 10;

            AVAILABILITY a = SETT.PATH().availability.get(dx, dy);
            if (a.isSolid(util.getArmy()))
            {
                return 1 + GAME.ARMIES().map.strength.get(dx, dy) / (C.TILE_SIZE * 10);
            }
            else
            {
                double res = a.movementSpeedI;//ArmyAIUtil.map().hasEnemy.is(dx, dy, c.army) ? 1 : 10;
                double s = SETT.ENV().map.SPACE.get(dx, dy);
                if (s < 0.5)
                    return res *= 2;
                return res;
            }
        }

        private PathTile setDest(PathTile start)
        {
            if (start.getParent() == null)
            {
                return start;
            }

            while (start.getParent() != null)
            {
                if ((SETT.PATH().availability.get(start).isSolid(util.getArmy())))
                {
                    return start;
                }
                start = start.getParent();
            }
            return start;
        }

        private class DivDeploment
        {
            public Div div;
            public Line l;
            public DIR dir;
            public int cx;
            public int cy;
        }
    }
}