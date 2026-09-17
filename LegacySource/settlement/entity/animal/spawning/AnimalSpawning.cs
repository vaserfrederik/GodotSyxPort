using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Entity.Animal.Spawning
{
    public sealed class AnimalSpawning
    {
        private readonly List<AnimalSpawnSpot> spots = new List<AnimalSpawnSpot>(16);
        public const double SpawnRateDay = 1.0 / 100;
        private readonly IUpdater updater;
        private int max = 0;
        private readonly double[] killed;

        public AnimalSpawning(Animals animals)
        {
            for (int i = 0; i < spots.Capacity; i++)
                spots.Add(new AnimalSpawnSpot(i));
            killed = new double[animals.Species.Count];
            updater = new IUpdater(16, TIME.SecondsPerDay())
            {
                Update = (i, timeSinceLast) =>
                {
                    AnimalSpawnSpot sp = spots[i];
                    if (sp.Active())
                        max -= sp.Max();
                    sp.Update(SpawnRateDay);
                    if (sp.Active())
                        max += sp.Max();
                }
            };
        }

        public void Update(double ds)
        {
            updater.Update(ds);
        }

        public int SpawnsPerDay()
        {
            return (int)Math.Ceiling(max * SpawnRateDay);
        }

        public void Generate(Animals animals, CapitolArea carea)
        {
            Saver.Clear();
            new Generator(animals, carea, spots);
            foreach (AnimalSpawnSpot s in spots)
                max += s.Max();
        }

        public readonly Saver Saver = new Saver();

        public List<AnimalSpawnSpot> All()
        {
            return spots;
        }

        public bool IsTimeForAKill(AnimalSpecies s)
        {
            return killed[s.Index()] / 4 >= 1;
        }

        public void ReportKilled(AnimalSpecies s)
        {
            killed[s.Index()] += s.Danger;
        }

        public void ReportKillRevenge(AnimalSpecies s)
        {
            killed[s.Index()] = 0;
        }

        public class Saver : ISavable
        {
            public void Save(FilePutter file)
            {
                foreach (AnimalSpawnSpot s in spots)
                    s.Save(file);
                updater.Save(file);
                SETT.ANIMALS().Map.Saver().Save(killed, file);
            }

            public void Load(FileGetter file)
            {
                Clear();
                foreach (AnimalSpawnSpot s in spots)
                {
                    s.Load(file);
                    max += s.Max();
                }
                updater.Load(file);
                SETT.ANIMALS().Map.Loader().Load(killed, file, 0);
            }

            public void Clear()
            {
                foreach (AnimalSpawnSpot s in spots)
                    s.Clear();
                updater.Clear();
                max = 0;
                Array.Fill(killed, 0.0);
            }
        }
    }
}