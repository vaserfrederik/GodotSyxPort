using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.law.stockade;
using settlement.room.main;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;

namespace settlement.entity.humanoid.ai.types.prisoner
{
    class Jail : AIPLAN.PLANRES
    {
        private readonly ROOM_STOCKADE b = SETT.ROOMS().STOCKADE;
        private static readonly CharSequence ¤¤name = "In Stockade";
        static
        {
            D.ts(typeof(Jail));
        }

        public Jail() : base("prisJail")
        {
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            RoomInstance ins = b.registerPrisoner(a.tc());
            if (ins == null)
                return null;
            d.planTile.set(ins.mX(), ins.mY());
            STATS.NEEDS().EXPOSURE.fix(a.indu());
            d.planByte1 = 8;
            return init.set(a, d);
        }

        private readonly Resumer init = new Resumer()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (d.planByte1 <= 0)
                {
                    return null;
                }
                d.planByte1--;

                if (!b.isWithin(d.planTile.x(), d.planTile.y(), a.tc()))
                {
                    return walkToDoor.set(a, d);
                }

                if (NEEDS.TYPES().HUNGER.stat().getPrio(a.indu()) > 0)
                {
                    AISubActivation s = eat.set(a, d);
                    if (s != null)
                        return s;
                }

                if (TIME.light().nightIs())
                {
                    AISubActivation s = sleep.set(a, d);
                    if (s != null)
                        return s;
                }

                if (RND.oneIn(5))
                {
                    if (RND.oneIn(8))
                    {
                        AISubActivation s = poop.set(a, d);
                        if (s != null)
                            return s;
                    }
                    else
                    {
                        AISubActivation s = changeSpot.set(a, d);
                        if (s != null)
                            return s;
                    }
                }

                return AI.SUBS().STAND.activateRndDir(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer walkToDoor = new Resumer()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().walkTo.room(a, d, b.getter.get(d.planTile));
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer sleep = new Resumer()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().subSleep.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        private readonly Resumer poop = new Resumer()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                COORDINATE c = b.latrineReserve(d.planTile);
                d.planByte2 = 0;
                if (c != null)
                {
                    d.planTile.set(c);
                    AISubActivation ss = AI.SUBS().walkTo.cooFull(a, d, c.x(), c.y());
                    if (ss != null)
                        return ss;
                    b.latrineUse(d.planTile, false);
                }
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (d.planByte2 == 0)
                {
                    d.planByte2 = 1;
                    return AI.SUBS().STAND.activateRndDir(a, d);
                }
                b.latrineUse(d.planTile, true);
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                b.latrineUse(d.planTile, false);
            }
        };

        private readonly Resumer eat = new Resumer()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                COORDINATE c = b.foodReserve(d.planTile);
                d.planByte2 = 0;
                if (c != null)
                {
                    d.planTile.set(c);
                    AISubActivation ss = AI.SUBS().walkTo.cooFull(a, d, c.x(), c.y());
                    if (ss != null)
                        return ss;
                    b.foodUse(d.planTile, false);
                }
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                STATS.FOOD().eat(a, 0, 0);
                NEEDS.TYPES().HUNGER.stat().fix(a.indu());
                if (d.planByte2 == 0)
                {
                    d.planByte2 = 1;
                    return AI.SUBS().single.activate(a, d, AI.STATES().anima.grab, 3);
                }
                b.foodUse(d.planTile, true);
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                b.foodUse(d.planTile, false);
            }
        };

        private readonly Resumer changeSpot = new Resumer()
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                DIR dir = DIR.ORTHO.rnd();
                int dx = a.tc().x() + dir.x();
                int dy = a.tc().y() + dir.y();
                if (b.isWithin(dx, dy, a.tc()))
                {
                    if (!SETT.ENTITIES().hasAtTile(dx, dy))
                        return AI.SUBS().walkTo.cooFull(a, d, dx, dy);
                }
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return init.set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.ROOM_REMOVED && e.room.blueprintI() == b && e.room.is(d.planTile))
            {
                d.overwrite(a, AI.plans().NOP);
                return true;
            }
            return false;
        }

        protected override void cancel(Humanoid a, AIManager d)
        {
            b.unregisterPrisoner(d.planTile);
            base.cancel(a, d);
        }

        protected override AISubActivation resume(Humanoid a, AIManager d)
        {
            AISubActivation s = base.resume(a, d);
            if (s == null)
                b.unregisterPrisoner(d.planTile);
            return s;
        }

        protected override bool shouldContinue(Humanoid a, AIManager d)
        {
            return base.shouldContinue(a, d);
        }

        protected override void name(Humanoid a, AIManager d, Str str)
        {
            str.add(¤¤name);
        }
    }
}