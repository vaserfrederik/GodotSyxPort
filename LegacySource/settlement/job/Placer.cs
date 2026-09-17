using System;
using System.Collections.Generic;
using settlement.main;
using init.resources;
using init.sprite;
using settlement.job.StateManager;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.gui.misc;
using util.text;
using view.subview;
using view.tool;

class Placer : PlacableMulti
{
    private readonly Job j;
    private readonly RESOURCE res;
    private readonly int resAmount;
    public readonly string desc;

    public Placer(Job j, string desc)
        : this(j, null, 0, desc)
    {
    }

    public Placer(Job j, RESOURCE res, int resAmount, string desc)
        : base(j.name, desc, j.icon)
    {
        this.j = j;
        this.res = res;
        this.resAmount = resAmount;
        this.desc = desc;
    }

    public override string Name()
    {
        return j.name;
    }

    public override void UpdateRegardless(GameWindow window, AREA selected)
    {
        j.DoSomethingExtraRender();
        base.UpdateRegardless(window, selected);
    }

    public override string IsPlacable(int tx, int ty, AREA a, PLACER_TYPE t)
    {
        return j.Problem(tx, ty, Job.overwrite);
    }

    public override void Place(int tx, int ty, AREA a, PLACER_TYPE t)
    {
        Place(tx, ty, j);
    }

    public static void Place(int tx, int ty, Job j)
    {
        if (!IN_BOUNDS(tx, ty))
        {
            return;
        }
        int i = tx + ty * TWIDTH;
        Job old = JOBS().getter.Get(i);

        if (old == j)
        {
            if (!JOBS().planMode.Is() && JOBS().state.Is(tx, ty, State.DORMANT))
            {
                JOBS().state.Set(State.RESERVABLE, JOBS().getter.Get(i));
            }
            return;
        }

        if (old != null)
        {
            old.Cancel(tx, ty);
            PlacerDelete.Place(tx, ty);
        }

        j.Init(tx, ty);
        JOBS().Set(j, tx, ty);

        if (!JOBS().planMode.Is())
        {
            JOBS().state.Set(State.RESERVABLE, JOBS().getter.Get(i));
        }
        else
        {
            JOBS().state.Set(State.DORMANT, JOBS().getter.Get(i));
        }
    }

    public override void RenderPlaceHolder(SPRITE_RENDERER r, int mask, int x, int y, int tx, int ty, AREA a, PLACER_TYPE t, bool isPlacable, bool areaIsPlacable)
    {
        SPRITES.cons().BIG.dashedThick.Render(r, mask, x, y);
    }

    public override void PlaceInfo(GBox b, int okTiles, AREA a)
    {
        if (a.Body().Width() < 2 && a.Body().Height() < 2)
            return;
        b.SetArea(a.Body());
        if (okTiles > 0 && res != null)
            b.SetResource(res, resAmount * okTiles);
    }

    public override void HoverDesc(GBox box)
    {
        base.HoverDesc(box);
        if (res != null)
        {
            box.NL();
            box.SetResource(res, resAmount);
        }
        box.NL();
        j.ExtraHovInfo(box);
    }

    public override PLACABLE GetUndo()
    {
        return JOBS().tool_clear;
    }

    private static string ¤¤overwrite = "¤Overwrite";
    private static string ¤¤overdesc = "¤Overwrites other jobs or structures when placed.";

    static Placer()
    {
        D.ts(typeof(Placer));
    }

    protected readonly LIST<CLICKABLE> bOverwrite = new ArrayList<CLICKABLE>(new GButt.Panel(SPRITES.icons().m.overwrite)
    {
        {
            hoverInfoSet(¤¤overdesc);
            hoverTitleSet(¤¤overwrite);
        }

        protected override void RenAction()
        {
            selectedSet(Job.overwrite);
        }

        protected override void ClickA()
        {
            Job.overwrite = !Job.overwrite;
        }
    });

    public override LIST<CLICKABLE> GetAdditionalButt()
    {
        return bOverwrite;
    }
}