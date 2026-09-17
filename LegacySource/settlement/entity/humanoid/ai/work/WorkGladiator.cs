using settlement.entity.humanoid.HEvent;
using settlement.entity.humanoid.HPoll;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.entity.humanoid.ai.util;
using settlement.room.main;
using settlement.room.service.arena;
using settlement.stats;

namespace settlement.entity.humanoid.ai.work
{
    public sealed class WorkGladiator : PlanBlueprint
    {
        private readonly AIPlanGladiator plan;
        private readonly RoomArenaWork w;

        protected WorkGladiator(RoomArenaWork g, RoomBlueprintIns<?> blue, AIModule_Work module, PlanBlueprint[] map) : base(module, blue, map)
        {
            w = g;
            plan = new AIPlanGladiator("Work_" + blue.key, false, blue.employment().verb)
            {
                public override double poll(Humanoid a, AIManager d, HPollData e)
                {
                    if (e.type == HPoll.WORKING)
                        return 1.0;
                    return base.poll(a, d, e);
                }

                protected override bool shouldContinue(Humanoid a, AIManager d)
                {
                    return hasEmployment(a, d);
                }

                protected override void cancel(Humanoid a, AIManager d)
                {
                    base.cancel(a, d);
                    if (work(a) != null && work(a).employees().isOverstaffed())
                    {
                        STATS.WORK().EMPLOYED.set(a, null);
                    }
                }

                protected override RoomArenaWork w(Humanoid a, AIManager d)
                {
                    return g;
                }
            };
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return res.set(a, d);
        }

        private readonly Resumer res = new Resumer()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().STAND.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (work(a) != null)
                {
                    if (!w.gladiatorInArena(a.tc().x(), a.tc().y()))
                    {
                        STATS.NEEDS().INJURIES.COUNT.indu().set(a.indu(), 0);
                    }
                    d.planTile.set(w.gladiatorGetSpot(work(a)));
                    return d.resumeOtherPlan(a, plan);
                }
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                return plan.event(a, d, e);
            }

            public override double poll(Humanoid a, AIManager d, HPollData e)
            {
                return plan.poll(a, d, e);
            }
        };
    }
}