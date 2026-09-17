using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.room.service.module;
using init.type;

namespace settlement.entity.humanoid.ai.service
{
    internal sealed class S_PlanEntertain : S_Plan
    {
        private readonly ROOM_SERVICE_NEED_HASER sh;
        private readonly M_PlanSpectator plan;

        public S_PlanEntertain(NEED need, ROOM_SERVICE_NEED_HASER sh, M_PlanSpectator plan) : base(sh.service().stats(), sh.service().usage)
        {
            this.sh = sh;
            this.plan = plan;
        }

        public override bool hasAccess(Humanoid a, AIManager d)
        {
            return sh.service().stats().access(a);
        }

        public override bool allowed(Humanoid a, AIManager d)
        {
            return sh.service().stats().permission().is(HCLASS_RACE.clP(a.indu()));
        }

        public override bool goodTime(Humanoid a, AIManager d)
        {
            return sh.service().isGoodTime();
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            return getPlan(a, d, sh.service().radius());
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d, int dist)
        {
            d.planByte3 = (byte)sh.service().room().typeIndex();
            MPlan.dist = dist;
            return plan.activate(a, d);
        }
    }
}