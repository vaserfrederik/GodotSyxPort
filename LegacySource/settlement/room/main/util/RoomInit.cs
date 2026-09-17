using System;
using System.Collections.Generic;

namespace Settlement.Room.Main.Util
{
    public sealed class RoomInit
    {
        public readonly double[] Res;
        public readonly double[] Stats;
        public double ResMul = 1;
        public readonly int Degrade;

        public RoomInit(RoomBlueprintImp b, int degrade)
        {
            if (b.Constructor() != null)
            {
                Res = new double[b.Constructor().Resources()];
                Stats = new double[b.Constructor().Stats().Count];
            }
            else
            {
                Stats = new double[0];
                Res = new double[0];
            }
            this.Degrade = degrade;
        }
    }
}