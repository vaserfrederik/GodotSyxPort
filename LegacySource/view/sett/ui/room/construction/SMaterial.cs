using System;
using System.Collections.Generic;
using init.sprite;
using init.structure;
using settlement.main;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.clickable;
using util.gui.misc;
using util.info;
using util.text;
using view.main;

namespace view.sett.ui.room.construction
{
    internal sealed class SMaterial
    {
        private readonly GuiSection section = new GuiSection();
        private readonly GuiSection buttonsIndoor = new GuiSection();
        private readonly State s;

        static SMaterial()
        {
            D.gInit(typeof(SMaterial));
        }

        private readonly CLICKABLE buttonIndoor = new GButt.ButtPanel(SPRITES.icons().m.cancel)
        {
            protected override void clickA()
            {
                VIEW.inters().popup.show(buttonsIndoor, this);
            }

            protected override void renAction()
            {
                replaceLabel(s.placement.placer.structure.get().iconCombo, DIR.C);
            }
        }.hoverInfoSet(D.g("indoor", "This room requires to be built indoors and you must pick a structure type."));

        private readonly CLICKABLE buttWalls = new GButt.ButtPanel(SPRITES.icons().m.wall)
        {
            protected override void clickA()
            {
                s.placement.placer.autoWalls.toggle();
            }

            protected override void renAction()
            {
                selectedSet(s.placement.placer.autoWalls.is());
            }
        }.hoverInfoSet(D.g("walls", "Auto build walls around room."));

        private readonly CLICKABLE buttDoor = new GButt.ButtPanel(SETT.ROOMS().placement.placer.placerDoor.getIcon())
        {
            protected override void clickA()
            {
                if (s.placement.placer.autoWalls.is())
                {
                    VIEW.s().tools.place(s.placement.placer.placerDoor, s.config);
                }
            }

            protected override void renAction()
            {
                activeSet(s.placement.placer.autoWalls.is());
                selectedSet(VIEW.s().tools.placer.getCurrent() == s.placement.placer.placerDoor);
            }

            protected override void render(SPRITE_RENDERER r, float ds, bool isActive, bool isSelected, bool isHovered)
            {
                base.render(r, ds, isActive, isSelected, isHovered);
                if (s.problemneedDoor && s.problemTimer > VIEW.renderSecond())
                {
                    COLOR.RED100.renderFrame(r, body, 2, 3);
                    OPACITY.O25To50.bind();
                    COLOR.RED100.render(r, body);
                    OPACITY.unbind();
                }
            }
        }.hoverInfoSet(D.g("door", "Places doorways on walls. Needed to make the room reachable. Doorways decrease insulation, but too little will make entering the room difficult."));

        private readonly CLICKABLE buttDoorRemove = new GButt.ButtPanel(SETT.ROOMS().placement.placer.placerDoor.getUndo().getIcon())
        {
            protected override void clickA()
            {
                if (s.placement.placer.autoWalls.is())
                {
                    VIEW.s().tools.place(s.placement.placer.placerDoor.getUndo(), s.config);
                }
            }

            protected override void renAction()
            {
                activeSet(s.placement.placer.autoWalls.is());
                selectedSet(VIEW.s().tools.placer.getCurrent() == s.placement.placer.placerDoor.getUndo());
            }
        }.hoverInfoSet(SETT.ROOMS().placement.placer.placerDoor.getUndo().name());

        private readonly HOVERABLE isolation = new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.perc(text, s.placement.placer.isolation());
            }
        }.hh(SETT.ROOMS().isolation.info.name).hoverInfoSet(SETT.ROOMS().isolation.info.desc);

        internal SMaterial(State s)
        {
            this.s = s;
            foreach (Structure t in STRUCTURES.all())
            {
                CLICKABLE c = new GButt.Panel(t.terrain().iconCombo, t.desc)
                {
                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        GBox b = (GBox)text;
                        b.title(t.name);
                        buttonIndoor.hoverInfoGet(text);
                        b.NL();
                        b.text(t.desc);
                        b.setResource(t.resource, t.resAmount);
                    }

                    protected override void clickA()
                    {
                        s.placement.placer.structure.set(t.terrain());
                        VIEW.inters().popup.close();
                    }

                    protected override void renAction()
                    {
                        selectedSet(s.placement.placer.structure.get() == t.terrain());
                    }
                };
                buttonsIndoor.addDownC(0, c);
            }
        }

        internal GuiSection Get()
        {
            section.clear();

            section.addRightC(0, buttWalls);
            if (s.b.constructor().mustBeIndoors() && s.b.constructor().usesArea())
            {
                section.addRightC(0, buttDoor);
                section.addRightC(0, buttDoorRemove);
            }
            section.addRightC(0, buttonIndoor);

            if (s.b.constructor().needsIsolation())
                section.addRelBody(4, DIR.N, isolation);
            return section;
        }
    }
}