using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using snake2d.util.datatypes;

namespace settlement.entity.humanoid.ai.battle
{
    public static class BattleUtil
    {
        public static double GetAttackPause(Humanoid a, AIManager d)
        {
            //double de = (BOOSTABLES.BATTLE().ATTACK_RATE.get(a.indu()) + 1.0) / (BOOSTABLES.BATTLE().ATTACK_RATE.max(Induvidual.class) + 1.0);
            return 1.0;
        }

        public static bool IsInPosition(COORDINATE dest, Humanoid a, AIManager d)
        {
            return dest.IsSameAs(a.physics.body().cX(), a.physics.body().cY());
        }

        public static bool HasSpot(Humanoid a, AIManager d)
        {
            Div div = a.division();
            return div != null && div.reporter.posHas(a);
        }

        public static bool ShouldMoveIntoDivPosition(Humanoid a, AIManager d)
        {
            if (a.division() == null)
                return false;
            if (!a.division().settings().mustering())
                return false;
            if (a.division().settings().moppingUp())
                return false;
            if (!a.division().reporter.posHas(a))
                return false;
            return true;
        }
    }
}