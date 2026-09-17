using System;
using System.Linq;

namespace snake2d.util.gui
{
    public class GuiSwitch
    {
        private RENDEROBJ current;
        private readonly DIR dir;
        private readonly GuiSection section;

        public GuiSwitch(DIR align, params RENDEROBJ[] objs)
            : this(align, new GuiSection(), objs)
        {
        }

        public GuiSwitch(DIR align, GuiSection s, params RENDEROBJ[] objs)
        {
            this.section = s;
            int w = 0;
            int h = 0;
            foreach (RENDEROBJ o in objs)
            {
                w = Math.Max(w, o.body().width());
                h = Math.Max(h, o.body().height());
            }
            s.body().setDim(w, h);
            dir = align;
            replace(objs[0]);
        }

        private void replace(RENDEROBJ o)
        {
            current = o;
            int w = section.body().width();
            int h = section.body().height();
            int x = section.body().x1();
            int y = section.body().y1();
            section.clear();
            section.body().setDim(w, h);
            section.body().moveX1Y1(x, y);
            o.body().centerIn(section.body());
            int dx = (w - o.body().width()) / 2;
            int dy = (h - o.body().height()) / 2;
            o.body().incr(dx * dir.x(), dy * dir.y());
            section.add(o);
        }

        public GuiSection section()
        {
            return section;
        }

        public RENDEROBJ current()
        {
            return current;
        }

        public void currentSet(RENDEROBJ c)
        {
            this.current = c;
            replace(c);
        }
    }
}