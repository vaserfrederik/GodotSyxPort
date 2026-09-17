using System;
using game.faction;
using snake2d.util.gui;
using util.gui.misc;
using view.interrupter;
using view.tool;
using world.map.regions;

namespace view.world.ui.region
{
    public sealed class UIRegions
    {
        private readonly Other other;
        public readonly Play player;
        public readonly PlayCapitol cap;

        public readonly ISidePanel playerList;
        public readonly ISidePanel allList;
        private readonly ISidePanels panels;

        public UIRegions(ISidePanels panels, ToolManager tools)
        {
            playerList = new ListPlayer(panels);
            allList = new ListAll();
            player = new Play(tools, panels);
            other = new Other(tools, panels);
            cap = new PlayCapitol(tools, panels);
            this.panels = panels;
        }

        public void Open(Region reg)
        {
            panels.Add(Get(reg), true);
        }

        public ISidePanel Get(Region reg)
        {
            if (reg.Faction() == FACTIONS.Player())
            {
                if (reg.Capitol())
                    return cap.Get(reg);
                return player.Get(reg);
            }
            else
            {
                return other.Get(reg);
            }
        }

        private RV Getp(Region reg)
        {
            if (reg.Faction() == FACTIONS.Player())
            {
                if (reg.Capitol())
                    return cap;
                return player;
            }
            else
            {
                return other;
            }
        }

        public void Hover(Region reg, GUI_BOX b)
        {
            Getp(reg).Hover((GBox)b, reg);
        }

        public void HoverGarrison(Region reg, GUI_BOX b)
        {
            Getp(reg).HoverGarrison((GBox)b, reg);
        }

        public void Open(Region reg, bool disturb)
        {
            panels.Add(Get(reg), disturb);
        }

        public void OpenPlayerList()
        {
            panels.Add(playerList, true);
        }

        public void OpenOtherList()
        {
            panels.Add(allList, true);
        }

        public bool Active(Region reg)
        {
            return cap.Added(panels, reg) || player.Added(panels, reg) || other.Added(panels, reg);
        }
    }
}