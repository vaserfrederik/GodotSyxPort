using System;
using System.IO;
using System.Collections.Generic;

namespace Settlement.Room.Home.House
{
    public class HomeContructor : Furnisher
    {
        private readonly ROOM_HOME blue;

        public readonly FurnisherStat occupants = new FurnisherStat.FurnisherStatI(this, 1);

        public readonly int[][] maxOccupants = new int[][]
        {
            new int[] { 3, 4, 5 },
            new int[] { 5, 7, 9 },
            new int[] { 10, 14, 18 }
        };

        static readonly int entrance = 2;

        public readonly FurnisherItemTile tOpening;
        public readonly Sprites sp;
        public readonly Floor flooring;

        protected HomeContructor(RoomInitData init, ROOM_HOME blue)
            : base(init, 3, 1)
        {
            flooring = floors[0];
            sp = new Sprites(init.data());

            this.blue = blue;
            var ee = new FurnisherItemTile(this, true, sp.theDummy, AVAILABILITY.ROOM, false);
            ee.setData(entrance);
            ee.noWalls = true;
            tOpening = ee;
            var __ = new FurnisherItemTile(this, false, sp.theDummy, AVAILABILITY.ROOM, false);
            __.setData(1);
            var xx = new FurnisherItemTile(this, false, sp.theDummy, AVAILABILITY.NOT_ACCESSIBLE, false);

            Console.WriteLine();

            Create(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { xx, xx, xx },
                new FurnisherItemTile[] { xx, __, xx },
                new FurnisherItemTile[] { xx, ee, xx }
            }, 9);

            Create(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { xx, xx, xx },
                new FurnisherItemTile[] { xx, __, xx },
                new FurnisherItemTile[] { xx, __, xx },
                new FurnisherItemTile[] { xx, __, xx },
                new FurnisherItemTile[] { xx, ee, xx }
            }, 15);

            Create(new FurnisherItemTile[][]
            {
                new FurnisherItemTile[] { xx, xx, xx, xx, xx },
                new FurnisherItemTile[] { xx, __, __, __, xx },
                new FurnisherItemTile[] { xx, __, __, __, xx },
                new FurnisherItemTile[] { xx, __, __, __, xx },
                new FurnisherItemTile[] { xx, __, __, __, xx },
                new FurnisherItemTile[] { xx, xx, ee, xx, xx }
            }, 30);
        }

        private void Create(FurnisherItemTile[][] tt, int am)
        {
            am++;
            new FurnisherItem(tt, 1);
            for (int i = 2; i < am; i++)
            {
                FurnisherItemTile[][] tn = new FurnisherItemTile[tt.Length][tt[0].Length * i];
                for (int y = 0; y < tt.Length; y++)
                {
                    for (int x = 0; x < tn[0].Length; x++)
                    {
                        tn[y][x] = tt[y][x % tt[0].Length];
                    }
                }
                new FurnisherItem(tn, i);
            }
            Flush(3);
        }

        public override bool MustBeIndoors()
        {
            return true;
        }

        public override bool MustBeOutdoors()
        {
            return false;
        }

        public override bool UsesArea()
        {
            return false;
        }

        public override Room Create(TmpArea area, RoomInit init)
        {
            HomeInstance i = new HomeInstance(blue, area);

            foreach (var c in i.body())
            {
                for (int di = 0; di < DIR.ALL.Count; di++)
                {
                    int x = c.x + DIR.ALL[di].x;
                    int y = c.y + DIR.ALL[di].y;
                    SETT.TERRAIN().Get(x, y).PlaceFixed(x, y);
                }
            }
            return i;
        }

        public override FurnisherItem SecretReplacementItem(int rot, FurnisherItem it)
        {
            return it.Group.item(0, rot);
        }

        public override RoomBlueprintImp Blue()
        {
            return blue;
        }

        public override bool NeedsIsolation()
        {
            return true;
        }

        public override RoomState GetConstructionState()
        {
            return new HomeInstance.State(null);
        }
    }
}