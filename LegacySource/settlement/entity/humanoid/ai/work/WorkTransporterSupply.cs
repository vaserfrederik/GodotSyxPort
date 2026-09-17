using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;

namespace settlement.entity.humanoid.ai.work
{
    sealed class WorkTransporterSupply : WorkAbs
    {
        private readonly WorkDeliveryman deliveryman;

        protected WorkTransporterSupply(AIModule_Work module, PlanBlueprint[] map, Works w) : base("WorkTransportSupplyExtra", module, SETT.ROOMS().SUPPLY, map, w)
        {
            map[blueprint.index()] = null;
            deliveryman = new WorkDeliveryman(module, map, blueprint, false);

            map[blueprint.index()] = this;
        }

        public override AiPlanActivation activate(Humanoid a, AIManager d)
        {
            AiPlanActivation p = deliveryman.activate(a, d);
            if (p != null)
                return p;

            return base.activate(a, d);
        }
    }
}