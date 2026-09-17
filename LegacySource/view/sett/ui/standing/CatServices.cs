using System;
using System.Collections.Generic;
using Init.Race;
using Init.Sprite.UI;
using Init.Type;
using Settlement.Stats;
using Settlement.Stats.Service;
using Settlement.Stats.Stat;
using Snake2D.Util.Gui;
using Snake2D.Util.Gui.Renderable;
using Snake2D.Util.Sets;
using Util.Colors;
using Util.Data;
using Util.Gui.Misc;
using Util.Gui.Table;
using Util.Text;

namespace View.Sett.Ui.Standing
{
    internal sealed class CatServices : Cat
    {
        private static readonly string ¤¤Other = "¤Other Services";

        static CatServices()
        {
            D.Ts(typeof(CatServices));
        }

        public CatServices(HCLASS cl, Getter<Race> race) : base(new StatCollection[] { STATS.SERVICE() })
        {
            StatsService s = STATS.SERVICE();
            TitleSet(s.Info.Name);
            List<RenderObj> rens = new List<RenderObj>(s.All().Count + 1);

            {
                GuiSection sec = new GuiSection();
                sec.Add(new GHeader(¤¤Other));
                foreach (StatServiceImp ss in s.ALL)
                {
                    bool has = false;
                    foreach (Race r in RACES.All())
                    {
                        if (ss.Total().Standing().Definition(r).Get(cl).Max > 0)
                        {
                            has = true;
                            break;
                        }
                    }
                    has = true;
                    if (!has)
                        continue;

                    rens.Add(new StatRowService(ss, cl, race));
                }
                //rens.Add(Hospital(cl));
            }

            Section.Add(new GScrollRows(rens, HEIGHT, 0).View());
        }

        private sealed class StatRowService : GuiSection
        {
            private readonly HCLASS cl;
            private readonly Getter<Race> race;

            public StatRowService(StatServiceImp g, HCLASS cl, Getter<Race> race)
            {
                this.race = race;
                this.cl = cl;

                Add(Service(g), 20, 0);
                Pad(2, 5);
            }

            public override void Render(SpriteRenderer r, float ds)
            {
                base.Render(r, ds);
                GColor.UI().Border().Render(r, Body().X1(), Body().X2(), Body().Y2() - 1, Body().Y2());
            }

            public override void HoverInfoGet(GuiBox text)
            {
                if (IsHoveringAHoverElement())
                {
                    base.HoverInfoGet(text);
                    return;
                }
                base.HoverInfoGet(text);
            }

            private RenderObj Service(StatServiceImp ss)
            {
                GuiSection s = new GuiSection()
                {
                    public override void HoverInfoGet(GuiBox text)
                    {
                        if (!IsHoveringAHoverElement())
                        {
                            ss.Total().Hover(text, cl, race.Get());
                        }
                        base.HoverInfoGet(text);
                    }
                };
                s.Add(new StatRow.Arrow(ss.Total(), cl, race));

                s.AddRightC(4, new GButt.Checkbox()
                {
                    protected override void ClickA()
                    {
                        ss.Permission().Toggle(cl.Get(race.Get()));
                        base.ClickA();
                    }

                    protected override void RenAction()
                    {
                        SelectedSet(ss.Permission().Is(cl.Get(race.Get())));
                    }

                    public override void HoverInfoGet(GuiBox text)
                    {
                        GBox b = (GBox)text;
                        b.Title(ss.Permission().Info().Name);
                    }
                });

                s.AddRightC(4, ss.Icon);
                s.AddRightC(4, new GText(UI.FONT().S, ss.Name).LablifySub());
                s.AddCentredY(new GStat()
                {
                    public override void Update(GText text)
                    {
                        text.SetFont(UI.FONT().S);
                        StatRow.Format(text, ss.Total(), ss.Total().Data(cl).GetD(race.Get()), cl, race.Get());
                    }
                }, StatRow.StatX - 20);

                s.AddCentredY(new RenderObj.RenderImp(StatRow.MeterW, 12)
                {
                    public override void Render(SpriteRenderer r, float ds)
                    {
                        double max = ss.Total().Standing().Max(cl, race.Get());
                        double now = ss.Total().Standing().Get(cl, race.Get());
                        double nor = ss.Total().Standing().Normalized(cl, race.Get());
                        GMeter.Render(r, GMeter.C_REDGREEN, now / max, Body.X1(), (int)(body().X1() + Body().Width() * nor), Body().Y1(), Body().Y2());
                    }
                }, StatRow.MeterX - 20);

                return s;
            }
        }

        //private RenderObj Hospital(HCLASS cl)
        //{
        //    ROOM_HOSPITAL hh = SETT.ROOMS().HOSPITAL;
        //    SERVICE_PROVIDER ss = hh.Service();

        //    GuiSection s = new GuiSection()
        //    {
        //        public override void HoverInfoGet(GuiBox text)
        //        {
        //            if (!IsHoveringAHoverElement())
        //            {
        //                StatRow.HoverStat(text, ss.Stats().Total(), cl);
        //                GBox b = (GBox)text;

        //                b.NL(8);
        //                b.TextLL(Dic.¤¤Total);
        //                b.Tab(6);
        //                b.Add(GFORMAT.i(b.Text(), hh.Service().Total()));
        //                b.NL();
        //                b.TextLL(Dic.¤¤Available);
        //                b.Tab(6);
        //                b.Add(GFORMAT.i(b.Text(), hh.Service().Available()));

        //                b.NL(8);

        //                StatRow.HoverStanding(b, ss.Stats().Total(), cl);
        //            }
        //            base.HoverInfoGet(text);
        //        }
        //    };
        //    s.Add(new StatRow.Arrow(ss.Stats().Total(), cl));

        //    s.AddRightC(4, new GButt.Checkbox()
        //    {
        //        protected override void ClickA()
        //        {
        //            ss.Stats().Permission().Toggle(cl.Get(race.Get()));
        //            base.ClickA();
        //        }

        //        protected override void RenAction()
        //        {
        //            SelectedSet(ss.Stats().Permission().Is(cl.Get(race.Get())));
        //        }

        //        public override void HoverInfoGet(GuiBox text)
        //        {
        //            GBox b = (GBox)text;
        //            b.Title(ss.Stats().Permission().Info().Name);
        //        }
        //    });

        //    s.AddRightC(4, hh.IconBig());
        //    s.AddRightC(4, new GText(UI.FONT().S, hh.Info.Names).LablifySub());
        //    s.AddCentredY(new GStat()
        //    {
        //        public override void Update(GText text)
        //        {
        //            text.SetFont(UI.FONT().S);
        //            StatRow.Format(text, ss.Stats().Total(), ss.Stats().Total().Data(cl).GetD(race.Get()), cl);
        //        }
        //    }, StatRow.StatX - 20);

        //    s.AddCentredY(new RenderObj.RenderImp(StatRow.MeterW, 16)
        //    {
        //        public override void Render(SpriteRenderer r, float ds)
        //        {
        //            double max = ss.Stats().Total().Standing().Max(cl, race.Get());
        //            double now = ss.Stats().Total().Standing().Get(cl, race.Get());
        //            double nor = ss.Stats().Total().Standing().Normalized(cl, race.Get());
        //            GMeter.Render(r, GMeter.C_REDGREEN, now / max, Body.X1(), (int)(body().X1() + Body().Width() * nor), Body().Y1(), Body().Y2());
        //        }
        //    }, StatRow.MeterX - 20);

        //    return s;
        //}
    }
}