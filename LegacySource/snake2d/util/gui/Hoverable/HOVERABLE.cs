using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;

public interface HOVERABLE : RENDEROBJ
{
    bool hover(COORDINATE mCoo);
    bool hoveredIs();
    void hoverInfoGet(GUI_BOX text);
    HOVERABLE visableSet(bool yes);
    HOVERABLE hoverInfoSet(CharSequence s);
    HOVERABLE hoverTitleSet(CharSequence s);
}

public abstract class HoverableAbs : RENDEROBJ.RenderImp, HOVERABLE
{
    protected readonly Rec body = new Rec();
    protected bool isHovered = false;
    private CharSequence hoverInfo = null;
    private CharSequence title = null;

    public HoverableAbs() { }

    public HoverableAbs(int width, int height)
    {
        body.setDim(width, height);
    }

    public HoverableAbs(int dim)
    {
        body.setDim(dim);
    }

    public HoverableAbs(DIMENSION dim)
    {
        body.setDim(dim.width(), dim.height());
    }

    public bool hover(COORDINATE mCoo)
    {
        if (!visableIs())
            return false;
        return isHovered = mCoo.isWithinRec(body);
    }

    public bool hoveredIs()
    {
        return isHovered;
    }

    public void hoverInfoGet(GUI_BOX text)
    {
        if (hoverInfo != null)
        {
            text.text(hoverInfo);
        }
        if (title != null)
            text.title(title);
    }

    public RecFacade body()
    {
        return body;
    }

    public HOVERABLE visableSet(bool yes)
    {
        base.visableSet(yes);
        return this;
    }

    public HOVERABLE hoverInfoSet(CharSequence s)
    {
        this.hoverInfo = s;
        return this;
    }

    public HOVERABLE hoverTitleSet(CharSequence s)
    {
        this.title = s;
        return this;
    }

    public override void render(SPRITE_RENDERER r, float ds)
    {
        if (visableIs())
            render(r, ds, isHovered);
        isHovered = false;
    }

    protected abstract void render(SPRITE_RENDERER r, float ds, bool isHovered);

    public void hoveredSet(bool h)
    {
        isHovered = h;
    }
}

public class Sprite : HoverableAbs
{
    protected SPRITE sprite;
    protected COLOR color = COLOR.WHITE100;
    private DIR align = DIR.NW;

    public Sprite() { }

    public Sprite(int dim)
    {
        body.setDim(dim);
    }

    public Sprite(SPRITE sprite)
    {
        setSprite(sprite);
    }

    public Sprite(SPRITE sprite, COLOR c)
    {
        setSprite(sprite);
        color = c;
    }

    public HOVERABLE.Sprite setAlign(DIR d)
    {
        this.align = d;
        return this;
    }

    public void setSprite(SPRITE sprite)
    {
        this.sprite = sprite;
        adjust();
    }

    protected void adjust()
    {
        if (body.width() != sprite.width() || body.height() != sprite.height())
        {
            align.reposition(body, sprite.width(), sprite.height());
        }
    }

    public void replaceSprite(SPRITE newSprite, DIR d)
    {
        this.sprite = newSprite;
        if (sprite == null)
        {
            d.reposition(body, 0, 0);
        }
        else
        {
            d.reposition(body, newSprite.width(), newSprite.height());
        }
    }

    public void setColor(COLOR color)
    {
        this.color = color;
    }

    protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
    {
        color.bind();
        adjust();
        sprite.render(r, body.x1(), body.y1());
        COLOR.unbind();
        OPACITY.unbind();
    }
}