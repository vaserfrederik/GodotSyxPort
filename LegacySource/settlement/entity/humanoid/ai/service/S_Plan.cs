using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats.service;

namespace settlement.entity.humanoid.ai.service
{
    abstract class S_Plan
    {
        public readonly StatService service;
        public readonly NEED need;
        public readonly double usage;

        protected S_Plan(StatService service, double usage)
        {
            this.need = service.need;
            this.service = service;
            this.usage = usage;
        }

        public abstract bool HasAccess(Humanoid a, AIManager d);

        public abstract bool Allowed(Humanoid a, AIManager d);
        public abstract bool GoodTime(Humanoid a, AIManager d);

        public abstract AiPlanActivation GetPlan(Humanoid a, AIManager d);
        public abstract AiPlanActivation GetPlan(Humanoid a, AIManager d, int dist);
    }
}