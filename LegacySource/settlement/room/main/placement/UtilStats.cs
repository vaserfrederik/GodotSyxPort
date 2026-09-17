using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Placement
{
    public class UtilStats
    {
        private double[] stats = new double[8];
        private double[] statsR = new double[8];
        private double[] needed = new double[8];
        private double[] allocated = new double[8];
        private double[] resRemoved = new double[8];
        private int[] placedGroups = new int[16];
        private readonly RoomPlacer p;
        public int walls;
        public int items;
        private int tick = -1;

        public UtilStats(RoomPlacer p)
        {
            this.p = p;
        }

        public int Needed(int ri)
        {
            Update();
            return (int)Math.Ceiling(needed[ri]);
        }

        public int Allocated(int ri)
        {
            Update();
            return (int)Math.Round(allocated[ri]);
        }

        public double Stat(int si)
        {
            Update();
            return statsR[si];
        }

        public int Groups(FurnisherItemGroup g)
        {
            Update();
            return placedGroups[g.Index()];
        }

        public void RemoveTile(int tx, int ty)
        {
            SETT.Floor().Clearer.Clear(tx, ty);
            for (int i = 0; i < p.Blueprint().Constructor().Resources(); i++)
            {
                resRemoved[i] += p.Blueprint().Constructor().AreaCost(i, p.Instance.Upgrade()) * 0.75;
                if (resRemoved[i] >= 1)
                {
                    SETT.Things().Resources.Create(tx, ty, p.Blueprint().Constructor().Resource(i), (int)resRemoved[i]);
                    GAME.Player().Res().Inc(p.Blueprint().Constructor().Resource(i), RTYPE.CONSTRUCTION, (int)resRemoved[i]);
                    resRemoved[i] -= (int)resRemoved[i];
                }
            }
        }

        public void RemoveItem(int tx, int ty, FurnisherItem it)
        {
            for (int i = 0; i < p.Blueprint().Constructor().Resources(); i++)
            {
                resRemoved[i] += it.Cost2(i, p.Instance.Upgrade()) * 0.75;
                if (resRemoved[i] >= 1)
                {
                    SETT.Things().Resources.Create(tx, ty, p.Blueprint().Constructor().Resource(i), (int)resRemoved[i]);
                    GAME.Player().Res().Inc(p.Blueprint().Constructor().Resource(i), RTYPE.CONSTRUCTION, (int)resRemoved[i]);
                    resRemoved[i] -= (int)resRemoved[i];
                }
            }
        }

        public void Updatee()
        {
            tick = GAME.UpdateI() - 1;
            Update();
        }

        private void Update()
        {
            if (tick == GAME.UpdateI())
                return;

            tick = GAME.UpdateI();
            items = 0;
            if (p.Blueprint() == null)
                return;
            Furnisher b = p.Blueprint().Constructor();
            if (b == null)
            {
                GAME.Notify("? " + p.Blueprint().Info.Name);
            }
            MapDataF d = SETT.Rooms().FData;
            AREA a = p.Instance;

            for (int i = 0; i < needed.Length; i++)
            {
                needed[i] = 0;
                stats[i] = 0;
                allocated[i] = 0;
                statsR[i] = 0;
            }
            for (int i = 0; i < placedGroups.Length; i++)
                placedGroups[i] = 0;

            walls = p.AutoWalls.Is() ? p.Door.GetWalls() : 0;
            int floored = 0;
            foreach (COORDINATE c in a.Body())
            {
                if (!a.Is(c))
                    continue;
                if (ConstructionData.DFloored.Is(c, 1))
                    floored++;

                if (d.IsMaster.Is(c))
                {
                    items++;
                    FurnisherItem it = d.Item.Get(c);
                    if (it == null)
                    {
                        Console.Error.WriteLine(p.Blueprint().Info.Name + " " + d.ItemIndexx.Get(c));
                    }

                    placedGroups[it.Group().Index()]++;
                    for (int i = 0; i < b.Resources(); i++)
                    {
                        needed[i] += it.Cost2(i, p.Instance.Upgrade());
                        if (ConstructionData.DConstructed.Is(c, 1) && ConstructionData.DBroken.Is(c, 0))
                            allocated[i] += it.Cost2(i, p.Instance.Upgrade());
                    }
                    foreach (FurnisherStat s in b.Stats())
                    {
                        stats[s.Index()] += it.Stat(s);
                    }
                }
            }

            for (int i = 0; i < b.Resources(); i++)
            {
                allocated[i] = (int)Math.Ceiling(allocated[i]);

                needed[i] += Math.Ceiling(a.Area() * b.AreaCost(i, p.Instance.Upgrade()));

                allocated[i] += Math.Ceiling(floored * b.AreaCost(i, p.Instance.Upgrade()));
            }
            foreach (FurnisherStat s in b.Stats())
            {
                statsR[s.Index()] = s.Get(a, stats);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < needed.Length; i++)
            {
                needed[i] = 0;
                stats[i] = 0;
                allocated[i] = 0;
                resRemoved[i] = 0;
                statsR[i] = 0;
            }
            for (int i = 0; i < placedGroups.Length; i++)
                placedGroups[i] = 0;
            items = 0;
        }

        public double StatIncr(FurnisherItem it, FurnisherStat s)
        {
            double pp = stats[s.Index()];
            double old = statsR[s.Index()];
            stats[s.Index()] += it.Stat(s);
            double n = s.Get(p.Instance, stats);
            stats[s.Index()] = pp;
            return n - old;
        }
    }
}