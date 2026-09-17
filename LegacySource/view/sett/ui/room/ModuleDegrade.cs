using System;
using System.Collections.Generic;
using game.faction;
using game.time;
using init.resources;
using init.settings;
using init.sprite;
using settlement.environment;
using settlement.main;
using settlement.maintenance;
using settlement.room.main;
using settlement.room.main.placement;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.room.Modules;

namespace view.sett.ui.room
{
    final class ModuleDegrade : ModuleMaker
    {
        private static readonly CharSequence ¤¤DEGRADE_AVE = "¤Average degradation amongst these rooms. Degradation affects a room negatively.";
        private static readonly CharSequence ¤¤RoomType = "¤Room Type";
        private static readonly CharSequence ¤¤Lock = "¤The technology for the room is locked, and it can't be maintained.";
        private static readonly CharSequence ¤¤badIsolation = "¤Room is poorly insulated!";
        private static readonly CharSequence ¤¤cost = "¤Support";
        private static readonly CharSequence ¤¤maintenance = "¤Required Maintenance (year)";

        static
        {
            D.ts(typeof(ModuleDegrade));
        }

        public ModuleDegrade(Init init)
        {
        }

        public void make(RoomBlueprint p, LISTE<UIRoomModule> l)
        {
            if (p is RoomBlueprintIns<?>)
            {
                RoomBlueprintIns<?> pp = (RoomBlueprintIns<?>)p;
                if (pp.degrades())
                {
                    l.add(new Hover());
                    l.add(new I(pp));
                }
            }
        }

        private sealed class Hover : UIRoomModule
        {
            public void hover(GBox box, Room room, int rx, int ry)
            {
                if (room.degrader(rx, ry) != null)
                {
                    box.text(Dic.¤¤Degrade);
                    box.add(GFORMAT.percInv(box.text(), room.getDegrade(rx, ry)));
                    box.add(UI.icons().s.hammer);
                    box.add(GFORMAT.i(box.text(), room.degrader(rx, ry).jobs()));
                    box.NL(2);
                    if (!room.constructor().blue().reqs.passes(FACTIONS.player()))
                        box.error(¤¤Lock);
                    else if (room.upgrade(rx, ry) > 0 && !room.constructor().blue().upgrades().requires(room.upgrade(rx, ry)).passes(FACTIONS.player()))
                        box.error(¤¤Lock);
                    box.NL(2);
                }
                if (room.constructor() != null && room.constructor().needsIsolation())
                {
                    box.text(SETT.ROOMS().isolation.info.name);
                    box.add(GFORMAT.perc(box.text(), room.isolation(rx, ry)));
                    box.NL(2);
                }
            }

            public void problem(Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings, Room room, int rx, int ry)
            {
                if (room.getDegrade(rx, ry) > 0.25)
                {
                    errors.add(Dic.¤¤Degrade);
                }
                if (room.constructor() != null && room.constructor().mustBeIndoors() && room.isolation(rx, ry) < 1.0)
                {
                    warnings.add(¤¤badIsolation);
                }
            }
        }

        private sealed class I : UIRoomModule
        {
            private readonly RoomBlueprintIns<?> blue;

            public I(RoomBlueprintIns<?> blue)
            {
                this.blue = blue;
            }

            public void appendManageScr(GGrid grid, GGrid text, GuiSection extra)
            {
                grid.add(new GStat()
                {
                    public void update(GText text)
                    {
                        GFORMAT.percInv(text, (blue.degradeAverage()));
                    }
                }.hh(SPRITES.icons().s.degrade).hoverInfoSet(¤¤DEGRADE_AVE));
            }

            public void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
            {
                sorts.add(new GTSort<RoomInstance>(Dic.¤¤Degrade)
                {
                    public int cmp(RoomInstance current, RoomInstance cmp)
                    {
                        return Double.Compare(current.getDegrade(), cmp.getDegrade());
                    }

                    public void format(RoomInstance h, GText text)
                    {
                        GFORMAT.perc(text, h.getDegrade());
                    }
                });

                sorts.add(new GTSort<RoomInstance>(SETT.ROOMS().isolation.info.name)
                {
                    public int cmp(RoomInstance current, RoomInstance cmp)
                    {
                        return Double.Compare(current.isolation(current.mX(), current.mY()), cmp.isolation(cmp.mX(), cmp.mY()));
                    }

                    public void format(RoomInstance h, GText text)
                    {
                        GFORMAT.perc(text, h.isolation(h.mX(), h.mY()));
                    }
                });
            }

            public void appendPanelIcon(LISTE<RENDEROBJ> section, GETTER<RoomInstance> get)
            {
                GStat s = new GStat()
                {
                    public void update(GText text)
                    {
                        GFORMAT.percInv(text, get.get().getDegrade());
                    }

                    public void hoverInfoGet(GBox b)
                    {
                        b.title(Dic.¤¤Degrade);
                        b.text(Dic.¤¤DegradeDesc);
                        b.NL();
                        b.textLL(Dic.¤¤Current);
                        b.tab(6);
                        b.add(GFORMAT.percInv(b.text(), get.get().getDegrade()));

                        if (S.get().developer)
                        {
                            b.add(GFORMAT.f(b.text(), get.get().degrader(get.get().mX(), get.get().mX()).getSecret(), 4));
                        }

                        b.NL(4);

                        if (!get.get().constructor().blue().reqs.passes(FACTIONS.player()))
                            b.error(¤¤Lock);
                        else if (get.get().upgrade() > 0 && !get.get().constructor().blue().upgrades().requires(get.get().upgrade()).passes(FACTIONS.player()))
                            b.error(¤¤Lock);

                        b.sep();
                        b.NL(8);

                        RoomInstance ins = get.get();
                        ROOM_DEGRADER deg = get.get().degrader(get.get().mX(), get.get().mY());
                        int area = ins.area();
                        double iso = ins.isolation(get.get().mX(), get.get().mY());
                        double boost = SETT.MAINTENANCE().speed();

                        double ram = 0;
                        for (int i = 0; i < deg.resSize(); i++)
                        {
                            ram += deg.resAmount(i);
                        }

                        b.textLL(¤¤maintenance);
                        b.NL();

                        col(b, Dic.¤¤Base, '*', SETT.MAINTENANCE().tilesPerDay * TIME.years().bitConversion(TIME.days()));
                        col(b, ¤¤RoomType, '*', deg.base());
                        col(b, Dic.¤¤Area, '*', area);
                        if (blue.constructor().isHeavy())
                        {
                            col(b, Foundation.¤¤name, '*', 1.0 + UtilExtraCost.foundation(get.get(), get.get().mX(), get.get().mY()));
                            col(b, ¤¤cost, '*', deg.expenseRate() / (1.0 + UtilExtraCost.foundation(get.get(), get.get().mX(), get.get().mY())));
                        }
                        else if (blue.constructor().mustBeIndoors() && blue.constructor().usesArea())
                            col(b, ¤¤cost, '*', deg.expenseRate());

                        col(b, SETT.ROOMS().isolation.info.name, '*', 1 + (1 - iso) * 2);
                        col(b, Dic.¤¤Boosts, '*', boost);

                        double tot = ROOM_DEGRADER.rate(boost, deg.base(), iso, ram, ins.area()) * TIME.years().bitConversion(TIME.days());
                        double resr = ROOM_DEGRADER.rateResource(boost, deg.base(), iso, deg.resAmount(i)) * TIME.years().bitConversion(TIME.days());

                        b.sep();

                        for (int i = 0; i < deg.resSize(); i++)
                        {
                            if (deg.resAmount(i) <= 0)
                                continue;
                            RESOURCE res = deg.res(i);
                            b.add(res.icon());
                            b.textL(res.name);
                            b.tab(6);
                            b.add(GFORMAT.f0(b.text(), -ROOM_DEGRADER.rateResource(boost, deg.base(), iso, deg.resAmount(i)) * TIME.years().bitConversion(TIME.days())));
                            b.NL();
                        }

                        b.sep();

                        b.NL(8);
                        b.textLL(Dic.¤¤Total);
                        b.tab(6);
                        b.add(GFORMAT.f(b.text(), tot, 2));
                    }
                };

                section.add(new GHeader.HeaderHorizontal(SPRITES.icons().s.degrade, s));
            }

            private void col(GBox b, CharSequence header, char append, double value)
            {
                b.textLL(header);
                b.tab(6);
                GText t = b.text();
                t.add(append);
                b.add(GFORMAT.f(t, value, 2));
                b.NL();
            }
        }
    }
}