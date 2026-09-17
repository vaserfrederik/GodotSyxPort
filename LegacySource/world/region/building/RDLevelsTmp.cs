using System;
using System.Linq;

namespace World.Region.Building
{
    using Game.Faction;
    using Snake2D.Util.File;
    using View.Main;
    using World.Map.Regions;
    using World.Region;
    using World.Region.Building.RDBuildPoints;

    public class RDLevelsTmp
    {
        Region reg;
        private readonly int[] levels;
        int active = 0;

        public RDLevelsTmp(int am)
        {
            levels = Alloc.Ii(am);
        }

        public void Init(Region reg)
        {
            this.reg = reg;
            levels.Fill(0);
            for (int i = 0; i < RD.BUILDINGS().All.Count; i++)
            {
                RDBuilding b = RD.BUILDINGS().All[i];
                levels[b.Index()] = b.Level.Get(reg);
            }
        }

        public int Level(RDBuilding bu, Region reg)
        {
            if (active > 0 && reg == this.reg)
                return levels[bu.Index()];
            return bu.Level.Get(reg);
        }

        public void LevelSet(RDBuilding bu, int i)
        {
            levels[bu.Index()] = i;
        }

        public bool HasChange()
        {
            foreach (RDBuilding b in RD.BUILDINGS().All)
            {
                if (levels[b.Index()] != b.Level.Get(reg))
                    return true;
            }
            return false;
        }

        public int Cost()
        {
            if (vi == VIEW.RI())
                return cc;
            vi = VIEW.RI();
            int am = 0;
            foreach (RDBuilding b in RD.BUILDINGS().All)
            {
                if (levels[b.Index()] > b.Level.Get(reg))
                    am += b.Levels[levels[b.Index()]].Cost - b.Levels[b.Level.Get(reg)].Cost;
            }
            cc = am;
            return cc;
        }

        public void Accept()
        {
            foreach (RDBuilding b in RD.BUILDINGS().All)
            {
                b.Level.Set(reg, levels[b.Index()]);
            }
        }

        public bool CanAfford()
        {
            if (Cost() > FACTIONS.Player().Credits().GetD())
                return false;
            foreach (RDBuildPoint b in RD.BUILDINGS().Costs.ALL)
            {
                if (b.Bo.Get(reg) < 0)
                {
                    return false;
                }
            }
            return true;
        }

        private int vi = -1;
        private int cc;

        public string CanAfford(RDBuilding bu, Region reg, int level)
        {
            int lc = Level(bu, reg);
            return bu.CanAfford(reg, lc, level);
        }
    }
}