using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid.ai.main.AIData;
using settlement.entity.humanoid.ai.main.AIPLAN;
using settlement.entity.humanoid.ai.main.AISTATE;
using settlement.entity.humanoid.ai.main.AISUB;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.consume
{
    final class AIModule_Food : AIModule
    {
        public readonly NEED_E need = NEEDS.TYPES().HUNGER;
        private readonly AIDataSuspender suspenderStarvation = AI.suspender("starve");
        private readonly AIDataSuspender suspenderService = AI.suspender("food_service");
        private readonly AIDataSuspender suspenderFind = AI.suspender("foodfind");

        readonly RBITImp bits = new RBITImp();

        public readonly AISUB eat = new AISUB.Simple("eating")
        {
            protected override AISTATE resume(Humanoid a, AIManager d)
            {
                d.subByte++;
                switch (d.subByte)
                {
                    case 1: return AI.STATES().STAND.activate(a, d, 1.5f + RND.rFloat(4));
                    case 2: return AI.STATES().anima.box.activate(a, d, 2.5 + RND.rFloat(2));
                    case 3: return AI.STATES().STAND.activate(a, d, 1.5f + RND.rFloat(4));
                    case 4: return AI.STATES().anima.box.activate(a, d, 2.5 + RND.rFloat(2));
                }
                return null;
            }
        };

        private readonly PlansServices plans;
        private readonly AIPLAN eatPlan = new F_PlanEat(eat);
        private readonly AIPLAN starve = new F_PlanStarve(eat, suspenderStarvation);

        private static readonly CharSequence ¤¤name = "¤eat";
        private static readonly CharSequence ¤¤desc = "¤Find food";

        static
        {
            D.ts(AIModule_Food);
        }

        public AIModule_Food() : base(UI.icons().s.plate, ¤¤name, ¤¤desc)
        {
            plans = new PlansServices(new F_SPlanCanteen(eat), new F_SPlanEatery(eat));
        }

        int dayI = -1;
        int am = 0;

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            if (dayI != TIME.days().bitsSinceStart())
            {
                dayI = TIME.days().bitsSinceStart();
            }

            am++;

            if (!suspenderService.is(d))
            {
                AiPlanActivation p = plans.getPlan(a, d);
                if (p == null)
                    suspenderService.suspend(d);
                else
                    return p;
            }

            int prio = need.stat().getPrio(a);

            if (prio >= 2 || !plans.worthTrying(a, d))
            {
                if (!suspenderFind.is(d))
                {
                    AiPlanActivation p = eatPlan.activate(a, d);
                    if (p == null)
                        suspenderFind.suspend(d);
                    else
                        return p;
                }

                if (STATS.FOOD().STARVATION.indu().getD(a.indu()) > 0)
                {
                    return starve.activate(a, d);
                }
            }

            am--;

            return null;
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateI)
        {
            suspenderStarvation.update(d);
            suspenderFind.update(d);
            suspenderService.update(d);
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            if (STATS.FOOD().STARVATION.indu().getD(a.indu()) > 0)
                return 10;
            int p = need.stat().getPrio(a);
            if (p == 0)
                return 0;
            if (suspenderFind.is(d) && suspenderService.is(d))
                return 0;

            else if (p == 1)
            {
                return 4;
            }
            return 6;
        }
    }
}