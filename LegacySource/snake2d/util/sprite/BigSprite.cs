using snake2d;
using snake2d.util.datatypes;

public abstract class BigSprite
{
    private readonly float px;

    protected readonly int gameWidth;
    protected readonly int gameHeight;
    private readonly int scale;

    protected BigSprite(int scale, int width, int height)
    {
        this.px = 1f / scale;
        this.gameWidth = width * scale;
        this.gameHeight = height * scale;
        this.scale = scale;
    }

    /**
     * 
     * @param r
     * @param ds
     * @param x1 where in gameCoo
     * @param y1 where in gameCoo
     * @param scale
     * @param quad
     */
    public void Render(SPRITE_RENDERER r, int x1, int y1, RECTANGLE quad)
    {
        short tx1 = (short)(startX() + quad.x1() * px);
        short ty1 = (short)(startY() + quad.y1() * px);

        int dx = quad.x1() % scale;
        int dy = quad.y1() % scale;

        x1 -= dx;
        y1 -= dy;

        r.renderSprite(x1, x1 + quad.width() + scale, y1, y1 + quad.height() + scale,
                TextureCoords.Normal.Get(tx1, ty1, (int)(px * quad.width() + 1), (int)(px * quad.height() + 1)));
    }

    private readonly TextureCoords stencil = new TextureCoords();

    /**
     * 
     * @param r
     * @param ds
     * @param x1 where in gameCoo
     * @param y1 where in gameCoo
     * @param scale
     * @param quad
     */
    public void Render(SPRITE_RENDERER r, int x1, int y1, int width, int height, int scale, double dpx1, double dpy1)
    {
        int pw = width / scale;
        int ph = height / scale;

        if (dpx1 < 0)
            dpx1 = 0;

        if (dpy1 < 0)
            dpy1 = 0;

        if (dpx1 >= gameWidth)
            return;

        if (dpy1 >= gameHeight)
            return;

        int px1 = (int)dpx1;
        int py1 = (int)dpy1;

        int dx = (int)((dpx1 - px1) * scale);
        int dy = (int)((dpy1 - py1) * scale);

        if (px1 + pw > gameWidth)
        {
            pw = gameWidth - px1;
        }
        else if (dx != 0)
        {
            pw++;
        }

        if (py1 + ph > gameHeight)
        {
            ph = gameHeight - py1;
        }
        else if (dy != 0)
        {
            ph++;
        }

        stencil.Get(px1 + startX(), py1 + startY(), pw, ph);

        width = pw * scale;
        height = ph * scale;

        x1 -= dx;
        y1 -= dy;

        r.renderSprite(x1, x1 + width, y1, y1 + height,
                stencil);
    }

    protected abstract int startX();
    protected abstract int startY();

    public int GetGameWidth()
    {
        return gameWidth;
    }

    public int GetGameHeight()
    {
        return gameHeight;
    }
}