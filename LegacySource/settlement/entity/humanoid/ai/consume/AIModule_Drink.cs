using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using init.resources.RBIT;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.consume
{
    sealed class AIModule_Drink : AIModule
    {
        public readonly NEED_E need = NEEDS.TYPES().THIRST;

        readonly RBITImp bits = new RBITImp();

        public readonly D_PlanDrunk drunk = new D_PlanDrunk();
        private readonly D_PlanDrinkGround ground = new D_PlanDrinkGround(this);

        private readonly PlansServices plans = new PlansServices(new PlanTavern(this));

        private static readonly CharSequence ¤¤name = "¤drink";
        private static readonly CharSequence ¤¤desc = "¤Consume drink off the ground or in a tavern.";

        static AIModule_Drink()
        {
            D.ts(AIModule_Drink.class);
        }

        public readonly AISUB subdrink = new AISUB.Simple("subsDrinking")
        {
            protected override AISTATE resume(Humanoid a, AIManager d)
            {
                d.subByte++;
                switch (d.subByte)
                {
                    case 1: return AI.STATES().STAND.activate(a, d, 2 + RND.rFloat(4));
                    case 2: return AI.STATES().anima.fist.activate(a, d, 1.5f);
                    case 3: return AI.STATES().STAND.activate(a, d, 2 + RND.rFloat(4));
                    case 4: return AI.STATES().anima.fist.activate(a, d, 1.5f);
                    case 5: return AI.STATES().STAND.activate(a, d, 2 + RND.rFloat(4));
                    case 6: return AI.STATES().anima.fist.activate(a, d, 1.5f);
                    case 7: return AI.STATES().STAND.activate(a, d, 2 + RND.rFloat(4));
                    case 8: return AI.STATES().anima.fist.activate(a, d, 1.5f);
                }
                return null;
            }
        };

        public AIModule_Drink() : base(UI.icons().s.jug, ¤¤name, ¤¤desc)
        {
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            need.stat().fixMax(a.indu());
            AiPlanActivation p = plans.getPlan(a, d);
            if (p != null)
                return p;
            p = ground.activate(a, d);
            if (p == null)
            {
                STATS.FOOD().drink(a, 0, 0);
            }
            return p;
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateI)
        {
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            int prio = need.stat().getPrio(a);

            if (prio == 0)
                return 0;

            return 4;
        }
    }
}