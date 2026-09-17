using System.Collections.Generic;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using init.sprite.UI;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.data;
using util.gui.misc;
using util.gui.table;
using view.main;

namespace view.world.ui.faction
{
    sealed class Debug : GButt.ButtPanel
    {
        private readonly GuiSection s = new GuiSection();

        public Debug(GETTER<FactionNPC> g) : base(UI.icons().s.cog)
        {
            LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

            rows.Add(new GButt.ButtPanel("War + 1")
            {
                protected override void clickA()
                {
                    foreach (FactionNPC f in FACTIONS.NPCs())
                    {
                        if (f.isActive() && f != g.get() && !DIP.WAR().is(f, g.get()))
                        {
                            DIP.WAR().set(f, g.get());
                            return;
                        }
                    }
                    base.clickA();
                }
            });

            rows.Add(new GButt.ButtPanel("Peace + 1")
            {
                protected override void clickA()
                {
                    foreach (FactionNPC f in FACTIONS.NPCs())
                    {
                        if (f.isActive() && f != g.get() && DIP.WAR().is(f, g.get()))
                        {
                            DIP.NEUTRAL().set(f, g.get());
                            return;
                        }
                    }
                    base.clickA();
                }
            });

            rows.Add(new GButt.ButtPanel("War player")
            {
                protected override void clickA()
                {
                    if (g.get().isActive())
                        DIP.WAR().set(FACTIONS.player(), g.get());
                }
            });

            s.add(new GScrollRows(rows, 500).view());
        }

        protected override void clickA()
        {
            VIEW.inters().popup.show(s, this);
        }
    }
}