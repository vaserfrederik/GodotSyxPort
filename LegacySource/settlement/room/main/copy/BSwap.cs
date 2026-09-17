using System;
using System.Collections.Generic;
using settlement.room.main.copy;
using game.faction;
using init.sprite;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sets;
using util.gui.misc;
using util.text;
using view.main;

namespace settlement.room.main.copy
{
    final class BSwap
    {
        private readonly KeyMap<GuiSection> otherPrints = new KeyMap<GuiSection>();
        private CLICKABLE button;
        private RoomBlueprintImp current;
        private readonly ArrayListResize<CLICKABLE> wrap = new ArrayListResize<CLICKABLE>(4, 16);

        private static readonly CharSequence ¤¤swap = "¤Switch to another type of room.";
        static
        {
            D.ts(typeof(BSwap));
        }

        public BSwap(ROOMS m)
        {
            SPRITE sp = new SPRITE.Imp(Icon.M)
            {
                public void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                {
                    current.iconBig().renderC(r, X1 + (X2 - X1) / 2, Y1 + (Y2 - Y1) / 2);
                    SPRITES.icons().m.rotate.renderC(r, X1 + (X2 - X1) / 2, Y1 + (Y2 - Y1) / 2);
                }
            };

            button = new GButt.ButtPanel(sp)
            {
                protected override void clickA()
                {
                    VIEW.inters().popup.show(alt(), this);
                }
            }.setDim(Icon.L + 4).hoverInfoSet(¤¤swap);

            for (int i = 0; i < m.all().size(); i++)
            {
                RoomBlueprint p = m.all().get(i);
                if (p is RoomBlueprintIns)
                    addGroup((RoomBlueprintIns)p, m);
            }
        }

        private GuiSection alt()
        {
            return otherPrints.get(current.key);
        }

        public void init(RoomBlueprintImp bb)
        {
            current = bb;
            if (bb.reqs.passes(FACTIONS.player()))
                return;

            foreach (RoomBlueprint p in SETT.ROOMS().all())
            {
                if (p is RoomBlueprintIns)
                {
                    RoomBlueprintIns ins = (RoomBlueprintIns)p;
                    if (ins.GetType() == bb.GetType() && ins.constructor().mustBeIndoors() == bb.constructor().mustBeIndoors() && ins.reqs.passes(FACTIONS.player()))
                    {
                        current = ins;
                        return;
                    }
                }
            }
        }

        public LIST<CLICKABLE> wrap(LIST<CLICKABLE> others)
        {
            wrap.clearSoft();
            if (others != null)
                wrap.add(others);
            if (alt() != null)
            {
                wrap.add(button);
            }
            return wrap;
        }

        public RoomBlueprintImp current()
        {
            return current;
        }

        private void addGroup(RoomBlueprintIns blue, ROOMS m)
        {
            if (otherPrints.get(blue.key) != null)
                return;
            if (!blue.constructor().usesArea())
                return;

            LinkedList<RoomBlueprintImp> res = new LinkedList<RoomBlueprintImp>();

            foreach (RoomBlueprint p in m.all())
            {
                if (p is RoomBlueprintIns)
                {
                    RoomBlueprintIns ins = (RoomBlueprintIns)p;
                    if (ins.GetType() == blue.GetType() && ins.constructor().mustBeIndoors() == blue.constructor().mustBeIndoors())
                        res.add(ins);
                }
            }

            if (res.size() <= 1)
                return;

            GuiSection s = new GuiSection();

            foreach (RoomBlueprintImp p in res)
            {
                CLICKABLE c = new GButt.Panel(p.iconBig(), p.info.name)
                {
                    protected override void renAction()
                    {
                        selectedSet(current == p);
                        activeSet(p.reqs.passes(FACTIONS.player()));
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        text.title(p.info.name);
                        text.text(p.info.desc);
                        text.NL();

                        if (!p.reqs.passes(FACTIONS.player()))
                        {
                            p.reqs.hover(text, FACTIONS.player());
                        }
                    }

                    protected override void clickA()
                    {
                        if (p.reqs.passes(FACTIONS.player()))
                            current = p;
                        VIEW.inters().popup.close();
                    }
                };
                s.addDownC(0, c);
            }

            otherPrints.put(blue.key, s);
        }
    }
}