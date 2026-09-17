using System;
using System.Text;

namespace Settlement.Tilemap
{
    public static class GuiTerrainHoverInfo
    {
        private static readonly string ¤¤Degrade = "¤Degrade:";
        private static readonly string ¤¤Strength = "¤Strength:";
        private static readonly string ¤¤Border = "¤This is a static entry point to your city. Keep this clear and reachable.";

        static GuiTerrainHoverInfo()
        {
            // Assuming D.ts is a method to register a class for translation, if needed
            // D.ts(GuiTerrainHoverInfo);
        }

        private GuiTerrainHoverInfo()
        {
        }

        public static void Add(GBox box, int tx, int ty)
        {
            GText t;

            SETT.TileMap().Ground.Hover(box, tx, ty);

            if (!TERRAIN().NADA.Is(tx, ty))
            {
                TERRAIN().Get(tx, ty).HoverInfo(box, tx, ty);
                double st = GAME.Armies().Map.Strength.Get(tx, ty) / C.TILE_SIZE;
                if (st > 0)
                {
                    box.NL();
                    box.TextL(¤¤Strength);
                    box.Add(GFORMAT.F0(box.Text(), st));
                }
                box.Sep();
            }

            if (FLOOR().Getter.Is(tx, ty))
            {
                t = box.Text();
                t.Lablify().Add(FLOOR().Getter.Get(tx, ty).Name());
                box.Add(t);
                box.Add(box.Text().Add(¤¤Degrade));
                box.Add(GFORMAT.PercInv(box.Text(), FLOOR().Degrade.Get(tx, ty)));

                if (MAINTENANCE().Isser.Is(tx, ty))
                {
                    box.Add(SPRITES.Icons().S.Hammer);
                }
                box.Sep();
            }

            if (SETT.Entry().Points.Map.Is(tx, ty))
            {
                box.Add(box.Text().Normalify2().Add(¤¤Border));
                box.Sep();
            }
        }
    }
}