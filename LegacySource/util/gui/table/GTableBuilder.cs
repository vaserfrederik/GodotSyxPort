using System;
using System.Collections.Generic;
using util.gui.table;
using init.constant;
using init.sprite.UI;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.clickable;
using snake2d.util.gui.clickable.Scrollable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sets;
using util.colors;
using util.data.GETTER;
using util.gui.misc;

public abstract class GTableBuilder
{
    private readonly ArrayListResize<HOVERABLE> titles = new ArrayListResize<HOVERABLE>(10, 20);
    private readonly ArrayListShort widths = new ArrayListShort(20);
    private readonly ArrayListResize<GRowBuilder> cells = new ArrayListResize<GRowBuilder>(10, 20);
    private readonly ArrayListResize<DIR> dirs = new ArrayListResize<DIR>(10, 20);
    public static readonly COLOR cHovered = new ColorImp(28, 23, 53);
    public static readonly COLOR cSelected = new ColorShifting(cHovered, cHovered.shade(1.5));
    private GScrollable scroller;

    public void column(CharSequence title, int width, GRowBuilder ren)
    {
        column(title, width, ren, DIR.W);
    }

    public void column(int width, GRowBuilder ren, DIR d)
    {
        column((HOVERABLE)null, width, ren, d);
    }

    public void column(CharSequence title, int width, GRowBuilder ren, DIR d)
    {
        HOVERABLE t = title != null && title.Length > 0 ? new GText(UI.FONT().S, title).lablifySub().r(DIR.NW) : null;
        column(t, width, ren, d);
    }

    public void column(HOVERABLE t, int width, GRowBuilder ren, DIR d)
    {
        titles.add(t);
        widths.add(width);
        cells.add(ren);
        dirs.add(d);
    }

    public GuiSection createHeight(int heightt, bool decorate)
    {
        int height = 0;
        final GETTER_IMP<int> inin = new GETTER_IMP<int>();
        for (int k = 0; k < titles.size(); k++)
        {
            if (cells.get(k) != null)
            {
                RENDEROBJ o = cells.get(k).build(inin);
                if (o.body().height() > height)
                    height = o.body().height();
            }
        }
        height += (decorate ? 6 : 0);
        if (titles.size() > 0)
            for (RENDEROBJ s : titles)
                if (s != null)
                {
                    heightt -= UI.FONT().M.height();
                    break;
                }

        int rows = heightt / height;
        if (rows <= 0)
            rows = 1;
        return create(rows, decorate);
    }

    private const int M = 3;

    public GuiSection create(int rows, bool decorate)
    {
        if (rows < 0)
            rows = 0;
        Scrollable.ScrollRow[] rs = new Scrollable.ScrollRow[rows];
        int width = 0;
        int height = 0;
        final GETTER_IMP<int> inin = new GETTER_IMP<int>();
        for (int k = 0; k < titles.size(); k++)
        {
            if (cells.get(k) != null)
            {
                RENDEROBJ o = cells.get(k).build(inin);
                if (o.body().height() > height)
                    height = o.body().height();
            }
            width += widths.get(k) + 2 * M;

        }
        width += (decorate ? 8 : 0);
        height += (decorate ? 6 : 0);
        for (int i = 0; i < rows; i++)
        {

            final GETTER_IMP<int> in = new GETTER_IMP<int>();

            ScrollRow.ScrollRowImp row = new ScrollRow.ScrollRowImp()
            {

                float clickT = 0;

                public override void init(int index)
                {
                    in.set(index);
                }

                public override void render(SPRITE_RENDERER r, float ds)
                {
                    if (decorate)
                    {
                        bool isHovered = hoveredIs();
                        bool isSelected = GTableBuilder.this.selectedIs(in.get());
                        GButt.BSection.renderBG(r, body(), true, isHovered, isSelected);
                    }
                    if (clickT >= 0)
                        clickT -= ds;
                    base.render(r, ds);
                    if (!GTableBuilder.this.activeIs(in.get()))
                    {
                        OPACITY.O50.bind();
                        COLOR.BLACK.render(r, body(), -1);
                        OPACITY.unbind();
                    }

                }

                public override bool hover(COORDINATE mCoo)
                {
                    if (base.hover(mCoo))
                    {
                        GTableBuilder.this.hover(in.get());
                        return true;
                    }
                    GTableBuilder.this.hover(-1);
                    return false;
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    base.hoverInfoGet(text);
                    GTableBuilder.this.hoverInfo(in.get(), (GBox)text);
                }

                public override bool click()
                {

                    if (!base.click())
                    {
                        if (clickT > 0)
                        {
                            doubleClick(in.get());
                            clickT = 0;
                        }
                        else
                        {
                            clickT = 0.3f;

                        }
                        return true;
                    }
                    return false;
                }

            };

            row.body().setWidth(width).setHeight(height);

            int x = 0;
            for (int k = 0; k < titles.size(); k++)
            {
                if (cells.get(k) != null)
                {
                    RENDEROBJ o = cells.get(k).build(in);
                    DIR d = dirs.get(k);

                    int x1 = x + M;

                    if (d.x() < 0)
                    {
                        o.body().moveX1(x1);
                    }
                    else if (x > 0)
                    {
                        o.body().moveX2(x1 + widths.get(k));
                    }
                    else
                    {
                        o.body().moveCX(x1 + widths.get(k) / 2);
                    }
                    if (d.y() < 0)
                    {
                        o.body().moveY1(0);
                    }
                    else if (d.y() > 0)
                    {
                        o.body().moveY2(height);
                    }
                    else
                    {
                        o.body().moveCY(height / 2);
                    }

                    if (decorate && k > 0)
                    {
                        row.add(new RENDEROBJ.RenderImp(2, height - 4)
                        {

                            public override void render(SPRITE_RENDERER r, float ds)
                            {
                                GCOLOR.UI().border().render(r, body);
                            }

                        }, x, o.body().cY() - (height - 4) / 2);
                    }
                    row.add(o);
                }

                x += widths.get(k) + 2 * M;

            }

            row.clickActionSet(new ACTION()
            {

                public override void exe()
                {
                    click(in.get());
                }
            });

            row.body().setWidth(x);
            rs[i] = row;

        }

        scroller = new GScrollable(rs)
        {

            public override int nrOFEntries()
            {
                return GTableBuilder.this.nrOFEntries();
            }

        };

        GuiSection res = new GuiSection();
        int x = 0;
        RENDEROBJ last = null;
        for (int k = 0; k < titles.size(); k++)
        {
            x += widths.get(k);
        }
        if (x < width)
        {
            column("", width - x, null);
        }

        res.add(scroller);

        return res;
    }

    public abstract int nrOFEntries();

    public void click(int index)
    {

    }

    public void doubleClick(int index)
    {

    }

    public void hover(int index)
    {

    }

    public final void set(int index)
    {
        scroller.set(index);
    }

    public void hoverInfo(int index, GBox box)
    {

    }

    public bool selectedIs(int index)
    {
        return false;
    }

    public bool activeIs(int index)
    {
        return true;
    }

    public abstract class GRowBuilder
    {

        public abstract RENDEROBJ build(GETTER<int> ier);

    }

    public final void pad(int width)
    {
        int x = 0;
        for (int k = 0; k < titles.size(); k++)
        {
            x += widths.get(k);
        }
        if (x < width)
        {
            column("", width - x, null);
        }
    }
}