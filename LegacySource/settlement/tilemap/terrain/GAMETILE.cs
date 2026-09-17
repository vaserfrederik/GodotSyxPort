using snake2d.util.sprite;

namespace settlement.tilemap.terrain
{
    public interface GAMETILE
    {
        /**
         * 
         * @return true if a client can safely place this tile
         */
        bool isPlacable(int tx, int ty);

        /**
         * place the tile on the map and fix surrounding tiles.
         * 
         * @param x
         * @param y
         */
        void placeFixed(int tx, int ty);
        
        /**
         * 
         * @return get an small image representing this tile
         */
        SPRITE getIcon();

        /**
         * 
         * @return the name of the tile
         */
        string name();
    }
}