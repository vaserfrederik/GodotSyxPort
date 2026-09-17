using snake2d.util.gui;
using util.data.GETTER;
using util.data.INT;
using util.gui.slider;
using world.army;

namespace view.battle.editor
{
    public class ArmyArtillery : GuiSection
    {
        public ArmyArtillery(GETTER_IMP<ArmySide> current)
        {
            foreach (ADArtillery a in AD.supplies().arts())
            {
                INTE ii = new INTE()
                {
                    Min = () => 0,
                    Max = () => ADSupplies.artilleryMax,
                    Get = () => current.Get().artillery[a.index()],
                    Set = (t) => current.Get().artillery[a.index()] = t
                };

                GuiSection s = new GuiSection();
                s.hoverInfoSet(a.art.info.names);
                add(a.art.icon, 0, 0);
                addRightC(8, new GTarget(48, true, true, ii));
            }
        }
    }
}