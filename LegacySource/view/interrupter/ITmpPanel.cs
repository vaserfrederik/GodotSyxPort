using System;
using init.constant;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.gui.panel;

namespace view.interrupter
{
    public sealed class ITmpPanel : Interrupter
    {
        private bool visable = false;
        private readonly GuiSection section = new GuiSection();
        private GPanel box = new GPanel();
        private int buttI;

        public ITmpPanel(InterManager manager)
        {
            Pin();

            box.SetButt();
            Show(manager);
        }

        public void AddTitle(CharSequence s)
        {
            box.SetTitle(s);
        }

        public void AddButton(GButt.Panel button)
        {
            if (buttI++ > 10)
            {
                button.Body.MoveX1(section.Body().X1()).MoveY1(section.Body().Y2());
                section.Add(button);
                buttI = 0;
            }
            else
            {
                section.AddRight(0, button);
            }

            visable = true;

            section.Body().CenterX(C.DIM());
            section.Body().MoveY1(120);
        }

        public void AddButtons(params GButt.Panel[] buttons)
        {
            foreach (var bu in buttons)
                AddButton(bu);
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            if (!visable)
                return false;
            return section.Hover(mCoo);
        }

        protected override void MouseClick(MButt button)
        {
            if (MButt.LEFT == button)
                section.Click();
        }

        protected override void HoverTimer(GBox text)
        {
            section.HoverInfoGet(text);
        }

        protected override bool Render(Renderer r, float ds)
        {
            if (visable)
            {
                box.Inner().Set(section);
                box.Render(r, ds);
                section.Render(r, ds);
            }

            visable = false;
            buttI = 0;
            box.Title().Clear();
            section.Clear();
            return true;
        }

        protected override bool Update(float ds)
        {
            return true;
        }
    }
}