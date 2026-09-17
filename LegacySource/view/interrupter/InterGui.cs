using System;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;

namespace view.interrupter
{
    public abstract class InterGui : Interrupter
    {
        protected GuiSection section = new GuiSection();

        public InterGui()
        {
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
            {
                deactivate();
            }
        }

        protected virtual bool back()
        {
            return true;
        }

        protected override bool otherClick(MButt button)
        {
            if (button == MButt.RIGHT && !pinned())
            {
                deactivate();
                return true;
            }
            return false;
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
            return true;
        }

        public void deactivate()
        {
            hide();
        }

        public RECTANGLE body()
        {
            return section.body();
        }
    }
}