using System.Collections.Generic;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using util.rendering;

namespace world.overlay
{
    class OverlayThings
    {
        private readonly List<Thing> objects = new List<Thing>(256);
        private int ai;

        public OverlayThings()
        {
            for (int i = 0; i < objects.Capacity; i++)
            {
                objects.Add(new Thing());
            }
        }

        public void Add(int x1, int y1, int w, int h, COLOR color, bool thick)
        {
            Thing t = objects[ai];
            t.rec.MoveX1Y1(x1, y1).SetDim(w, h);
            t.color = color;
            t.thick = thick;
            ai++;
        }

        public void Render(Renderer r, ShadowBatch s, RenderData data)
        {
            s.SetDistance2GroundUI(8);
            for (int i = 0; i < ai; i++)
            {
                Thing t = objects[i];
                t.color.Bind();
                Rec e = t.rec;
                if (t.thick)
                {
                    SPRITES.cons().BIG.outline.renderBox(r, e.x1() - data.offX1(), e.y1() - data.offY1(), e.width(), e.height());
                    SPRITES.cons().BIG.outline.renderBox(s, e.x1() - data.offX1(), e.y1() - data.offY1(), e.width(), e.height());
                }
                else
                {
                    SPRITES.cons().BIG.outline.renderBox(r, e.x1() - data.offX1(), e.y1() - data.offY1(), e.width(), e.height());
                    SPRITES.cons().BIG.outline.renderBox(s, e.x1() - data.offX1(), e.y1() - data.offY1(), e.width(), e.height());
                }
            }

            ai = 0;
        }

        public void Clear()
        {
            ai = 0;
        }

        private class Thing
        {
            public readonly Rec rec = new Rec();
            public COLOR color;
            public bool thick;
        }
    }
}