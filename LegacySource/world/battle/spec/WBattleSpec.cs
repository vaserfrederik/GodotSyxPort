using System;

namespace World.Battle.Spec
{
    public abstract class WBattleSpec
    {
        public WBattleSide Player { get; set; }
        public WBattleSide Enemy { get; set; }
        public bool Victory { get; set; }

        public abstract void Retreat();
        public abstract void Auto();
        public abstract void Engage();
    }
}