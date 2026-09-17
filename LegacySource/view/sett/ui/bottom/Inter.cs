using System;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using util.gui.misc;
using view.interrupter;
using view.main;

namespace view.sett.ui.bottom
{
    final class Inter : Interrupter
    {
        private CLICKABLE panel;
        public CLICKABLE exp;

        private CLICKABLE DUMMY = new GuiSection();
        private CLICKABLE trigger;

        public Inter()
        {
        }

        public void set(CLICKABLE trigger, CLICKABLE panel)
        {
            this.trigger = trigger;
            this.panel = panel;
            this.panel.body().moveY2(trigger.body().y1());
            this.panel.body().moveCX(trigger.body().cX());
            exp(trigger, DUMMY);
            show(VIEW.s().uiManager);
        }

        public void exp(CLICKABLE exbutt, CLICKABLE panel)
        {
            if (exp == panel)
                return;
            exp = panel;
            exp.body().moveY1(this.panel.body().y1());
            exp.body().moveX1(this.panel.body().x2());
        }

        protected override void hoverTimer(GBox text)
        {
            panel.hoverInfoGet(text);
            exp.hoverInfoGet(text);
        }

        protected override void mouseClick(MButt button)
        {
            if (button == MButt.RIGHT)
            {
                hide();
            }
            else if (button == MButt.LEFT)
            {
                panel.click();
                exp.click();
            }
        }

        public override void hide()
        {
            base.hide();
        }

        protected override bool otherClick(MButt butt)
        {
            hide();
            if (butt == MButt.RIGHT)
                return true;
            return false;
        }

        protected override void otherAdd(Interrupter other)
        {
            hide();
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            bool ret = panel.hover(mCoo);
            if (exp.hover(mCoo))
            {
                ret = true;
            }
            return ret;
        }

        protected override bool render(Renderer r, float ds)
        {
            trigger.selectTmp();
            panel.render(r, ds);
            exp.render(r, ds);
            return true;
        }

        protected override bool update(float ds)
        {
            return true;
        }
    }
}