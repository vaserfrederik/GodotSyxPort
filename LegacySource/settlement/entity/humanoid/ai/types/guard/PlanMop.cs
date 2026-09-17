using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.path.components;

namespace settlement.entity.humanoid.ai.types.guard
{
    class PlanMop : AIPLAN.PLANRES
    {
        public PlanMop() : base("Guard mop")
        {
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            return first.Set(a, d);
        }

        private readonly Resumer first = new Resumer("")
        {
            protected override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                return AI.SUBS().STAND.ActivateRndDir(a, d, 0);
            }

            protected override AISubActivation Res(Humanoid a, AIManager d)
            {
                int ee = 0;
                int ff = 0;

                SComponent ss = SETT.PATH().comps.levels.Get(0).Get(a.tc());
                if (ss == null)
                    return null;

                ee += PATH().comps.data.people(a.indu().hostile()).Get(ss);
                ff += PATH().comps.data.people(!a.indu().hostile()).Get(ss);
                SComponentEdge e = ss.EdgeFirst();
                while (e != null)
                {
                    ee += PATH().comps.data.people(a.indu().hostile()).Get(e.To());
                    ff += PATH().comps.data.people(!a.indu().hostile()).Get(e.To());
                    e = e.Next();
                }

                if (ee == 0)
                    return null;

                if (ee > ff / 2)
                {

                }

                Humanoid aa = SETT.PATH().finders.otherHumanoid.enemy(a, 64);

                if (ee > ff / 2)
                {
                    return d.ResumeOtherPlan(a, AI.listeners().Flee(a, d, aa));
                }
                else if (aa != null)
                {
                    return d.ResumeOtherPlan(a, AI.listeners().CatchCriminal(aa));
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
        };
    }
}