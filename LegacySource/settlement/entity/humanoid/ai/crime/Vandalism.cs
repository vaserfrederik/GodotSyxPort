using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using util.text;
using init.resources;

namespace settlement.entity.humanoid.ai.crime
{
    public sealed class Vandalism : AIPLAN.PLANRES
    {
        private static readonly string ¤¤verb = "¤Vandalizing";

        static Vandalism()
        {
            D.ts(typeof(Vandalism));
        }

        private readonly AIModule_Crime m;

        public Vandalism(string key, AIModule_Crime m) : base(key)
        {
            this.m = m;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return go.set(a, d);
        }

        private readonly Resumer go = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (SETT.PATH().finders.maintenance.find(RBIT.ALL, a.tc(), d.path, 100))
                {
                    return AI.SUBS().walkTo.path(a, d);
                }
                else if (SETT.MAINTENANCE().isser.is(a.tc().x(), a.tc().y()))
                {
                    return AI.SUBS().walkTo.coo(a, d, a.tc().x(), a.tc().y());
                }
                return null;
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                return next.set(a, d);
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

        private readonly Resumer next = new Resumer(¤¤verb)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().single.activate(a, d, AI.STATES().anima.box, 2.5);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                m.commitCrime(a, d, true, STATS.LAW().prisonerType.get(a.indu()));
                SETT.MAINTENANCE().vandalise(d.path.destX(), d.path.destY());
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

        public override bool event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.NOTIFY_CRIME)
                return false;
            return base.event(a, d, e);
        }
    }
}