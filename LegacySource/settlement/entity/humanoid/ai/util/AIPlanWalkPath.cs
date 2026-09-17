using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using snake2d.util.sprite.text;

namespace settlement.entity.humanoid.ai.util
{
    public abstract class AIPlanWalkPath : AIPLAN.PLANRES
    {
        private readonly string name;
        private readonly bool full;

        public AIPlanWalkPath(string key, string name)
            : this(key, name, false)
        {
        }

        public AIPlanWalkPath(string key, string name, bool full)
            : base(key)
        {
            this.name = name;
            this.full = full;
        }

        protected override AISubActivation Init(Humanoid a, AIManager d)
        {
            return walk.Set(a, d);
        }

        protected override void Name(Humanoid a, AIManager d, Str str)
        {
            str.Add(name);
        }

        private readonly Resumer walk = new Resumer();

        private class Resumer : AISubActivation
        {
            public override AISubActivation SetAction(Humanoid a, AIManager d)
            {
                if (full)
                    return AI.SUBS().walkTo.pathFull(a, d);
                return AI.SUBS().walkTo.path(a, d);
            }

            public override AISubActivation Res(Humanoid a, AIManager d)
            {
                return next(a, d);
            }

            public override bool Con(Humanoid a, AIManager d)
            {
                return true;
            }

            public override void Can(Humanoid a, AIManager d)
            {
            }
        }

        public abstract AISubActivation Next(Humanoid a, AIManager d);
    }
}