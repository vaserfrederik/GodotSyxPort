using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using snake2d.util.rnd;
using util.text;
using System;

namespace settlement.entity.humanoid.ai.crime
{
    internal sealed class Flasher : AIPLAN.PLANRES
    {
        private static readonly string ¤¤verb = "¤Streaking";

        static Flasher()
        {
            D.ts(typeof(Flasher));
        }

        private readonly AIModule_Crime m;

        public Flasher(string key, AIModule_Crime m) : base(key)
        {
            this.m = m;
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return go.Set(a, d);
        }

        private readonly Resumer go = new Resumer(¤¤verb);

        protected override AISubActivation SetAction(Humanoid a, AIManager d)
        {
            m.CommitCrime(a, d, true, STATS.LAW().PrisonerType.Get(a.Indu()));
            STATS.POP().NAKED.Set(a.Indu(), 1);
            return AI.SUBS().WalkTo.Run_Around_Crazy(a, d, 1);
        }

        protected override AISubActivation Res(Humanoid a, AIManager d)
        {
            if (!RND.OneIn(4))
            {
                STATS.POP().NAKED.Set(a.Indu(), 0);
                return null;
            }

            AIModule_Crime.Notify(a);
            return AI.SUBS().WalkTo.Run_Around_Crazy(a, d, 1);
        }

        public override bool Con(Humanoid a, AIManager d)
        {
            return true;
        }

        public override void Can(Humanoid a, AIManager d)
        {
            // TODO Auto-generated method stub
        }

        public override bool Event(Humanoid a, AIManager d, HEventData e)
        {
            if (e.Event == HEvent.NOTIFY_CRIME)
                return false;

            return base.Event(a, d, e);
        }

        protected override void Cancel(Humanoid a, AIManager d)
        {
            STATS.POP().NAKED.Set(a.Indu(), 0);
            base.Cancel(a, d);
        }
    }
}