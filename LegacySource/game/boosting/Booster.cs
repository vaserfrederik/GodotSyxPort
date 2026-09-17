using System;
using System.Text;

namespace Game.Boosting
{
    public abstract class Booster : BoosterAbs<BOOSTABLE_O>
    {
        public Booster(BSourceInfo info, bool isMul)
            : base(info, isMul)
        {
        }

        public BoostSpec Add(Boostable boostable, CharSequence append)
        {
            BoostSpec b = new BoostSpec(this, boostable, append);
            boostable.AddFactor(b);
            return b;
        }

        public BoostSpec Add(Boostable boostable)
        {
            return Add(boostable, null);
        }

        public Booster AddRet(Boostable boostable)
        {
            Add(boostable, null);
            return this;
        }
    }
}