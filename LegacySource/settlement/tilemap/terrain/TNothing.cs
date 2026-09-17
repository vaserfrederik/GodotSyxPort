using settlement.tilemap.terrain;
using init.sprite;
using settlement.path;
using snake2d;
using util.gui.misc;
using util.rendering;

namespace settlement.tilemap.terrain
{
    public class TNothing : TerrainTile
    {
        protected TNothing(Terrain shared) : base("NOTHING", shared, "clear", new SPRITE.Twin(SPRITES.icons().m.terrain, SPRITES.icons().m.cancel), null)
        {
        }

        protected override bool place(int x, int y)
        {
            placeRaw(x, y);
            return false;
        }

        protected override bool renderAbove(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            return false;
        }

        protected override bool renderBelow(SPRITE_RENDERER r, ShadowBatch s, RenderData.RenderIterator i, int data)
        {
            return false;
        }

        public override bool isPlacable(int tx, int ty)
        {
            return true;
        }

        public override AVAILABILITY getAvailability(int tx, int ty)
        {
            return null;
        }

        public override void hoverInfo(GBox box, int tx, int ty)
        {
        }

        public override int miniDepth()
        {
            return 0;
        }
    }
}