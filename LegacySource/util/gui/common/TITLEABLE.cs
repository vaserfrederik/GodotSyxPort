using init.sprite.UI;
using snake2d;
using snake2d.util.gui.Hoverable;
using snake2d.util.sprite;
using util.gui.misc;
using util.info;

namespace util.gui.common
{
    public interface TITLEABLE : SPRITE
    {
        default HOVERABLE hv(CharSequence name)
        {
            return new GHeader.HeaderVertical(name, this);
        }

        default HOVERABLE hv(CharSequence name, CharSequence desc)
        {
            return new GHeader.HeaderVertical(name, this).hoverInfoSet(desc);
        }

        default HOVERABLE hv(INFO info)
        {
            return new GHeader.HeaderVertical(info.name, this).hoverTitleSet(info.name).hoverInfoSet(info.desc);
        }

        default HOVERABLE hv(SPRITE name)
        {
            return new GHeader.HeaderVertical(name, this);
        }

        default HOVERABLE hh(CharSequence name)
        {
            return new GHeader.HeaderHorizontal(name, this);
        }

        default HOVERABLE hhw(CharSequence name, int trail)
        {
            GHeader.HeaderHorizontal h = new GHeader.HeaderHorizontal(name, this);
            h.body().incrW(trail);
            return h;
        }

        default HOVERABLE hhw(SPRITE name, int trail)
        {
            GHeader.HeaderHorizontal h = new GHeader.HeaderHorizontal(name, this);
            h.body().incrW(trail);
            return h;
        }

        default HOVERABLE hh(INFO info)
        {
            return new GHeader.HeaderHorizontal(info.name, this).hoverInfoSet(info.desc);
        }

        default HOVERABLE hh(SPRITE name)
        {
            return new GHeader.HeaderHorizontal(name, this);
        }

        default HOVERABLE hh(SPRITE name, int width)
        {
            return new GHeader.HeaderHorizontal(name, this, width);
        }

        default GHeader.HeaderHorizontal hh(CharSequence name, int width)
        {
            return new GHeader.HeaderHorizontal(name, this, width);
        }

        default GHeader.HeaderHorizontal hh(SPRITE icon, CharSequence name, int width)
        {
            final GText t = new GText(UI.FONT().S, name).lablify();
            final SPRITE ii = icon.resized(t.height());
            SPRITE sp = new SPRITE.Imp(ii.width() + 4 + t.width(), t.height())
            {
                public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    icon.render(r, X1, Y1);
                    t.render(r, X1 + t.height() + 4, Y1);
                }
            };
            return new GHeader.HeaderHorizontal(sp, this, width);
        }

        default GHeader.HeaderHorizontal hh(CharSequence name, CharSequence desc, int width)
        {
            GHeader.HeaderHorizontal h = new GHeader.HeaderHorizontal(name, this, width);
            h.hoverTitleSet(name);
            h.hoverInfoSet(desc);
            return h;
        }

        default GHeader.HeaderHorizontal hh(CharSequence name, CharSequence desc)
        {
            GHeader.HeaderHorizontal h = new GHeader.HeaderHorizontal(name, this);
            h.hoverTitleSet(name);
            h.hoverInfoSet(desc);
            return h;
        }

        default void hoverInfoGet(GBox b)
        {
        }
    }
}