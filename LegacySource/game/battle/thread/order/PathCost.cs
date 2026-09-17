using System;
using System.Collections.Generic;

namespace Game.Battle.Thread.Order
{
    using static Settlement.Main.SETT;

    using Game.Battle.Div;
    using Game.Battle.Formation;
    using Game.Battle.Thread.Order.BattleOrderUpdater;
    using Game.Battle.Thread.Status;
    using Settlement.Main;
    using Settlement.Path;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Sets;

    internal sealed class PathCost
    {
        private readonly Bitmap1D map = new Bitmap1D(TAREA / 4, false);
        private readonly ArrayList<Div> list = new ArrayList<Div>(8);

        public void Init(DivPositionImp ok, int dx, int dy)
        {
            map.Clear();
            Add(ok);

            list.Clear();
            BattleStatus.Map().Get(list, dx, dy);

            foreach (Div d in list)
            {
                Add(d.Current());
            }
        }

        private void Add(DivPositionImp ok)
        {
            for (int i = 0; i < ok.Deployed(); i++)
            {
                int tx = ok.Tile(i).X;
                int ty = ok.Tile(i).Y;

                for (int y = -DivsSpaceMap.Radius; y <= DivsSpaceMap.Radius; y++)
                {
                    for (int x = -DivsSpaceMap.Radius; x <= DivsSpaceMap.Radius; x++)
                    {
                        int r = Math.Abs(x) + Math.Abs(y);
                        if (r <= DivsSpaceMap.Radius + 1)
                        {
                            int dx = tx + x;
                            int dy = ty + y;
                            if (IN_BOUNDS(dx, dy))
                            {
                                map.Set((dx >> 1) + (dy >> 2) * TWIDTH, true);
                            }
                        }
                    }
                }
            }
        }

        public double Cost(int fx, int fy, DIR d)
        {
            return Cost(fx, fy, fx + d.X, fy + d.Y);
        }

        public double Cost(int fx, int fy, int tx, int ty)
        {
            if (!IN_BOUNDS(tx, ty))
                return -1;
            AVAILABILITY a = PATH().GetAvailability(fx, ty);
            if (a.IsSolid(Plan.A))
                return -1;
            a = PATH().GetAvailability(tx, fy);
            if (a.IsSolid(Plan.A))
                return -1;
            a = PATH().GetAvailability(tx, ty);
            if (a.IsSolid(Plan.A))
                return -1;

            int t = tx + ty * TWIDTH;
            double res = 1 + a.MovementSpeedI;
            double space = ENV().Map.SPACE.Get(t);
            if (space < 0.5)
                res += 10 - 10 * space;
            if (map.Get((tx >> 1) + (ty >> 2) * TWIDTH))
                return res;
            return res; // + ArmyAIUtil.Space().Cost.Get(tx, ty);
        }

        public double Cost(int tx, int ty)
        {
            if (!IN_BOUNDS(tx, ty))
                return -1;
            AVAILABILITY a = PATH().GetAvailability(tx, ty);
            if (a.IsSolid(Plan.A))
                return -1;
            int t = tx + ty * TWIDTH;
            double res = 1 + a.MovementSpeedI;
            double space = ENV().Map.SPACE.Get(t);
            if (space < 0.5)
                res += 10 - 10 * space;
            if (map.Get((tx >> 1) + (ty >> 2) * TWIDTH))
                return res;
            return res; // + ArmyAIUtil.Space().Cost.Get(tx, ty);
        }
    }
}