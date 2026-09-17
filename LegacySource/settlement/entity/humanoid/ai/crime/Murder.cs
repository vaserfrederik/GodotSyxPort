using System;
using settlement.entity.humanoid.ai.crime;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.crime
{
    final class Murder : AIPLAN.PLANRES
    {
        private static string ¤¤verb = "¤Murdering";

        static
        {
            D.ts(typeof(Murder));
        }

        final AIModule_Crime m;

        public Murder(string key, AIModule_Crime m) : base(key)
        {
            this.m = m;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            d.planByte1 = 0;
            return go.set(a, d);
        }

        private readonly Resumer go = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (d.planByte1 == 5)
                    return null;
                d.planByte1++;
                Humanoid h = SETT.PATH().finders.otherHumanoid.find(a, 100);
                if (h != null)
                {
                    //GAME.Notify(a.tc());
                    return AI.SUBS().walkTo.follow(a, d, h, false, (byte)100);
                }
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (AI.SUBS().walkTo.followSucess(a, d))
                    return murder.set(a, d);
                return set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event == HEvent.MEET_HARMLESS)
                {
                    ENTITY ee = SETT.ENTITIES().getByID(d.planObject);
                    if (ee != null && ee is Humanoid && ee == e.other)
                    {
                        m.commitCrime(a, d, true, STATS.LAW().prisonerType.get(a.indu()));
                        d.overwrite(a, murder.set(a, d));
                        return true;
                    }
                }
                return base.event(a, d, e);
            }
        };

        private readonly Resumer murder = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                ENTITY e = SETT.ENTITIES().getByID(d.planObject);
                if (e == null)
                    return null;
                a.speed.turn2(a.body(), e.body());
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.sword_out, AI.STATES().anima.sword_out.time);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                ENTITY e = SETT.ENTITIES().getByID(d.planObject);
                if (e == null)
                    return null;
                if (e is Humanoid)
                {
                    Humanoid h = (Humanoid)e;
                    double damage = RND.rFloat() * 0.99;
                    h.inflictDamage(damage, CAUSE_LEAVES.MURDER());
                    AIModule_Crime.notify(a);
                    if (h.isRemoved())
                        return cool_down.set(a, d);
                    return chase.set(a, d);
                }

                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly Resumer cool_down = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.sword, 4);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                AIModule_Crime.notify(a);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }
        };

        private readonly Resumer chase = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                AIModule_Crime.notify(a);
                return AI.SUBS().walkTo.follow(a, d, SETT.ENTITIES().getByID(d.planObject), true, (byte)5);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (AI.SUBS().walkTo.followSucess(a, d))
                    return murder.set(a, d);
                return null;
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }

            public override bool event(Humanoid a, AIManager d, HEventData e)
            {
                if (e.event == HEvent.MEET_HARMLESS)
                {
                    ENTITY ee = SETT.ENTITIES().getByID(d.planObject);
                    if (ee != null && ee is Humanoid && ee == e.other)
                    {
                        d.overwrite(a, murder.set(a, d));
                        return true;
                    }
                }
                return base.event(a, d, e);
            }
        };

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.NOTIFY_CRIME)
                return false;
            return base.event(a, d, e);
        }

        protected override void cancel(Humanoid a, AIManager d)
        {
            base.cancel(a, d);
        }
    }
}