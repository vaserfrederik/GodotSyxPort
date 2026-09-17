using System;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.gui.panel;
using view.interrupter;

namespace view.world.generator
{
    internal sealed class Intr : Interrupter
    {
        private GuiSection s;

        public Intr(WorldViewGenerator v)
        {
            Pin();
            v.uiManager.Add(this);
        }

        public void Add(GuiSection s, string title)
        {
            Add(s, title, true);
        }

        public void Add(GuiSection s, string title, bool panel)
        {
            this.s = s;
            if (s != null && panel)
            {
                s.body().MoveCY(C.HEIGHT() / 2);
                s.body().MoveCX(C.WIDTH() / 2);

                GPanel pan = new GPanel();
                pan.SetBig();
                pan.inner().SetDim(s.body().width(), s.body().height());
                pan.body.CenterIn(s);
                pan.SetTitle(title);
                s.Add(pan);
                s.MoveLastToBack();
            }
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            if (s != null)
            {
                return s.Hover(mCoo);
            }
            return false;
        }

        protected override void MouseClick(MButt button)
        {
            if (button == MButt.LEFT && s != null)
                s.Click();
        }

        protected override void HoverTimer(GBox text)
        {
            if (s != null)
                s.HoverInfoGet(text);
        }

        protected override bool Render(Renderer r, float ds)
        {
            if (s != null)
                s.Render(r, ds);
            return true;
        }

        protected override bool Update(float ds)
        {
            return false;
        }

        public override bool CanSave()
        {
            return true;
        }
    }
}