using System;
using game.battle.thread.order;
using static game.battle.thread.order.BattleOrderUpdater.Plan;
using static settlement.main.SETT.IN_BOUNDS;

namespace game.battle.thread.order
{
    final class ToolsDiv
    {
        private readonly Tools t;

        ToolsDiv(Tools tools)
        {
            this.t = tools;
        }

        public bool needsFixing(DivFormationImp dest, int men, Army a, DIV_FORMATION f)
        {
            if (men <= 0)
                return false;

            if (f != dest.formation() || dest.deployed() > men || (men > dest.deployed() && dest.hasExtraRoom())
                || !t.deployer.isValid(div.info, dest, a))
            {
                return true;
            }
            return false;
        }

        public bool fixIfNeeded(DivFormationImp target)
        {
            if (Plan.men <= 0)
                return false;

            if (needsFixing(target, Plan.men, Plan.a, Plan.div.settings().formation))
            {
                DivFormationImp nn = t.deployer.getFixedFormation(Plan.div.info, target, Plan.div.settings().formation, Plan.men, Plan.a);

                if (nn != null)
                {
                    DivFormationImp res = t.mover.getFromMovedIntoTo(target, nn);
                    target.copy(res);
                    return true;
                }

            }
            return false;
        }


        public int inPosition(DivPositionCopyable current, DivFormationImp dest, double dist)
        {
            int am = 0;
            int max = CLAMP.i(current.deployed(), 0, dest.deployed());
            for (int i = 0; i < max; i++)
            {
                if (Plan.div.reporter.reachable(i) && dest.pixel(i).tileDistanceTo(current.pixel(i)) < dist)
                    am++;
            }
            return am;
        }

        public int distanceAverageFromCurrentToNext(DivPositionImp current, DivPositionImp next)
        {

            int dist = 0;
            int am = CLAMP.i(current.deployed(), 0, next.deployed());

            if (am == 0)
                return 0;

            for (int i = 0; i < am; i++)
            {
                dist += next.pixel(i).tileDistanceTo(current.pixel(i));
            }
            return dist / am;

        }

        private readonly Coo coo = new Coo();

        public COORDINATE getSafeCentrePixel(DivFormationImp dest)
        {
            if (dest.deployed() == 0)
                return dest.centrePixel();
            int x = 0, y = 0;
            for (int i = 0; i < dest.deployed(); i++)
            {
                x += dest.pixel(i).x();
                y += dest.pixel(i).y();
            }
            x /= dest.deployed();
            y /= dest.deployed();
            if (DivPlacability.pixelIsBlocked(x, y, dest.formation().size(div), Plan.a))
            {
                return dest.centrePixel();
            }
            coo.set(x, y);
            return coo;
        }

        public COORDINATE getSafeCentreTile(DivPositionImp dest)
        {
            if (dest.deployed() == 0)
                return coo;
            int xx = 0;
            int yy = 0;
            int am = 0;
            for (int i = 0; i < dest.deployed(); i++)
            {
                if (Plan.div.reporter.reachable(i))
                {
                    xx += dest.px(i);
                    yy += dest.py(i);
                    am++;
                }
            }

            if (am == 0)
            {
                for (int i = 0; i < dest.deployed(); i++)
                {
                    xx += dest.px(i);
                    yy += dest.py(i);
                    am++;
                }
            }

            xx /= am;
            yy /= am;

            int bestI = -1;
            double bestValue = double.MaxValue;
            int size = div.settings().formation.size(div);
            for (int i = 0; i < dest.deployed(); i++)
            {
                int x = dest.px(i);
                int y = dest.py(i);
                double dist = COORDINATE.tileDistance(x, y, xx, yy);

                if (!Plan.div.reporter.reachable(i))
                    dist += double.MaxValue / 2;
                if (!DivPlacability.pixelIsBlocked(x, y, size, Plan.a))
                {
                    if (dist < bestValue)
                    {
                        bestValue = dist;
                        bestI = i;
                    }

                }
                else
                {

                }
            }

            coo.set(dest.tile(bestI));
            return coo;
        }

        public int distanceMaxFromCurrentToNext(DivPositionImp current, DivPositionImp next)
        {

            double dist = 0;
            int am = CLAMP.i(current.deployed(), 0, next.deployed());

            for (int i = 0; i < am; i++)
            {
                dist = Math.Max(dist, next.pixel(i).tileDistanceTo(current.pixel(i)));
            }
            return (int)dist;

        }

        public int distanceTO(int x, int y, DivPositionCopyable next)
        {

            int dist = 0;
            int am = next.deployed();
            if (am == 0)
                return 0;

            for (int i = 0; i < next.deployed(); i++)
            {
                dist += next.pixel(i).tileDistanceTo(x, y);
            }
            return dist / am;

        }

        public COORDINATE currentCentre()
        {

            int xx = 0;
            int yy = 0;
            int am = 0;

            for (int i = 0; i < current.deployed(); i++)
            {
                if (div.reporter.reachable(i))
                {
                    xx += current.px(i);
                    yy += current.py(i);
                    am++;
                }
            }

            if (am == 0)
            {
                for (int i = 0; i < current.deployed(); i++)
                {
                    xx += current.px(i);
                    yy += current.py(i);
                    am++;
                }
            }

            if (am > 0)
            {
                xx /= am;
                yy /= am;
            }

            coo.set(xx, yy);
            return coo;

        }


        public bool intersectsSomewhat(DivFormationImp a, DivFormationImp b)
        {
            t.pather.filler.init(this);
            int s = 0;
            for (int i = 0; i < a.deployed(); i++)
            {
                COORDINATE c = a.tile(i);
                if (IN_BOUNDS(c))
                {
                    t.pather.filler.fill(c.x(), c.y());
                    s++;
                }
            }

            int k = 0;
            for (int i = 0; i < b.deployed(); i++)
            {
                COORDINATE c = b.tile(i);
                if (IN_BOUNDS(c))
                {
                    if (t.pather.filler.isFilled(c.x(), c.y()))
                    {
                        k++;
                    }
                }

            }

            t.pather.filler.done();

            return k > s / 2;
        }


        public bool isCloseToFighting()
        {
            if (div.status().engagements() > Math.Sqrt(Plan.men) / 2)
                return true;

            int dist = div.status().enemyClosestDist();
            if (dist < 0)
                return false;
            if (dist < 16 || (div.status().isFighting() && dist < 32))
            {
                return true;
            }
            return false;
        }
    }
}