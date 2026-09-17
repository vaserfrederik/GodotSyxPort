using System.Collections.Generic;
using snake2d.util.datatypes;
using world.army.ADSupplies;

namespace world.battle.spec
{
    public interface WBattleSide
    {
        COORDINATE Coo();
        int Men();
        int Losses();
        int LossesRetreat();
        LIST<WBattleUnit> Units();
        int Artillery(ADArtillery a);
        double PowerBalance();
    }
}