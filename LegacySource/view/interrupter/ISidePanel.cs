using System;
using System.Text;
using init.constant;
using snake2d.util.gui;
using view.ui.top;

namespace view.interrupter
{
    public class ISidePanel
    {
        protected GuiSection section;
        protected CharSequence title;
        public const int M = 8 * C.SG;
        static readonly int Y1 = UIPanelTop.HEIGHT;
        static readonly int Y2 = Y1 + C.SG * 32 + M;
        public static readonly int HEIGHT = C.HEIGHT() - Y2 - M;
        protected ISidePanels last;

        public ISidePanel(GuiSection section)
        {
            this.section = section;
        }

        public ISidePanel()
        {
            section = new GuiSection();
        }

        public GuiSection Section()
        {
            return section;
        }

        public void TitleSet(CharSequence title)
        {
            this.title = title;
        }

        public CharSequence Title()
        {
            return this.title;
        }

        protected void Update(float ds)
        {
        }

        public ISidePanels Last()
        {
            return last;
        }

        protected void AddAction()
        {
        }

        protected bool Back()
        {
            return false;
        }
    }
}