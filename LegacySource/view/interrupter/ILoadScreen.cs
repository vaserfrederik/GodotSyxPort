using System;
using snake2d;
using util.gui.misc;
using util.text;
using view.keyboard;

namespace view.interrupter
{
    public class ILoadScreen : Interrupter
    {
        private static readonly CharSequence ¤¤clickToContinue = "¤CLICK TO CONTINUE!";
        static ILoadScreen()
        {
            D.ts(typeof(ILoadScreen));
        }

        private readonly InterManager m;

        public ILoadScreen(InterManager manager)
        {
            Pin();
            this.m = manager;
        }

        public void Activate()
        {
            Show(m);
        }

        public void Deactivate()
        {
            Hide();
        }

        protected override void HoverTimer(GBox text)
        {
        }

        protected override bool Render(Renderer r, float ds)
        {
            SPRITES.Loader().Render(¤¤clickToContinue, true);
            return false;
        }

        protected override void MouseClick(MButt button)
        {
            Deactivate();
        }

        protected override bool Hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return true;
        }

        protected override bool Update(float ds)
        {
            if (MButt.LEFT.ConsumeClick())
                Deactivate();

            if (MButt.RIGHT.ConsumeClick())
                Deactivate();

            if (KEYS.AnyDown())
                Deactivate();

            return false;
        }
    }
}