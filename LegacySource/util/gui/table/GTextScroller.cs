using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.sprite.text;
using util.data;
using util.gui.slider;

namespace util.gui.table
{
    public class GTextScroller : ClickableAbs
    {
        private readonly Font f;
        private readonly GETTER<CharSequence> text;
        private readonly GSliderVer slider;

        private readonly IntImp target = new IntImp();

        public GTextScroller(Font f, GETTER<CharSequence> text, int width, int height)
        {
            this.f = f;
            this.text = text;
            this.body.setDim(width, height);
            slider = new GSliderVer(target, height);
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
        {
            CharSequence body = text.get();
            target.min = 0;
            slider.body().moveX2(body().x2());
            slider.body().moveY1(body().y1());

            double d = MButt.clearWheelSpin();
            if (d > 0)
            {
                target.inc(-1);
            }
            else if (d < 0)
            {
                target.inc(1);
            }

            int width = body().width() - slider.body().width() - 16;

            int rows = 0;

            if (body != null && width > f.height())
            {
                int ei = 0;
                while (ei < body.length())
                {
                    int n = f.getEndIndex(body, ei, width);
                    n = f.getStartIndex(body, n);
                    ei = f.getStartIndex(body, n);
                    rows++;
                }
            }

            rows -= body().height() / f.height();
            if (rows < 0)
                rows = 0;
            target.max = rows;
            if (target.i > rows)
                target.i = rows;

            slider.render(r, ds);

            if (body != null && width > f.height())
            {
                int y1 = body().y1();
                int x1 = body().x1() + 8;
                int ei = 0;
                int ri = 0;
                while (ei < body.length())
                {
                    int n = f.getEndIndex(body, ei, width);
                    if (ri++ >= target.i && y1 < body().y2() - f.height())
                    {
                        f.render(r, body, x1, y1, ei, n, 1.0);
                        y1 += f.height();
                    }
                    n = f.getStartIndex(body, n);
                    ei = f.getStartIndex(body, n);
                    rows++;
                }
            }
        }

        public override bool click()
        {
            return slider.click();
        }

        public override bool hover(COORDINATE mCoo)
        {
            return slider.hover(mCoo) || mCoo.isWithinRec(this);
        }
    }
}