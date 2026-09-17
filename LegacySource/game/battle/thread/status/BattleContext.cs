using System;
using System.IO;

namespace Game.Battle.Thread.Status
{
    class Config
    {
        public static class Battle
        {
            public static int DIVISIONS_PER_BATTLE { get; } = 10; // Example value, replace with actual value
        }
    }

    interface ISavable
    {
        void Save(FilePutter file);
        void Load(FileGetter file);
        void Clear();
    }

    class FilePutter
    {
        private readonly BinaryWriter writer;

        public FilePutter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void Put(int value)
        {
            writer.Write(value);
        }

        public void Put(float value)
        {
            writer.Write(value);
        }

        // Add other necessary methods for saving different types
    }

    class FileGetter
    {
        private readonly BinaryReader reader;

        public FileGetter(BinaryReader reader)
        {
            this.reader = reader;
        }

        public int GetInt()
        {
            return reader.ReadInt32();
        }

        public float GetFloat()
        {
            return reader.ReadSingle();
        }

        // Add other necessary methods for loading different types
    }

    class DivStatus
    {
        // Define properties and methods for DivStatus
    }

    class DivsTileMap
    {
        public DivsTileMap(DivStatus[] statuses)
        {
            // Initialize DivsTileMap
        }
    }

    class DivsQuadMap
    {
        // Define properties and methods for DivsQuadMap
    }

    class DivsSpaceMap
    {
        public DivsSpaceMap(DivStatus[] statuses)
        {
            // Initialize DivsSpaceMap
        }
    }

    class DivArmyMap
    {
        public DivArmyMap(DivStatus[] statuses)
        {
            // Initialize DivArmyMap
        }
    }

    final class BattleContext : ISavable
    {
        public readonly DivStatus[] statuses = new DivStatus[Config.Battle.DIVISIONS_PER_BATTLE];
        public readonly DivsTileMap map = new DivsTileMap(statuses);
        public readonly DivsQuadMap quads = new DivsQuadMap();
        public readonly DivsSpaceMap space = new DivsSpaceMap(statuses);
        public readonly DivArmyMap army = new DivArmyMap(statuses);

        public BattleContext()
        {
            for (int i = 0; i < statuses.Length; i++)
                statuses[i] = new DivStatus();
        }

        public void Save(FilePutter file)
        {
            foreach (DivStatus s in statuses)
                s.Save(file);
        }

        public void Load(FileGetter file)
        {
            foreach (DivStatus s in statuses)
                s.Load(file);
        }

        public void Clear()
        {
            foreach (DivStatus s in statuses)
                s.Clear();
        }
    }
}