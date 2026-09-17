using System;
using System.Collections.Generic;
using System.Linq;

namespace View.Sett.Ui.Law
{
    using Init.Type;
    using Settlement.Entity;
    using Settlement.Entity.Humanoid;
    using Settlement.Entity.Humanoid.Ai.Types.Prisoner;
    using Settlement.Main;
    using Settlement.Stats;
    using Snake2D;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.File;
    using Snake2D.Util.Gui;
    using Snake2D.Util.Gui.Renderable;
    using Snake2D.Util.Misc;
    using Snake2D.Util.Sets;
    using Util.Data.Int;
    using Util.Gui.Misc;
    using Util.Gui.Slider;
    using Util.Info;
    using Util.Text;

    class WarCriminals : GuiSection
    {
        private static readonly CharSequence ¤¤apply = "Apply";
        private static readonly CharSequence ¤¤per100 = "({0} increase for 100 captives)";
        private static readonly CharSequence ¤¤selected = "Selected";
        private static readonly CharSequence ¤¤assign = "Manually assign available captives to this punishment.";

        static WarCriminals()
        {
            D.ts(typeof(WarCriminals));
        }

        private int[] punishments;
        private readonly Selector sel;

        public WarCriminals(int HEIGHT)
        {
            punishments = Alloc.Ii(CRIME_PUNISHMENTS.ALL().Size());
            sel = new Selector(HEIGHT, new ArrayList<CRIME>(CRIMES.WAR()));

            Add(new GStat
            {
                Update = text =>
                {
                    GFORMAT.perc(text, STATS.BATTLE().CHIVALRY.Data().GetD(null));
                },
                HoverInfoGet = b =>
                {
                    STATS.BATTLE().CHIVALRY.Hover(b, HCLASSES.CITIZEN(), null);
                }
            }.Increase().Hh(STATS.BATTLE().CHIVALRY.Info().Icon, STATS.BATTLE().CHIVALRY.Info().Name, 180));

            AddDown(8, new GStat
            {
                Update = text =>
                {
                    GFORMAT.perc(text, STATS.BATTLE().CRUELTY.Data().GetD(null));
                },
                HoverInfoGet = b =>
                {
                    STATS.BATTLE().CRUELTY.Hover(b, HCLASSES.CITIZEN(), null);
                }
            }.Increase().Hh(STATS.BATTLE().CRUELTY.Info().Icon, STATS.BATTLE().CRUELTY.Info().Name, 180));

            {
                GuiSection s = new GuiSection();
                foreach (PUNISHMENT p in CRIME_PUNISHMENTS.Get(HCLASSES.OTHER()))
                {
                    s.AddRightC(0, AutoPunish(p));
                }
                AddDownC(8, s);
            }

            foreach (PUNISHMENT p in CRIME_PUNISHMENTS.Get(HCLASSES.OTHER()))
            {
                AddDownC(8, Row(p));
            }

            AddRelBody(8, DIR.W, sel);
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            Array.Fill(punishments, 0);

            foreach (ENTITY e in SETT.ENTITIES().GetAllEnts())
            {
                if (e is Humanoid)
                {
                    Humanoid a = (Humanoid)e;
                    if (a.Indu().HType() == HTYPES.PRISONER() && STATS.LAW().PrisonerType.Get(a.Indu()) == CRIMES.WAR())
                    {
                        if (sel.GetRace() == null || sel.GetRace() == a.Race())
                        {
                            punishments[AIModule_Prisoner.Punishment(a, a.Ai()).Index()]++;
                        }
                    }
                }
            }

            base.Render(r, ds);
        }

        private RENDEROBJ Row(PUNISHMENT punish)
        {
            INTE ii = new INTE
            {
                i = 0,

                Get = () => CLAMP.I(i, 0, Max()),
                Min = () => 0,
                Max = () => CRIMES.WAR().Stat().Criminals(sel.GetRace()) - punishments[punish.Index()],
                Set = t => i = CLAMP.I(t, 0, Max())
            };

            GuiSection ss = new GuiSection
            {
                HoverInfoGet = text =>
                {
                    GBox b = (GBox)text;
                    b.Title(punish.Action);
                    b.Text(¤¤assign);
                    b.NL();

                    b.Add(STATS.BATTLE().CHIVALRY.Info().Icon);
                    b.TextLL(STATS.BATTLE().CHIVALRY.Info().Name);
                    b.Tab(6);
                    b.Add(GFORMAT.Perc(b.Text(), punish.MercyValue(HCLASSES.OTHER(), sel.GetRace())));
                    b.NL();

                    b.Add(STATS.BATTLE().CRUELTY.Info().Icon);
                    b.TextLL(STATS.BATTLE().CRUELTY.Info().Name);
                    b.Tab(6);
                    b.Add(GFORMAT.Perc(b.Text(), punish.CrueltyValue(HCLASSES.OTHER(), sel.GetRace())));
                    b.NL();

                    b.Sep();

                    b.NL(8);
                    b.TextLL(¤¤selected);
                    b.Tab(6);
                    b.Add(GFORMAT.I(b.Text(), ii.Get()));
                    b.NL();

                    b.Add(STATS.BATTLE().CHIVALRY.Info().Icon);
                    b.TextLL(STATS.BATTLE().CHIVALRY.Info().Name);
                    b.Tab(6);
                    b.Text(GFORMAT.F0(b.Text(), ii.Get() * punish.MercyPerPerson(HCLASSES.OTHER(), sel.GetRace())));
                    b.NL();

                    b.Add(STATS.BATTLE().CRUELTY.Info().Icon);
                    b.TextLL(STATS.BATTLE().CRUELTY.Info().Name);
                    b.Tab(6);
                    b.Text(GFORMAT.F0(b.Text(), ii.Get() * punish.CrueltyPerPerson(HCLASSES.OTHER(), sel.GetRace())));
                    b.NL();
                }
            };

            ss.Add(punish.Icon, 0, 0);
            ss.AddRightC(8, new GHeader(punish.Name));

            ss.AddRightCAbs(200, new GSliderInt(ii, 100, true));

            ss.AddRightC(16, new GButt.ButtPanel(¤¤apply)
            {
                RenAction = () => ActiveSet(ii.Get() > 0),
                ClickA = () =>
                {
                    int am = ii.Get();

                    foreach (ENTITY e in SETT.ENTITIES().GetAllEnts())
                    {
                        if (am <= 0)
                            break;
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (a.Indu().HType() == HTYPES.PRISONER() && STATS.LAW().PrisonerType.Get(a.Indu()) == CRIMES.WAR())
                            {
                                if (sel.GetRace() == null || sel.GetRace() == a.Race())
                                {
                                    if (AIModule_Prisoner.DATA().PunishmentSet.Get(a.Ai()) == CRIME_PUNISHMENTS.STOCKS())
                                    {
                                        AIModule_Prisoner.DATA().PunishmentSet.Set(a.Ai(), punish);
                                        am--;
                                    }
                                }
                            }
                        }
                    }

                    foreach (ENTITY e in SETT.ENTITIES().GetAllEnts())
                    {
                        if (am <= 0)
                            break;
                        if (e is Humanoid)
                        {
                            Humanoid a = (Humanoid)e;
                            if (a.Indu().HType() == HTYPES.PRISONER() && STATS.LAW().PrisonerType.Get(a.Indu()) == CRIMES.WAR())
                            {
                                if (sel.GetRace() == null || sel.GetRace() == a.Race())
                                {
                                    if (AIModule_Prisoner.DATA().PunishmentSet.Get(a.Ai()) != CRIME_PUNISHMENTS.STOCKS())
                                    {
                                        AIModule_Prisoner.DATA().PunishmentSet.Set(a.Ai(), punish);
                                        am--;
                                    }
                                }
                            }
                        }
                    }
                }
            });

            return ss;
        }

        private RENDEROBJ AutoPunish(PUNISHMENT punish)
        {
            return new GButt.ButtPanel(punish.Icon.Scaled(2))
            {
                ClickA = () =>
                {
                    STATS.LAW().Crimes.Get(CRIMES.WAR().Index()).PunishmentSet(HCLASSES.OTHER(), sel.GetRace(), punish);
                    base.ClickA();
                },
                RenAction = () => SelectedSet(STATS.LAW().Crimes.Get(CRIMES.WAR().Index()).Punishment(HCLASSES.OTHER(), sel.GetRace()).Punish == punish),
                HoverInfoGet = text =>
                {
                    GBox b = (GBox)text;
                    b.Title(punish.Action);
                    b.Text(punish.Desc);
                    b.Sep();

                    b.Add(STATS.BATTLE().CHIVALRY.Info().Icon);
                    b.TextLL(STATS.BATTLE().CHIVALRY.Info().Name);
                    b.Tab(6);
                    b.Add(GFORMAT.Perc(b.Text(), punish.MercyValue(HCLASSES.OTHER(), sel.GetRace())));
                    b.Tab(8);
                    b.Text(b.Text().Add(¤¤per100).Insert(0, 100 * punish.MercyPerPerson(HCLASSES.OTHER(), sel.GetRace()), 4));
                    b.NL(8);

                    b.Add(STATS.BATTLE().CRUELTY.Info().Icon);
                    b.TextLL(STATS.BATTLE().CRUELTY.Info().Name);
                    b.Tab(6);
                    b.Add(GFORMAT.Perc(b.Text(), punish.CrueltyValue(HCLASSES.OTHER(), sel.GetRace())));
                    b.NL();
                    b.Text(b.Text().Add(¤¤per100).Insert(0, 100 * punish.CrueltyPerPerson(HCLASSES.OTHER(), sel.GetRace()), 4));
                    b.NL();

                    base.HoverInfoGet(text);
                }
            };
        }
    }
}