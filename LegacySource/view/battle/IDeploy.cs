using System;
using game;
using init.constant;
using snake2d;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;

namespace view.battle
{
    public class IDeploy : Interrupter
    {
        private readonly GButt.ButtPanel butt = new GButt.ButtPanel(Dic.¤¤Start)
        {
            ClickA = () =>
            {
                VIEW.b().state().deploy();
                hide();
            }
        };

        public IDeploy(InterManager m)
        {
            butt.pad(50, 5);
            butt.body.centerX(C.DIM());
            butt.body.moveY1(0);
            pin();
            show(m);
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return butt.hover(mCoo);
        }

        protected override void mouseClick(MButt button)
        {
            if (button == MButt.LEFT)
                butt.click();
        }

        protected override void hoverTimer(GBox text)
        {
            // TODO Auto-generated method stub
        }

        protected override bool render(Renderer r, float ds)
        {
            butt.render(r, ds);
            return true;
        }

        protected override bool update(float ds)
        {
            GAME.SPEED.tmpPause();
            return true;
        }
    }
}