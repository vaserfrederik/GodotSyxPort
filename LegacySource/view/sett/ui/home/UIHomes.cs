using System;
using game.faction;
using init.sprite;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.keyboard;
using view.main;
using view.sett.ui.bottom;
using view.tool;

namespace view.sett.ui.home
{
    public class UIHomes : ISidePanel
    {
        private readonly UIHomesTable table;
        private bool overlay = true;

        public UIHomes()
        {
            {
                PlacableMulti pp = new UIHomeAssign();

                ACTION a = new ACTION()
                {
                    public override void exe()
                    {
                        VIEW.s().tools.place(pp);
                    }
                };

                GButt.ButtPanel c = new GButt.ButtPanel(pp.getIcon())
                {
                    protected override void clickA()
                    {
                        VIEW.s().tools.place(pp);
                    }
                };

                c.setDim(60, 40);
                CLICKABLE cc = KeyButt.wrap(a, c, KEYS.SETT(), "SET_HOMES", pp.name(), pp.desc);
                section.add(cc);
            }
            {
                PlacableMulti pp = new UIHomeOdd();
                ACTION a = new ACTION()
                {
                    public override void exe()
                    {
                        VIEW.inters().popup.close();
                        VIEW.s().tools.place(pp);
                    }
                };

                GButt.ButtPanel c = new GButt.ButtPanel(pp.getIcon())
                {
                    protected override void clickA()
                    {
                        a.exe();
                    }
                };

                c.setDim(60, 40);

                CLICKABLE cc = KeyButt.wrap(a, c, KEYS.SETT(), "MOVE_HOMES", pp.name(), pp.desc);
                section.addRightC(8, cc);
            }

            {
                PlacableMulti pp = new RoomUpgrader();
                ACTION a = new ACTION()
                {
                    public override void exe()
                    {
                        VIEW.inters().popup.close();
                        VIEW.s().tools.place(pp);
                    }
                };

                GButt.ButtPanel c = new GButt.ButtPanel(pp.getIcon())
                {
                    protected override void clickA()
                    {
                        a.exe();
                    }

                    protected override void renAction()
                    {
                        activeSet(SETT.ROOMS().HOME.reqs.passes(FACTIONS.player()));
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        base.hoverInfoGet(text);
                        text.NL(8);
                        if (!SETT.ROOMS().HOME.reqs.passes(FACTIONS.player()))
                            SETT.ROOMS().HOME.reqs.hover(text, FACTIONS.player());
                    }
                };

                c.setDim(60, 40);

                CLICKABLE cc = KeyButt.wrap(a, c, KEYS.SETT(), "UPGRADE_HOMES", pp.name(), pp.desc);
                section.addRightC(8, cc);
            }

            {
                GButt.ButtPanel c = new GButt.ButtPanel(SPRITES.icons().m.place_brush)
                {
                    protected override void clickA()
                    {
                        overlay = overlay || SETT.OVERLAY().HOMELESS.added();
                        overlay = !overlay;
                        if (!overlay)
                            VIEW.s().overlayThing.set(null);
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        text.title(Dic.¤¤Overlay);
                        text.text(SETT.OVERLAY().HOMELESS.desc);
                    }

                    protected override void renAction()
                    {
                        selectedSet(overlay || SETT.OVERLAY().HOMELESS.added());
                        if (overlay)
                            SETT.OVERLAY().HOMELESS.add();
                    }
                };
                c.setDim(60, 40);
                section.addRightC(8, c);
            }

            titleSet(Dic.¤¤Housing);

            table = new UIHomesTable(ISidePanel.HEIGHT - 400);
            section.addRelBody(8, DIR.S, table);
            section.addRelBody(8, DIR.S, new UIHomesFurniture(300));
        }

        protected override void addAction()
        {
            table.subject = null;
        }

        protected override bool back()
        {
            if (table.subject != null)
            {
                table.subject = null;
                return true;
            }
            return false;
        }
    }
}