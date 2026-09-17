using System.Collections.Generic;
using settlement.room.main.copy;
using init.sprite;
using init.sprite.UI;
using settlement.main;
using settlement.room.main;
using settlement.room.main.construction;
using settlement.room.main.copy.SavedPrints;
using settlement.room.main.furnisher;
using settlement.room.main.placement;
using settlement.room.main.util;
using settlement.tilemap.terrain;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.gui.misc;
using util.text;
using view.main;
using view.tool;

public sealed class SavedPrintsPlacer
{
    private SavedPrint print;
    private readonly BSwap swap;
    private readonly GuiSection sSelect = new GuiSection();
    private bool w = true;

    private readonly CLICKABLE bOverlay = new GButt.ButtPanel(UI.icons().s.eye.sized(Icon.M))
    {
        protected override void clickA()
        {
            SETT.ROOMS().placement.placer.showOverlay.toggle();
        }

        protected override void renAction()
        {
            selectedSet(SETT.ROOMS().placement.placer.showOverlay.is());
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            text.title(Dic.¤¤Overlay);
            SETT.ROOMS().placement.placer.structure.get();
            if (swap.current().constructor().overlay() != null && swap.current().constructor().overlay().desc != null)
            {
                text.text(swap.current().constructor().overlay().desc);
            }
        }
    };

    private readonly CLICKABLE bFoundation = new GButt.ButtPanel(UI.icons().m.foundation)
    {
        protected override void clickA()
        {
            SETT.ROOMS().placement.placer.showFoundation.toggle();
        }

        protected override void renAction()
        {
            selectedSet(SETT.ROOMS().placement.placer.showFoundation.is());
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            text.title(SETT.OVERLAY().FOUNDATION.name);
            text.text(SETT.OVERLAY().FOUNDATION.desc);
        }
    };

    private readonly List<CLICKABLE> walls = new List<CLICKABLE>
    {
        new GButt.ButtPanel(SPRITES.icons().m.wall)
        {
            private string s = "Include walls";

            protected override void clickA()
            {
                w = !w;
            }

            protected override void renAction()
            {
                selectedSet(w);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                text.text(s);
            }
        }.setDim(Icon.L + 4),
        new GButt.ButtPanel(SPRITES.icons().m.wall)
        {
            protected override void clickA()
            {
                VIEW.inters().popup.show(sSelect, this);
            }

            protected override void renAction()
            {
                activeSet(w);
                if (structure() != null)
                    replaceLabel(structure().iconCombo, DIR.C);
            }
        }.setDim(Icon.L + 4)
    };

    private readonly List<CLICKABLE> butts = new List<CLICKABLE>(walls.Count + 1);

    public SavedPrintsPlacer(BSwap swap)
    {
        this.swap = swap;
        foreach (TBuilding b in SETT.TERRAIN().BUILDINGS.all())
        {
            sSelect.addDown(0, new GButt.ButtPanel(b.iconCombo)
            {
                protected override void clickA()
                {
                    SETT.ROOMS().placement.placer.structure.set(b);
                    VIEW.inters().popup.close();
                }

                protected override void renAction()
                {
                    selectedSet(structure() == b);
                }
            }.hoverTitleSet(b.structure.name));
        }
    }

    private TBuilding structure()
    {
        TBuilding structure = SETT.ROOMS().placement.placer.structure.get();
        if (structure == null)
        {
            SETT.ROOMS().placement.placer.structure.set(SETT.TERRAIN().BUILDINGS.MUD);
            structure = SETT.TERRAIN().BUILDINGS.MUD;
        }
        return structure;
    }

    public void place(SavedPrint print)
    {
        this.print = print;
        swap.init(print.blue);
        // structure = print.structure;
        // if (print.blue.constructor().mustBeIndoors() && structure == null) {
        // structure = SETT.TERRAIN().BUILDINGS.MUD;
        // }
        VIEW.s().tools.place(placer);
    }

    public void place(SavedPrint print, RoomBlueprint blue)
    {
        if (blue.GetType() != print.blue.GetType())
            throw new System.RuntimeException();
        this.print = print;
        swap.init((RoomBlueprintImp)blue);
        // structure = print.structure;
        // if (print.blue.constructor().mustBeIndoors() && structure == null) {
        // structure = SETT.TERRAIN().BUILDINGS.MUD;
        // }
        VIEW.s().tools.place(placer);
    }

    private readonly Placer placer = new Placer();
    private class Placer : PlacerBase
    {
        // Implement all the methods from PlacerBase here
        // ...
    }
}