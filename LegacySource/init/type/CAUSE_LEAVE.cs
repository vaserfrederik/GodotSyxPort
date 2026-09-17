using System.Collections.Generic;

namespace Init.Type
{
    public sealed class CauseLeave : INFO, MAPPED
    {
        public readonly string key;
        public readonly bool death, natural, leavesCorpse;
        private readonly int index;
        public readonly int indexDeath;
        private double defAgony;

        public CauseLeave(List<CauseLeave> all, List<CauseLeave> deaths, string key, string name, string names, string desc, bool death, bool natural, bool leavesCorpse)
            : base(name, names, desc, null)
        {
            this.key = key;
            this.death = death;
            this.natural = natural;
            this.leavesCorpse = leavesCorpse;
            index = all.Add(this);
            if (death)
                indexDeath = deaths.Add(this);
            else
                indexDeath = -1;
        }

        public int Index()
        {
            return index;
        }

        public string Key()
        {
            return key;
        }

        public double DefaultStanding()
        {
            return defAgony;
        }
    }
}