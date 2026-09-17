using System;
using init.constant;
using init.sprite;
using init.type;
using settlement.room.main;
using settlement.room.main.category;
using settlement.room.main.employment;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using util.gui.misc;
using util.gui.slider;
using util.info;
using util.text;
using view.interrupter;
using view.sett.ui.room;

namespace view.sett.ui.room
{
    public class UIPanelWorkTools : ISidePanel
    {
        private static CharSequence ¤¤level = "Current Available Max Level:";

        static
        {
            D.ts(typeof(UIPanelWorkTools));
        }

        public UIPanelWorkTools(RoomEquip work)
        {
            section.Add(new GStat()
            {
                public override void Update(GText text)
                {
                    GFORMAT.iofkInv(text, work.currentTotal(), work.neededTotal());
                }
            }.Increase().R(DIR.N));

            RENDEROBJ q = new UIPanelUtil.BlueprintList(HEIGHT - section.Body().Y2() - C.SG * 16)
            {
                public override RENDEROBJ Row(RoomBlueprintIns<?> bb)
                {
                    if (bb.Employment() == null || !work.Has(bb.Employment()))
                    {
                        return null;
                    }

                    RoomRow r = new RoomRow(bb)
                    {
                        public override void HoverInfoGet(GUI_BOX text)
                        {
                            base.HoverInfoGet(text);
                            GBox b = (GBox)text;
                            b.NL(8);

                            b.TextL(¤¤level);
                            b.Add(GFORMAT.i(b.Text(), work.Target(bb.Employment()).AvailableMax()));
                            b.NL();
                            if (work.Target(bb.Employment()).Boost() != null)
                            {
                                work.Target(bb.Employment()).Boost().HoverDetailed(b, HCLASS_RACE.clP(), null, true);
                            }
                            b.Sep();

                            b.TextL(Dic.¤¤Boosts);
                            b.NL();
                            work.Boost(bb.Employment()).Booster.Hover(b, (double)work.TargetI(bb.Employment()) / (work.Target(bb.Employment()).Max() * bb.Employment().Employed()));
                            work.Boost(bb.Employment()).Booster.HoverSpan(b, (double)work.TargetI(bb.Employment()) / (work.Target(bb.Employment()).Max() * bb.Employment().Employed()));
                            b.NL();
                        }
                    };

                    r.AddRelBody(8, DIR.E, new GStat()
                    {
                        public override void Update(GText text)
                        {
                            GFORMAT.i(text, work.TargetI(bb.Employment()));
                        }
                    });

                    r.AddRelBody(48, DIR.E, new GAllocator(COLOR.ORANGE100.MakeSaturated(0.7), work.Target(bb.Employment()), 6, 16));
                    r.Body().IncrW(420 - r.Body().Width());
                    r.Pad(16, 0);
                    return r;
                }

                protected override void AddToCat(GuiSection s, RoomCategoryMain cat)
                {
                    RENDEROBJ sss = new GStat()
                    {
                        public override void Update(GText text)
                        {
                            int needed = 0;
                            foreach (RoomBlueprintImp b in cat.All())
                                if (b.Employment() != null)
                                    needed += work.TargetI(b.Employment());

                            GFORMAT.i(text, needed);
                        }
                    }.R(DIR.W);
                    sss.Body().MoveX1(s.Body().X1());
                    sss.Body().MoveY1(s.GetLastY2());
                    s.Add(sss);

                    RENDEROBJ r = new GButt.Glow(SPRITES.icons().s.magnifier)
                    {
                        protected override void ClickA()
                        {
                            for (int bi = 0; bi < cat.All().Count; bi++)
                            {
                                RoomBlueprintImp b = cat.All()[bi];
                                if (b is RoomBlueprintIns<?>)
                                {
                                    RoomBlueprintIns<?> bb = (RoomBlueprintIns<?>)b;
                                    if (bb.Employment() != null)
                                    {
                                        work.Target(bb.Employment()).Inc(1);
                                    }
                                }
                            }
                        }
                    };

                    r.Body().MoveX1(s.Body().X2() + 32);
                    r.Body().MoveCY(s.Body().CY() - 16);
                    s.Add(r);

                    r = new GButt.Glow(SPRITES.icons().s.minifier)
                    {
                        protected override void ClickA()
                        {
                            for (int bi = 0; bi < cat.All().Count; bi++)
                            {
                                RoomBlueprintImp b = cat.All()[bi];
                                if (b is RoomBlueprintIns<?>)
                                {
                                    RoomBlueprintIns<?> bb = (RoomBlueprintIns<?>)b;
                                    if (bb.Employment() != null)
                                    {
                                        work.Target(bb.Employment()).Inc(-1);
                                    }
                                }
                            }
                        }
                    };

                    s.AddDownC(8, r);
                }
            };

            section.AddRelBody(8, DIR.S, q);

            TitleSet(work.Resource.Names);
        }
    }
}