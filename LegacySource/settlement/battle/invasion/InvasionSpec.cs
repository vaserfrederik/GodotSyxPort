using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Battle.Invasion
{
    public sealed class InvasionSpec
    {
        public readonly TRStockpile loot = new TRStockpile();
        public readonly List<DivGeneration> divs = new List<DivGeneration>(Config.Battle().DivisionsPerArmy);
        public readonly int[] artillery = new int[SETT.Rooms().ArtillerySize];
        public int wx = -1, wy = -1;
        public int fi = -1;
        public bool canBeAttacked = true;
        public int @ref = -1;
        public double power;

        public InvasionSpec() { }

        public InvasionSpec(FileGetter f) : this() => Load(f);

        private void Load(FileGetter f)
        {
            loot.Load(f);
            int am = f.I();
            for (int i = 0; i < am; i++)
                divs.Add(new DivGeneration(f));
            f.IsE(artillery);
            wx = f.I();
            wy = f.I();
            fi = f.I();
            canBeAttacked = f.Bool();
            @ref = f.I();
            power = f.D();
        }

        public void Add(DivGeneration g)
        {
            power = GAME.Battle().Power.Get(g.MakeSpec());
            divs.Add(g);
        }

        public void Add(IEnumerable<DivGeneration> g)
        {
            power = 0;
            divs.Clear();
            foreach (DivGeneration d in g)
                Add(d);
        }

        public void Save(FilePutter file)
        {
            loot.Save(file);
            file.I(divs.Count);
            foreach (DivGeneration g in divs)
                g.Save(file);
            file.IsE(artillery);
            file.I(wx);
            file.I(wy);
            file.I(fi);
            file.Bool(canBeAttacked);
            file.I(@ref);
            file.D(power);
        }
    }
}