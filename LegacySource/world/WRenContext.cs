using snake2d;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.rendering;

namespace world
{
    public class WRenContext
    {
        public SPRITE_RENDERER r;
        public ShadowBatch s;
        public float ds;
        public readonly RenderData data;

        public readonly Bitmap2D fow;
        public readonly Bitmap2D hiBuildings;

        public WRenContext(int width, int height)
        {
            data = new RenderData(width, height);
            fow = new Bitmap2D(width, height, false);
            hiBuildings = new Bitmap2D(width, height, false);
        }

        public void Init(SPRITE_RENDERER r, ShadowBatch s, RECTANGLE renWindow, int offX, int offY, float ds)
        {
            this.r = r;
            this.s = s;
            data.Init(renWindow, offX, offY);
            this.ds = ds;

            for (int y = data.ty1() - 1; y <= data.ty2() + 1; y++)
            {
                for (int x = data.tx1() - 1; x <= data.tx2() + 1; x++)
                {
                    fow.Set(x, y, false);
                    hiBuildings.Set(x, y, false);
                }
            }
        }
    }
}