using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.main;
using settlement.room.main.job;

namespace settlement.entity.humanoid.ai.work
{
    final class WorkBuilder : PlanBlueprint
    {
        protected WorkBuilder(AIModule_Work module, PlanBlueprint[] map) : base(module, SETT.ROOMS().BUILDER, map)
        {
        }

        public override AiPlanActivation activate(Humanoid a, AIManager d)
        {
            RoomInstance i = work(a);
            ROOM_RADIUS_INSTANCE r = i as ROOM_RADIUS_INSTANCE;
            if (!r.searching())
            {
                return null;
            }

            int sx = i.body().cX();
            int sy = i.body().cY();

            return AI.modules().work.oddjobber.activateWorker(a, d, sx, sy, r.radius());
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return null;
        }

        public override bool shouldReportWorkFailure(Humanoid a, AIManager d)
        {
            return d.plan() != AI.modules().work.oddjobber;
        }
    }
}