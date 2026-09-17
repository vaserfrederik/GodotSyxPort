using System;
using game.faction;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.data.GETTER;
using util.gui.misc;
using util.gui.panel;
using view.interrupter;
using view.tool;
using world;
using world.map.regions;

namespace view.world.ui.region
{
    sealed class Play : RV
    {
        private GETTER_IMP<Region> g = new GETTER_IMP<Region>();
        private readonly ISidePanel panel;
        private PlayToolAttack tool;
        private Faction ff;
        private readonly PlayMilitary mi;

        public Play(ToolManager m, ISidePanels p)
        {
            tool = new PlayToolAttack(m)
            {
                Added = () => p.Added(panel)
            };

            GuiSection s = new GuiSection()
            {
                Render = (r, ds) =>
                {
                    WORLD.OVERLAY().hover(g.Get());
                    if (ff != g.Get().faction())
                        panel.Last().Remove(panel);
                    base.Render(r, ds);
                }
            };

            int w = 600;
            int sep = 580;
            s.Body().SetWidth(w);

            s.Add(new PlayInfo(g, w), 0, s.GetLastY2());
            s.AddRelBody(0, DIR.S, GFrame.separator(sep));
            mi = new PlayMilitary(g, w);
            s.AddRelBody(0, DIR.S, mi);
            s.AddRelBody(0, DIR.S, GFrame.separator(sep));

            s.AddRelBody(0, DIR.S, new PlayReligion(g, w));
            s.AddRelBody(0, DIR.S, GFrame.separator(sep));

            s.AddRelBody(0, DIR.S, new PlayPop(g, w, (ISidePanel.HEIGHT - s.Body().height()) / 3));
            s.AddRelBody(8, DIR.S, new PlayOutput(g, w));
            s.Add(new PlayBuildings(g, w, ISidePanel.HEIGHT - s.Body().height() - 8), 0, s.GetLastY2() + 8);

            panel = new ISidePanel(s);
        }

        public override ISidePanel Get(Region reg)
        {
            g.Set(reg);
            tool.Add(g.Get());
            ff = reg.faction();
            panel.TitleSet(reg.info.name());
            return panel;
        }

        public override void Hover(GBox box, Region reg)
        {
            PlayHov.hover(reg, box);
        }

        public override bool Added(ISidePanels pans, Region reg)
        {
            return pans.Added(panel) && reg == g.Get();
        }

        public override void HoverGarrison(GBox box, Region reg)
        {
            box.Title(reg.info.name());
            g.Set(reg);
            box.Add(mi);
        }
    }
}