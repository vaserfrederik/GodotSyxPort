using System;
using settlement.main;
using init.paths;
using snake2d.util.file;
using snake2d.util.map;
using snake2d.util.rnd;

namespace settlement.tilemap.generator
{
    class GeneratorUtil
    {
        public readonly HeightMap height;
        public readonly FertilityTmp fer;
        public readonly Polymap polly;
        public readonly Json json;
        public readonly Checker checker;

        public GeneratorUtil()
        {
            height = new HeightMap(SETT.TWIDTH, SETT.THEIGHT, 128, 4);
            fer = new FertilityTmp();
            polly = new Polymap(SETT.TWIDTH, SETT.THEIGHT);
            json = new Json(PATHS.CONFIG().init.gets("GenerationSettlement")).json("GENERATION");
            checker = new Checker();
        }

        public class Checker : BooleanMapE
        {
            private readonly short[] checks;
            private short sI = 0;

            public Checker() : base(SETT.TWIDTH, SETT.THEIGHT)
            {
                checks = new short[SETT.THEIGHT * SETT.TWIDTH];
            }

            public override MAP_BOOLEANE set(int tile, bool value)
            {
                checks[tile] = (short)(value ? sI : sI - 1);
                return this;
            }

            public override bool is(int tile)
            {
                return checks[tile] == sI;
            }

            public void init()
            {
                sI++;
            }
        }

        public class FertilityTmp : MAP_DOUBLEE
        {
            private double[,] fer = new double[SETT.TWIDTH, SETT.TWIDTH];

            private FertilityTmp() { }

            public override double get(int x, int y)
            {
                if (SETT.TILE_BOUNDS.holdsPoint(x, y))
                    return fer[y, x];
                return 0;
            }

            public override double get(int tile)
            {
                throw new RuntimeException();
            }

            public override MAP_DOUBLEE set(int tile, double value)
            {
                throw new RuntimeException();
            }

            public override MAP_DOUBLEE set(int tx, int ty, double value)
            {
                if (SETT.TILE_BOUNDS.holdsPoint(tx, ty))
                    fer[ty, tx] = value;
                return this;
            }

            public void target(int tx, int ty, double value, double delta)
            {
                fer[ty, tx] = value * delta + (1.0 - delta) * fer[ty, tx];
            }
        }
    }
}