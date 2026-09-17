using settlement.maintenance;
using init.sprite.UI;
using settlement.main;
using settlement.room.main;
using snake2d.util.datatypes;
using snake2d.util.sprite;
using util.text;
using view.subview;
using view.tool;

namespace settlement.maintenance
{
    internal sealed class PlacerDormant : PlacableMulti
    {
        private static readonly CharSequence ¤¤name = "¤Enable maintenance";
        private static readonly CharSequence ¤¤nameAnti = "¤Disable maintenance";
        private static readonly CharSequence ¤¤desc = "¤Enables maintenance performed by janitors in an area.";
        private static readonly CharSequence ¤¤descAnti = "¤Disables maintenance performed by janitors on an area.";

        static PlacerDormant()
        {
            D.ts(typeof(PlacerDormant));
        }

        public PlacerDormant() : base(¤¤name, ¤¤desc, null)
        {
        }

        public override CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            if (SETT.MAINTENANCE().disabled.is(tx, ty))
                return null;
            return E;
        }

        public override void place(int tx, int ty, AREA a, PLACER_TYPE t)
        {
            SETT.MAINTENANCE().disabled.set(tx, ty, false);
        }

        public override PLACABLE getUndo()
        {
            return undo;
        }

        public override bool expandsTo(int fromX, int fromY, int toX, int toY)
        {
            Room r = SETT.ROOMS().map.get(fromX, fromY);
            if (r != null && r.degrader(fromX, fromY) != null && r.isSame(fromX, fromY, toX, toY))
                return true;
            return false;
        }

        public override void updateRegardless(GameWindow window, AREA selected)
        {
            SETT.OVERLAY().MAINTENANCE.add();
            base.updateRegardless(window, selected);
        }

        private SPRITE icon;

        public override SPRITE getIcon()
        {
            if (icon == null)
                icon = SETT.ROOMS().JANITOR.icon.twin(UI.icons().s.cog, DIR.NE, 1);
            return icon;
        }

        public readonly PlacableMulti undo = new PlacableMulti(¤¤nameAnti, ¤¤descAnti, null)
        {
            private SPRITE icon;

            public override CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
            {
                if (!SETT.MAINTENANCE().disabled.is(tx, ty))
                    return null;
                return E;
            }

            public override void place(int tx, int ty, AREA a, PLACER_TYPE t)
            {
                SETT.MAINTENANCE().disabled.set(tx, ty, true);
            }

            public override SPRITE getIcon()
            {
                if (icon == null)
                    icon = SETT.ROOMS().JANITOR.icon.twin(UI.icons().m.anti, DIR.C, 1);
                return icon;
            }

            public override void updateRegardless(GameWindow window, AREA selected)
            {
                SETT.OVERLAY().MAINTENANCE.add();
                base.updateRegardless(window, selected);
            }

            public override bool expandsTo(int fromX, int fromY, int toX, int toY)
            {
                Room r = SETT.ROOMS().map.get(fromX, fromY);
                if (r != null && r.degrader(fromX, fromY) != null && r.isSame(fromX, fromY, toX, toY))
                    return true;
                return false;
            }

            public override PLACABLE getUndo()
            {
                return PlacerDormant.this;
            }
        };
    }
}