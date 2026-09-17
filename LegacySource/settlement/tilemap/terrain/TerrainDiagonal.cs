using System;
using settlement.tilemap.terrain;
using settlement.main;
using settlement.room.main;
using snake2d.util.datatypes;
using util.text;
using view.tool;

namespace settlement.tilemap.terrain
{
    public static class TerrainDiagonal
    {
        private static string ¤¤name = "¤Make Diagonal";
        private static string ¤¤desc = "¤Turns walls diagonal, only for aesthetic purposes.";

        private static string ¤¤undo = "¤Make Rectangular";
        private static string ¤¤undoDesc = "¤Turns walls rectangular, only for aesthetic purposes.";

        private static string ¤¤problem = "¤Must be placed on a wall like structure or a road.";

        static TerrainDiagonal()
        {
            D.ts(typeof(TerrainDiagonal));
        }

        public static PlacableMulti placer = new PlacableMulti(¤¤name, ¤¤desc, SPRITES.icons().l.dia)
        {
            public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                Room ff = SETT.ROOMS().map.get(tx, ty);
                if (ff != null && ff.constructor() != null && ff.constructor().dia(tx, ty) != null)
                {
                    ff.constructor().dia(tx, ty).setDia(tx, ty, true);
                }
                if (!SETT.ROOMS().map.is(tx, ty) && SETT.FLOOR().getter.get(tx, ty) != null)
                    SETT.FLOOR().square.set(tx, ty, false);
                if (SETT.TERRAIN().get(tx, ty) is Diagonalizer)
                {
                    Diagonalizer t = (Diagonalizer)SETT.TERRAIN().get(tx, ty);
                    t.setDia(tx, ty, true);
                }
            }

            public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                Room ff = SETT.ROOMS().map.get(tx, ty);
                if (ff != null && ff.constructor() != null && ff.constructor().dia(tx, ty) != null)
                {
                    return null;
                }
                if (!SETT.ROOMS().map.is(tx, ty) && SETT.FLOOR().getter.get(tx, ty) != null)
                    return null;
                if (SETT.TERRAIN().get(tx, ty) is Diagonalizer)
                    return null;
                return ¤¤problem;
            }

            public override PLACABLE getUndo()
            {
                return undo;
            }
        };

        public static PlacableMulti undo = new PlacableMulti(¤¤undo, ¤¤undoDesc, SPRITES.icons().l.square)
        {
            public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                Room ff = SETT.ROOMS().map.get(tx, ty);
                if (ff != null && ff.constructor() != null && ff.constructor().dia(tx, ty) != null)
                {
                    ff.constructor().dia(tx, ty).setDia(tx, ty, false);
                }
                if (!SETT.ROOMS().map.is(tx, ty) && SETT.FLOOR().getter.get(tx, ty) != null)
                    SETT.FLOOR().square.set(tx, ty, true);
                if (SETT.TERRAIN().get(tx, ty) is Diagonalizer)
                {
                    Diagonalizer t = (Diagonalizer)SETT.TERRAIN().get(tx, ty);
                    t.setDia(tx, ty, false);
                }
            }

            public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                Room ff = SETT.ROOMS().map.get(tx, ty);
                if (ff != null && ff.constructor() != null && ff.constructor().dia(tx, ty) != null)
                {
                    return null;
                }
                if (!SETT.ROOMS().map.is(tx, ty) && SETT.FLOOR().getter.get(tx, ty) != null)
                    return null;
                if (SETT.TERRAIN().get(tx, ty) is Diagonalizer)
                    return null;
                return ¤¤problem;
            }

            public override PLACABLE getUndo()
            {
                return placer;
            }
        };

        public static bool is(int tx, int ty)
        {
            Room ff = SETT.ROOMS().map.get(tx, ty);
            if (ff != null && ff.constructor() != null && ff.constructor().dia(tx, ty) != null)
            {
                return true;
            }
            if (ff == null && SETT.FLOOR().getter.get(tx, ty) != null)
                return true;
            if (SETT.TERRAIN().get(tx, ty) is Diagonalizer)
                return true;
            return false;
        }

        public interface Diagonalizer
        {
            public void setDia(int tx, int ty, bool dia);
            public bool getDia(int tx, int ty);
        }
    }
}