using System;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.room.main.throne;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.crime
{
    final class Disrespect : AIPLAN.PLANRES
    {
        private static readonly CharSequence ¤¤verb = "¤Disrespecting the ruler!";

        static
        {
            D.ts(typeof(Disrespect));
        }

        private readonly AIModule_Crime m;

        public Disrespect(string key, AIModule_Crime m) : base(key)
        {
            this.m = m;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return go.Set(a, d);
        }

        private readonly Resumer go = new Resumer(¤¤verb);

        private class Resumer : AISUB.AISubActivation
        {
            public Resumer(CharSequence verb) : base(verb)
            {
            }

            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                d.planByte1 = (byte)(1 + RND.rInt(16));
                d.planByte2 = 0;
                return Res(a, d);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                d.planByte2++;
                if (d.planByte2 == 1)
                    m.commitCrime(a, d, false, STATS.LAW().prisonerType.Get(a.indu()));
                if (d.planByte1-- >= 0)
                {
                    a.speed.Turn2(a.tc().x(), a.tc().y(), THRONE.coo().x(), THRONE.coo().y());
                    return AI.SUBS().single.Activate(a, d, AI.STATES().anima.fist.Activate(a, d));
                }
                return null;
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
                // TODO Auto-generated method stub
            }
        }

        public override bool Event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.event == HEvent.NOTIFY_CRIME)
                return false;

            return base.Event(a, d, e);
        }

        protected override void Cancel(Humanoid a, AIManager d)
        {
            base.Cancel(a, d);
        }
    }
}