using System;
using System.Collections.Generic;
using game;
using game.battle.thread.general;
using settlement.main;
using settlement.room.military.artillery;
using settlement.thing.projectiles;
using snake2d.PathTile;
using snake2d.PathUtilOnline;
using snake2d.util.datatypes;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    class StepArtilleryBombard
    {
        private readonly int absSize = 8;
        private readonly Bitmap2D mapArt;
        private readonly Bitmap2D mapBreak;

        private readonly StrategosUtil c;

        private readonly ArrayListResize<ArtilleryInstance> ins = new ArrayListResize<ArtilleryInstance>(256);
        private readonly ArrayList<ArtilleryInstance> tmp = new ArrayList<ArtilleryInstance>(32);
        private readonly Trajectory traj = new Trajectory();

        public StepArtilleryBombard(StrategosUtil context)
        {
            this.c = context;

            int wi = (int)Math.Ceiling((double)SETT.TWIDTH / absSize);
            int hi = (int)Math.Ceiling((double)SETT.THEIGHT / absSize);
            mapArt = new Bitmap2D(wi, hi, false);
            mapBreak = new Bitmap2D(wi, hi, false);
        }

        public void bombard()
        {
            ins.clearSoft();

            for (int ai = 0; ai < SETT.ROOMS().ARTILLERY.size(); ai++)
            {
                SETT.ROOMS().ARTILLERY.get(ai).threadInstances(ins);
            }
            for (int ii = 0; ii < ins.size(); ii++)
            {
                ArtilleryInstance i = ins.get(ii);
                if (i.menMustering() == 0 || i.isFiring() || i.army() != c.getArmy())
                {
                    ins.remove(ii);
                    ii--;
                }
            }

            if (ins.size() > 0)
            {
                mapBreak.clear();
                for (int y = 0; y < SETT.THEIGHT; y++)
                {
                    for (int x = 0; x < SETT.TWIDTH; x++)
                    {
                        if (SETT.PATH().availability.get(x, y).isSolid(c.getArmy()) || GAME.ARMIES().map.attackable.is(x, y, c.getArmy()))
                        {
                            int ax = x / absSize;
                            int ay = y / absSize;
                            mapBreak.set(ax, ay, true);
                        }
                    }
                }

                foreach (ArtilleryInstance i in ins)
                {
                    int ax = i.mX() / absSize;
                    int ay = i.mY() / absSize;

                    mapArt.set(ax, ay, true);
                }

                Flooder f = c.flooder.getFlooder();
                f.init(this);
                {
                    int ax = c.getDestCoo().x() / absSize;
                    int ay = c.getDestCoo().y() / absSize;
                    f.pushSloppy(ax, ay, 0);
                }

                while (f.hasMore() && ins.size() > 0)
                {
                    PathTile t = f.pollSmallest();
                    if (mapArt.is(t))
                    {
                        pushTarget(t, ins);
                        continue;
                    }

                    for (int di = 0; di < DIR.ALL.size(); di++)
                    {
                        DIR d = DIR.ALL.get(di);
                        int dx = t.x() + d.x();
                        int dy = t.y() + d.y();
                        if (mapBreak.body().holdsPoint(dx, dy))
                        {
                            double v = mapBreak.is(dx, dy) ? 8 : 1;
                            f.pushSmaller(dx, dy, t.getValue() + v * d.tileDistance(), t);
                        }
                    }
                }

                f.done();
            }
        }

        private void pushTarget(PathTile t, ArrayListResize<ArtilleryInstance> ins)
        {
            tmp.clearSloppy();
            double min = Double.MaxValue;
            double max = 0;

            foreach (ArtilleryInstance i in ins)
            {
                int ax = i.mX() / absSize;
                int ay = i.mY() / absSize;
                if (ax == t.x() && ay == t.y())
                {
                    if (tmp.hasRoom())
                    {
                        min = Math.Min(min, i.rangeMin());
                        max = Math.Max(max, i.rangeMax());
                        tmp.add(i);
                    }
                }
            }

            min *= min;
            max *= max;

            if (tmp.size() == 0)
                return;

            int ox = t.x();
            int oy = t.y();
            while (t != null && tmp.size() > 0)
            {
                PathTile p = t;
                t = t.getParent();
                double dx = (ox - p.x()) * absSize;
                double dy = (oy - p.y()) * absSize;
                double d = dx * dx + dy * dy;
                if (d < min)
                    continue;
                if (d > max)
                    continue;
                if (mapBreak.is(p))
                {
                    push(p, tmp);
                }
            }
        }

        private void push(PathTile t, ArrayList<ArtilleryInstance> ins)
        {
            int x1 = t.x() * absSize;
            int y1 = t.y() * absSize;

            for (int dy = 0; dy < absSize; dy++)
            {
                for (int dx = 0; dx < absSize && tmp.size() > 0; dx++)
                {
                    int x = x1 + dx;
                    int y = y1 + dy;

                    if (SETT.PATH().availability.get(x, y).isSolid(c.getArmy()) || GAME.ARMIES().map.attackable.is(x, y, c.getArmy()))
                    {
                        foreach (ArtilleryInstance i in tmp)
                        {
                            if (i.testTarget(x, y, traj, false) == null)
                            {
                                i.targetCooSet(x, y, false, false);
                                tmp.remove(i);
                                ins.remove(i);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}