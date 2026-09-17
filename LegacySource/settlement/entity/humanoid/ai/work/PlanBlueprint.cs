using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.room.main;
using settlement.stats;

namespace settlement.entity.humanoid.ai.work
{
    abstract class PlanBlueprint : PlanWork
    {
        protected readonly RoomBlueprintIns<?> blueprint;
        protected readonly AIModule_Work module;
        static int maxCarry = AIModule_Work.MAX_FETCH_AMOUNT;

        protected PlanBlueprint(AIModule_Work module, RoomBlueprintIns<?> blueprint, PlanBlueprint[] map)
            : this("work_" + blueprint.key, module, blueprint, map)
        {
        }

        protected PlanBlueprint(string key, AIModule_Work module, RoomBlueprintIns<?> blueprint, PlanBlueprint[] map)
            : base(key)
        {
            if (map[blueprint.index()] != null)
                throw new RuntimeException();
            map[blueprint.index()] = this;
            this.blueprint = blueprint;
            this.module = module;
        }

        public bool shouldReportWorkFailure(Humanoid a, AIManager d)
        {
            return true;
        }

        protected override void cancel(Humanoid a, AIManager d)
        {
            base.cancel(a, d);
            if (work(a) != null && work(a).employees().isOverstaffed())
            {
                STATS.WORK().EMPLOYED.set(a, null);
            }
        }

        public override double poll(Humanoid a, AIManager d, HPollData e)
        {
            if (e.type == HPoll.WORKING)
            {
                return 1.0;
            }
            return base.poll(a, d, e);
        }

        protected double transportAmount(Humanoid a, AIManager d)
        {
            return 0;
        }
    }
}