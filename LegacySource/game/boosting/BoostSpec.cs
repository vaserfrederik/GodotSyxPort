using System;

namespace Game.Boosting
{
    public sealed class BoostSpec
    {
        public readonly Booster booster;
        public readonly Boostable boostable;
        public readonly ICharSequence tName;
        // public readonly bool isMul;

        public BoostSpec(Booster source, Boostable target, ICharSequence append)
        {
            this.booster = source;
            this.boostable = target;
            ICharSequence tName = target.name;
            if (append != null)
                tName = tName + " (" + append + ")";
            this.tName = tName;
            // this.isMul = source.isMul;
        }

        public double Get(BOOSTABLE_O t)
        {
            return booster.Get(t);
        }

        public double Inc(BOOSTABLE_O t)
        {
            return booster.Get(t) - (booster.isMul ? 1 : 0);
        }

        public bool IsPositive(double input)
        {
            return (booster.isMul && booster.GetValue(input) >= 1) || booster.GetValue(input) > 0;
        }

        public bool IsSameAs(BoostSpec other)
        {
            if (booster.isMul == booster.isMul && boostable == other.boostable)
            {
                return true;
            }
            return false;
        }

        public string Identifier()
        {
            return boostable.key + booster.isMul;
        }
    }
}