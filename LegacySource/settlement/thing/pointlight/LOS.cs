using settlement.main;

namespace settlement.thing.pointlight
{
    public interface LOS
    {
        bool PassesToOtherFromThis(int fx, int fy, int tx, int ty);
        bool PassesFromOtherToThis(int fx, int fy, int tx, int ty);
        bool IsLightBlocker(int tx, int ty);
        bool BlocksEnv(int tx, int ty);

        public static readonly LOS OPEN = new LOSImplementation
        {
            PassesToOtherFromThis = (fx, fy, tx, ty) => true,
            PassesFromOtherToThis = (fx, fy, tx, ty) => true,
            BlocksEnv = (tx, ty) => false,
            IsLightBlocker = (tx, ty) => false
        };

        public static readonly LOS SOLID = new LOSImplementation
        {
            PassesToOtherFromThis = (fx, fy, tx, ty) => !SETT.PATH().solidity.Is(tx, ty),
            PassesFromOtherToThis = (fx, fy, tx, ty) => false,
            BlocksEnv = (tx, ty) => true,
            IsLightBlocker = (tx, ty) => true
        };

        public static readonly LOS CEILING = new LOSImplementation
        {
            PassesToOtherFromThis = (fx, fy, tx, ty) => !SETT.LIGHTS().los().Get(tx, ty).IsLightBlocker(tx, ty),
            PassesFromOtherToThis = (fx, fy, tx, ty) => true,
            BlocksEnv = (tx, ty) => false,
            IsLightBlocker = (tx, ty) => false
        };
    }

    public class LOSImplementation : LOS
    {
        public Func<int, int, int, int, bool> PassesToOtherFromThis { get; set; }
        public Func<int, int, int, int, bool> PassesFromOtherToThis { get; set; }
        public Func<int, int, bool> BlocksEnv { get; set; }
        public Func<int, int, bool> IsLightBlocker { get; set; }
    }
}