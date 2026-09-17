using System;
using snake2d;
using util.data.GETTER;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.tool;
using world;
using world.map.regions;

namespace view.world.ui.region
{
    final class PlayCapitol : RV
    {
        private GETTER_IMP<Region> g = new GETTER_IMP<>();
        private readonly ISidePanel panel;
        private PlayToolAttack tool;

        public PlayCapitol(ToolManager m, ISidePanels p)
        {
            tool = new PlayToolAttack(m)
            {
                added = () => p.added(panel)
            };

            GuiSection s = new GuiSection
            {
                render = (r, ds) =>
                {
                    WORLD.OVERLAY().hover(g.get());
                    base.render(r, ds);
                }
            };

            s.add(MiscMore.garrison(g, 256));

            panel = new ISidePanel(s);
        }

        public override ISidePanel get(Region reg)
        {
            g.set(reg);
            tool.add(g.get());

            panel.titleSet(Dic.¤¤Capitol);
            return panel;
        }

        public override void hover(GBox box, Region reg)
        {
            PlayHov.hover(reg, box);
        }

        public override bool added(ISidePanels pans, Region reg)
        {
            return pans.added(panel) && reg == g.get();
        }

        public override void hoverGarrison(GBox box, Region reg)
        {
            PlayHov.hover(reg, box);
        }
    }
}