using System;
using System.Collections.Generic;
using System.Linq;

namespace game.battle.thread.status
{
    public sealed class DivsTileMap
    {
        private readonly Tile[] tiles = new Tile[0x0FFFF];
        private int tileNewI = 1;
        private readonly short[] firstTiles;
        private readonly Bitsmap2D[] soldiers;
        private readonly DivStatus[] statuses;
        private static readonly IntChecker checker = new IntChecker(Config.battle().DIVISIONS_PER_ARMY * 2);
        private readonly List<Div> tmp = new List<Div>(16);

        public DivsTileMap(DivStatus[] statuses)
        {
            this.statuses = statuses;

            if (Config.battle().MEN_PER_ARMY * 2 > 0xFFFF)
                throw new Exception("too many men in battle. This class must be fixed! Use ints");

            firstTiles = new short[SETT.TAREA];
            for (int i = 1; i < tiles.Length; i++)
            {
                tiles[i] = new Tile();
            }
        }

        public void Add(short div, DivPositionImp next)
        {
            checker.Init();

            for (int i = 0; i < next.Deployed; i++)
            {
                int x = next.Tx(i);
                int y = next.Ty(i);
                if (SETT.IN_BOUNDS(x, y))
                {
                    Add(x, y, div);
                }
            }
        }

        private void Add(int x, int y, short currentI)
        {
            int tileI = x + y * SETT.TWIDTH;
            Tile first = tiles[firstTiles[tileI] & 0x0FFFF];
            Div current = GAME.ARMIES().Division(currentI);

            soldiers[current.Army().Index].Increment(tileI, 1);

            if (tileNewI > 0x0FFFF)
                return;

            if (first == null)
            {
                firstTiles[tileI] = MakeNewTile((short)0, current);
                tmp.Clear();
                return;
            }

            {
                Tile t = first;
                while (t != null)
                {
                    if (t.DivI == currentI)
                        return;
                    t = tiles[t.Next & 0x0FFFF];
                }
            }

            Tile t = first;

            DivStatus currentOrder = statuses[currentI];

            bool engaged = false;

            while (t != null)
            {
                Div other = GAME.ARMIES().Division(t.DivI);
                if (!engaged && other.Army() != current.Army())
                {
                    currentOrder.Engagements++;
                    statuses[other.Index].Engagements++;
                    engaged = true;
                }

                if (!checker.IsSetAndSet(t.DivI))
                {
                    if (other.Army() == current.Army())
                    {
                        currentOrder.FriendlyCollisionSet(t.DivI);
                        statuses[t.DivI].FriendlyCollisionSet(currentI);
                    }
                    else
                    {
                        currentOrder.EnemyCollisionSet(t.DivI);
                        statuses[t.DivI].EnemyCollisionSet(currentI);
                    }
                }
                t = tiles[t.Next & 0x0FFFF];
            }

            for (int di = 0; di < DIR.ORTHO.Size && !engaged; di++)
            {
                DIR dd = DIR.ORTHO.Get(di);
                if (!SETT.IN_BOUNDS(x, y, dd))
                    continue;

                int ti = tileI + dd.X + dd.Y * SETT.TWIDTH;

                if (ti >= firstTiles.Length)
                    continue;
                first = tiles[firstTiles[ti] & 0x0FFFF];
                while (t != null)
                {
                    Div other = GAME.ARMIES().Division(t.DivI);
                    if (other.Army() != current.Army())
                    {
                        currentOrder.Engagements++;
                        statuses[other.Index].Engagements++;
                        engaged = true;
                        break;
                    }
                    t = tiles[t.Next & 0x0FFFF];
                }
            }

            firstTiles[tileI] = MakeNewTile(firstTiles[tileI], current);
        }

        private short MakeNewTile(short next, Div div)
        {
            short i = (short)tileNewI;
            Tile t = tiles[tileNewI & 0x0FFFF];
            t.Next = next;
            t.DivI = div.Index;
            tileNewI++;
            return i;
        }

        public IEnumerable<Div> Get(List<Div> res, int tx, int ty)
        {
            return Get(res, tx, ty, Armies.ARMIES_BITS);
        }

        public IEnumerable<Div> Get(List<Div> res, int tx, int ty, DIR d)
        {
            tx += d.X;
            ty += d.Y;
            return Get(res, tx, ty);
        }

        public IEnumerable<Div> GetAlly(List<Div> res, int tx, int ty, Army a)
        {
            return Get(res, tx, ty, a.Bit);
        }

        public IEnumerable<Div> GetEnemy(List<Div> res, int tx, int ty, Army a)
        {
            return Get(res, tx, ty, ~a.Bit);
        }

        public Div GetEnemySingle(int tx, int ty, Army a)
        {
            return GetSingle(tx, ty, ~a.Bit);
        }

        public Div Get(int tx, int ty, Army a)
        {
            return Get(tx + ty * SETT.TWIDTH, a.Bit);
        }

        public MAP_INT Soldiers(Army a)
        {
            return soldiers[a.Index];
        }

        public MAP_OBJECT_ISSER<Army> HasEnemy = new MAP_OBJECT_ISSER<Army>
        {
            Is = (tile, value) => Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH, value),
            Is = (tx, ty, value) =>
            {
                if (!SETT.IN_BOUNDS(tx, ty))
                    return false;
                int tile = tx + ty * SETT.TWIDTH;
                return soldiers[(value.Index + 1) & 1].Get(tile) > 0;
            }
        };

        public MAP_OBJECT_ISSER<Army> HasAlly = new MAP_OBJECT_ISSER<Army>
        {
            Is = (tile, value) => Is(tile % SETT.TWIDTH, tile / SETT.TWIDTH, value),
            Is = (tx, ty, value) =>
            {
                if (!SETT.IN_BOUNDS(tx, ty))
                    return false;
                int tile = tx + ty * SETT.TWIDTH;
                return soldiers[value.Index].Get(tile) > 0;
            }
        };

        public MAP_OBJECT_ISSER<Div> HasOtherAlly = new MAP_OBJECT_ISSER<Div>
        {
            Is = (tileI, value) =>
            {
                Tile t = tiles[firstTiles[tileI] & 0x0FFFF];
                while (t != null)
                {
                    Div d = GAME.ARMIES().Division(t.DivI);
                    if (d != value && d.Army() == value.Army())
                        return true;
                    t = tiles[t.Next & 0x0FFFF];
                }
                return false;
            },
            Is = (tx, ty, value) =>
            {
                if (!SETT.IN_BOUNDS(tx, ty))
                    return false;
                int tileI = tx + ty * SETT.TWIDTH;
                return GetMaskSingle(tileI, value.Bit) != null;
            }
        };

        public MAP_OBJECT_ISSER<Div> HasOtherAlly = new MAP_OBJECT_ISSER<Div>
        {
            Is = (tileI, value) =>
            {
                Tile t = tiles[firstTiles[tileI] & 0x0FFFF];
                while (t != null)
                {
                    Div d = GAME.ARMIES().Division(t.DivI);
                    if (d != value && d.Army() == value.Army())
                        return true;
                    t = tiles[t.Next & 0x0FFFF];
                }
                return false;
            },
            Is = (tx, ty, value) =>
            {
                if (!SETT.IN_BOUNDS(tx, ty))
                    return false;
                int tileI = tx + ty * SETT.TWIDTH;
                return GetMaskSingle(tileI, value.Bit) != null;
            }
        };

        public IEnumerable<Div> Get(List<Div> res, int tileI)
        {
            return GetMask(res, tileI, Armies.ARMIES_BITS);
        }

        private IEnumerable<Div> GetMask(List<Div> res, int tileI, int aMask)
        {
            Tile t = tiles[firstTiles[tileI] & 0x0FFFF];
            while (t != null && res.HasRoom())
            {
                Div d = GAME.ARMIES().Division(t.DivI);
                if ((d.Army().Bit & aMask) != 0)
                    res.Add(d);
                t = tiles[t.Next & 0x0FFFF];
            }
            return res;
        }

        private Div GetMaskSingle(int tileI, int aMask)
        {
            Tile t = tiles[firstTiles[tileI] & 0x0FFFF];
            while (t != null)
            {
                Div d = GAME.ARMIES().Division(t.DivI);
                if ((d.Army().Bit & aMask) != 0)
                    return d;
                t = tiles[t.Next & 0x0FFFF];
            }
            return null;
        }

        private Div Get(int tileI, int aMask)
        {
            Tile t = tiles[firstTiles[tileI] & 0x0FFFF];
            while (t != null)
            {
                Div d = GAME.ARMIES().Division(t.DivI);
                if ((d.Army().Bit & aMask) != 0)
                    return d;
                t = tiles[t.Next & 0x0FFFF];
            }
            return null;
        }

        public void Clear()
        {
            firstTiles.Fill((short)0);
            foreach (Bitsmap2D m in soldiers)
                m.Clear();
            tileNewI = 1;
        }

        private sealed class Tile
        {
            public short Next;
            public short DivI;
        }
    }
}