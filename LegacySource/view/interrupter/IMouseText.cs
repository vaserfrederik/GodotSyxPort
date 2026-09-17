using init.constant;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.renderable;
using util.gui.misc;
using util.gui.panel;
using view.keyboard;
using view.main;

namespace view.interrupter
{
    /**
     * A text box appearing top-centred. Will disappear after once cycle
     * automatically.
     * 
     * @author mail__000
     *
     */
    public class IMouseText : Interrupter
    {
        private readonly GTextR text = new GTextR(UI.FONT().M, 200);
        private RENDEROBJ ren = text;
        private readonly GPanel box = new GPanel();
        private readonly InterManager manager;

        public IMouseText(InterManager manager)
        {
            this.manager = manager;
            box.inner().moveY1(C.TILE_SIZE * 2);
            text.text().setMaxWidth(C.WIDTH() / 3);
            text.text().lablify();
        }

        protected override void mouseClick(MButt button)
        {
            hide();
        }

        protected override bool otherClick(MButt button)
        {
            hide();
            return false;
        }

        public void activate(CharSequence t)
        {
            this.text.text().set(t);
            ren = text;
            set();
            show(manager);
        }

        public void activate(RENDEROBJ ren)
        {
            this.ren = ren;
            if (ren == null)
                return;
            set();
            show(manager);
        }

        private void set()
        {
            box.inner().set(ren);

            box.inner().moveX1Y1(VIEW.mouse().x() + C.SG * 20, VIEW.mouse().y() + C.SG * 20);

            if (box.inner().x2() > C.WIDTH())
            {
                box.inner().moveX2(VIEW.mouse().x() - C.SG * 5);
            }

            if (box.inner().y2() > C.HEIGHT())
            {
                box.inner().moveY2(C.HEIGHT());
            }

            ren.body().centerIn(box);
        }

        protected override bool hover(COORDINATE mCoo, bool mouseHasMoved)
        {
            return mCoo.isWithinRec(box);
        }

        protected override bool render(Renderer r, float ds)
        {
            box.render(r, ds);
            ren.render(r, ds);
            return true;
        }

        protected override bool update(float ds)
        {
            if (KEYS.anyDown())
                hide();
            return true;
        }

        protected override void hoverTimer(GBox text)
        {
        }
    }
}