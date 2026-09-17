using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.service.food.eatery;
using settlement.stats;

namespace settlement.entity.humanoid.ai.consume
{
    final class F_SPlanEatery : SPlanAbs<ROOM_EATERY>
    {
        private readonly AISUB eat;

        public F_SPlanEatery(AISUB eat) : base("Eatery", SETT.ROOMS().EATERIES, false)
        {
            this.eat = eat;
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.set(a, d);
        }

        private readonly Resumer first = new Resumer("")
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return eat.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                int da = blue(d).eat(a.race().pref().food, STATS.FOOD().FOOD.decree().get(a), d.planTile.x(), d.planTile.y());

                STATS.FOOD().eat(a, Meal.amount(da), Meal.pref(da));
                if (NEEDS.TYPES().HUNGER.stat().getPrio(a) > 0)
                    return init(a, d);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                FSERVICE ss = blue(d).service().service(d.planTile.x(), d.planTile.y());
                if (ss != null)
                    ss.findableReserveCancel();
            }
        };
    }
}