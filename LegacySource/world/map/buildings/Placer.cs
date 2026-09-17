using System;
using init.sprite.UI;
using snake2d.util.datatypes;
using util.text;
using view.subview;
using view.tool;
using world;

namespace world.map.buildings
{
    class Placer : PlacableMulti
    {
        private static readonly CharSequence ¤¤name = "village";

        private readonly PlacableMulti undo = new PlacableMulti(Dic.¤¤remove + ": " + ¤¤name, "", UI.icons().m.building.twin(UI.icons().m.anti))
        {
            public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                BUILDINGS().village.set(tx, ty, false);
            }

            public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
            {
                return BUILDINGS().village.is(tx, ty) ? null : E;
            }

            public override void updateRegardless(GameWindow window, AREA selected)
            {
                BUILDINGS().debugVisible = true;
            }
        };

        static Placer()
        {
            D.ts(typeof(Placer));
        }

        public Placer() : base(¤¤name, "", UI.icons().m.building)
        {
        }

        public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            int tile = tx + ty * TWIDTH();
            BUILDINGS().village.set(tile, true);
        }

        public override CharSequence isPlacable(int tx, int ty, AREA area, PLACER_TYPE type)
        {
            if (!IN_BOUNDS(tx, ty))
                return PlacableMessages.¤¤IN_MAP;
            if (WORLD.REGIONS().isCentre.is(tx, ty))
                return PlacableMessages.¤¤BLOCKED;
            return null;
        }

        public override void updateRegardless(GameWindow window, AREA selected)
        {
            BUILDINGS().debugVisible = true;
        }

        public override PLACABLE getUndo()
        {
            return undo;
        }
    }
}