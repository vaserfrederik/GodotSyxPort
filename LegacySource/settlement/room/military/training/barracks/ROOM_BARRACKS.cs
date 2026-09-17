using System.IO;

namespace Settlement.Room.Military.Training.Barracks
{
    public sealed class RoomBarracks : RoomMTrainer<BarracksInstance>
    {
        private readonly Constructor constructor;
        private readonly BarracksThing thing = new BarracksThing(this);

        public RoomBarracks(int typeIndex, RoomInitData data, string key) : base(typeIndex, data, key)
        {
            constructor = new Constructor(this, data);
        }

        public override Furnisher Constructor()
        {
            return constructor;
        }

        public COORDINATE FaceCoo(int tx, int ty)
        {
            return thing.Init(tx, ty).CooMan;
        }
    }
}