using System;
using game.boosting;
using game.faction;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.interrupter;
using view.main;
using world.map.regions;
using world.region;

public sealed class UIAdminPanel : ISidePanel
{
    public UIAdminPanel(ISidePanels panels)
    {
        titleSet(BOOSTABLES.CIVICS().GOV.name);

        section.addRelBody(2, DIR.S, new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.iofk(text, (int)BOOSTABLES.CIVICS().GOV.get(FACTIONS.player()), (int)BOOSTABLES.CIVICS().GOV.added(FACTIONS.player()));
            }
        }.hv(Dic.¤¤Available));

        section.body().incrH(16);

        GTableBuilder bu = new GTableBuilder()
        {
            public override int nrOFEntries()
            {
                return FACTIONS.player().realm().regions() - 1;
            }
        };

        bu.column(null, 250, new GRowBuilder()
        {
            public override RENDEROBJ build(GETTER<int> ier)
            {
                GuiSection s = new GuiSection()
                {
                    protected override void clickA()
                    {
                        Region r = FACTIONS.player().realm().region(ier.get() + 1);
                        if (r != null)
                        {
                            ISidePanel pp = VIEW.world().UI.regions.get(r);
                            panels.clear();
                            panels.add(this, true);
                            panels.add(pp, false);
                        }
                    }

                    public override void hoverInfoGet(GUI_BOX text)
                    {
                        Region r = FACTIONS.player().realm().region(ier.get() + 1);
                        if (r != null)
                            VIEW.world().UI.regions.hover(r, text);
                    }
                };

                s.add(new GStat()
                {
                    public override void update(GText text)
                    {
                        Region r = FACTIONS.player().realm().region(ier.get() + 1);
                        if (r != null)
                            text.add(r.info.name());
                    }
                }.r(DIR.W));

                s.addRightC(180, new GStat()
                {
                    public override void update(GText text)
                    {
                        Region r = FACTIONS.player().realm().region(ier.get() + 1);
                        if (r != null)
                            GFORMAT.iIncr(text, RD.BUILDINGS().costs.GOV.consumed(r));
                        text.s();
                        text.add(RD.BUILDINGS().costs.GOV.consumed(FACTIONS.player()));
                    }
                });

                s.body().setWidth(250);
                s.pad(0, 6);

                return s;
            }
        });

        section.addRelBody(16, DIR.S, bu.createHeight(HEIGHT - section.body().height() - 32, true));
    }
}