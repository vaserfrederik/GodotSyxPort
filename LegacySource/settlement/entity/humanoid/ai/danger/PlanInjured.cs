using game.audio;
using init.type;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.stats;
using snake2d.util.rnd;
using util.text;

namespace settlement.entity.humanoid.ai.danger
{
    class PlanInjured : AIPLAN.PLANRES
    {
        private static readonly string ¤¤name = "Bleeding out";
        private readonly SubPlanSeekHospital ho = new SubPlanSeekHospital(this);

        public readonly SoundRace sound = AUDIO.race("IN_PAIN_MOAN");

        static PlanInjured()
        {
            D.ts(typeof(PlanInjured));
        }

        public PlanInjured(string key) : base(key)
        {
        }

        protected override AISubActivation init(Humanoid a, AIManager d)
        {
            AISubActivation s = ho.init(a, d);
            if (s != null)
                return s;
            return res.set(a, d);
        }

        private readonly Resumer res = new Resumer(¤¤name)
        {
            protected override AISubActivation setAction(Humanoid a, AIManager d)
            {
                if (RND.oneIn(10))
                    sound.rnd(a);
                return AI.SUBS().LAY.activate(a, d);
            }

            protected override AISubActivation res(Humanoid a, AIManager d)
            {
                if (STATS.NEEDS().INJURIES.COUNT.indu().isMax(a.indu()))
                {
                    HumanoidResource.dead = a.lastLeaveCause() != null ? a.lastLeaveCause() : CAUSE_LEAVES.getAccident();
                }
                else if (!STATS.NEEDS().INJURIES.inDanger(a.indu()))
                {
                    return null;
                }

                AISubActivation s = ho.init(a, d);
                if (s != null)
                    return s;

                return set(a, d);
            }

            public override bool con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void can(Humanoid a, AIManager d)
            {
            }
        };
    }
}