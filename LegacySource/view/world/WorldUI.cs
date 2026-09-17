using view.interrupter;
using view.tool;
using view.world.ui.army;
using view.world.ui.battle;
using view.world.ui.camps;
using view.world.ui.faction;
using view.world.ui.region;

namespace view.world
{
    public class WorldUI
    {
        public readonly UIRegions regions;
        public readonly UIArmies armies = new UIArmies();
        public readonly UICampList camps = new UICampList();
        public readonly UIWBattlePrompt battle = new UIWBattlePrompt();
        public readonly UIFactions factions = new UIFactions();

        public WorldUI(InterManager m, ISidePanels panels, ToolManager tools)
        {
            regions = new UIRegions(panels, tools);
        }
    }
}