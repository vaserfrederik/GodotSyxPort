using System;
using init.trade;
using snake2d.util.gui;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using world.entity;
using world.entity.army;
using world.entity.caravan;
using world.entity.haven;
using world.map.regions;

namespace view.world.ui
{
    public class WorldHoverer
    {
        private WorldHoverer()
        {
            // TODO Auto-generated constructor stub
        }

        public static void Hover(GUI_BOX box, WEntity e)
        {
            if (e is WArmy)
                VIEW.World().UI.Armies.Hover(box, (WArmy)e);
            else if (e is Shipment)
                Hover(box, (Shipment)e);
            else if (e is WHaven)
            {
                VIEW.World().UI.Camps.Hover(box, (WHaven)e);
            }
        }

        private static void Hover(GUI_BOX box, Shipment e)
        {
            GBox b = (GBox)box;
            b.Title(Dic.¤¤Caravan);
            b.TextL(e.Type().Name);
            b.NL(8);
            Region c = e.Destination();
            if (c == null || c.Faction() == null)
                return;

            GText t = b.Text();
            t.Color(c.Faction().Banner().ColorBG());
            t.Add(Dic.¤¤BoundFor).Insert(0, c.Info.Name());
            box.Add(t);
            box.NL(4);

            int i = 0;
            foreach (TRADABLE r in TR.ALL())
            {
                if (e.LoadGet(r) > 0)
                {
                    box.Add(r.Icon());
                    box.Add(GFORMAT.i(b.Text(), e.LoadGet(r)));
                    i++;
                    if (i > 8)
                    {
                        i = 0;
                        box.NL();
                    }
                }
            }
        }
    }
}