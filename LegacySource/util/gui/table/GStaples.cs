using System;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.misc;
using util.colors;
using util.gui.misc;

public abstract class GStaples : HoverableAbs
{
    private readonly int amount;
    private int hoveredI = -1;
    private readonly ColorImp color = new ColorImp();
    private bool negative;
    private bool border = true;
    private bool backGround = true;
    private bool normalize = true;
    private bool normalizePlus = false;

    public GStaples(int amount) : this(amount, false)
    {
    }

    public GStaples(int amount, bool negative)
    {
        this.amount = amount;
        this.negative = negative;
    }

    public void border(bool border)
    {
        this.border = border;
    }

    public void background(bool border)
    {
        this.backGround = border;
    }

    public void normalize(bool n)
    {
        normalize = n;
    }

    public void normalizePlus(bool n)
    {
        normalizePlus = n;
    }

    protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
    {
        int w = sw();
        if (w < 3)
            return;
        if (body().height() < 3)
            return;

        int x1 = body().x1() + (body().width() - amount * sw()) / 2;
        int y1 = body().y1();
        int y2 = body().y2();

        if (border)
            GCOLOR.UI().border().render(r, x1 - 1, x1 + amount * sw() + 1, y1 - 1, y2 + 1);

        double max = 0;
        double min = 0;
        double mid = 0;
        if (negative)
        {
            min = -1;
        }

        if (normalizePlus)
        {
            min = double.MaxValue;
            max = double.MinValue;
            for (int i = 0; i < amount; i++)
            {
                double v = getValue(i);
                max = Math.Max(max, v);
                min = Math.Min(min, v);
            }

            if (max == 0)
                max = 1;
        }
        else if (normalize)
        {
            min = double.MaxValue;
            max = double.MinValue;
            for (int i = 0; i < amount; i++)
            {
                double v = getValue(i);
                max = Math.Max(max, v);
                min = Math.Min(min, v);
            }
            min /= 2;
            if (max == 0)
                max = 1;
        }
        else
        {
            max = 1;
            min = 0;
        }

        if (negative)
        {
            int cy = body().cY();

            for (int i = 0; i < amount; i++)
            {
                int x = x1 + i * w;
                double v = getValue(i);
                setColorBg(color, i, v);
                if (backGround && (!isHovered || !hovered(i)))
                    color.render(r, x, x + w, y1, y2);

                v = (mid + v - min) / (max - min);
                v = CLAMP.d(v, -1, 1);
                int h = (int)Math.Ceiling(Math.Abs(v) * (body().height() / 2));
                if (h > 0)
                {
                    setColor(color, i, v);
                    if (isHovered && hovered(i))
                        color.shadeSelf(1.5);
                    else
                        color.shadeSelf(0.5);

                    if (v < 0)
                    {
                        int y22 = cy + h;
                        color.render(r, x, x + w, cy, y22);
                        setColor(color, i, v);
                        color.render(r, x + 1, x + w - 1, cy, y22 - 1);
                    }
                    else
                    {
                        int y11 = cy - h;
                        color.render(r, x, x + w, y11, cy);
                        setColor(color, i, v);
                        color.render(r, x + 1, x + w - 1, y11 + 1, cy);
                    }
                }
            }
            color.set(GCOLOR.UI().border()).shadeSelf(0.75);
            color.render(r, body().x1(), body().x2(), cy, cy + 1);
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                int x = x1 + i * w;
                double v = getValue(i);
                setColorBg(color, i, v);
                if (backGround && (!isHovered || !hovered(i)))
                    color.render(r, x, x + w, y1, y2);

                v = (mid + v - min) / (max - min);

                v = CLAMP.d(v, 0, 1);
                int h = (int)Math.Ceiling(v * (body().height()));
                if (h > 0)
                {
                    int y11 = y2 - h;
                    setColor(color, i, v);
                    if (isHovered && hovered(i))
                        color.shadeSelf(1.5).render(r, x, x + w, y11, y2);
                    else
                        color.shadeSelf(0.5).render(r, x, x + w, y11, y2);
                    setColor(color, i, v);
                    color.render(r, x + 1, x + w - 1, y11 + 1, y2);
                    renderExtra(r, color, i, isHovered && hovered(i), v, x, x + w, y11, y2);
                }
            }
        }
    }

    protected virtual void renderExtra(SPRITE_RENDERER r, COLOR color, int stapleI, bool hovered, double value, int x1, int x2, int y1, int y2)
    {
    }

    protected abstract double getValue(int stapleI);

    protected abstract void hover(GBox box, int stapleI);

    public void setHovered(int i)
    {
        hoveredI = i;
        hoveredSet(hoveredI >= 0);
    }

    public int hoverI()
    {
        return hoveredI;
    }

    protected bool hovered(int ii)
    {
        return ii == hoveredI;
    }

    protected void setColor(ColorImp c, int stapleI, double value)
    {
        c.set(GCOLOR.UI().SOSO.normal);
    }

    protected void setColorBg(ColorImp c, int stapleI, double value)
    {
        c.set(GCOLOR.UI().bg());
    }

    public override bool hover(COORDINATE mCoo)
    {
        if (base.hover(mCoo))
        {
            int x1 = body().x1() + (body().width() - amount * sw()) / 2;
            int x = mCoo.x() - x1;
            hoveredI = CLAMP.i(x / sw(), -1, amount - 1);
            return true;
        }
        return false;
    }

    public override void hoverInfoGet(GUI_BOX text)
    {
        if (hoveredI != -1)
        {
            hover((GBox)text, hoveredI);
        }
        base.hoverInfoGet(text);
    }

    private int sw()
    {
        return (body().width() / amount);
    }
}