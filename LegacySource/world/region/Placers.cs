using System;
using System.Collections.Generic;
using game.faction;
using game.faction.npc;
using init.sprite.UI;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.clickable;
using snake2d.util.misc;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.data.GETTER;
using util.data.INT;
using util.gui.common;
using util.gui.misc;
using util.gui.slider;
using view.main;
using view.tool;
using world;
using world.map.regions;
using world.region.pop;

namespace world.region
{
    class Placers : ArrayListGrower<PLACABLE>
    {
        private static readonly long serialVersionUID = 1L;

        public Placers()
        {
            IntImp ii = new IntImp(1, FACTIONS.MAX());
            GSliderInt sl = new GSliderInt(ii, 100, true);
            LinkedList<CLICKABLE> butts = new LinkedList<CLICKABLE>();
            butts.Add(sl);

            PLACABLE undo = new PlacableMulti("remove faction")
            {
                Place = (tx, ty, area, type) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null && reg.faction() != null)
                    {
                        RD.setFaction(reg, null, false);
                    }
                },
                IsPlacable = (tx, ty, area, type) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null && reg.faction() != null)
                    {
                        return null;
                    }
                    return E;
                },
                ExpandsTo = (fromX, fromY, toX, toY) =>
                {
                    Region reg = WORLD.REGIONS().map.get(fromX, fromY);
                    return reg != null && reg.is(toX, toY);
                }
            };

            PLACABLE set = new PlacableMulti("set faction", "", UI.icons().m.flag)
            {
                Place = (tx, ty, area, type) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null)
                    {
                        RD.setFaction(reg, FACTIONS.getByIndex(ii.get()), false);
                    }
                },
                IsPlacable = (tx, ty, area, type) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null)
                    {
                        return null;
                    }
                    return E;
                },
                ExpandsTo = (fromX, fromY, toX, toY) =>
                {
                    Region reg = WORLD.REGIONS().map.get(fromX, fromY);
                    return reg != null && reg.is(toX, toY);
                },
                GetUndo = () => undo,
                GetAdditionalButt = () => butts
            };

            PLACABLE setRace = new PlacableSimpleTile("generate stats race")
            {
                private RDRace race = RD.RACES().all.get(0);
                LinkedList<CLICKABLE> butts = new LinkedList<CLICKABLE>();

                {
                    foreach (RDRace r in RD.RACES().all)
                    {
                        butts.Add(new GButt.ButtPanel(r.race.appearance().icon)
                        {
                            clickA = () => race = r,
                            renAction = () => selectedSet(race == r)
                        }.hoverInfoSet(r.race.info.name));
                    }
                },

                Place = (tx, ty) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null && reg.faction() is FactionNPC)
                    {
                        ((FactionNPC)reg.faction()).generate(race, false);
                    }
                },
                IsPlacable = (tx, ty) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null && reg.faction() is FactionNPC)
                    {
                        return null;
                    }
                    return E;
                },
                GetIcon = () => UI.icons().m.citizen,
                GetAdditionalButt = () => butts
            };

            PLACABLE setName = new PlacableSimpleTile("set faction name")
            {
                Place = (tx, ty) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null && reg.faction() is FactionNPC)
                    {
                        STRING_RECIEVER str = new STRING_RECIEVER
                        {
                            acceptString = (string s) =>
                            {
                                if (s != null)
                                {
                                    reg.faction().name.clear().add(s);
                                    reg.faction().capitolRegion().info.name().clear().add(reg.faction().name);
                                }
                            }
                        };
                        VIEW.inters().input.requestInput(str, "set faction name");
                    }
                },
                IsPlacable = (tx, ty) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null && reg.faction() is FactionNPC)
                    {
                        return null;
                    }
                    return E;
                },
                GetIcon = () => UI.icons().m.menu
            };

            PLACABLE setBanner = new PlacableSimpleTile("set faction visuals")
            {
                GETTER_IMP<FactionNPC> g = new GETTER_IMP<FactionNPC>();
                GuiSection s = new GuiSection();
                BitmapSpriteEditor ee = new BitmapSpriteEditor();

                {
                    s.add(ee);
                    s.addRelBody(8, DIR.S, new GColorPicker(true)
                    {
                        color = () => g.get().banner().colorBG()
                    });
                    s.addRelBody(8, DIR.S, new GColorPicker(true)
                    {
                        color = () => g.get().banner().colorFG()
                    });
                },

                Place = (tx, ty) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null && reg.faction() is FactionNPC)
                    {
                        g.set((FactionNPC)reg.faction());
                        ee.spriteSet(reg.faction().banner().sprite);
                        VIEW.inters().popup.show(s, null);
                    }
                },
                IsPlacable = (tx, ty) =>
                {
                    Region reg = WORLD.REGIONS().map.get(tx, ty);
                    if (reg != null && reg.faction() is FactionNPC)
                    {
                        return null;
                    }
                    return E;
                },
                GetIcon = () => UI.icons().m.flag
            };

            add(set);
            add(undo);
            add(setRace);
            add(setName);
            add(setBanner);
        }
    }
}