using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.stats;
using util.text;

namespace settlement.entity.humanoid.ai.danger
{
    public sealed class AIModule_Health : AIModule
    {
        private readonly PlanSick sick = new PlanSick("dangerSick");
        private readonly PlanInjured bleed = new PlanInjured("dangerInjured");
        private static readonly string ¤¤name = "Recover";
        private static readonly string ¤¤desc = "Recover from injuries or illness, either at home or at a hospital.";

        static AIModule_Health()
        {
            D.ts(typeof(AIModule_Health));
        }

        public AIModule_Health() : base(UI.icons().s.plus, ¤¤name, ¤¤desc)
        {
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            if (STATS.WORK().incap.stat.indu().get(a.indu()) == 0)
                STATS.WORK().incap.stat.indu().set(a.indu(), 1);

            if (STATS.WORK().EMPLOYED.get(a) != null && STATS.WORK().EMPLOYED.get(a).blueprintI() == SETT.ROOMS().HOSPITAL)
                STATS.WORK().EMPLOYED.set(a, null);

            if (STATS.DISEASE().status(a.indu()).active)
                return sick.activate(a, d);

            if (STATS.NEEDS().INJURIES.inDanger(a.indu()))
                return bleed.activate(a, d);

            return null;
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            if (STATS.DISEASE().status(a.indu()).active)
                return 7;

            if (STATS.NEEDS().INJURIES.inDanger(a.indu()))
                return 7;

            return 0;
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateOfDay)
        {
            // TODO Auto-generated method stub
        }
    }
}