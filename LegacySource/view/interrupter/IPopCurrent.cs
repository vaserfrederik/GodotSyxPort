using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using util.gui.misc;
using util.gui.panel;
using view.keyboard;
using view.main;

namespace view.interrupter
{
    public class IPopCurrent : Interrupter
    {
        public readonly GuiSection expansion = new GuiSection();
        private CLICKABLE trigger;
        private readonly GPanel panel = new GPanel();

        public IPopCurrent() : base()
        {
            panel.setCloseAction(new ACTION
            {
                exe = () =>
                {
                    hide();
                }
            });
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            bool ret = panel.hover(mCoo) | expansion.hover(mCoo);

            return ret;
        }

        protected override void mouseClick(MButt button)
        {
            if (button == MButt.LEFT)
            {
                expansion.click();
                panel.click();
            }
            if (panel.hoveredIs())
                MButt.clearWheelSpin();
            else if (button == MButt.RIGHT)
                hide();
        }

        public void show(CLICKABLE trigger)
        {
            if (isActivated())
                hide();

            panel.inner().set(expansion);

            panel.body().moveC(trigger.body().cX(), 0);
            panel.body().moveY1(trigger.body().y2());
            if (panel.body().y2() > C.HEIGHT())
                panel.body().moveY2(trigger.body().y2());
            if (panel.body.x2() > C.WIDTH() - 32)
                panel.body.moveX2(C.WIDTH() - 32);
            if (panel.body.x1() < 32)
                panel.body.moveX1(32);

            expansion.body().centerIn(panel.inner());
            base.show(VIEW.current().uiManager);
        }

        protected override bool otherClick(MButt button)
        {
            hide();
            return false;
        }

        public override void hide()
        {
            base.hide();
        }

        protected override void hoverTimer(GBox text)
        {
            expansion.hoverInfoGet(text);
        }

        protected override bool render(Renderer r, float ds)
        {
            panel.inner().set(expansion);
            panel.body.centerIn(expansion);
            panel.render(r, ds);
            expansion.render(r, ds);
            if (trigger != null)
                trigger.selectTmp();
            return true;
        }

        protected override bool update(float ds)
        {
            if (KEYS.anyDown())
                hide();
            if (panel.hoveredIs())
                MButt.clearWheelSpin();
            return true;
        }

        protected override void otherAdd(Interrupter other)
        {
            hide();
        }
    }
}