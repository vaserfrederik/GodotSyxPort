using init.resources;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.misc.util;
using settlement.room.service.food.tavern;
using settlement.room.service.module;
using settlement.stats;
using snake2d.util.rnd;
using System;

namespace settlement.entity.humanoid.ai.consume
{
    internal sealed class PlanTavern : SPlanAbs<ROOM_TAVERN>
    {
        private readonly AIModule_Drink m;

        public PlanTavern(AIModule_Drink m) : base("Tavern", SETT.ROOMS().TAVERNS, false)
        {
            this.m = m;
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return eat.Set(a, d);
        }

        private readonly Resumer eat = new Resumer("eat")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                d.planByte1 = (byte)(STATS.FOOD().DRINK.decree().get(a));
                d.planByte2 = 0;

                FSERVICE f = Get(a, d);
                f.startUsing();
                return m.subdrink.activate(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                FSERVICE f = Get(a, d);
                f.startUsing();

                d.planByte2++;
                if (d.planByte2 >= STATS.FOOD().DRINK.decree().get(a))
                {
                    int rr = Blue(d).consume(a.race().pref().drink, STATS.FOOD().DRINK.decree().get(a), d.planTile.x(), d.planTile.y());
                    STATS.FOOD().drink(a, Meal.amount(rr), Meal.pref(rr));
                    Can(a, d);
                    if (RND.rFloat() < STATS.FOOD().DRINK.indu().getD(a.indu()))
                        return d.resumeOtherPlan(a, m.drunk);
                    return null;
                }

                return m.subdrink.activate(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                FSERVICE f = Get(a, d);
                return f != null && f.findableReservedIs();
            }

            public override void Can(Humanoid a, AIManager d)
            {
                FSERVICE f = Get(a, d);
                if (f != null && f.findableReservedIs())
                    f.consume();
            }
        };

        public bool worthTrying(Humanoid a, AIManager d)
        {
            foreach (ROOM_SERVICE_ACCESS_HASER s in SETT.ROOMS().FOOD)
            {
                RoomServiceAccess b = s.service();
                if (b.accessRequest(a) && b.finder.has(a.tc()))
                    return true;
            }
            return false;
        }
    }
}