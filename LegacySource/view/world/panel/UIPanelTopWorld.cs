using System;
using System.Collections.Generic;
using game.GAME;
using game.faction.FACTIONS;
using game.faction.diplomacy.DIP;
using game.faction.diplomacy.DipStance;
using game.faction.npc.FactionNPC;
using game.faction.player.emmi.Emissaries;
using game.faction.royalty.opinion.ROPINION;
using init.settings.S;
using init.sprite.SPRITES;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.clickable.CLICKABLE;
using snake2d.util.gui.renderable.RENDEROBJ;
using snake2d.util.misc.CLAMP;
using snake2d.util.sprite.SPRITE;
using util.gui.misc.GBox;
using util.gui.misc.GButt;
using util.text.Dic;
using view.main.VIEW;
using view.ui.UIEmissaries;
using view.ui.top.UIPanelTop;
using view.ui.top.UIPanelTopButtL;
using view.ui.top.UIPanelTopButtS;
using view.world.WorldView;
using view.world.ui.panels.UIAdminPanel;
using view.world.ui.panels.UICaravanList;
using world.WORLD;
using world.army.AD;
using world.entity.haven.WHavenType;
using world.map.regions.Region;
using world.region.RD;
using world.region.building.RDBuildPoints.RDBuildPoint;
using world.region.pop.RDRace;

namespace view.world.panel
{
    public class UIPanelTopWorld
    {
        private int bi = 0;

        public UIPanelTopWorld(WorldView w, UIPanelTop top)
        {
            CLICKABLE b;

            GuiSection bigButts = new GuiSection();

            GuiSection butts = new GuiSection();

            b = new UIPanelTopButtL(SPRITES.icons().s.flag)
            {
                private GAME.Cache cache = new GAME.Cache(60);
                private int neighs;
                private double trust;

                protected override double valueNext()
                {
                    cache();
                    return trust;
                }

                protected override double value()
                {
                    cache();
                    return trust;
                }

                private void cache()
                {
                    if (!cache.shouldAndReset())
                        return;

                    trust = 10.0;
                    neighs = 0;

                    for (int fi = 0; fi < FACTIONS.NPCs().size(); fi++)
                    {
                        FactionNPC f = FACTIONS.NPCs().get(fi);
                        if (RD.DIST().factionHasRegionBorderingPlayer(f))
                        {
                            trust = Math.Min(ROPINION.trust().get(f), trust);
                            neighs++;
                        }
                    }
                }

                protected override bool isActive()
                {
                    return true;
                }

                protected override int getNumber()
                {
                    cache();
                    return neighs;
                }

                protected override void renAction()
                {
                    selectedSet(w.UI.factions.openIs());
                }

                protected override void clickA()
                {
                    w.UI.factions.open(null);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    b.addText(Dic.¤¤CapitolYou);
                    b.addText(Dic.¤¤desc);
                }
            };

            add(bigButts, b, "flag");

            b = new UIPanelTopButtL(SPRITES.icons().s.capitol)
            {
                private RDBuildPoint b = RD.BUILDINGS().costs.GOV;

                protected override double valueNext()
                {
                    return value();
                }

                protected override double value()
                {
                    return (double)(b.bo.get(FACTIONS.player()) + 1.0) / (b.bo.added(FACTIONS.player()) + 1.0);
                }

                protected override bool isActive()
                {
                    return b.consumed(FACTIONS.player()) > 0;
                }

                protected override int getNumber()
                {
                    return (int)b.bo.get(FACTIONS.player());
                }

                protected override void renAction()
                {
                    selectedSet(VIEW.world().panels.added(li));
                }

                protected override void clickA()
                {
                    if (VIEW.world().panels.added(li))
                        VIEW.world().panels.remove(li);
                    else
                        VIEW.world().panels.add(li, true);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox bb = (GBox)text;
                    bb.addText(b.bo.name);
                    bb.addText(b.bo.desc);
                }
            };

            add(bigButts, b, "capitol");

            b = new UIPanelTopButtS(SPRITES.icons().s.flags)
            {
                private UIEmissaries li = new UIEmissaries();

                protected override double valueNext()
                {
                    return value();
                }

                protected override double value()
                {
                    int v = FACTIONS.player().emissaries.spent();
                    if (v == 0)
                        return 1;
                    return (double)CLAMP.d(FACTIONS.player().emissaries.penaltyMul(), 0, 1);
                }

                protected override bool isActive()
                {
                    return FACTIONS.player().emissaries.spent() > 0 || FACTIONS.player().emissaries.produced() > 0;
                }

                protected override int getNumber()
                {
                    return FACTIONS.player().emissaries.available();
                }

                protected override void renAction()
                {
                    selectedSet(VIEW.world().panels.added(li));
                }

                protected override void clickA()
                {
                    if (VIEW.world().panels.added(li))
                        VIEW.world().panels.remove(li);
                    else
                        VIEW.world().panels.add(li, true);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox b = (GBox)text;
                    b.addText(Emissaries.¤¤name);
                    b.addText(Emissaries.¤¤desc);
                }
            };

            add(butts, b, "flags");

            b = new UIPanelTopButtS(SPRITES.icons().s.capitol)
            {
                private UIAdminPanel li = new UIAdminPanel(w.panels);
                private RDBuildPoint b = RD.BUILDINGS().costs.GOV;

                protected override double valueNext()
                {
                    return value();
                }

                protected override double value()
                {
                    return (double)(b.bo.get(FACTIONS.player()) + 1.0) / (b.bo.added(FACTIONS.player()) + 1.0);
                }

                protected override bool isActive()
                {
                    return b.consumed(FACTIONS.player()) > 0;
                }

                protected override int getNumber()
                {
                    return (int)b.bo.get(FACTIONS.player());
                }

                protected override void renAction()
                {
                    selectedSet(VIEW.world().panels.added(li));
                }

                protected override void clickA()
                {
                    if (VIEW.world().panels.added(li))
                        VIEW.world().panels.remove(li);
                    else
                        VIEW.world().panels.add(li, true);
                }

                public override void hoverInfoGet(GUI_BOX text)
                {
                    GBox bb = (GBox)text;
                    bb.addText(b.bo.name);
                    bb.addText(b.bo.desc);
                }
            };

            add(butts, b, "capitol");

            GuiSection s = new GuiSection();
            s.add(bigButts);
            s.addRightC(0, butts);

            top.addLeft(s);

            s = new GuiSection();
            s.addRightC(0, GAME.EVENT().butt());
            s.addRightC(0, UIPanelTop.junk());
            s.addRightC(0, UIPanelTop.messages());
            s.addRightC(0, UIPanelTop.vToggle());
            s.addRightC(0, UIPanelTop.wLog());
            top.addRightRight(s);

            new UIMinimap(top, w.uiManager, w.window, null);
        }

        private void add(GuiSection bb, RENDEROBJ o, string key)
        {
            bb.add(o, (bi / 2) * 80, (bi % 2) * 24);
            bi++;
        }

        private void addB(GuiSection bb, RENDEROBJ o, string key)
        {
            bb.addRightC(0, o);
        }

        private static class Buttt : GButt.ButtPanel
        {
            public Buttt(SPRITE label) : base(label)
            {
                setDim(32, 48);
            }
        }
    }
}