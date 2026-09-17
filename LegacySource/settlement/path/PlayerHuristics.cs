using System;
using System.IO;
using System.Linq;

namespace Settlement.Path
{
    using static Settlement.Main.SETT;
    using static Snake2D.PathGame.COST;
    using static Snake2D.Util.Map.MAP_DOUBLE;
    using static Snake2D.Util.File.Alloc;
    using static Snake2D.Util.File.FileGetter;
    using static Snake2D.Util.File.FilePutter;
    using static Snake2D.Util.File.SAVABLE;
    using static Snake2D.Util.Map.MAP_DOUBLE;
    using static Util.Updating.TileUpdater;

    public sealed class PlayerHuristics : COST
    {
        private readonly byte[] counts = Bb(TAREA);
        public readonly MAP_DOUBLE getter = new MAP_DOUBLE()
        {
            public double Get(int tx, int ty) => Get(tx + ty * TWIDTH);

            public double Get(int tile) => I * (counts[tile] & 0x0FF);
        };

        private readonly TileUpdater updater = new TileUpdater(TWIDTH, THEIGHT, 4 * 128)
        {
            protected override void Update(int tx, int ty, int i, double timeSinceLast)
            {
                if (counts[i] != 0)
                    counts[i] = (byte)((counts[i] & 0x0FF) / 16);
            }
        };

        public PlayerHuristics()
        {
        }

        private readonly SAVABLE saver = new SAVABLE()
        {
            public void Save(FilePutter file)
            {
                file.Bs(counts);
                updater.Save(file);
            }

            public void Load(FileGetter file)
            {
                file.Bs(counts);
                updater.Load(file);
            }

            public void Clear()
            {
                counts.Fill((byte)0);
                updater.Clear();
            }
        };

        public void Set(int tx, int ty)
        {
            int i = tx + ty * TWIDTH;
            if (counts[i] != -1)
                counts[i] = (byte)((counts[i] & 0x0FF) + 1);
        }

        public void Update(double ds)
        {
            updater.UpdateRandom(ds);
        }

        public double GetCost(int fromX, int fromY, DIR d)
        {
            return GetCost(fromX, fromY, fromX + d.X, fromY + d.Y);
        }

        private static readonly double I = 1.0 / 512.0;

        public double GetCost(int fromX, int fromY, int toX, int toY)
        {
            AVAILABILITY a = PATH().GetAvailability(toX, toY);
            if (a.player < 0)
            {
                return BLOCKED;
            }
            if (fromX != toX && fromY != toY)
            {
                if (PATH().GetAvailability(fromX, toY).player <= -1 || PATH().GetAvailability(toX, fromY).player <= -1)
                {
                    return SKIP;
                }
            }

            int i = toX + toY * TWIDTH;
            double pen = 1.0 + (counts[i] != 0 ? (counts[i] & 0x0FF) * I : 0.0);

            return (a.player * pen + PATH().GetAvailability(fromX, fromY).from);
        }
    }
}