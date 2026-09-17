using System;
using System.Collections.Generic;

namespace World.Map.Pathing
{
    using Snake2D;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Sets;
    using Util;
    using World;
    using World.Map.Regions;
    using World.Map.Road;

    internal sealed class GenConnect
    {
        private readonly RegGroup[] table = new RegGroup[WREGIONS.MAX];

        public GenConnect(ACTION aa)
        {
            Tree<RegGroup> sort = new Tree<RegGroup>(WREGIONS.MAX)
            {
                IsGreaterThan = (current, cmp) => current.regs.Count > cmp.regs.Count
            };

            foreach (Region reg in WORLD.REGIONS().All())
            {
                if (reg.Info.Area() > 0)
                {
                    table[reg.Index()] = new RegGroup(reg);
                    sort.Add(table[reg.Index()]);
                }
            }

            while (sort.HasMore())
            {
                RegGroup g = sort.PollSmallest();
                if (g.regs.Count == 0)
                    continue;
                if (!sort.HasMore())
                    break;

                Flooder f = GUTIL.Flooder();
                f.Init(this);

                foreach (Region r in g.regs)
                {
                    f.PushSloppy(r.Cx(), r.Cy(), 0, null);
                }

                while (f.HasMore())
                {
                    PathTile t = f.PollSmallest();
                    Region now = WORLD.REGIONS().Map[t];
                    if (now != null && t.IsSameAs(now.Cx(), now.Cy()) && table[now.Index()] != g)
                    {
                        Join(g, table[now.Index()]);
                        Gen.Connect(t);
                        sort.Add(g);
                        f.Done();
                        break;
                    }

                    foreach (DIR d in DIR.ALL)
                    {
                        if (!WORLD.IN_BOUNDS(t, d))
                            continue;

                        double v = 1;
                        if (!WTRAV.Can(t.X(), t.Y(), d, false))
                            v = 100;
                        if (!WTRAV.Can(t.X(), t.Y(), d, true))
                            v = 50;
                        if (!WORLD.PATH().Map.Can(t, d))
                            v = 25;
                        f.PushSmaller(t, d, t.GetValue() + v * d.TileDistance(), t);
                    }
                }
                f.Done();
                aa.Exe();
            }
        }

        private void Join(RegGroup ga, RegGroup gb)
        {
            if (ga == gb)
                return;

            for (int i = 0; i < table.Length; i++)
            {
                if (table[i] != null && table[i] == gb)
                {
                    table[i] = ga;
                }
            }
            ga.regs.Add(gb.regs);
            gb.regs.Clear();
        }

        private sealed class RegGroup
        {
            public readonly ArrayListGrower<Region> regs = new ArrayListGrower<Region>();

            public RegGroup(Region reg)
            {
                regs.Add(reg);
            }
        }
    }
}