using Init.Resources;

namespace Settlement.Maintenance
{
    abstract class MType
    {
        public abstract bool Validate(int tx, int ty);

        public abstract bool Degrade(int tx, int ty, int tile, double rate);
        public abstract void Maintain(int tx, int ty);
        public abstract void Vandalize(int tx, int ty);
        public abstract bool ShouldPlace(int tx, int ty, bool was);
        public abstract int ShouldPlaceResource(int tx, int ty);

        public abstract RESOURCE Res(int tx, int ty, int ri);
        public abstract double ResRate(int tx, int ty, int ri);

        public abstract double Degrade(int tx, int ty);
    }
}