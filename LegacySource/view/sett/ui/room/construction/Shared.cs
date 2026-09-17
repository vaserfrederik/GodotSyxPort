using System.Collections.Generic;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using util.gui.misc;
using util.text;

namespace view.sett.ui.room.construction
{
    public class Shared
    {
        private readonly GuiSection buttonsIndoor = new GuiSection();

        public Shared()
        {
            D.gInit(this);

            foreach (TBuilding t in SETT.TERRAIN().BUILDINGS.all())
            {
                CLICKABLE c = new GButt.Panel(t.iconCombo, t.structure.desc)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(t.structure.name);
                        buttonIndoor.hoverInfoGet(text);
                        b.NL();
                        b.text(t.structure.desc);
                        b.setResource(t.structure.resource, t.structure.resAmount);
                    }

                    protected override void clickA()
                    {
                        SETT.ROOMS().placement.placer.structure.set(t);
                        VIEW.inters().popup.close();
                    }

                    protected override void renAction()
                    {
                        selectedSet(SETT.ROOMS().placement.placer.structure.get() == t);
                    }
                };
                buttonsIndoor.addDownC(0, c);
            }
        }

        private readonly CLICKABLE buttonIndoor = new GButt.ButtPanel(SPRITES.icons().m.cancel)
        {
            protected override void clickA()
            {
                VIEW.inters().popup.show(buttonsIndoor, this);
            }

            protected override void renAction()
            {
                replaceLabel(SETT.ROOMS().placement.placer.structure.get().iconCombo, DIR.C);
            }
        }.hoverInfoSet(D.g("indoor", "This room requires to be built indoors and you must pick a structure type."));
    }
}