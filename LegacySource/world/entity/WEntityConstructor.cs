using System;
using System.Collections.Generic;
using System.IO;
using Init.Constant;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Sets;
using World.Map.Regions;

namespace World.Entity
{
    public abstract class WEntityConstructor<T> where T : WEntity
    {
        private readonly int index;
        private readonly ArrayList<T> tmp = new ArrayList<T>(64);
        protected readonly bool fast;

        protected WEntityConstructor(LISTE<WEntityConstructor<? extends WEntity>> all, bool fast)
        {
            index = all.Add(this);
            this.fast = fast;
        }

        protected abstract T Create();

        protected abstract void Clear();

        protected virtual void Save(FilePutter file)
        {
        }

        protected virtual void Load(FileGetter file)
        {
        }

        protected virtual void Update(double ds)
        {
        }

        public void Fill(LISTE<T> res, Region reg)
        {
            WEntity e = WORLD.ENTITIES().RegFirst(reg);
            while (e != null && res.HasRoom())
            {
                if (e.Constructor() == this)
                {
                    res.Add((T)e);
                }
                e = e.RegionNext;
            }
        }

        public LIST<T> Fill(Region reg)
        {
            tmp.ClearSloppy();
            Fill(tmp, reg);
            return tmp;
        }

        public LIST<T> FillTile(int tx, int ty)
        {
            return Fill(tx * C.TILE_SIZE, (tx + 1) * C.TILE_SIZE, ty * C.TILE_SIZE, (ty + 1) * C.TILE_SIZE);
        }

        public LIST<T> FillTiles(int tx1, int tx2, int ty1, int ty2)
        {
            return Fill(tx1 * C.TILE_SIZE, tx2 * C.TILE_SIZE, ty1 * C.TILE_SIZE, ty2 * C.TILE_SIZE);
        }

        public LIST<T> FillTiles(RECTANGLE tiles)
        {
            return Fill(tiles.X1() * C.TILE_SIZE, tiles.X2() * C.TILE_SIZE, tiles.Y1() * C.TILE_SIZE, tiles.Y2() * C.TILE_SIZE);
        }

        public LIST<T> Fill(int x1, int x2, int y1, int y2)
        {
            tmp.ClearSloppy();
            Fill(tmp, x1, x2, y1, y2);
            return tmp;
        }

        public LIST<T> Fill(LISTE<T> tmp, int x1, int x2, int y1, int y2)
        {
            foreach (WEntity e in WORLD.ENTITIES().Fill(x1, x2, y1, y2))
            {
                if (e.Constructor() == this)
                {
                    tmp.Add((T)e);
                }
            }
            return tmp;
        }
    }
}