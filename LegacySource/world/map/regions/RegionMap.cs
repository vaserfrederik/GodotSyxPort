using System;
using System.IO;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.sets;
using world;

namespace world.map.regions
{
    public sealed class RegionMap : MAP_OBJECTE<Region>, SAVABLE
    {
        private readonly Bitsmap1D mapID;

        public RegionMap()
        {
            mapID = new Bitsmap1D(0, Convert.ToInt32(Math.Log2(WREGIONS.MAX + 1)), TAREA());
        }

        public Region Get(int tile)
        {
            if (mapID.Get(tile) == 0)
                return null;
            return WORLD.REGIONS().GetByIndex(mapID.Get(tile) - 1);
        }

        public Region Get(int tx, int ty)
        {
            if (IN_BOUNDS(tx, ty))
                return Get(tx + ty * TWIDTH());
            return null;
        }

        public void Set(int tile, Region obj)
        {
            if (obj == null)
                mapID.Set(tile, 0);
            else
                mapID.Set(tile, obj.Index() + 1);
            WORLD.REGIONS().Dirty = true;
        }

        public void Set(int tx, int ty, Region obj)
        {
            if (IN_BOUNDS(tx, ty))
            {
                Set(tx + ty * TWIDTH(), obj);
            }
        }

        public void Save(FilePutter file)
        {
            mapID.Save(file);
        }

        public void Load(FileGetter file)
        {
            mapID.Load(file);
        }

        public void Clear()
        {
            mapID.Clear();
            WORLD.REGIONS().Dirty = true;
        }
    }
}