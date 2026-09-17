using System;
using util.gui.clickable;
using util.data.INT;
using util.gui.slider;

namespace util.gui.table
{
    public abstract class GScrollable : Scrollable, INTE
    {
        public GScrollable(params ScrollRow[] rows) : base(null, rows)
        {
            GSliderVer slider = new GSliderVer(this, getView().body().height());
            slider.body().moveX1(getView().body().x2() + C.SG * 4);
            slider.body().moveY1(getView().body().y1());
            getView().add(slider);
        }
    }
}