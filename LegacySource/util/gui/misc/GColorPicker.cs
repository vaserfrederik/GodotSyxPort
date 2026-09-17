using System;
using init.constant;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.data.INT;
using util.gui.slider;
using util.text;

namespace util.gui.misc
{
    public abstract class GColorPicker : GuiSection
    {
        private static readonly string ¤¤color = "¤color";
        private static readonly string ¤¤red = "¤red";
        private static readonly string ¤¤green = "¤green";
        private static readonly string ¤¤blue = "¤blue";

        static GColorPicker()
        {
            D.ts(typeof(GColorPicker));
        }

        public GColorPicker(bool glow) : this(glow, ¤¤color) { }

        public GColorPicker(bool glow, string name)
        {
            int max = glow ? 255 : 127;
            int w = 120;
            GSliderInt r = new GSliderInt(new INTE()
            {
                public int min()
                {
                    return 0;
                }

                public int max()
                {
                    return max;
                }

                public int get()
                {
                    return color().red() & 0x0FF;
                }

                public void set(int t)
                {
                    color().setRed(t);
                    change();
                }
            }, w, false);

            r.addRelBody(C.SG * 8, DIR.W, new GText(UI.FONT().S, ¤¤red).lablify());
            GSliderInt g = new GSliderInt(new INTE()
            {
                public int min()
                {
                    return 0;
                }

                public int max()
                {
                    return max;
                }

                public int get()
                {
                    return color().green() & 0x0FF;
                }

                public void set(int t)
                {
                    color().setGreen(t);
                    change();
                }
            }, w, false);

            g.addRelBody(C.SG * 8, DIR.W, new GText(UI.FONT().S, ¤¤green).lablify());
            GSliderInt b = new GSliderInt(new INTE()
            {
                public int min()
                {
                    return 0;
                }

                public int max()
                {
                    return max;
                }

                public int get()
                {
                    return color().blue() & 0x0FF;
                }

                public void set(int t)
                {
                    color().setBlue(t);
                    change();
                }
            }, w, false);

            b.addRelBody(C.SG * 8, DIR.W, new GText(UI.FONT().S, ¤¤blue).lablify());

            add(r);
            g.body().moveX2(r.body().x2());
            g.body().moveY1(r.body().y2());
            add(g);
            b.body().moveX2(g.body().x2());
            b.body().moveY1(g.body().y2());
            add(b);

            addRelBody(C.SG * 4, DIR.N, new GText(UI.FONT().H2, name).lablify());
        }

        public void change()
        {
        }

        public abstract ColorImp color();
    }
}