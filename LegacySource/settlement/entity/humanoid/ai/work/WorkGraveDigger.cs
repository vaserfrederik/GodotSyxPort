using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.room.spirit.grave;
using snake2d.util.rnd;
using static settlement.entity.humanoid.ai.main.AISUB;

namespace settlement.entity.humanoid.ai.work
{
    final class WorkGraveDigger : PlanBlueprint
    {
        private readonly Resumer workNormal;

        public WorkGraveDigger(AIModule_Work module, PlanBlueprint[] map, GraveData.GRAVE_DATA_HOLDER pl)
            : base(module, pl.graveData().blueprint(), map)
        {
            workNormal = new Resumer(pl.graveData().blueprint().employment().verb)
            {
                protected override AISubActivation setAction(Humanoid a, AIManager d)
                {
                    d.planByte1 = (byte)(1 + RND.rInt(8));
                    return walkTo.room(a, d, work(a));
                }

                protected override AISubActivation res(Humanoid a, AIManager d)
                {
                    if (d.planByte1-- < 0)
                    {
                        return d.resumeOtherPlan(a, AI.modules().subject.corpse);
                    }
                    return WORK_HANDS.activate(a, d, 20);
                }

                public override bool con(Humanoid a, AIManager d)
                {
                    return true;
                }

                public override void can(Humanoid a, AIManager d)
                {
                    
                }
            };
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return workNormal.set(a, d);
        }
    }
}