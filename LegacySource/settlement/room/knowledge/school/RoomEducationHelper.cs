using System;
using System.Collections.Generic;
using game.boosting;
using init.sprite.UI;
using init.type;
using settlement.entity.humanoid;
using settlement.room.industry.module;
using settlement.room.main;
using settlement.stats;
using settlement.stats.colls;
using snake2d.util.datatypes;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.GuiSection;
using snake2d.util.gui.renderable.RENDEROBJ;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.data;
using util.gui.misc;
using util.gui.slider;
using util.gui.table;
using util.info;
using util.text;
using view.sett.ui.room;

namespace settlement.room.knowledge.school
{
    public abstract class RoomEducationHelper
    {
        private static readonly CharSequence ¤¤daysToEducate = "¤Days to Educate";
        private static readonly CharSequence ¤¤boost = "¤Species Boost";
        static
        {
            D.ts(typeof(RoomEducationHelper));
        }

        private readonly Boostable bonus;
        private readonly IndustryRate rate;
        private readonly RoomBlueprintIns blue;

        public RoomEducationHelper(RoomBlueprintIns blue, params RoomBoost[] boosts)
        {
            this.bonus = blue.bonus();
            this.blue = blue;

            RoomBoost deg = new RoomBoost()
            {
                info = new INFO(Dic.¤¤Degrade, Dic.¤¤DegradeDesc),

                Info = () => info,

                Get = (RoomInstance r) => (1.0 - r.GetDegrade())
            };

            this.rate = new IndustryRate()
            {
                boos = new ArrayList<RoomBoost>(boosts).Join(deg),

                Boosts = () => boos,

                Bonus = () => bonus
            };
        }

        public abstract AgeType Type { get; }

        public void AppendView(LISTE<UIRoomModule> mm)
        {
            mm.Add(new UIRoomModule()
            {
                AppendPanel = (GuiSection section, GETTER<RoomInstance> get, int x1, int y1) =>
                {
                    section.AddRelBody(8, DIR.S, new GStat()
                    {
                        Update = (GText text) =>
                        {
                            GFORMAT.f0(text, learningSpeed(get.Get()));
                        },

                        HoverInfoGet = (GBox b) =>
                        {
                            b.Title(bonus.name);
                            b.Text(bonus.desc);
                            b.NL();

                            IndustryUtil.hoverProductionRate(b, 1, rate, get.Get());

                            bonus.hover(b, HCLASS_RACE.clP(), true);
                        }
                    }.Hh(bonus.icon));
                },

                AppendManageScr = (GGrid icons, GGrid text, GuiSection extra) =>
                {
                    LinkedList<RENDEROBJ> rows = new LinkedList<RENDEROBJ>();

                    foreach (HCLASS_RACE rr in HCLASS_RACE.ALL())
                    {
                        if (rr.race == null)
                            continue;
                        if (rr.race.bvalue(bonus) == 0)
                            continue;
                        if (rr.cl == HCLASSES.CITIZEN())
                            rows.Add(new RaceRow(rr, bonus, Type));
                    }

                    text.Add(new GScrollRows(rows, rows.Get(0).Body().Height() * 5).View());
                },

                AppendTableFilters = (LISTE<GTFilter<RoomInstance>> filters, LISTE<GTSort<RoomInstance>> sorts, LISTE<UIRoomBulkApplier> appliers) =>
                {
                },

                AppendButt = (GuiSection s, GETTER<RoomInstance> get) =>
                {
                },

                Hover = (GBox box, Room room, int rx, int ry) =>
                {
                },

                Problem = (Stack<Str> free, LISTE<CharSequence> errors, LISTE<CharSequence> warnings, Room room, int rx, int ry) =>
                {
                }
            });
        }

        private class RaceRow : GuiSection
        {
            public RaceRow(HCLASS_RACE race, Boostable bonus, AgeType type) : base()
            {
                GuiSection s = new GuiSection()
                {
                    HoverInfoGet = (GUI_BOX text) =>
                    {
                        GBox b = (GBox)text;

                        b.textLL(race.race.info.names);
                        b.textL(race.cl.names);
                        b.NL();

                        type.hoverLimit(text, race);
                        b.NL(8);

                        b.textLL(¤¤boost);
                        b.tab(6);
                        double am = race.race.bvalue(bonus);
                        b.Add(GFORMAT.f(b.text(), am));
                        b.NL();

                        b.textLL(¤¤daysToEducate);
                        b.tab(6);
                        am = type.limit(race) / (bonus.get(race) * type.limitSpeed(race));
                        b.Add(GFORMAT.f(b.text(), am));
                        b.NL();
                    }
                };

                s.AddRightC(0, race.race.appearance().icon);
                s.AddRightC(-8, race.cl.iconSmall());
                s.AddRightC(4, new GStat()
                {
                    Update = (GText text) =>
                    {
                        GFORMAT.i(text, (long)Math.Ceiling(type.limit(race) / (bonus.get(race) * type.limitSpeed(race))));
                    }
                });

                INTE ii = new INTE()
                {
                    Min = () => 0,
                    Max = () => StatsEducation.LIMIT_MAX,
                    Get = () => type.limit(race),
                    Set = (int t) => type.limitSet(race, t)
                };

                s.AddRightC(60, new GSliderInt(ii, 120, true, false));
                Add(s);
                body().incrW(16);

                foreach (StatEducation ss in STATS.EDUCATION().all)
                {
                    AddRightC(8, new GButt.ButtPanel(ss.total.info().icon.resized(Icon.S))
                    {
                        RenAction = () =>
                        {
                            selectedSet(STATS.EDUCATION().policy(race) == ss);
                        },

                        ClickA = () =>
                        {
                            STATS.EDUCATION().policySet(race, ss);
                        },

                        HoverInfoGet = (GUI_BOX text) =>
                        {
                            ss.total.hover(text, race.cl, race.race);
                        }
                    });
                }

                pad(4, 1);
            }
        }

        public double learningSpeed(Humanoid student, int tx, int ty)
        {
            RoomInstance ins = blue.Get(tx, ty);
            if (ins == null)
                return 0;
            return learningSpeed(ins, student.indu());
        }

        public double learningSpeed(RoomInstance ins, BOOSTABLE_O h)
        {
            double d = 1.0;
            foreach (RoomBoost rr in rate.boosts())
            {
                d *= rr.Get(ins);
            }

            return bonus.get(h) * d;
        }

        public double learningSpeed(RoomInstance ins)
        {
            double ee = ins.employees().employed();
            if (ee == 0)
                return bonus.get(HCLASS_RACE.clP());
            return IndustryUtil.calcProductionRate(1, rate, ins);
        }
    }
}