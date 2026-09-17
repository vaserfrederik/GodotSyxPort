using System;
using System.Collections.Generic;
using init.sprite;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.colors;
using util.rendering;

namespace world.overlay
{
    public sealed class EThings
    {
        private readonly ArrayList<Thing> objects = new ArrayList<Thing>(256);
        private int ai;

        public EThings()
        {
            for (int i = 0; i < objects.Max(); i++)
            {
                objects.Add(new Thing());
            }
        }

        public void Add(int x1, int y1, int w, int h, COLOR color, bool thick)
        {
            if (ai >= objects.Size())
                return;
            Thing t = objects.Get(ai);
            t.rec.MoveX1Y1(x1, y1).SetDim(w, h);
            t.color = color;
            t.thick = thick;
            ai++;
        }

        public void Hover(RECTANGLE body, COLOR color, bool thick, int margin)
        {
            Add(body.x1() - margin, body.y1() - margin, body.width() + margin * 2, body.height() + margin * 2, color, thick);
        }

        public void Hover(int x1, int y1, int w, int h, COLOR color, bool thick)
        {
            Add(x1, y1, w, h, color, thick);
        }

        public void Hover(WEntity e)
        {
            Hover(e.body(), GCOLOR.MAP().Get(e.faction()), true, 6);
        }

        public void Render(Renderer r, ShadowBatch s, RenderData data)
        {
            s.SetDistance2GroundUI(8);
            s.SetHeightUI(2);
            s.SetHard();
            for (int i = 0; i < ai; i++)
            {
                Thing t = objects.Get(i);
                t.color.Bind();
                Rec e = t.rec;
                if (t.thick)
                {
                    SPRITES.cons().BIG.outline.RenderBox(r, e.x1() - data.offX1(), e.y1() - data.offY1(), e.width(), e.height());
                    SPRITES.cons().BIG.outline.RenderBox(s, e.x1() - data.offX1(), e.y1() - data.offY1(), e.width(), e.height());
                }
                else
                {
                    SPRITES.cons().BIG.outline.RenderBox(r, e.x1() - data.offX1(), e.y1() - data.offY1(), e.width(), e.height());
                    SPRITES.cons().BIG.outline.RenderBox(s, e.x1() - data.offX1(), e.y1() - data.offY1(), e.width(), e.height());
                }
            }

            ai = 0;
            s.SetPrev();
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