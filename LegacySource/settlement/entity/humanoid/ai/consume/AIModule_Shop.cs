using init.resources.RBIT;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using settlement.stats.equip;
using util.text;

namespace settlement.entity.humanoid.ai.consume
{
    final class AIModule_Shop : AIModule
    {
        public readonly NEED_E need = NEEDS.TYPES().SHOPPING;
        readonly RBITImp bits = new RBITImp();

        public readonly M_PlanEquip ground = new M_PlanEquip();
        public readonly M_PlanReturn ret = new M_PlanReturn();
        private readonly PlansServices plans = new PlansServices(new M_PlanMarket(this));
        private static readonly CharSequence ¤¤name = "Shopping";
        private static readonly CharSequence ¤¤desc = "Browse the local markets or warehouses for equipment and Furniture.";

        static AIModule_Shop()
        {
            D.ts(AIModule_Shop);
        }

        AIModule_Shop() : base(UI.icons().s.storage, ¤¤name, ¤¤desc)
        {
        }

        public override AiPlanActivation getPlan(Humanoid a, AIManager d)
        {
            need.stat().fixMax(a.indu());

            AiPlanActivation p;

            p = ret.activate(a, d);
            if (p != null)
                return p;

            p = plans.getPlan(a, d);
            if (p == null)
                return ground.activate(a, d);

            return p;
        }

        protected override void update(Humanoid a, AIManager d, bool newDay, int byteDelta, int updateI)
        {
        }

        public override int getPriority(Humanoid a, AIManager d)
        {
            foreach (WearableResource e in STATS.EQUIP().BATTLE_ALL())
            {
                if (e.needed(a.indu()) < 0)
                {
                    return 2;
                }
            }

            int prio = need.stat().getPrio(a);

            if (prio == 0)
                return 0;

            return 4;
        }
    }
}