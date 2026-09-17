using System;
using System.Collections.Generic;

namespace Settlement.Path.Components
{
    using static Settlement.Main.Sett.PATH;

    public sealed class FindableDataSingle : FindableData, SCompPatherFinder
    {
        private static readonly LinkedList<FindableDataSingle> all = new LinkedList<FindableDataSingle>();

        public FindableDataSingle(ICharSequence name) : base(name)
        {
            all.Add(this);
        }

        public override bool IsInComponent(SComponent c, double distance)
        {
            return Get(c) > 0;
        }

        public bool Has(int startX, int startY)
        {
            SComponent s = PATH().Comps.Zero.Get(startX, startY);
            if (s == null)
                return false;
            while (s.SuperComp() != null)
                s = s.SuperComp();
            return Get(s) > 0;
        }

        public bool Has(SComponent s)
        {
            if (s == null)
                return false;
            while (s.SuperComp() != null)
                s = s.SuperComp();
            return Get(s) > 0;
        }
    }
}