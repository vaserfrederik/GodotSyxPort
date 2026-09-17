using System;
using System.Collections.Generic;
using settlement.room.infra.logistics;
using game;
using init.sprite.UI;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.text;
using view.main;

public sealed class MoveOrderPullersUI : GButt.ButtPanel
{
    private static ArrayList<RoomInstance> ins;

    private static string ¤¤pullers = "Pullers";

    private readonly GuiSection pop;

    static MoveOrderPullersUI()
    {
        new GameDisposable
        {
            protected override void Dispose()
            {
                ins = null;
            }
        };
        D.ts(typeof(MoveOrderPullersUI));
    }

    private readonly GETTER<RoomInstance> g;

    public MoveOrderPullersUI(GETTER<RoomInstance> g) : base(UI.icons().m.storage_pullers)
    {
        this.g = g;
        body.setDim(48);

        GTableBuilder bu = new GTableBuilder
        {
            NrOFEntries = () => all().size()
        };

        bu.Column(400, new GRowBuilder
        {
            Build = (GETTER<int> ier) =>
            {
                SPRITE sp = new SPRITE.Imp(400, Icon.L)
                {
                    Render = (SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2) =>
                    {
                        RoomInstance ins = all().get(ier.Get());
                        if (ins != null)
                        {
                            ins.icon().render(r, X1, Y1);
                            GCOLOR.T().H1.bind();
                            UI.FONT().H2.render(r, ins.name(), X1 + 48, Y1 + Icon.L / 2 - UI.FONT().H2.height() / 2);
                        }
                    }
                };
                return new GButt.ButtPanel(sp)
                {
                    ClickA = () =>
                    {
                        RoomInstance ins = all().get(ier.Get());
                        if (ins != null)
                            VIEW.s().getWindow().centererTile.set(ins.body().cX(), ins.body().cY());
                    }
                };
            }
        }, DIR.NW);

        pop = new GuiSection();
        pop.add(bu.Create(20, false));
    }

    protected override void RenAction()
    {
        activeSet(!all().isEmpty());
    }

    protected override void ClickA()
    {
        VIEW.inters().popup.show(pop, this, true);
    }

    public override void HoverInfoGet(GUI_BOX text)
    {
        GBox b = (GBox)text;
        b.title(¤¤pullers);
        foreach (RoomInstance ins in all())
        {
            b.add(ins.icon());
            b.text(ins.name());
            b.NL();
        }

        base.HoverInfoGet(text);
    }

    private int upI = -1;

    private LIST<RoomInstance> all()
    {
        if (ins == null)
        {
            ins = new ArrayList<RoomInstance>(100);
        }
        if (upI == GAME.updateI())
            return ins;

        ins.clearSloppy();
        RoomInstance tar = g.Get();
        foreach (RoomBlueprint b in SETT.ROOMS().all())
        {
            if (b is RoomBlueprintIns<?>)
            {
                RoomBlueprintIns<?> bb = (RoomBlueprintIns<?>)b;
                if (bb.instancesSize() > 0 && bb.getInstance(0) is MoveOrderPullInstance)
                {
                    for (int i = 0; i < bb.instancesSize(); i++)
                    {
                        RoomInstance ii = bb.getInstance(i);
                        if (ii is MoveOrderPullInstance)
                        {
                            MoveOrderPullInstance pi = (MoveOrderPullInstance)ii;
                            foreach (MoveOrderPull p in pi.moveOrdersPull())
                            {
                                if (p != null && p.source() == tar)
                                {
                                    ins.add(ii);
                                    if (!ins.hasRoom())
                                        return ins;
                                }
                            }
                        }
                    }
                }
            }
        }
        return ins;
    }
}