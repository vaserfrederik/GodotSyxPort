using System;
using System.Collections.Generic;
using game;
using settlement.path;
using settlement.room.sprite;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.sets;

namespace settlement.room.main.furnisher
{
    public sealed class FurnisherItem : INDEXED, MAP_OBJECT<FurnisherItemTile>
    {
        private readonly FurnisherItemTile[][] tiles;
        public readonly double multiplierStats;
        public readonly double multiplierCosts;
        private readonly int id;
        private readonly int firstX;
        private readonly int firstY;
        public readonly FurnisherItemGroup group;
        private static readonly ArrayList<FurnisherItem> itemsTmp = new ArrayList<FurnisherItem>(127);
        static FurnisherItem()
        {
            new GameDisposable()
            {
                protected override void Dispose()
                {
                    itemsTmp.ClearSloppy();
                }
            };
        }
        public readonly int area;
        private readonly int[] brokenResourceAmount;
        public readonly int rotation;
        private readonly int size;
        private readonly int reachableTiles;

        public FurnisherItem(int size, FurnisherItemTile[][] its, double multiplierStats, double multiplierCosts, FurnisherItemGroup group, int index, int rot)
        {
            this.size = size;
            this.id = index;
            tiles = its;
            this.multiplierStats = multiplierStats;
            this.multiplierCosts = multiplierCosts;
            this.group = group;
            this.rotation = rot;
            int fx = -1;
            int fy = -1;
            int a = 0;
            for (int y = 0; y < its.Length; y++)
                for (int x = 0; x < its[0].Length; x++)
                {
                    if (tiles[y][x] != null && tiles[y][x].sprite() != null)
                    {
                        a++;
                        if (fx == -1 && fy == -1)
                        {
                            fx = x;
                            fy = y;
                        }
                    }
                }
            if (fx == -1 || fy == -1)
            {
                for (int y = 0; y < its.Length; y++)
                    for (int x = 0; x < its[0].Length; x++)
                    {
                        if (tiles[y][x] != null)
                        {
                            a++;
                            if (fx == -1 && fy == -1)
                            {
                                fx = x;
                                fy = y;
                            }
                        }
                    }
            }
            if (fx == -1 || fy == -1)
                throw new Exception();

            area = a;
            firstX = fx;
            firstY = fy;

            if (group != null)
            {
                brokenResourceAmount = Alloc.ii(group.blueprint.resources());
                for (int i = 0; i < group.blueprint.resources(); i++)
                {
                    brokenResourceAmount[i] = (int)Math.Ceiling(group.costs[i] * multiplierCosts / area);
                }
            }
            else
            {
                brokenResourceAmount = null;
            }

            int re = 0;
            for (int y = 0; y < its.Length; y++)
                for (int x = 0; x < its[0].Length; x++)
                {
                    FurnisherItemTile t = get(x, y);
                    if (t == null)
                        continue;
                    if (t.availability.player <= AVAILABILITY.ROOM.player)
                        re++;
                    else
                    {
                        foreach (DIR d in DIR.ORTHO)
                        {
                            t = get(x, y, d);
                            if (t == null || t.availability.player <= AVAILABILITY.ROOM.player)
                            {
                                re++;
                                break;
                            }
                        }
                    }
                }
            reachableTiles = re;
        }

        public FurnisherItem(FurnisherItemTile[][] its, double multiplierCosts, double multiplierStats)
        {
            this(0, its, multiplierStats, multiplierCosts, null, 0, 0);
            itemsTmp.Add(this);
        }

        public FurnisherItem(FurnisherItemTile[][] its, double multiplier) : this(its, multiplier, multiplier) { }

        public int width()
        {
            return tiles[0].Length;
        }

        public int height()
        {
            return tiles.Length;
        }

        public int firstX()
        {
            return firstX;
        }

        public int firstY()
        {
            return firstY;
        }

        public FurnisherItemGroup group()
        {
            return group;
        }

        public double cost2(int index, int upgrade)
        {
            return (group.costs[index] * multiplierCosts) * group.blueprint.blue().upgrades().resMask(upgrade, index);
        }

        public double costFlat(int index)
        {
            return group.costs[index] * multiplierCosts;
        }

        public double stat(FurnisherStat stat)
        {
            return group.stats[stat.index()] * multiplierStats;
        }

        public int brokenResourceAmount(int index)
        {
            return brokenResourceAmount[index];
        }

        public string placable(int tx1, int ty1)
        {
            return null;
        }

        public int index()
        {
            return id;
        }

        public FurnisherItemTile get(int tile)
        {
            throw new Exception();
        }

        public int reachableTiles()
        {
            return reachableTiles;
        }

        public FurnisherItemTile get(int tx, int ty)
        {
            if (tx < 0 || tx >= width())
                return null;
            if (ty < 0 || ty >= height())
                return null;
            return tiles[ty][tx];
        }

        public RoomSprite sprite(int tx, int ty)
        {
            FurnisherItemTile t = get(tx, ty);
            if (t != null)
                return t.sprite();
            return null;
        }

        public int variation()
        {
            return size;
        }
    }
}