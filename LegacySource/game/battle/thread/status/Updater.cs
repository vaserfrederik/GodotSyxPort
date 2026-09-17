using System;
using System.Collections.Generic;
using System.Linq;

namespace game.battle.thread.status
{
    public class Updater
    {
        volatile bool stop = false;
        private readonly List<Div> list = new List<Div>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly Surrounder surrounder = new Surrounder();
        private readonly Flanker flanker = new Flanker();

        public Updater()
        {
        }

        public void Init(BattleContext u)
        {
            u.map.Clear();
            u.quads.Clear();
            u.space.Clear();
            foreach (DivStatus s in u.statuses)
            {
                s.Clear();
            }

            AddToMaps(u);
            SetStats(u);
            AddArtillery(u);
        }

        private void AddToMaps(BattleContext u)
        {
            for (short i = 0; i < u.statuses.Length; i++)
            {
                if (stop)
                    return;
                Div d = GAME.ARMIES().Division(i);
                if (d.MenNrOf() == 0)
                    continue;
                DivPositionCopyable pos = d.Current();
                if (pos.Deployed() == 0)
                    continue;

                u.map.Add(i, pos);
                u.space.Add(d, pos);
                u.army.Add(d, pos);
                u.quads.Add(d, d.Centre().CUnitX(), d.Centre().CUnitY());
            }
        }

        private void SetStats(BattleContext u)
        {
            for (short i = 0; i < u.statuses.Length; i++)
            {
                if (stop)
                    return;
                Div d = GAME.ARMIES().Division(i);
                if (d.MenNrOf() == 0)
                    continue;
                DivPosition pos = d.Current();
                if (pos.Deployed() == 0)
                    continue;

                DivStatus s = u.statuses[i];

                double friends = d.Settings().GetPower();

                list.Clear();

                double distMax = 300 * C.TILE_SIZE;
                surrounder.Init();
                u.quads.GetNearest(list, d.Centre().CUnitX(), d.Centre().CUnitY(), (int)distMax, d.ArmyEnemy(), d);

                double distMin = distMax * 0.5;
                byte threatDirs = 0;
                double enemyThreats = 0;
                int k = 0;
                foreach (Div e in list)
                {
                    int dx = e.Centre().CUnitX() - d.Centre().CUnitX();
                    int dy = e.Centre().CUnitY() - d.Centre().CUnitY();
                    double dist = Math.Sqrt(dx * dx + dy * dy);
                    if (dist > distMax)
                        continue;

                    double threat = Math.Max(e.Settings().GetPower() * (1.0 - dist / distMax), 0);

                    surrounder.Add(e.Centre().CUnitX(), e.Centre().CUnitY(), threat);
                    enemyThreats += threat;
                    DIR dir = DIR.Get(dx, dy);
                    if (dist < distMin)
                    {
                        if (dir.IsOrtho())
                            threatDirs |= dir.Mask();
                        else
                            threatDirs |= dir.Mask() << 4;
                    }
                    if (k < DivStatus.iSize)
                    {
                        s.EnemiesClosestSet(e.Index(), (int)(Math.Sqrt(dist) * C.ITILE_SIZE));
                        k++;
                    }
                }

                double encirclement = surrounder.GetValue(d.Centre().CUnitX(), d.Centre().CUnitY());

                s.Encirclement = encirclement;

                s.EnemyDirMask = threatDirs;
                s.EnemyThreats = enemyThreats;

                list.Clear();
                u.quads.GetNearest(list, d.Centre().CUnitX(), d.Centre().CUnitY(), (int)distMax, d.Army(), d);
                k = 0;
                foreach (Div e in list)
                {
                    s.FriendlyClosestSet(e.Index());

                    int dx = e.Centre().CUnitX() - d.Centre().CUnitX();
                    int dy = e.Centre().CUnitY() - d.Centre().CUnitY();
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < distMax)
                    {
                        friends += Math.Max(e.Settings().GetPower() * (1.0 - dist / distMax), 0);
                    }
                }
                s.Friends = friends;

                s.Flanks = flanker.Get2(u, d, pos);
            }
        }

        private readonly ListResize<ArtilleryInstance> arts = new ListResize<ArtilleryInstance>(256);

        private void AddArtillery(BattleContext u)
        {
            for (int bi = 0; bi < SETT.ROOMS().ARTILLERY.Size; bi++)
            {
                ROOM_ARTILLERY ab = SETT.ROOMS().ARTILLERY.Get(bi);
                arts.ClearSoft();
                ab.ThreadInstances(arts);
                foreach (ArtilleryInstance ins in arts)
                {
                    u.quads.AddArtillery(ins);
                }
            }
        }

        private class Flanker
        {
            int size = Math.Max(Config.battle().DIVISIONS_PER_ARMY, Config.battle().MEN_PER_DIVISION);
            private readonly int[] engagedX = new int[size];
            private readonly int[] engagedY = new int[size];

            private readonly int[] dirs = new int[DIR.ALL.Size];

            private readonly MapInt intmap = new MapInt();

            public double Get2(BattleContext u, Div d, DivPosition pos)
            {
                int deployed = pos.Deployed();
                if (deployed == 0)
                    return 0;

                int centreX = 0;
                int centreY = 0;
                int soldiersTotal = 0;
                int engaged = 0;

                for (int pi = 0; pi < pos.Deployed(); pi++)
                {
                    int px = pos.Px(pi);
                    int py = pos.Py(pi);
                    if (Enemy(u, d, px, py))
                    {
                        engagedX[engaged] = px;
                        engagedY[engaged] = py;
                        engaged++;
                        centreX += px;
                        centreY += py;
                        soldiersTotal++;
                    }
                    else if (d.Reporter.Reachable(px, py))
                    {
                        centreX += px;
                        centreY += py;
                        soldiersTotal++;
                    }
                }

                if (soldiersTotal == 0)
                    return 0;

                centreX /= soldiersTotal;
                centreY /= soldiersTotal;

                intmap.Clear();
                double totalAmount = 0;

                for (int i = 0; i < engaged; i++)
                {
                    int x = engagedX[i];
                    int y = engagedY[i];
                    double amount = d.Settings().GetPowerAt(x, y);
                    totalAmount += amount;
                    intmap.Add(x, y, amount);
                }

                if (totalAmount == 0)
                    return 0;

                intmap.SortByAmountDescending();

                double directionX = 0;
                double directionY = 0;

                foreach (var entry in intmap)
                {
                    directionX += (entry.Key - centreX) * entry.Value;
                    directionY += (entry.Value - centreY) * entry.Value;
                }

                double magnitude = Math.Sqrt(directionX * directionX + directionY * directionY);
                if (magnitude == 0)
                    return 0;

                directionX /= magnitude;
                directionY /= magnitude;

                double totalThreat = 0;

                for (int i = 0; i < engaged; i++)
                {
                    int x = engagedX[i];
                    int y = engagedY[i];
                    double threat = d.Settings().GetThreatAt(x, y, directionX, directionY);
                    totalThreat += threat;
                }

                return totalThreat;
            }

            private bool Enemy(BattleContext u, Div d, int px, int py)
            {
                for (int di = 0; di < DIR.ALL.Size; di++)
                {
                    DIR dir = DIR.ALL.Get(di);
                    int tx = (int)((px + dir.XN() * C.TILE_SIZE) / C.TILE_SIZE);
                    int ty = (int)((py + dir.YN() * C.TILE_SIZE) / C.TILE_SIZE);
                    if (u.map.HasEnemy(tx, ty, d.Army()))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private class Surrounder
        {
            private readonly VectorImp vec = new VectorImp();
            int size = Math.Max(Config.battle().DIVISIONS_PER_ARMY, Config.battle().MEN_PER_DIVISION);
            int current = 0;
            private readonly double[] dxs = new double[size];
            private readonly double[] dys = new double[size];
            private readonly double[] amounts = new double[size];

            public void Init()
            {
                current = 0;
            }

            public void Add(double px, double py, double amount)
            {
                dxs[current] = px;
                dys[current] = py;
                amounts[current] = amount;
                current++;
            }

            public double GetValue(double cx, double cy)
            {
                if (current == 0)
                    return 0;

                ConvertToVectors(cx, cy);

                double xs = 0;
                double ys = 0;
                double am = 0;
                for (int i = 0; i < current; i++)
                {
                    xs += dxs[i] * amounts[i];
                    ys += dys[i] * amounts[i];
                    am += amounts[i];
                }

                if (am == 0)
                    return 0;

                xs /= am;
                ys /= am;
                if (xs == 0 && ys == 0)
                {
                    xs = dxs[0];
                    ys = dys[0];
                }

                vec.Set(xs, ys);
                xs = vec.NX();
                ys = vec.NY();

                double v = 0;

                for (int i = 0; i < current; i++)
                {
                    double dot = dxs[i] * xs + dys[i] * ys;
                    if (dot < -0.6)
                    {
                        dot = -dot;
                        dot /= 0.4;

                        v += dot * amounts[i];
                    }
                }
                return v;
            }

            private void ConvertToVectors(double cx, double cy)
            {
                for (int i = 0; i < current; i++)
                {
                    double dx = dxs[i] - cx;
                    double dy = dys[i] - cy;
                    vec.Set(dx, dy);
                    dx = vec.NX();
                    dy = vec.NY();
                    dxs[i] = vec.NX();
                    dys[i] = vec.NY();
                }
            }
        }
    }
}