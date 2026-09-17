using System;
using System.Collections.Generic;
using System.Linq;

namespace World.Battle
{
    using Game.Faction;
    using Init.Constant;
    using Snake2D.Log;
    using Snake2D.Util.File;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Sets;
    using World;
    using World.Army;
    using World.Entity.Army;
    using World.Map.Regions;
    using World.Region;

    class Side
    {
        private readonly ArrayList<SideUnit> all = new ArrayList<SideUnit>(Config.Battle().DivisionsPerArmy);
        {
            while (all.HasRoom())
                all.Add(new SideUnit());
        }
        public readonly ArrayList<SideUnit> us = new ArrayList<SideUnit>(Config.Battle().DivisionsPerArmy);

        private int divisions;

        private readonly SideUnit unitTmp = new SideUnit();
        private readonly int[] ui = Alloc.Ii(Config.Battle().DivisionsPerArmy);
        private readonly int[] di = Alloc.Ii(Config.Battle().DivisionsPerArmy);

        public Side()
        {
        }

        void Clear()
        {
            all.Add(us);
            us.ClearSloppy();
            divisions = 0;
        }

        public Side Copy()
        {
            Side s = new Side();
            for (int ui = 0; ui < us.Size; ui++)
            {
                SideUnit u = us.Get(ui);
                SideUnit n = new SideUnit();

                n.Copy(u);
                s.us.Add(n);
            }

            s.divisions = divisions;

            for (int i = 0; i < di.Length; i++)
            {
                s.ui[i] = this.ui[i];
                s.di[i] = di[i];
            }

            return s;
        }

        public void Debug()
        {
            Log.Ln(Divs());
            for (int i = 0; i < Divs(); i++)
                Log.Ln(Div(i).Men + " " + Div(i).Name + " " + Div(i).BannerI());
        }

        public void Add(WArmy a)
        {
            int max = Config.Battle().DivisionsPerArmy - divisions;
            foreach (SideUnit s in us)
                if (s.A() == a)
                    return;

            unitTmp.Set(a, max);
            Inited();
        }

        public void Add(Region reg)
        {
            int max = Config.Battle().DivisionsPerArmy - divisions;

            foreach (SideUnit s in us)
                if (s.R() == reg)
                    return;

            unitTmp.Set(reg, max);

            Inited();
        }

        private void Inited()
        {
            if (unitTmp.A() != null && unitTmp.Divs() <= 0)
                return;

            for (int i = 0; i < us.Size; i++)
            {
                if (us.Get(i).IsSameAs(unitTmp))
                    return;
            }

            SideUnit u = all.RemoveLast();

            u.Copy(unitTmp);
            us.Add(u);

            for (int i = 0; i < u.Divs(); i++)
            {
                ui[divisions] = us.Size - 1;
                di[divisions] = i;
                divisions++;
            }
        }

        public int Divs()
        {
            return divisions;
        }

        public WDIV Div(int di)
        {
            SideUnit u = us.Get(ui[di]);
            int i = this.di[di];
            if (i < 0 || i >= u.Divs())
                return null;
            return u.Div(i);
        }

        public int Ui(int di)
        {
            return ui[di];
        }

        public SideUnit DivUnit(int di)
        {
            return us.Get(ui[di]);
        }

        static class SideUnit
        {
            private int type;
            private const int T_ARMY = 0;
            private const int T_GARRISON = 1;

            private int regionI;
            private int armyI;
            private int maxDivs;

            public SideUnit()
            {
            }

            void Copy(SideUnit o)
            {
                this.type = o.type;
                this.regionI = o.regionI;
                this.armyI = o.armyI;
                this.maxDivs = o.maxDivs;
            }

            public SideUnit Set(Region reg, int maxDivs)
            {
                type = T_GARRISON;
                regionI = reg.Index();
                this.maxDivs = maxDivs;
                return this;
            }

            public SideUnit Set(WArmy a, int maxDivs)
            {
                type = T_ARMY;
                armyI = a.ArmyIndex();
                this.maxDivs = maxDivs;
                return this;
            }

            public int Divs()
            {
                switch (type)
                {
                    case T_ARMY: return Clamp.I(a().Divs().Size, 0, maxDivs);
                    case T_GARRISON: return Clamp.I(RD.MILITARY().Divisions(r()).Size, 0, maxDivs);
                    default: throw new RuntimeException();
                }
            }

            public WDIV Div(int index)
            {
                switch (type)
                {
                    case T_ARMY: return a().Divs().Get(index);
                    case T_GARRISON: return RD.MILITARY().Divisions(r()).Get(index);
                    default: throw new RuntimeException();
                }
            }

            public Faction Faction()
            {
                switch (type)
                {
                    case T_ARMY: return a().Faction();
                    case T_GARRISON: return r().Faction();
                    default: throw new RuntimeException();
                }
            }

            public double Power()
            {
                switch (type)
                {
                    case T_ARMY: return (a().Faction() == FACTIONS.Player() ? 0.8 : 1.0) * AD.Power().Get(a());
                    case T_GARRISON: return (r().Faction() == FACTIONS.Player() ? 0.8 : 1.0) * RD.MILITARY().Power.GetD(r());
                    default: throw new RuntimeException();
                }
            }

            public int X()
            {
                switch (type)
                {
                    case T_ARMY: return a().Ctx();
                    case T_GARRISON: return r().Cx();
                    default: throw new RuntimeException();
                }
            }

            public int Y()
            {
                switch (type)
                {
                    case T_ARMY: return a().Cty();
                    case T_GARRISON: return r().Cy();
                    default: throw new RuntimeException();
                }
            }

            public int Men()
            {
                switch (type)
                {
                    case T_ARMY: return AD.Men(null).Get(a());
                    case T_GARRISON: return RD.MILITARY().Garrison.Get(r());
                    default: throw new RuntimeException();
                }
            }

            public WArmy A()
            {
                if (type == T_ARMY)
                    return WORLD.ENTITIES().Armies.Get(armyI);
                return null;
            }

            public Region R()
            {
                if (type == T_GARRISON)
                    return WORLD.REGIONS().GetByIndex(regionI);
                return null;
            }

            public bool IsSameAs(SideUnit o)
            {
                if (type == o.type)
                    return (type == T_GARRISON && regionI == o.regionI) || (type == T_ARMY && armyI == o.armyI);
                return false;
            }
        }

        static class Conflict
        {
            public readonly Side A = new Side();
            public readonly Side B = new Side();

            public Conflict()
            {
            }

            void Clear()
            {
                A.Clear();
                B.Clear();
            }
        }
    }
}