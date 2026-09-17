using System;
using settlement.entity.humanoid.ai.main;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.crime;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.crime
{
    internal class Theft : AIPLAN.PLANRES
    {
        private static string ¤¤verb = "¤Stealing!";

        static Theft()
        {
            D.ts(typeof(Theft));
        }

        internal readonly AIModule_Crime m;

        public Theft(string key, AIModule_Crime m) : base(key)
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
                return AI.SUBS().walkTo.resource(a, d, RBIT.ALL, 100);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                m.commitCrime(a, d, false, STATS.LAW().prisonerType.get(a.indu()));
                GAME.player().res().inc(d.resourceCarried(), RTYPE.THEFT, -1);
                int x = d.path.destX();
                int y = d.path.destY();
                int e = PATH().finders.resource.normal.reserveExtra(d.resourceCarried(), x, y, 4 + RND.rInt(10));
                PATH().finders.resource.pickup(d.resourceCarried(), x, y, e);
                GAME.player().res().inc(d.resourceCarried(), RTYPE.THEFT, -e);
                d.resourceCarriedSet(null);
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