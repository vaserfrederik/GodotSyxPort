using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.misc;
using util.gui.misc;
using view.keyboard;

namespace view.interrupter
{
    public class InterGuisection : Interrupter
    {
        private GuiSection section;
        private ACTION closeAction;
        private readonly InterManager m;

        public InterGuisection(InterManager m)
        {
            this.m = m;
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return section.hover(mCoo) || mCoo.isWithinRec(section.body());
        }

        protected override void mouseClick(MButt button)
        {
            if (button == MButt.LEFT)
                section.click();
            else if (button == MButt.RIGHT && back())
                deactivate();
        }

        protected bool back()
        {
            return true;
        }

        protected override bool otherClick(MButt button)
        {
            if (button == MButt.RIGHT)
            {
                deactivate();
                return true;
            }
            return false;
        }

        public GuiSection section()
        {
            return section;
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

        protected override bool update(float ds)
        {
            if (KEYS.MAIN().ESCAPE.consumeClick())
                hide();
            return true;
        }

        public void activate(GuiSection section)
        {
            this.section = section;
            show(m);
        }

        public void close()
        {
            hide();
        }

        public void setCloseAction(ACTION action)
        {
            this.closeAction = action;
        }

        public void deactivate()
        {
            hide();
        }

        protected override void deactivateAction()
        {
            if (closeAction != null)
                closeAction.exe();
            closeAction = null;
            base.deactivateAction();
        }

        public GuiSection current()
        {
            if (isActivated())
                return this.section;
            return null;
        }
    }
}