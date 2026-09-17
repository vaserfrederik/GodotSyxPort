using System;
using System.Collections.Generic;
using System.Text;
using game.faction;
using util.data;
using world.map.regions;

namespace world.region
{
    public class RData : INT_O<Region>
    {
        public readonly ICharSequence name;

        public RData(string key, INT_OE<Region> plocal, RDInit init, ICharSequence name)
        {
            this.name = name;
            new RD.RDOwnerChanger
            {
                change = (Region reg, Faction oldOwner, Faction newOwner) =>
                {
                    Change(oldOwner);
                    Change(newOwner);
                },
                Change = (Faction f) =>
                {
                    if (f != null)
                    {
                        ftotal.Set(f, 0);
                        for (int i = 0; i < f.realm().regions(); i++)
                            ftotal.Inc(f, plocal.Get(f.realm().region(i)));
                    }
                }
            };
            this.plocal = plocal;
            ftotal = init.rCount.NewDataInt(key);
        }

        protected readonly INT_OE<Region> plocal;
        protected readonly INT_OE<Faction> ftotal;

        public override int Get(Region t)
        {
            return plocal.Get(t);
        }

        public override int Min(Region t)
        {
            return 0;
        }

        public override int Max(Region t)
        {
            return plocal.Max(t);
        }

        public INT_O<Faction> faction()
        {
            return ftotal;
        }

        public class RDataE : RData, INT_OE<Region>
        {
            public RDataE(string key, INT_OE<Region> plocal, RDInit init, ICharSequence name)
                : base(key, plocal, init, name)
            {
            }

            public override void Set(Region t, int i)
            {
                if (i != Get(t))
                {
                    if (t.faction() != null)
                    {
                        ftotal.Inc(t.faction(), -plocal.Get(t));
                    }
                    this.plocal.Set(t, i);
                    if (t.faction() != null)
                    {
                        ftotal.Inc(t.faction(), plocal.Get(t));
                    }
                }
            }
        }
    }
}