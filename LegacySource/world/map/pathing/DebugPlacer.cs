using System;
using System.Text;
using snake2d;
using init.constant;
using init.sprite;
using world;
using world.map.pathing;
using view.main;
using view.subview;
using view.tool;

namespace world.map.pathing
{
    public class DebugPlacer : PlacableSimpleTile
    {
        private Coo clicked = new Coo();

        public DebugPlacer() : base("world path")
        {
        }

        public override CharSequence IsPlacable(int tx, int ty)
        {
            return WORLD.PATH().map.is.is(tx, ty) ? null : E;
        }

        public override void Place(int tx, int ty)
        {
            clicked.set(tx, ty);
        }

        public override void RenderOverlay(GameWindow window)
        {
        }

        public override void RenderExtra(SPRITE_RENDERER r)
        {
            if (!WORLD.PATH().map.is.is(clicked))
                return;

            PathTile t = WORLD.PATH().path(clicked, VIEW.world().window.tile(), Treaty.DUMMY);
            GameWindow w = VIEW.world().window;

            while (t != null)
            {
                int x = (t.x() - w.tile().x()) * C.TILE_SIZE + w.tile().rel().x();
                int y = (t.y() - w.tile().y()) * C.TILE_SIZE + w.tile().rel().y();
                SPRITES.cons().BIG.line.render(r, 0, x, y);
                t = t.getParent();
            }
        }
    }
}