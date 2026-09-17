using System.Collections.Generic;
using game.faction;
using init.resources;
using snake2d;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using view.main;
using world.map.regions;
using world.region;

namespace view.ui.goods
{
    class Pop : GuiSection
    {
        RESOURCE res;
        private ArrayList<Region> regs = new ArrayList<Region>(128);

        public Pop()
        {
            GTableBuilder bu = new GTableBuilder
            {
                nrOFEntries = () => regs.size()
            };

            bu.column(null, 250, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    GuiSection s = new GuiSection
                    {
                        clickA = () =>
                        {
                            Region r = regs.get(ier.get());
                            if (r != null)
                            {
                                VIEW.UI().manager.close();
                                VIEW.world().activate();
                                VIEW.world().UI.regions.open(r);
                            }
                            base.clickA();
                        }
                    };

                    s.add(new GStat
                    {
                        update = (GText text) =>
                        {
                            Region r = regs.get(ier.get());
                            if (r != null)
                            {
                                text.add(r.info.name());
                            }
                        }
                    }.r());

                    s.addRightC(180, new GStat
                    {
                        update = (GText text) =>
                        {
                            Region r = regs.get(ier.get());
                            if (r != null)
                            {
                                GFORMAT.iIncr(text, RD.OUTPUT().get(res.tr()).getDelivery(r));
                            }
                        }
                    }.r());
                    s.pad(0, 6);

                    return s;
                }
            });

            add(bu.createHeight(400, true));
        }

        public override void render(SPRITE_RENDERER ren, float ds)
        {
            regs.clear();
            for (int i = 0; i < FACTIONS.player().realm().regions(); i++)
            {
                Region r = FACTIONS.player().realm().region(i);
                if (RD.OUTPUT().get(res.tr()).getDelivery(r) > 0)
                {
                    regs.add(r);
                }
            }

            base.render(ren, ds);
        }
    }
}