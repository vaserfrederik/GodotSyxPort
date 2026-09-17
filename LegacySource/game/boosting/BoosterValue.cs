using System;

namespace Game.Boosting
{
    public class BoosterValue : Booster
    {
        private readonly double from;
        private readonly double to;
        private readonly BValue value;

        public BoosterValue(BValue v, BSourceInfo info, double from, double to, bool isMul)
            : base(info, isMul)
        {
            this.from = from;
            this.to = to;
            this.value = v;
        }

        public BoosterValue(BValue v, BSourceInfo info, double to, bool isMul)
            : base(info, isMul)
        {
            this.value = v;
            this.to = to;

            if (isMul)
            {
                from = 1;
            }
            else
            {
                from = 0;
            }
        }

        public override double From()
        {
            return from;
        }

        public override double To()
        {
            return to;
        }

        public override double GetValue(double input)
        {
            input = Math.Clamp(input, 0, 1);
            return From() + input * (To() - From());
        }

        protected override double PGet(BOOSTABLE_O o)
        {
            return o.BoostableValue(value);
        }
    }
}