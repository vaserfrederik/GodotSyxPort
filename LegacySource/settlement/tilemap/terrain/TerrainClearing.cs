using System;

namespace Settlement.Tilemap.Terrain
{
    public abstract class TerrainClearing
    {
        protected TerrainClearing()
        {
        }

        /**
         * 
         * @return true for all structures and for mountain
         */
        public virtual bool IsStructure()
        {
            return false;
        }

        /**
         * 
         * @param tx
         * @param ty
         * @return true for all except deep ocean and mountain.
         */
        public abstract bool Can(int tx, int ty);

        /**
         * 
         * @param tx
         * @param ty
         * @return false if cant be cleared and if nothings is here.
         */
        public virtual bool Needs()
        {
            return true;
        }

        public abstract RESOURCE Clear1(int tx, int ty);

        /**
         * 
         * @param tx
         * @param ty
         * @return the amount of resources yeilded, specified by {@link #resource()}
         */
        public abstract int ClearAll(int tx, int ty);

        public virtual bool CanDestroy(int tx, int ty)
        {
            return true;
        }

        public virtual void Destroy(int tx, int ty)
        {
            SETT.TERRAIN().NADA.PlaceFixed(tx, ty);
        }

        public virtual double Strength()
        {
            return 500 * C.TILE_SIZE;
        }

        public abstract SoundRace Sound(int tx, int ty);

        /**
         * true for flowers, bush, etc
         * @return
         */
        public virtual bool IsEasilyCleared()
        {
            return false;
        }

        static readonly TerrainClearing dummy = new TerrainClearing
        {
            Can = (tx, ty) => true,
            Needs = (tx, ty) => false,
            Clear1 = (tx, ty) => null,
            CanDestroy = (tx, ty) => false,
            Destroy = (tx, ty) => { },
            Strength = () => 0,
            ClearAll = (tx, ty) => 0,
            Sound = (tx, ty) => null,
            IsStructure = () => false,
            IsEasilyCleared = () => true,
        };
    }
}