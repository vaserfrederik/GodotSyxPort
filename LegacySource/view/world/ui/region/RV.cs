using util.gui.misc;
using view.interrupter;
using world.map.regions;

namespace view.world.ui.region
{
    interface RV
    {
        public ISidePanel get(Region reg);
        public void hover(GBox box, Region reg);
        public void hoverGarrison(GBox box, Region reg);
        public bool added(ISidePanels pans, Region reg);
    }
}