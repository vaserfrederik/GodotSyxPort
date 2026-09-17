using System;
using game;
using game.faction;
using init.resources;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.consume
{
    internal class D_PlanDrinkGround : AIPLAN.PLANRES
    {
        private static readonly CharSequence ¤¤sDrink = "Having a drink";
        private static readonly RBITImp bi = new RBITImp();

        static D_PlanDrinkGround()
        {
            D.ts(typeof(D_PlanDrinkGround));
        }

        private readonly AIModule_Drink m;

        public D_PlanDrinkGround(AIModule_Drink m) : base("SerDrink")
        {
            this.m = m;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return walk.Set(a, d);
        }

        public bool has(Humanoid a, AIManager d)
        {
            bi.ClearSet(RESOURCES.DRINKS().mask).And(STATS.FOOD().FetchMask(a));
            return SETT.PATH().Finders.Resource.Has(a.Tc().X(), a.Tc().Y(), bi);
        }

        private readonly Resumer walk = new Resumer(¤¤sDrink)
        {
            SetAction = (a, d) =>
            {
                bi.ClearSet(RESOURCES.DRINKS().mask).And(STATS.FOOD().FetchMask(a));
                return AI.SUBS().WalkTo.Resource(a, d, bi);
            },

            Res = (a, d) =>
            {
                GAME.Player().Res().Inc(d.ResourceCarried(), RTYPE.CONSUMED, -1);
                NEEDS.TYPES().THIRST.Stat().Fix(a.Indu());

                STATS.FOOD().DRINK.Indu().Set(a.Indu(), 1);

                return drink.Set(a, d);
            },

            Con = (a, d) => true,

            Can = (a, d) => d.ResourceCarriedSet(null)
        };

        private readonly Resumer drink = new Resumer(¤¤sDrink)
        {
            SetAction = (a, d) =>
            {
                if (RND.rBoolean())
                    return AI.SUBS().STAND.ActivateTime(a, d, 1 + RND.rInt(5));
                d.ResourceCarriedSet(null);
                return m.Subdrink.Activate(a, d);
            },

            Res = (a, d) =>
            {
                d.ResourceCarriedSet(null);
                if (RND.rFloat() < STATS.FOOD().DRINK.Indu().GetD(a.Indu()))
                    return d.ResumeOtherPlan(a, m.Drunk);
                return null;
            },

            Con = (a, d) => true,

            Can = (a, d) => d.ResourceCarriedSet(null)
        };
    }
}