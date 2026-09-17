using System;
using System.Collections.Generic;
using game;
using game.faction;
using game.raiding;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.Hoverable;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.interrupter;
using view.main;
using world.map.regions;
using world.region;
using world.region.RDOutputs;

namespace view.world.ui.region
{
    final class ListPlayer : ISidePanel
    {
        public ListPlayer(ISidePanels panels)
        {
            GTableBuilder bu = new GTableBuilder
            {
                nrOFEntries = () => FACTIONS.player().realm().regions() - 1,
                hoverInfo = (index, box) =>
                {
                    Region reg = FACTIONS.player().realm().region(index + 1);
                    VIEW.world().UI.regions.hover(reg, box);
                },
                click = (index) =>
                {
                    Region reg = FACTIONS.player().realm().region(index + 1);
                    VIEW.world().window.centererTile.set(reg.cx(), reg.cy());
                    ISidePanel p = VIEW.world().UI.regions.get(reg);
                    panels.add(this, true);
                    panels.add(p, false);
                },
                selectedIs = (index) =>
                {
                    Region reg = FACTIONS.player().realm().region(index + 1);
                    return VIEW.world().UI.regions.active(reg);
                }
            };

            HOVERABLE title;

            bu.column(Dic.¤¤name, 120, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    return new GStat(UI.FONT().S)
                    {
                        update = (GText text) =>
                        {
                            text.setMaxWidth(110);
                            text.setMultipleLines(false);
                            text.lablify().add(reg(ier).info.name());
                        }
                    }.r(DIR.NW);
                }
            });

            title = new HOVERABLE.Sprite(UI.icons().s.human).hoverTitleSet(Dic.¤¤Population);
            bu.column(title, 80, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    return new GStat
                    {
                        update = (GText text) =>
                        {
                            GFORMAT.i(text, RD.RACES().population.get(reg(ier)));
                        }
                    }.r(DIR.E);
                }
            }, DIR.E);

            title = new HOVERABLE.Sprite(UI.icons().s.happy).hoverTitleSet(RD.RACES().loyaltyAll.info().name);
            bu.column(title, 48, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    return new GStat
                    {
                        update = (GText text) =>
                        {
                            GFORMAT.perc(text, RD.RACES().loyaltyAll.getD(reg(ier)));
                        }
                    }.r(DIR.E);
                }
            }, DIR.E);

            title = new HOVERABLE.Sprite(UI.icons().s.heart).hoverTitleSet(RD.HEALTH().name);
            bu.column(title, 48, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    return new GStat
                    {
                        update = (GText text) =>
                        {
                            GFORMAT.perc(text, RD.HEALTH().getD(reg(ier)));
                        }
                    }.r(DIR.E);
                }
            }, DIR.E);

            title = new HOVERABLE.Sprite(UI.icons().s.shield).hoverTitleSet(RaidingMap.¤¤Name);
            bu.column(title, 48, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    return new GStat
                    {
                        update = (GText text) =>
                        {
                            GFORMAT.perc(text, CLAMP.d(GAME.raiders().entry.get(reg(ier)).security(), -1, 1));
                        },
                        hoverInfoGet = (GBox b) => { }
                    }.r(DIR.E);
                }
            }, DIR.E);

            title = new HOVERABLE.Sprite(UI.icons().s.arrow_left).hoverTitleSet(RTYPE.TAX.name);
            bu.column(title, Icon.S * 10, new GRowBuilder
            {
                build = (GETTER<int> ier) =>
                {
                    return new RENDEROBJ.RenderImp(120, Icon.L)
                    {
                        render = (SPRITE_RENDERER r, float ds) =>
                        {
                            int i = 0;
                            int dy = 0;
                            foreach (RDOutput res in RD.OUTPUT().ALL)
                            {
                                if (res.getDelivery(reg(ier)) > 0)
                                {
                                    res.boost.icon.render(r, body.x1() + i * Icon.S, body.y1() + dy * Icon.S * dy);
                                    i += dy;
                                    dy += 1;
                                    dy &= 1;
                                    if (i >= 10)
                                        break;
                                }
                            }
                        }
                    };
                }
            }, DIR.E);

            section.add(bu.createHeight(HEIGHT - 64, true));

            section.addDownC(8, new GButt.ButtPanel(Dic.¤¤All)
            {
                clickA = () =>
                {
                    VIEW.world().UI.regions.openOtherList();
                }
            });

            titleSet(Dic.¤¤Realm);
        }

        private static Region reg(GETTER<int> ier)
        {
            return FACTIONS.player().realm().region(ier.get() + 1);
        }
    }
}