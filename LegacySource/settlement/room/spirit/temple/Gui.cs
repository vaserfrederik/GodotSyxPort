using System;
using System.Collections.Generic;
using settlement.room.main;
using settlement.room.spirit.temple;
using settlement.stats;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.room;

namespace settlement.room.spirit.temple
{
    class Gui : UIRoomModuleImp<TempleInstance, ROOM_TEMPLE>
    {
        private static readonly CharSequence ¤¤Respect = "The respect value of this temple. It is a combination of Sacrifices, room layout and priests.";
        private static readonly CharSequence ¤¤Sacrifice = "¤Sacrificing";
        private static readonly CharSequence ¤¤SacrificeD = "¤Sacrificed";
        private static readonly CharSequence ¤¤SacrificeYearD = "¤Sacrificed this year.";
        private static readonly CharSequence ¤¤SacrificingD = "¤How well this temple is sacrificing. In order to sacrifice, the priests must have access to resources. The temple requires {0} sacrifices per day.";
        private static readonly CharSequence ¤¤SacrificingHuman = "¤This temple sacrifices prisoners. Currently there is a supply of {0} prisoners.";
        private static readonly CharSequence ¤¤SacrificingAnimal = "¤This temple sacrifices animals. Livestock is needed";
        private static readonly CharSequence ¤¤SacrificingResource = "This temple sacrifices {0}.";
        private static readonly CharSequence ¤¤NoSacrifices = "No sacrifices are available!";
        private static readonly CharSequence ¤¤PriestsD = "The staffing of this temple. Has an effect on respect. Temples must be fully staffed.";

        static Gui()
        {
            D.ts(typeof(Gui));
        }

        public Gui(ROOM_TEMPLE s) : base(s)
        {
        }

        protected override void appendPanel(GuiSection section, GGrid grid, GETTER<TempleInstance> getter, int x1, int y1)
        {
            blueprint.constructor.decor.appendPanel(section, grid, getter, x1, y1);
            blueprint.constructor.grandure.appendPanel(section, grid, getter, x1, y1);
            blueprint.constructor.space.appendPanel(section, grid, getter, x1, y1);
            grid.add(new GStat
            {
                Update = text =>
                {
                    GFORMAT.perc(text, getter.get().sacrificeValue());
                },
                HoverInfoGet = b =>
                {
                    {
                        GText t = b.text();
                        t.add(¤¤SacrificingD);
                        t.insert(0, getter.get().jobs.size() * getter.get().blueprintI().STIME * 0.5, 2);
                        b.add(t);
                    }
                    b.NL(8);
                    if (blueprint.altar is TempleAltar.Resource)
                    {
                        GText t = b.text();
                        t.add(¤¤SacrificingResource);
                        t.insert(0, blueprint.resource.name);
                        b.add(t);
                    }
                    else if (blueprint.altar is TempleAltar.Animal)
                    {
                        GText t = b.text();
                        t.add(¤¤SacrificingAnimal);
                        t.insert(0, blueprint.resource.name);
                        b.add(t);
                    }
                    else if (blueprint.altar is Prisoner)
                    {
                        GText t = b.text();
                        t.add(¤¤SacrificingHuman);
                        t.insert(0, STATS.POP().pop(HTYPES.PRISONER()));
                        b.add(t);
                    }
                }
            }.hh(¤¤Sacrifice));

            grid.add(new GStat
            {
                Update = text =>
                {
                    double d = (double)getter.get().employees().employed() / getter.get().employees().target();
                    GFORMAT.perc(text, d);
                },
                HoverInfoGet = b =>
                {
                    b.text(¤¤PriestsD);
                    b.NL(8);
                }
            }.hh(blueprint.constructor.priests.name()));

            grid.add(new GStat
            {
                Update = text =>
                {
                    GFORMAT.i(text, getter.get().consumed);
                },
                HoverInfoGet = b =>
                {
                    b.text(¤¤SacrificeYearD);
                    b.NL(8);
                }
            }.hh(¤¤SacrificeD));

            RENDEROBJ rr = new GStat
            {
                Update = text =>
                {
                    GFORMAT.perc(text, getter.get().respect());
                },
                HoverInfoGet = b =>
                {
                    b.text(¤¤Respect);
                    b.NL(8);
                }
            }.hv(STATS.RELIGION().TEMPLE.QUALITY.info().name);

            section.addRelBody(16, DIR.S, rr);
        }

        protected override void appendTableFilters(LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers)
        {
        }

        protected override void hover(GBox box, TempleInstance i)
        {
            base.hover(box, i);
            box.NL(8);
            box.textLL(¤¤Sacrifice);
            box.add(GFORMAT.perc(box.text(), i.sacrificeValue()));
        }

        protected override void problem(TempleInstance i, Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings)
        {
            if (!i.resHas)
            {
                errors.add(¤¤NoSacrifices);
            }
        }
    }
}