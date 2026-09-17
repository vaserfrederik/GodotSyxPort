using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Battle.Thread.Order
{
    public class Tools
    {
        public readonly PathUtilOnline pather = new PathUtilOnline(SETT.TWIDTH);
        public readonly DivDeployer deployer = new DivDeployer(pather);
        public readonly PathCost pathCost = new PathCost();
        public readonly CircleCooIterator circle = new CircleCooIterator(25, pather.getFlooder());

        public readonly ToolMover mover = new ToolMover(pather);
        public readonly ToolsDiv div = new ToolsDiv(this);
        public readonly ToolsWalk walk = new ToolsWalk(this);
        public readonly Arranger arranger = new Arranger();
        public readonly Columns columns = new Columns();

        public Tools(PlanData[] all)
        {
        }

        public int[] ArrangeFromFront(DivFormation f)
        {
            return arranger.GetArrangedPointsForward(f);
        }

        public LIST<Pos> GetPosColumnSort(DivFormation f)
        {
            return columns.SortByColumnRow(f);
        }

        public LIST<Pos> GetPosRowsSort(DivFormation f)
        {
            return columns.SortByRow(f);
        }

        private class Arranger
        {
            private readonly Tree<Point> tree = new Tree<Point>(Config.battle().MEN_PER_DIVISION)
            {
                protected override bool IsGreaterThan(Point current, Point cmp)
                {
                    return current.value > cmp.value;
                }
            };

            private readonly Point[] points = new Point[Config.battle().MEN_PER_DIVISION];
            private int[] arranged = Alloc.ii(Config.battle().MEN_PER_DIVISION);

            public Arranger()
            {
                for (int i = 0; i < points.Length; i++)
                    points[i] = new Point();
            }

            public int[] GetArrangedPointsForward(DivFormation f)
            {
                tree.Clear();
                arranged.Fill(0);
                double lineX1 = f.Start().x();
                double lineY1 = f.Start().y();
                double lineDirX = f.dx();
                double lineDirY = f.dy();

                for (int i = 0; i < f.deployed(); i++)
                {
                    Point p = points[i];
                    p.index = i;
                    p.value = CalculateDistanceToLine(f.px(i), f.py(i), lineX1, lineY1, lineDirX, lineDirY);
                    tree.Add(p);
                }

                int i = 0;
                while (tree.hasMore())
                {
                    Point p = tree.PollSmallest();
                    arranged[i++] = p.index;
                }
                return arranged;
            }

            private static double CalculateDistanceToLine(double pointX, double pointY,
                                                        double lineX1, double lineY1,
                                                        double lineDirX, double lineDirY)
            {
                // Line equation coefficients (Ax + By + C = 0)
                double A = lineDirY; // Coefficient of x
                double B = -lineDirX; // Coefficient of y
                double C = -(A * lineX1 + B * lineY1); // Constant term

                // Distance formula
                return Math.Abs(A * pointX + B * pointY + C) / Math.Sqrt(A * A + B * B);
            }

            private class Point
            {
                public int index;
                public double value;
            }
        }

        private class Columns
        {
            private readonly VectorImp vec = new VectorImp();
            private readonly ArrayList<Pos> all = new ArrayList<Pos>(Config.battle().MEN_PER_DIVISION);
            private readonly ArrayList<Pos> res = new ArrayList<Pos>(Config.battle().MEN_PER_DIVISION);

            private readonly Tree<Pos> tree = new Tree<Pos>(Config.battle().MEN_PER_DIVISION)
            {
                protected override bool IsGreaterThan(Pos current, Pos cmp)
                {
                    return current.value > cmp.value;
                }
            };

            public Columns()
            {
                while (all.HasRoom())
                    all.Add(new Pos(all.Size()));
            }

            private ArrayList<Pos> Cols(DivFormation f)
            {
                res.ClearSloppy();

                vec.Set(f.dx(), f.dy());
                vec.Rotate90().Rotate90().Rotate90();
                double dx = vec.nX();
                double dy = vec.nY();

                double minRow = double.MaxValue;
                double minCol = double.MaxValue;
                for (int i = 0; i < f.deployed(); i++)
                {
                    Pos p = all.Get(i);
                    double px = f.px(i);
                    double py = f.py(i);

                    double x = px;
                    double y = py;

                    p.rowI = (int)Math.Round(dx * x + dy * y);
                    p.columnI = (int)Math.Round(-dy * x + dx * y);
                    minRow = Math.Min(p.rowI, minRow);
                    minCol = Math.Min(minCol, p.columnI);
                    res.Add(p);
                }

                foreach (Pos p in res)
                {
                    p.columnI -= minCol;
                    p.rowI -= minRow;
                }

                return res;
            }

            public ArrayList<Pos> SortByRow(DivFormation f)
            {
                Cols(f);
                tree.Clear();
                foreach (Pos p in res)
                {
                    p.value = -p.rowI;
                    tree.Add(p);
                }
                res.ClearSloppy();
                while (tree.hasMore())
                {
                    res.Add(tree.PollGreatest());
                }
                return res;
            }

            public ArrayList<Pos> SortByColumnRow(DivFormation f)
            {
                Cols(f);
                tree.Clear();
                foreach (Pos p in res)
                {
                    p.value = p.columnI * C.TILE_SIZE * 200 + p.rowI;
                    tree.Add(p);
                }
                res.ClearSloppy();
                while (tree.hasMore())
                {
                    res.Add(tree.PollGreatest());
                }
                return res;
            }
        }

        public class Pos
        {
            public int columnI;
            public int rowI;
            public readonly int pos;
            private double value;

            private Pos(int i)
            {
                this.pos = i;
            }
        }
    }
}