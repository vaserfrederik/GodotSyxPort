using System;
using System.Collections.Generic;
using System.Linq;
using game.boosting;
using init.settings;
using init.sprite.UI;
using init.type;
using settlement.entity;
using settlement.main;
using settlement.stats;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using util.colors;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using view.interrupter;
using view.main;

namespace view.sett.ui.health
{
    public class UIHealth : ISidePanel
    {
        private static readonly CharSequence ¤¤Diseases = "Known Diseases";
        private static readonly CharSequence ¤¤Epidemic = "Current Epidemic";

        static
        {
            D.ts(typeof(UIHealth));
        }

        public UIHealth()
        {
            Boostable bo = BOOSTABLES.PHYSICS().HEALTH;
            titleSet(bo.name);

            {
                section.Add(new GStat()
                {
                    public override void Update(GText text)
                    {
                        if (STATS.DISEASE().CurrentEpidemic() == null)
                            text.Add("-").Add("-").Add("-");
                        else
                            text.Add(STATS.DISEASE().CurrentEpidemic().Info.Name);
                    }

                    public override void HoverInfoGet(GBox b)
                    {
                        if (STATS.DISEASE().CurrentEpidemic() != null)
                            STATS.DISEASE().CurrentEpidemic().Hover(b);
                    }
                }.Increase().Hv(¤¤Epidemic));

                GuiSection s = new GuiSection();

                if (S.get().developer)
                    s.Add(stat(STATS.DISEASE().Incubating()), 0, s.Body().Y2() + 6);
                s.Add(stat(STATS.DISEASE().Sick()), 0, s.Body().Y2() + 6);

                s.Add(new GStat()
                {
                    public override void Update(GText text)
                    {
                        GFORMAT.iofk(text, SETT.ROOMS().HOSPITAL.Service().Available(), SETT.ROOMS().HOSPITAL.Service().Total());
                    }
                }.Increase().Hh(SETT.ROOMS().HOSPITAL.Info.Names, 250), 0, s.Body().Y2() + 6);

                s.Add(new GStat()
                {
                    public override void Update(GText text)
                    {
                        GFORMAT.f1(text, bo.Get(HCLASS_RACE.clP()));
                    }

                    public override void HoverInfoGet(GBox b)
                    {
                        bo.HoverDetailed(b, HCLASS_RACE.clP(), null, true);
                    }
                }.Increase().Hh(bo.Name, 250), 0, s.Body().Y2() + 6);

                section.AddRelBody(8, DIR.S, s);
            }

            {
                double m = 0;
                foreach (Booster b in BOOSTABLES.PHYSICS().HEALTH.All())
                {
                    m = Math.Max(m, b.Max());
                }

                List<RENDEROBJ> rows = new List<RENDEROBJ>(BOOSTABLES.PHYSICS().HEALTH.All().Count);

                List<Booster> bb = new List<Booster>(BOOSTABLES.PHYSICS().HEALTH.All());
                bb.Sort((o1, o2) => o1.Max() == o2.Max() ? 0 : o1.Max() > o2.Max() ? -1 : 1);

                foreach (Booster b in bb)
                {
                    GuiSection s = new GuiSection();
                    s.AddRightC(0, b.Info.Icon.Resized(Icon.L));
                    s.AddRightC(8, new GText(UI.FONT().H2, b.Info.Name).SetMaxWidth(200));

                    s.AddRightCAbs(200, new GStat()
                    {
                        public override void Update(GText text)
                        {
                            b.Format(text, b.Get(HCLASS_RACE.clP()));
                        }
                    });
                    rows.Add(s);

                    s.Body().IncrW(100);
                    s.Body().Pad(2, 2);
                }

                section.AddRelBody(8, DIR.S, new GScrollRows(rows, rows[0].Body().Height() * 8).View());
            }

            {
                GuiSection s = new GuiSection();

                GStaples chart = new GStaples(STATS.DAYS_SAVED)
                {
                    protected override double GetValue(int stapleI)
                    {
                        int i = STATS.DAYS_SAVED - stapleI - 1;
                        return STATS.DISEASE().HealthHistory.GetD(i);
                    }

                    protected override void Hover(GBox box, int stapleI)
                    {
                        int i = STATS.DAYS_SAVED - stapleI - 1;
                        box.Title(bo.Name);
                        GText t = box.Text();
                        DicTime.SetDaysAgo(t, i);
                        box.Add(t);
                        box.Tab(6);
                        box.Add(GFORMAT.perc(box.Text(), STATS.DISEASE().HealthHistory.GetD(i)));
                    }

                    protected override void SetColor(ColorImp c, int stapleI, double value)
                    {
                        if (value < 0.5)
                        {
                            c.Interpolate(GCOLOR.UI().SOSO.normal, GCOLOR.UI().BAD.normal, 1.0 - value * 2);
                        }
                        else
                        {
                            c.Interpolate(GCOLOR.UI().SOSO.normal, GCOLOR.UI().GOOD.normal, (value - 0.5) * 2);
                        }
                    }
                };

                chart.Body().SetDim(400, 90);

                s.AddRelBody(4, DIR.S, chart);

                section.AddDown(16, s);
            }

            {
                GuiSection s = new GuiSection();
                s.Add(new GHeader(¤¤Diseases));

                List<RENDEROBJ> rows = new List<RENDEROBJ>();

                foreach (DISEASE d in DISEASES.All())
                {
                    GuiSection ss = new GuiSection();

                    GButt.ButtPanel b = new GButt.ButtPanel(UI.FONT().M.GetText(d.Info.Name))
                    {
                        protected override void ClickA()
                        {
                            if (S.get().developer)
                            {
                                STATS.DISEASE().Outbreak(0.25, d);
                            }
                            base.ClickA();
                        }

                        public override void HoverInfoGet(GUI_BOX text)
                        {
                            d.Hover(text);
                        }
                    };

                    b.Body.SetWidth(280);
                    ss.Add(b);
                    if (S.get().developer)
                    {
                        ss.AddRightC(0, new GButt.ButtPanel(UI.icons().s.death.Resized(10))
                        {
                            protected override void ClickA()
                            {
                                STATS.DISEASE().Outbreak(d.InfectRate, d);
                            }
                        });
                        ss.AddRightC(0, new GButt.ButtPanel(UI.icons().s.death)
                        {
                            protected override void ClickA()
                            {
                                STATS.DISEASE().Outbreak(1, d);
                            }
                        });

                    }

                    rows.Add(ss);
                }

                s.AddDown(2, new GScrollRows(rows, HEIGHT - section.Body().Height() - 80, section.Body().Width()).View());

                section.AddDown(16, s);
            }
        }

        private static RENDEROBJ stat(STAT st)
        {
            GuiSection s = new GuiSection();

            s.Add(new GStat()
            {
                public override void Update(GText text)
                {
                    GFORMAT.i(text, st.Data().Get(null));
                }
            }.Increase().Hh(st.Info().Name, st.Info().Desc, 250));

            s.AddRightC(80, new GButt.ButtPanel(UI.icons().s.crossheir)
            {
                int ie = 0;

                protected override void ClickA()
                {
                    for (int i = 0; i < SETT.ENTITIES().GetAllEnts().Length; i++)
                    {
                        ie++;
                        if (ie >= SETT.ENTITIES().GetAllEnts().Length)
                            ie = 0;
                        ENTITY e = SETT.ENTITIES().GetAllEnts()[ie];
                        if (e != null && e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (st.Indu().Get(a.Indu()) > 0)
                            {
                                VIEW.s().GetWindow().CentererTile.Set(a.Tc());
                                break;
                            }
                        }
                    }
                }

                protected override void RenAction()
                {
                    activeSet(st.Data().Get(null) > 0);
                }
            });

            return s;
        }
    }
}