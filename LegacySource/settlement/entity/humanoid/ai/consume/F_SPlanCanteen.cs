using System;
using init.resources;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.service.food.canteen;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;

namespace settlement.entity.humanoid.ai.consume
{
    internal sealed class F_SPlanCanteen : SPlanAbs<ROOM_CANTEEN>
    {
        private readonly AISUB eat;

        public F_SPlanCanteen(AISUB eat) : base("Canteen", SETT.ROOMS().CANTEENS, false)
        {
            this.eat = eat;
        }

        protected override AISubActivation arrive(Humanoid a, AIManager d)
        {
            return first.Set(a, d);
        }

        private readonly Resumer first = new Resumer("1")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                int da = Blue(d).Grab(a.Race().Pref().Food, STATS.FOOD().FOOD.Decree().Get(a), d.PlanTile.X(), d.PlanTile.Y());

                STATS.FOOD().Eat(a, Meal.Amount(da), Meal.Pref(da));
                COORDINATE c = Blue(d).GetChair(d.PlanTile.X(), d.PlanTile.Y());

                if (c != null)
                {
                    AISubActivation s = AI.SUBS().WalkTo.CooFull(a, d, c);
                    if (s != null)
                    {
                        d.PlanTile.Set(c);
                        d.PlanObject = da;
                        walkTable.Set(a, d);
                        return s;
                    }
                }
                return eat.Activate(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                return null;
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer walkTable = new Resumer("2")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return null;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                return walkLast.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return Blue(d).Is(d.PlanTile);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                Blue(d).ReturnChair(d.PlanTile.X(), d.PlanTile.Y());
            }
        };

        private readonly Resumer walkLast = new Resumer("3")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                DIR dir = Blue(d).SetChair(d.PlanTile.X(), d.PlanTile.Y(), d.PlanObject);
                if (dir != null)
                {
                    return AI.SUBS().Single.Activate(a, d, AI.STATES().WALK2.MoveToEdge(a, d, dir));
                }
                return null;
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                a.Speed.MagnitudeInit(0);
                return eatTable.Set(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return walkTable.Con(a, d);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                walkTable.Can(a, d);
            }
        };

        private readonly Resumer eatTable = new Resumer("4")
        {
            public override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                d.PlanByte1 = (byte)(4 + RND.rInt(10));
                return eat.Activate(a, d);
            }

            public override AISubActivation Res(Humanoid a, AIManager d)
            {
                d.PlanByte1--;
                if (d.PlanByte1 < 0)
                {
                    Can(a, d);
                    if (NEEDS.TYPES().HUNGER.Stat().GetPrio(a) > 0)
                        return Init(a, d);
                    return null;
                }
                else
                {
                    return eat.Activate(a, d);
                }
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return walkTable.Con(a, d);
            }

            public override void Can(Humanoid a, AIManager d)
            {
                walkTable.Can(a, d);
            }
        };
    }
}