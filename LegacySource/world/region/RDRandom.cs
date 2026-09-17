using System;
using init.sprite.UI;
using init.value;
using util.data;
using world.map.regions;
using world.region.RD;

public sealed class RDRandom : DataRandom<Region>
{
    public RDRandom(RDInit init) : base(init.count, 4)
    {
        for (int i = 0; i < 8; i++)
        {
            int bit = 16 * i;
            DOUBLE_O<Region> vv = new DOUBLE_O<Region>()
            {
                public double getD(Region reg)
                {
                    return this.get(reg, bit, 16);
                }
            };
            GVALUES.REGION.push("RANDOM_" + i, "Random: " + i, UI.icons().s.question, vv, false);
        }
    }
}