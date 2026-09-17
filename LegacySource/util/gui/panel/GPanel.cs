using System;
using util.gui.panel;
using init.constant;
using init.sprite.UI;
using init.sprite.UI.UIPanels;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;

public class GPanel : CLICKABLE.ClickableAbs
{
    private const int MW = 8 * C.SG;
    private readonly GText title = new GText(UI.FONT().H2, 32).lablify();

    private UIPanel panel = UI.PANEL().thin;
    private bool closeH;

    private readonly RecFacade outer = new RecFacade
    {
        Width = () => body.width() + MW * 2 + (clickAction != null ? 8 : 0),
        Height = () => body.height() + MW * 2 + titleHeight() / 2 + (clickAction != null ? 8 : 0),
        Y1 = () => body.y1() - MW - titleHeight() / 2 - (clickAction != null ? 8 : 0),
        X1 = () => body.x1() - MW,
        MoveY1 = Y1 => { body.moveY1(Y1 + MW + titleHeight() / 2); return this; },
        MoveX1 = X1 => { body.moveX1(X1 + MW); return this; },
        SetWidth = width => { body.setWidth(width - MW * 2 + (clickAction != null ? 8 : 0)); return this; },
        SetHeight = height => { body.setHeight(height - MW * 2 - titleHeight() / 2 + (clickAction != null ? 8 : 0)); return this; }
    };

    public GPanel() { }

    public GPanel(RECTANGLE r)
    {
        body.set(r);
    }

    public GPanel(int width, int height)
    {
        body.setDim(width, height);
    }

    public GPanel set(RECTANGLE r)
    {
        body.set(r);
        return this;
    }

    public GPanel setDim(int width, int height)
    {
        body.setWidth(width).setHeight(height);
        return this;
    }

    public GPanel setBig()
    {
        panel = UI.PANEL().big;
        return this;
    }

    public GPanel setButt()
    {
        panel = UI.PANEL().butt;
        return this;
    }

    public override RecFacade body()
    {
        return outer;
    }

    public Rec inner()
    {
        return body;
    }

    private int titleHeight()
    {
        if (title.length() == 0)
            return 0;
        return UI.PANEL().titleBox(title.getFont().height()).height;
    }

    public void setTitle(CharSequence title)
    {
        this.title.clear();
        if (title != null)
            this.title.set(title);
    }

    public void setTitle(CharSequence title, Font f)
    {
        this.title.setFont(f);
        this.title.clear().set(title);
    }

    public GText title()
    {
        return title;
    }

    public GPanel setCloseAction(ACTION action)
    {
        this.clickAction = action;
        hoverInfoSet(Dic.¤¤Close);
        return this;
    }

    public override bool hover(COORDINATE mCoo)
    {
        closeH = false;
        if (clickAction != null)
        {
            int cy = outer.y1() - panel.margin + panel.tMid;
            int cx = outer.x2() + panel.margin - panel.tMid;
            closeH = mCoo.tileDistanceTo(cx, cy) < 16;
            if (closeH && MButt.LEFT.consumeClick())
            {
                clickAction.exe();
                return true;
            }
        }
        this.isHovered = closeH;
        return closeH;
    }

    protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
    {
        // COLOR.YELLOW100.render(r, outer);
        panel.render(r, outer, 0);

        renderTitle(r);

        if (clickAction != null)
        {
            int cy = outer.y1() - panel.margin + panel.tMid;
            int cx = outer.x2() + panel.margin - panel.tMid;
            UI.PANEL().panelClose.renderC(r, closeH ? 1 : 0, cx, cy);
        }

        // COLOR.RED100.render(r, body);
    }

    public void renderTitle(SPRITE_RENDERER r)
    {
        if (title.length() != 0)
        {
            TitleBox b = UI.PANEL().titleBox(title.getFont().height());
            // int w = body.width() - b.height * 2;
            // if (w <= 0)
            //     return;
            title.setMultipleLines(false);
            int cy = outer.y1() - panel.margin + panel.tMid;
            int y1 = cy - b.height / 2;
            int x1 = body.cX() - title.width() / 2;

            b.render(r, x1, y1, title.width());
            title.render(r, x1, cy - title.height() / 2);
        }
    }
}