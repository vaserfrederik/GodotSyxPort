using System;
using init.constant;
using init.sprite;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.misc;
using util.gui.misc;
using util.gui.panel;
using util.text;
using view.keyboard;

/**
 * centered medium sized panel. Can be persistent or dismissable. Composes of a
 * question and a yes and no button.
 * 
 * @author mail__000
 *
 */
public class IPromtYesNO : Interrupter
{
    private readonly GTextR text = new GTextR(UI.FONT().M, 1000, DIR.C);
    private readonly GuiSection section = new GuiSection();
    private readonly GPanel box = new GPanel().setDim(800, 400);
    private readonly ACTION close = new ACTION(() => deactivate());

    private readonly GButt.ButtPanel yes = new GButt.ButtPanel(SPRITES.icons().m.ok)
    {
        protected override void clickA()
        {
            hide();
        }
    };

    private readonly GButt.ButtPanel no = new GButt.ButtPanel(SPRITES.icons().m.cancel)
    {
        protected override void clickA()
        {
            hide();
        }
    };

    private bool dismissable;
    private readonly InterManager m;

    public IPromtYesNO(InterManager manager)
    {
        this.m = manager;
        text.text().lablify();
        section.add(box);
        section.body().centerIn(C.DIM());
        box.setBig();

        yes.body.setWidth(100);
        no.body.setWidth(100);
        yes.hoverInfoSet(Dic.¤¤Yes);
        no.hoverInfoSet(Dic.¤¤No);

        GuiSection butts = new GuiSection();

        butts.add(yes).addRight(0, no);

        butts.body().centerX(section);
        butts.body().moveY2(section.body().y2() - 16);
        section.add(butts);

        text.text().setMaxWidth(800);
        section.add(text);
    }

    static bool tmp = false;

    public void activate(CharSequence message, ACTION yesAction, ACTION noAction, bool dismissable)
    {
        show(m);

        this.dismissable = dismissable;

        section.clear();
        text.text().set(message);
        text.adjust();
        if (text.body().width() < 600)
            section.body().setDim(600, 1);
        section.addDownC(0, text);
        yes.clickActionSet(yesAction);

        if (noAction != null)
        {
            int cx = section.body().cX();
            no.clickActionSet(noAction);
            section.add(yes, cx - yes.body.width(), section.getLastY2() + 16);
            section.add(no, cx, section.getLastY1());
        }
        else
        {
            section.addDownC(16, yes);
        }

        section.body().centerIn(C.DIM());

        box.setCloseAction(dismissable ? close : null);
        box.inner().set(section.body());
        section.add(box);
        section.moveLastToBack();
    }

    public void deactivate()
    {
        hide();
    }

    protected override void hoverTimer(GBox text)
    {
        section.hoverInfoGet(text);
    }

    protected override bool render(Renderer r, float ds)
    {
        section.render(r, ds);
        return true;
    }

    protected override void mouseClick(MButt button)
    {
        if (button == MButt.LEFT)
            section.click();
        else if (dismissable && button == MButt.RIGHT)
            deactivate();
    }

    protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
    {
        section.hover(mCoo);
        return true;
    }

    protected override bool update(float ds)
    {
        if (KEYS.MAIN().ESCAPE.consumeClick())
        {
            deactivate();
            return true;
        }
        KEYS.clear();
        return false;
    }

    public override bool canSave()
    {
        return dismissable;
    }
}