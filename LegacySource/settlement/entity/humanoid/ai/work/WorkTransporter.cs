using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;

namespace settlement.entity.humanoid.ai.work
{
    internal class WorkTransporter : WorkAbs
    {
        private readonly WorkDeliveryman deliveryman;

        protected WorkTransporter(AIModule_Work module, PlanBlueprint[] map, Works w) : base("WorkTransportExtra", module, SETT.ROOMS().TRANSPORT, map, w)
        {
            map[SETT.ROOMS().TRANSPORT.index()] = null;
            deliveryman = new WorkDeliveryman(module, map, blueprint, false);
            map[SETT.ROOMS().TRANSPORT.index()] = this;
        }

        public override AiPlanActivation activate(Humanoid a, AIManager d)
        {
            //go off
            AiPlanActivation p = base.activate(a, d);
            if (p != null)
                return p;

            return deliveryman.activate(a, d);
        }
    }
}