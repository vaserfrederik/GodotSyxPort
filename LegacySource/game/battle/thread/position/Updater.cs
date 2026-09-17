using System;
using game.battle.div;
using game.battle.formation;
using game.battle.thread.position.DivCentres;
using init.constant;
using snake2d.util.datatypes;
using snake2d.util.misc;

namespace game.battle.thread.position
{
    internal sealed class Updater
    {
        public volatile bool Stop = false;
        private readonly Rec body = new Rec();

        public Updater()
        {
        }

        public void Init(Context c)
        {
            for (short di = 0; di < c.Statuses.Length; di++)
            {
                if (Stop)
                    return;
                Div d = GAME.ARMIES().Division(di);
                Init(c, d);
            }
        }

        public void Init(Context c, Div d)
        {
            DivCentre s = c.Statuses[d.Index()];
            s.Clear();

            if (d.MenNrOf() == 0)
                return;
            DivPositionCopyable pos = d.Current();
            if (pos.Deployed() == 0)
                return;
            DivFormation form = d.Position();
            double am = 0;
            for (int i = 0; i < form.Deployed() && i < pos.Deployed(); i++)
            {
                if (!d.Reporter.Reachable(i))
                    continue;
                double dist = COORDINATE.TileDistance(form.Px(i), form.Py(i), pos.Px(i), pos.Py(i));
                if (dist < C.TILE_SIZE)
                {
                    double a = 1.0 - (dist / C.TILE_SIZE);
                    a = CLAMP.D(a, 0, 1);
                    am += a;
                }
            }

            s.InPosition = (short)am;

            int xx = 0;
            int yy = 0;
            am = 0;

            for (int pi = 0; pi < pos.Deployed(); pi++)
            {
                if (d.Reporter.Reachable(pi))
                {
                    int x = pos.Px(pi);
                    int y = pos.Py(pi);
                    if (pi == 0)
                    {
                        body.Clear();
                        body.MoveX1Y1(x, y);
                        body.SetDim(1, 1);
                    }
                    else
                    {
                        body.Unify(x, y);
                    }
                    xx += x;
                    yy += y;
                    am++;
                }
            }

            if (am == 0)
            {
                for (int pi = 0; pi < pos.Deployed(); pi++)
                {
                    int x = pos.Px(pi);
                    int y = pos.Py(pi);
                    if (pi == 0)
                    {
                        body.Clear();
                        body.MoveX1Y1(x, y);
                        body.SetDim(1, 1);
                    }
                    else
                    {
                        body.Unify(x, y);
                    }
                    xx += x;
                    yy += y;
                    am++;
                }
            }

            s.SquareCX = body.CX();
            s.SquareCY = body.CY();

            if (am == 0)
            {
                s.Cx = -1;
                s.Cy = -1;
            }
            else
            {
                xx /= am;
                yy /= am;

                s.CxSoft = xx;
                s.CySoft = yy;
                int best = -1;
                int bestV = int.MaxValue;

                for (int pi = 0; pi < pos.Deployed(); pi++)
                {
                    int dist = Math.Abs(xx - pos.Px(pi)) + Math.Abs(yy - pos.Py(pi));

                    if (!d.Reporter.Reachable(pi))
                    {
                        dist += int.MaxValue / 2;
                    }

                    if (dist < bestV)
                    {
                        best = pi;
                        bestV = dist;
                    }
                }

                xx = pos.Px(best);
                yy = pos.Py(best);
            }

            s.Cx = xx;
            s.Cy = yy;
        }
    }
}