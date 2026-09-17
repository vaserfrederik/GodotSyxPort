using init.constant;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using view.main;
using view.subview;
using view.tool;

namespace view.tool
{
    internal sealed class PlacableSimpleTileTool : placeFunc
    {
        private PlacableSimpleTile placable;

        public override void updateHovered(float ds, GameWindow window, bool pressed)
        {
            placable.renderOverlay(window);
        }

        public override void update(float ds, GameWindow window, bool pressed)
        {
            placable.renderOverlay(window);
        }

        public override void render(SPRITE_RENDERER r, float ds, GameWindow window)
        {
            int tx = window.tile().x();
            int ty = window.tile().y();

            CharSequence problem = placable.isPlacable(tx, ty);

            if (problem == null)
            {
                placable.renderPlaceHolder(r, tx, ty, window.tile().rel().x() + C.TILE_SIZEH, window.tile().rel().y() + C.TILE_SIZEH, false);
                placable.hoverInfo(tx, ty, VIEW.hoverBox());
            }
            else
            {
                placable.renderPlaceHolder(r, tx, ty, window.tile().rel().x() + C.TILE_SIZEH, window.tile().rel().y() + C.TILE_SIZEH, true);
                VIEW.hoverBox().error(problem);
            }
            COLOR.unbind();
            placable.renderExtra(r);
        }

        public override void click(GameWindow window)
        {
            int tx = window.tile().x();
            int ty = window.tile().y();

            CharSequence problem = placable.isPlacable(tx, ty);
            if (problem != null)
                return;

            placable.place(tx, ty);
        }

        public override void activate(PLACABLE placer, GameWindow window)
        {
            placable = (PlacableSimpleTile)placer;
        }

        public override void clickRelease(GameWindow window)
        {
        }

        public override LIST<CLICKABLE> gui()
        {
            return null;
        }
    }
}