using settlement.job;
using settlement.tilemap.terrain;

namespace settlement.tilemap
{
    public interface TILE_FIXABLE
    {
        public Job fixJob(int tx, int ty);
        public TerrainTile getTerrain(int tx, int ty);
    }
}