using System;
using System.Collections.Generic;
using settlement.overlay;
using game;
using init.constant;
using init.sprite;
using settlement.main;
using settlement.tilemap.floor;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.colors;
using util.info;
using util.rendering;
using view.main;

public abstract class Addable : INFO
{
    static List<Addable> ALL = new List<Addable>();

    static Addable()
    {
        new GameDisposable()
        {
            protected override void dispose()
            {
                ALL.Clear();
            }
        };
    }

    static readonly int iSize = Icon.M * 2;
    static readonly int iOff = (C.TILE_SIZE - iSize) / 2;

    bool added = false;
    readonly bool above;
    readonly bool under;
    public readonly string key;
    protected bool exclusive = false;
    public readonly SPRITE icon;

    public Addable(SPRITE icon, string key, string name, string desc) : this(icon, key, name, desc, false, true) { }

    public Addable(SPRITE icon, string key, string name, string desc, bool underling, bool above) : base(name, desc)
    {
        ALL.Add(this);
        this.under = underling;
        this.above = above;
        this.key = key;
        this.icon = icon;
    }

    public Addable(bool underling, bool above) : this(null, null, null, null, underling, above) { }

    public void Add()
    {
        SETT.OVERLAY().added = true;
        added = true;
    }

    public bool Added()
    {
        return added;
    }

    public virtual void InitBelow(RenderData data) { }

    public virtual void FinishBelow() { }

    public virtual void InitAbove(RenderData data) { }

    public virtual void FinishAbove() { }

    public virtual bool Render(Renderer r, RenderIterator it)
    {
        return false;
    }

    public virtual void RenderBelow(Renderer r, RenderIterator it) { }

    public static void RenderUnder(double v, Renderer r, RenderIterator it)
    {
        RenderUnder(v, r, it, true);
    }

    public static bool RenderAbove(double v, Renderer r, RenderIterator it, bool pluses)
    {
        COLOR c = COLOR.WHITE05;

        if (v >= 0)
        {
            c = ColorImp.TMP.Interpolate(COLOR.WHITE05, GCOLOR.MAP().OVERLAY_GOOD, v);
        }
        else
        {
            c = ColorImp.TMP.Interpolate(COLOR.WHITE05, GCOLOR.MAP().OVERLAY_BAD, -v);
        }

        if (!RenderAbove(c, r, it))
            return false;

        if (pluses && VIEW.s().getWindow().zoomout() <= 1)
        {
            int am = (int)Math.Round(v * 4.0);
            for (int i = 0; i < am; i++)
            {
                SPRITES.icons().s.plus.RenderScaled(r, it.x() + 16 * (i % 2), it.y() + 16 + (i / 2) * 16, 2);
            }
        }
        return true;
    }

    public static void RenderUnder(double v, Renderer r, RenderIterator it, bool pluses)
    {
        COLOR c = COLOR.WHITE05;

        if (v >= 0)
        {
            c = ColorImp.TMP.Interpolate(COLOR.WHITE05, GCOLOR.MAP().OVERLAY_GOOD, v);
        }
        else
        {
            c = ColorImp.TMP.Interpolate(COLOR.WHITE05, GCOLOR.MAP().OVERLAY_BAD, -v);
        }

        if (!RenderUnder(c, r, it))
            return;

        if (pluses && VIEW.s().getWindow().zoomout() <= 1)
        {
            int am = (int)Math.Round(v * 4.0);
            for (int i = 0; i < am; i++)
            {
                SPRITES.icons().s.plus.RenderScaled(r, it.x() + 16 * (i % 2), it.y() + 16 + (i / 2) * 16, 2);
            }
        }
        it.hiddenSet();
    }

    public static void RenderPluses(double v, Renderer r, RenderIterator it)
    {
        if (VIEW.s().getWindow().zoomout() <= 1)
        {
            int am = (int)Math.Round(v * 4.0);
            for (int i = 0; i < am; i++)
            {
                SPRITES.icons().s.plus.RenderScaled(r, it.x() + 16 * (i % 2), it.y() + 16 + (i / 2) * 16, 2);
            }
        }
    }

    public static void RenderColor(double v, Renderer r, RenderIterator it, bool pluses)
    {
        COLOR c = COLOR.WHITE05;

        if (v >= 0)
        {
            c = ColorImp.TMP.Interpolate(COLOR.WHITE05, GCOLOR.MAP().OVERLAY_GOOD, v);
        }
        else
        {
            c = ColorImp.TMP.Interpolate(COLOR.WHITE05, GCOLOR.MAP().OVERLAY_BAD, -v);
        }

        c.Bind();
        SPRITES.cons().BIG.filled.Render(r, 0x0F, it.x(), it.y());

        if (pluses && VIEW.s().getWindow().zoomout() <= 1)
        {
            int am = (int)Math.Round(v * 4.0);
            for (int i = 0; i < am; i++)
            {
                SPRITES.icons().s.plus.RenderScaled(r, it.x() + 16 * (i % 2), it.y() + 16 + (i / 2) * 16, 2);
            }
        }
        it.hiddenSet();
    }

    public static bool RenderUnder(COLOR c, Renderer r, RenderIterator it)
    {
        if (VIEW.s().getWindow().zoomout() >= 3 && SETT.TERRAIN().Get(it.tile()).miniDepth() > 0)
            return false;

        if (SETT.ROOMS().placement.embryo.Is(it.tile()))
            return false;

        c.Bind();

        int m = 0x0F;

        Floor f = SETT.FLOOR().getter.Get(it.tile());

        if (f != null)
        {
            COLOR.Unbind();
            SETT.FLOOR().RenderSimple(r, it, f);
            c.Bind();
            m = RMask(it);
            SPRITES.cons().BIG.filled_striped.Render(r, m, it.x(), it.y());
        }
        else if (SETT.ROOMS().map.Is(it.tile()))
        {
            m = RMask(it);
            SPRITES.cons().BIG.filled_striped.Render(r, m, it.x(), it.y());
        }
        else
        {
            SPRITES.cons().BIG.filled.Render(r, m, it.x(), it.y());
        }

        it.hiddenSet();
        return true;
    }

    public static bool RenderAbove(COLOR c, Renderer r, RenderIterator it)
    {
        if (SETT.ROOMS().placement.embryo.Is(it.tile()))
            return false;

        TerrainTile tt = SETT.TERRAIN().Get(it.tile());

        if (tt != SETT.TERRAIN().NADA && !(tt is TBuilding.Ceiling))
        {
            c.Bind();
            SPRITES.cons().BIG.filled_striped.Render(r, 0, it.x(), it.y());
            return true;
        }
        return false;
    }

    private static int RMask(RenderIterator it)
    {
        int m = 0;
        foreach (DIR d in DIR.ORTHO)
        {
            int tx = it.tx() + d.x();
            int ty = it.ty() + d.y();

            if (SETT.FLOOR().getter.Get(tx, ty) != null || SETT.ROOMS().map.Is(tx, ty))
            {
                m |= d.mask();
            }
        }
        return m;
    }
}