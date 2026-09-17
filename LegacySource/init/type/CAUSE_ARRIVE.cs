using System.Collections.Generic;
using util.info;
using util.keymap;

namespace init.type
{
    public sealed class CAUSE_ARRIVE : INFO, MAPPED
    {
        private readonly int index;
        public bool fromoutside;
        private readonly string key;

        public CAUSE_ARRIVE(LISTE<CAUSE_ARRIVE> all, string key, string name, string desc, bool fromOutside)
            : base(name, desc)
        {
            index = all.Add(this);
            this.fromoutside = fromOutside;
            this.key = key;
        }

        public override int Index()
        {
            return index;
        }

        public override string Key()
        {
            return key;
        }
    }
}