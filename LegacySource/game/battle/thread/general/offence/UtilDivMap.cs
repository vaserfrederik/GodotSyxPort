using System;
using System.Collections.Generic;
using game.battle.div;
using init.constant;
using settlement.main;
using snake2d.util.file;
using snake2d.util.sets;

namespace game.battle.thread.general.offence
{
    internal sealed class UtilDivMap
    {
        private readonly GTile[] tiles = new GTile[Config.battle().DIVISIONS_PER_ARMY];
        private int tileNewI = 0;
        private readonly GTile[][] grid = new GTile[(int)Math.Ceiling(SETT.TWIDTH / 16.0)][(int)Math.Ceiling(SETT.THEIGHT / 16.0)];
        private readonly Bitmap2D is = new Bitmap2D(SETT.TILE_BOUNDS, false);
        private int[] xs = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY);
        private int[] ys = Alloc.ii(Config.battle().DIVISIONS_PER_ARMY);

        private readonly ArrayList<Div> res = new ArrayList<Div>(Config.battle().DIVISIONS_PER_ARMY);
        private readonly LIST<Div> none = new ArrayList<Div>(0);

        public UtilDivMap()
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = new GTile();
            }
        }

        public void Clear()
        {
            tileNewI = 0;
            is.Clear();
            for (int gy = 0; gy < grid.Length; gy++)
            {
                for (int gx = 0; gx < grid[gy].Length; gx++)
                {
                    grid[gy][gx] = null;
                }
            }
        }

        public void Add(Div div)
        {
            int tx = div.centre().cUnitX() >> C.T_SCROLL;
            int ty = div.centre().cUnitY() >> C.T_SCROLL;
            if (!SETT.IN_BOUNDS(tx, ty))
                return;
            xs[div.indexArmy()] = tx;
            ys[div.indexArmy()] = ty;
            is.Set(tx, ty, true);
            int gx = tx / 16;
            int gy = ty / 16;
            GTile t = tiles[tileNewI++];
            t.div = div;
            t.next = grid[gy][gx];
            grid[gy][gx] = t;
        }

        public LIST<Div> Get(int tx, int ty)
        {
            if (!SETT.IN_BOUNDS(tx, ty))
                return none;
            if (!is.Is(tx, ty))
                return none;
            res.ClearSloppy();
            GTile t = grid[ty / 16][tx / 16];
            while (t != null && res.HasRoom())
            {
                if (xs[t.div.indexArmy()] == tx && ys[t.div.indexArmy()] == ty)
                    res.Add(t.div);
                t = t.next;
            }
            return res;
        }

        private sealed class GTile
        {
            public Div div;
            public GTile next;
        }
    }
}