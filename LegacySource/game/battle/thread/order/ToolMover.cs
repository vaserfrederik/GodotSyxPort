using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Battle.Thread.Order
{
    using static BattleOrderUpdater.Plan.div;

    using Game.Battle.Formation;
    using Init.Constant;
    using Snake2D.PathUtilOnline;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Sets;

    internal sealed class ToolMover
    {
        private readonly DivFormationImp positions;
        private readonly TREE toTree;
        private readonly TREE fromTree;
        private readonly POS[] toPosses;
        private readonly POS[] fromPosses;
        private readonly List<POS> cache;
        private readonly bool[] tosThatHasBeenPlaced;
        private readonly PathUtilOnline pu;
        private readonly VectorImp vec = new VectorImp();

        public ToolMover(PathUtilOnline pu)
        {
            int maxmen = Config.battle().MEN_PER_DIVISION;
            positions = new DivFormationImp();

            tosThatHasBeenPlaced = new bool[maxmen];
            toTree = new TREE(maxmen);
            fromTree = new TREE(maxmen);

            toPosses = new POS[maxmen];
            fromPosses = new POS[maxmen];
            for (int i = 0; i < maxmen; i++)
            {
                toPosses[i] = new POS();
                fromPosses[i] = new POS();
            }
            cache = new List<POS>(maxmen);
            this.pu = pu;
        }

        public void RearrangeDest(DivFormationImp from, DivFormationImp to)
        {
            if (from.deployed() == 0 || to.deployed() == 0)
            {
                positions.Copy(to);
                from.Copy(positions);
                return;
            }

            for (int i = 0; i < to.deployed(); i++)
                tosThatHasBeenPlaced[i] = false;

            positions.Copy(to);

            Centers(from, to);

            if (toTree.HasMore())
            {
                for (int i = 0; i < to.deployed(); i++)
                {
                    if (!tosThatHasBeenPlaced[i])
                    {
                        COORDINATE c = to.Pixel(toTree.PollGreatest().i);
                        positions.Set(i, c.x, c.y);
                    }
                }
            }

            to.Copy(positions);
            to.DeployFinish(pu.filler, div.info);
        }

        public DivFormationImp GetFromMovedIntoTo(DivPositionImp current, DivFormationImp to)
        {
            if (current == positions || to == positions)
                throw new Exception();

            if (current.deployed() == 0 || to.deployed() == 0)
            {
                positions.Copy(to);
                return positions;
            }

            for (int i = 0; i < to.deployed(); i++)
                tosThatHasBeenPlaced[i] = false;

            positions.Copy(to);

            Centers(current, to);

            if (toTree.HasMore())
            {
                for (int i = 0; i < to.deployed(); i++)
                {
                    if (!tosThatHasBeenPlaced[i])
                    {
                        COORDINATE c = to.Pixel(toTree.PollGreatest().i);
                        positions.Set(i, c.x, c.y);
                    }
                }
            }
            positions.DeployFinish(pu.filler, div.info);
            return positions;
        }

        private readonly Rec centre = new Rec();

        private void Centers(DivPosition from, DivFormationImp to)
        {
            toTree.Clear();
            fromTree.Clear();

            centre.Clear();
            for (int i = 0; i < from.deployed() && i < to.deployed(); i++)
            {
                centre.Unify(from.Pixel(i).x, from.Pixel(i).y);
            }

            double toDistX = to.Body().cX - centre.cX;
            double toDistY = to.Body().cY - centre.cY;

            for (int i = 0; i < from.deployed() && i < to.deployed(); i++)
            {
                double dx = from.Pixel(i).x - centre.cX;
                double dy = from.Pixel(i).y - centre.cY;
                double d = Math.Sqrt(dx * dx + dy * dy);
                fromPosses[i].i = i;
                fromPosses[i].value = d;
                fromTree.Add(fromPosses[i]);
            }

            for (int i = 0; i < from.deployed() && i < to.deployed(); i++)
            {
                double dx = to.Pixel(i).x - toDistX - centre.cX;
                double dy = to.Pixel(i).y - toDistY - centre.cY;
                double d = Math.Sqrt(dx * dx + dy * dy);
                toPosses[i].i = i;
                toPosses[i].value = d;
                toTree.Add(toPosses[i]);
            }

            final double M = Math.Max(to.Formation().Size(div), 0) * 2;

            while (toTree.HasMore() && fromTree.HasMore())
            {
                POS pTo = toTree.PollGreatest();

                cache.Clear();

                POS pFrom = fromTree.PollGreatest();
                cache.Add(pFrom);
                double max = pFrom.value;
                double dx = to.Pixel(pTo.i).x - (from.Pixel(pFrom.i).x + toDistX);
                double dy = to.Pixel(pTo.i).y - (from.Pixel(pFrom.i).y + toDistY);
                double lastDist = Math.Sqrt(dx * dx + dy * dy);

                while (fromTree.HasMore())
                {
                    POS candidate = fromTree.PollGreatest();
                    cache.Add(candidate);

                    if (Math.Abs(candidate.value - max) > M)
                        break;

                    dx = to.Pixel(pTo.i).x - (from.Pixel(candidate.i).x + toDistX);
                    dy = to.Pixel(pTo.i).y - (from.Pixel(candidate.i).y + toDistY);
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist < lastDist)
                    {
                        lastDist = dist;
                        pFrom = candidate;
                    }
                }

                COORDINATE t = to.Pixel(pTo.i);
                positions.Set(pFrom.i, t.x, t.y);
                tosThatHasBeenPlaced[pFrom.i] = true;

                for (int i = 0; i < cache.Count; i++)
                {
                    POS p = cache[i];
                    if (p != pFrom)
                    {
                        fromTree.Add(p);
                    }
                }
            }
        }

        /**
         * will interpolate between two positions, returns true if the old from is not the same as to
         * @param from
         * @param to
         * @return
         */
        public bool Merge(DivFormationImp from, DivFormationImp to)
        {
            bool move = false;

            if (from.deployed() != to.deployed())
                return false;

            for (int i = 0; i < to.deployed(); i++)
            {
                double tx = to.Pixel(i).x;
                double ty = to.Pixel(i).y;
                if (i >= from.deployed())
                {
                    move = true;
                    from.Set(i, (int)tx, (int)ty);
                }

                double fx = from.Pixel(i).x;
                double fy = from.Pixel(i).y;
                int size = from.Formation().Size(div);
                if (fx != tx || fy != ty)
                {
                    move = true;
                    double mag = vec.Set(fx, fy, tx, ty);
                    if (mag > C.TILE_SIZE)
                        mag = C.TILE_SIZE;
                    int nx = (int)(fx + vec.nX() * mag);
                    int ny = (int)(fy + vec.nY() * mag);
                    if (DivPlacability.PixelIsBlocked(nx, ny, size, Plan.a))
                    {
                        from.Set(i, (int)tx, (int)ty);
                    }
                    else
                    {
                        from.Set(i, (int)nx, (int)ny);
                    }
                }
            }
            if (move)
            {
                from.Init(to.deployed());
                from.DeployFinish(pu.filler, div.info);
            }
            return move;
        }

        public bool MergeNeeds(DivFormationImp from, DivFormationImp to)
        {
            for (int i = 0; i < to.deployed(); i++)
            {
                double fx = from.Pixel(i).x;
                double fy = from.Pixel(i).y;
                double tx = to.Pixel(i).x;
                double ty = to.Pixel(i).y;
                if (fx != ty || fy != ty)
                {
                    double mag = vec.Set(fx, fy, tx, ty);
                    if (mag > C.TILE_SIZE * 2)
                        return true;
                }
            }
            return false;
        }

        private class POS
        {
            public int i;
            public double value;
        }

        private class TREE : Tree<POS>
        {
            public TREE(int size) : base(size) { }

            protected override bool IsGreaterThan(POS current, POS cmp)
            {
                return current.value > cmp.value;
            }
        }
    }
}