using System;

namespace Snake2d.Util.Color
{
    public class OpaPuls : OpacityImp
    {
        private int baseValue;
        private int delta;

        public OpaPuls(int baseValue, int max)
            : base(baseValue)
        {
            this.baseValue = baseValue;
            delta = max - baseValue;
        }

        public override void Set(int op)
        {
            baseValue = op;
        }

        public override void Increase(float factor)
        {
            baseValue *= factor;
        }

        public override void Increase(int amount)
        {
            baseValue += amount;
        }

        public void IncreaseMax(float factor)
        {
            delta *= factor;
        }

        public void IncreaseMax(int amount)
        {
            delta += amount;
        }

        public override void Bind()
        {
            base.Set((int)(baseValue + delta * CORE.GetUpdateInfo().GetPendulum0To1s1()));
            base.Bind();
        }
    }
}