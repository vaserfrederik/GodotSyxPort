using System;
using System.Collections.Generic;
using game;
using init.constant;
using init.resources;
using init.sprite.UI;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.data.INT;
using util.gui.panel;
using util.gui.slider;
using util.info;
using view.main;

public class GBox : SPRITE, GUI_BOX
{
    public int maxHeight = 600;
    public int maxWidth = 500;

    private static readonly GBox dummy = new GBox();
    public static readonly GBox tmp = new GBox();

    private const int MARGIN = 4;

    private List<Ren> rens = new List<Ren>();
    private List<Ren> rensFree = new List<Ren>();
    private GBox box = new GBox();
    private Scroll scroll = new Scroll();
    private RENDEROBJ object = new RENDEROBJ();

    public GBox()
    {
        for (int i = 0; i < 100; i++)
        {
            rensFree.Add(new Ren());
        }
    }

    public void clear()
    {
        rens.Clear();
        rensFree.AddRange(rensFree.TakeWhile(r => !rensFree.Contains(r)));
        box.clear();
        scroll.clear();
    }

    public void title(CharSequence s)
    {
        box.title(s);
    }

    public void text(CharSequence s)
    {
        add(new GText(s));
    }

    public void NL(int am = 1)
    {
        for (int i = 0; i < am; i++)
        {
            add(new GText(" "));
        }
    }

    public void add(RENDEROBJ obj)
    {
        if (rensFree.Count == 0)
        {
            GAME.Notify("Out of free render objects");
            return;
        }
        Ren ren = rensFree[0];
        rensFree.RemoveAt(0);
        ren.init(obj, obj.body().width() + MARGIN);
        rens.Add(ren);
    }

    public void add(INFO info)
    {
        title(info.name);
        text(info.desc);
        NL(4);
    }

    public void setArea(RECTANGLE b)
    {
        if (b.width() > 1 || b.height() > 1)
        {
            GText t = text();
            t.add(b.width()).add('x').add(b.height()).adjustWidth();
            add(t);
            space();
        }
    }

    public void setResource(RESOURCE r, double amount)
    {
        if (rensFree.Count == 0)
        {
            GAME.Notify(r.name);
            return;
        }
        if (amount == 0)
            return;
        add(r.icon().small);
        GText t = text();
        if (amount - (int)amount == 0)
            t.add((int)amount).adjustWidth();
        else
            t.add(amount).adjustWidth();
        if (!SETT.PATH().finders.resource.normal.has(r))
            t.errorify();
        add(t);
    }

    public void resLine(RESOURCE r, double amount)
    {
        if (rensFree.Count == 0)
        {
            GAME.Notify(r.name);
            return;
        }
        if (amount == 0)
            return;
        add(r.icon().small);
        text(r.names);
        tab(6);
        GFORMAT.f0(text(), amount);
        NL();
    }

    public void setResource(RESOURCE r, int amount, int of)
    {
        add(r.icon().small);
        GText t = text();
        GFORMAT.iofkInv(t, amount, of);
        if (!SETT.PATH().finders.resource.normal.has(r) && amount < of)
            t.errorify();
        else
            t.normalify();
        add(t);
    }

    public int width()
    {
        return box.width();
    }

    public int height()
    {
        return box.height() + scroll.height();
    }

    public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
    {
        box.render(r, X1, X2, Y1, Y2);
        scroll.render(r, X1, X2, Y1, Y2);
    }

    public void renderTextured(TextureCoords texture, int X1, int X2, int Y1, int Y2)
    {
        throw new NotImplementedException();
    }

    public RENDEROBJ asRenObj()
    {
        return object;
    }

    public static GBox Dummy()
    {
        dummy.clear();
        return dummy;
    }

    public void error(CharSequence s)
    {
        if (s.Length > 0)
        {
            add(text().errorify().add(s));
            NL();
        }
    }

    public void warn(CharSequence s)
    {
        if (s.Length > 0)
        {
            add(text().warnify().add(s));
            NL();
        }
    }

    public bool emptyIs()
    {
        return rensFree.Count == 0 && box.title().Length == 0;
    }

    public bool emptyIs2()
    {
        return rensFree.Count == 0;
    }

    public void sep()
    {
        NL();
        add(new Sep());
        NL();
    }

    private class Ren
    {
        public RENDEROBJ obj;
        public int x;
        public int y;

        public void init(RENDEROBJ obj, int width)
        {
            this.obj = obj;
            this.x = 0;
            this.y = 0;
        }
    }

    private class Sep : RENDEROBJ.RenderImp
    {
        public Sep() : base(1, 12)
        {
        }

        public override void render(SPRITE_RENDERER r, float ds)
        {
            GCOLOR.UI().border().render(r, body.x1(), body.x1() + (width), body.y1() + 6, body.y1() + 7);
        }
    }

    private class Scroll
    {
        public int current;
        public int max;
        public int dh = (int)(maxHeight / 5.0);
        public int ri = -1;
        public INTE ii = new INTE()
        {
            min = () => 0,
            max = () => max,
            get = () => current,
            set = (t) => current = t
        };

        public GSliderVer sl = new GSliderVer(ii, maxHeight);

        public bool init()
        {
            int h = maxHeight + dh;
            if (h < maxHeight)
                return false;

            max = (int)Math.Ceiling((double)(h - maxHeight) / dh);

            if (Math.Abs(VIEW.RI() - ri) > 2)
            {
                current = 0;
            }
            ri = VIEW.RI();

            double dv = MButt.clearWheelSpin();
            if (dv < 0)
                current++;
            else if (dv > 0)
                current--;
            current = CLAMP.i(current, 0, max);
            return true;
        }

        public bool passes(Ren ren)
        {
            int y1 = current * dh;
            int y2 = y1 + maxHeight;
            return ren.y >= y1 && ren.y + ren.obj.body().height() < y2;
        }

        public void render(SPRITE_RENDERER r, Ren ren, int X1, int Y1)
        {
            if (passes(ren))
            {
                Y1 -= current * dh;
                ren.obj.body().moveX1Y1(X1 + ren.x, Y1 + ren.y);
                ren.obj.render(r, 0);
            }
        }
    }
}