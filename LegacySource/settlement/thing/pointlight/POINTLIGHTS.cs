using System;
using System.IO;
using game.debug;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.file;
using util.rendering;
using view.sett;
using view.tool;

namespace settlement.thing.pointlight
{
    public class POINTLIGHTS : SettResource
    {
        private readonly PointMap map;
        public readonly Sprites sprites;
        private readonly LOS_MAP los;

        public POINTLIGHTS() : base("LIGHTS", true)
        {
            LightModel.flickerr(0);
            IDebugPanelSett.Add(new PlacableMulti("torch")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    torch(tx, ty, 0);
                }

                public override CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
                {
                    return null;
                }
            });

            IDebugPanelSett.Add(new PlacableMulti("torch remove")
            {
                public override void place(int tx, int ty, AREA area, PLACER_TYPE type)
                {
                    remove(tx, ty);
                }

                public override CharSequence isPlacable(int tx, int ty, AREA a, PLACER_TYPE type)
                {
                    return null;
                }
            });

            los = new LOS_MAP();
            map = new PointMap(SETT.TWIDTH, SETT.THEIGHT);
            sprites = new Sprites();
        }

        protected override void load(FileGetter file)
        {
            map.load(file);
        }

        public override void save(FilePutter file)
        {
            map.save(file);
        }

        protected override void clear()
        {
            map.clear();
        }

        public void torch(int tx, int ty, int off)
        {
            map.add(tx, ty, off, off, LightModel.torch);
        }

        public void torchBig(int tx, int ty, int off)
        {
            map.add(tx, ty, off, off, LightModel.torch_big);
        }

        public void fire(int tx, int ty, int off)
        {
            map.add(tx, ty, off, off, LightModel.fire);
        }

        public void candle(int tx, int ty, int off)
        {
            map.add(tx, ty, off, off, LightModel.candle);
        }

        public void candle(int tx, int ty, int offx, int offy)
        {
            map.add(tx, ty, offx, offy, LightModel.candle);
        }

        public void remove(int tx, int ty)
        {
            map.remove(tx, ty);
        }

        public void hide(int tx, int ty, bool hide)
        {
            map.hide(tx, ty, hide);
        }

        public bool is(int tx, int ty)
        {
            return map.is(tx, ty);
        }

        protected override void update(double ds, Profiler profiler)
        {
        }

        public void render(Renderer r, ShadowBatch s, float ds, RECTANGLE renWindow, int offX, int offY)
        {
            FireSparks.update(ds);
            LightModel.flickerr(ds);
            sprites.displacement.update(ds);
            sprites.texture.update(ds);
            map.render(r, s, ds, renWindow, offX, offY);
        }

        public void renderMouse(int x, int y, int offx, int offy, int rnd)
        {
            CORE.renderer().shadeLight(false);
            LightModel.mouse.register(CORE.renderer(), rnd, x, y, offx, offy);
        }

        public LOS_MAP los()
        {
            return los;
        }
    }
}