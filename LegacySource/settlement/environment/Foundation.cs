using System;
using System.IO;

namespace Settlement.Environment
{
    using static Settlement.Main.SETT;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Map;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sets;

    public class Foundation : IMapDouble
    {
        private readonly BitsMap1D data = new BitsMap1D(0, 2, SETT.TAREA);
        private const double II = 1.0 / 0b11;

        public static readonly string ¤¤name = "Foundation";
        public static readonly string ¤¤desc = "How well the ground is suited for supported buildings and rooms. Poor isolation will make constructed rooms require more building materials and maintenance.";

        static Foundation()
        {
            D.Ts(typeof(Foundation));
        }

        public Foundation()
        {
        }

        public double Get(int tile)
        {
            if (SETT.GROUND().Types.ROCK.Is(tile))
                return 0.5;
            return II * data.Get(tile);
        }

        public double Get(int tx, int ty)
        {
            if (SETT.GROUND().Types.ROCK.Is(tx, ty))
                return 0.5;
            return II * data.Get(tx + ty * SETT.TWIDTH);
        }

        public void Generate()
        {
            var h = new HeightMap(TWIDTH, THEIGHT, 128, 4);
            foreach (var c in SETT.TILE_BOUNDS)
            {
                double d = h.Get(c);
                if (d < 0.5)
                    d *= d;
                else
                    d = Math.Sqrt(d);
                data.Set(c.X() + c.Y() * SETT.TWIDTH, Clamp.I((int)Math.Round(d * 0b11), 0, 0b11));
            }
        }

        private readonly ISavable saver = new SavableClass();

        private class SavableClass : ISavable
        {
            public void Save(FilePutter file)
            {
                data.Save(file);
            }

            public void Load(FileGetter file)
            {
                data.Load(file);
            }

            public void Clear()
            {
                // TODO Auto-generated method stub
            }
        }
    }
}