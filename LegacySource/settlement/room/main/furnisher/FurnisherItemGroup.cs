using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Furnisher
{
    public sealed class FurnisherItemGroup : INDEXED
    {
        private readonly FurnisherItem[][] items;
        public readonly string name;
        public readonly string desc;
        public readonly int max;
        public readonly int min;
        public readonly Furnisher blueprint;
        private readonly double[] costs;
        private readonly double[] stats;
        private readonly int index;

        public FurnisherItemGroup(Furnisher b, int rots, string name, string desc, int min, int max, double[] costs, double[] stats)
        {
            if (rots >= 4)
                throw new RuntimeException();

            this.blueprint = b;
            if (FurnisherItem.itemsTmp.Count == 0)
                throw new RuntimeException("No items declared");
            this.costs = costs;
            this.stats = stats;
            this.name = name;
            this.desc = desc;
            this.items = new FurnisherItem[FurnisherItem.itemsTmp.Count][rots + 1];
            index = b.pgroups.Add(this);
            this.max = max;
            this.min = min;
            for (int s = 0; s < FurnisherItem.itemsTmp.Count; s++)
            {
                FurnisherItem copy = FurnisherItem.itemsTmp[s];
                FurnisherItem item = new FurnisherItem(s, copy.tiles, copy.multiplierStats, copy.multiplierCosts, this, b.allItems.Count, 0);
                b.allItems.Add(item);

                this.items[s][0] = item;
                for (int r = 1; r <= rots; r++)
                    this.items[s][r] = GetRot(s, item, r);
            }
            FurnisherItem.itemsTmp.Clear();
        }

        public string Name()
        {
            return name;
        }

        public string Desc()
        {
            return desc;
        }

        private FurnisherItem GetRot(int size, FurnisherItem other, int rot)
        {
            FurnisherItemTile[][] its = new FurnisherItemTile[other.tiles.Length][];
            for (int i = 0; i < other.tiles.Length; i++)
                its[i] = (FurnisherItemTile[])other.tiles[i].Clone();

            int r = rot;
            while (rot > 0)
            {
                its = Rotate(its);
                rot--;
            }
            FurnisherItem item = new FurnisherItem(size, its, other.multiplierStats, other.multiplierCosts, this, blueprint.allItems.Count, r);

            blueprint.allItems.Add(item);
            return item;
        }

        private FurnisherItemTile[][] Rotate(FurnisherItemTile[][] l)
        {
            int M = l.Length;
            int N = l[0].Length;
            FurnisherItemTile[][] ret = new FurnisherItemTile[N][M];
            for (int r = 0; r < M; r++)
            {
                for (int c = 0; c < N; c++)
                {
                    ret[c][M - 1 - r] = l[r][c];
                }
            }
            return ret;
        }

        public FurnisherItem Item(int size, int rot)
        {
            return items[size][rot];
        }

        public int Size()
        {
            return items.Length;
        }

        public int Rotations()
        {
            return items[0].Length;
        }

        public override int Index()
        {
            return index;
        }

        public double Cost(int ri, int upgrade)
        {
            return costs[ri] * blueprint.Blue().Upgrades().ResMask(upgrade, ri);
        }

        public double Stat(int si)
        {
            return stats[si];
        }
    }
}