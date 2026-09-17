using System.Collections.Generic;
using Game.Battle.Util;
using Init.Constant;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Sets;
using World.Army;

namespace Game.Battle.State
{
    public class BattleStateSpec
    {
        public SpecSide Player = new SpecSide();
        public SpecSide Enemy = new SpecSide();

        public class SpecSide
        {
            public readonly Coo WCoo = new Coo();
            public readonly int[] Artillery = Alloc.Ii(AD.Supplies().Arts().Size());
            public double MoraleBase;
            public ArrayList<DivGeneration> Divs = new ArrayList<DivGeneration>(Config.Battle().DIVISIONS_PER_ARMY);
        }
    }
}