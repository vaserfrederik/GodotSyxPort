using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.faction.npc;
using game.faction.player.emmi;
using game.faction.royalty;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui.GUI_BOX;
using snake2d.util.gui.renderable;
using snake2d.util.sets;
using snake2d.util.sprite;
using util.colors;
using util.data.GETTER;
using util.data.INT;
using util.gui.misc;
using util.gui.slider;
using util.gui.table;
using util.info;
using util.text;
using view.interrupter;
using view.main;
using world;
using world.map.regions;

namespace view.ui
{
    public class UIEmissaries : ISidePanel
    {
        private ArrayList<Data> data = new ArrayList<Data>(512);
        private int tot = 0;

        public UIEmissaries()
        {
            TitleSet(Emissaries.¤¤name);

            while (data.HasRoom())
                data.Add(new Data());

            Section.AddRelBody(8, DIR.N, new GStat()
            {
                public override void Update(GText text)
                {
                    GFORMAT.iIncr(text, (int)BOOSTABLES.CIVICS().DIPLOMACY.Get(FACTIONS.Player()));
                }

                public override void HoverInfoGet(GBox b)
                {
                    BOOSTABLES.CIVICS().DIPLOMACY.Hover(b, FACTIONS.Player(), true);
                }
            }.Hh(BOOSTABLES.CIVICS().DIPLOMACY.icon, BOOSTABLES.CIVICS().DIPLOMACY.name, 180));

            Section.AddRelBody(8, DIR.N, new GStat()
            {
                public override void Update(GText text)
                {
                    GFORMAT.Perc(text, FACTIONS.Player().emissaries.PenaltyMul());
                }
            }.Hh(Dic.¤¤Efficiency, 250));

            GTableBuilder bu = new GTableBuilder()
            {
                public override int NROFEntries()
                {
                    if (MButt.LEFT.IsDown() && tot > 0)
                        return tot;

                    tot = 0;
                    foreach (Region reg in WORLD.REGIONS().Active())
                    {
                        if (tot > data.Size)
                            break;
                        foreach (EmiTypeReg t in FACTIONS.Player().emissaries.regs)
                        {
                            if (t.Get(reg) > 0)
                            {
                                data.Get(tot).reg = reg;
                                data.Get(tot).t = t;
                                data.Get(tot).treg = t;
                                tot++;
                            }
                        }
                    }
                    foreach (FactionNPC f in FACTIONS.NPCs())
                    {
                        if (tot > data.Size)
                            break;
                        foreach (EmiTypeRoy t in FACTIONS.Player().emissaries.roys)
                        {
                            foreach (Royalty r in f.Court().All())
                            {
                                if (t.Get(r) > 0)
                                {
                                    if (tot > data.Size)
                                        break;
                                    data.Get(tot).roy = r;
                                    data.Get(tot).t = t;
                                    data.Get(tot).troy = t;
                                    tot++;
                                }
                            }
                        }
                    }

                    return tot;
                }
            };

            bu.Column(null, new But(new GETTER.GETTER_IMP<int>(0)).Body().Width(), new GRowBuilder()
            {
                public override RENDEROBJ Build(GETTER<int> ier)
                {
                    return new But(ier);
                }
            });

            Section.AddRelBody(8, DIR.S, bu.CreateHeight(HEIGHT - Section.Body().Height() - 32, false));
        }

        private class But : GButt.BSection
        {
            private readonly GETTER<int> ier;
            private readonly GSliderInt sl;

            public But(GETTER<int> ier)
            {
                this.ier = ier;

                Add(new GStat()
                {
                    public override void Update(GText text)
                    {
                        text.Lablify();
                        text.Add(D().t.name);
                    }
                }.R(DIR.NW));

                INTE ii = new INTE()
                {
                    public override int Min()
                    {
                        return 0;
                    }

                    public override int Max()
                    {
                        if (D().t == D().treg)
                        {
                            return D().treg.Max(D().reg);
                        }
                        else if (D().t == D().troy)
                        {
                            return D().troy.Max(D().roy);
                        }
                        return 0;
                    }

                    public override int Get()
                    {
                        if (D().t == D().treg)
                        {
                            return D().treg.Get(D().reg);
                        }
                        else if (D().t == D().troy)
                        {
                            return D().troy.Get(D().roy);
                        }
                        return 0;
                    }

                    public override void Set(int t)
                    {
                        if (t == 0)
                        {
                            sl.Reset();
                        }
                        if (D().t == D().treg)
                        {
                            D().treg.Set(D().reg, t);
                        }
                        else if (D().t == D().troy)
                        {
                            D().troy.Set(D().roy, t);
                        }
                    }
                };

                sl = new GSliderInt(ii, 160, true, true);
                AddRightCAbs(120, sl);

                Add(new GStat()
                {
                    public override void Update(GText text)
                    {
                        if (D().t == D().treg)
                        {
                            text.Color(GCOLOR.T().Faction(D().reg.Faction()));
                            text.Add(D().reg.Info.Name());
                        }
                        else if (D().t == D().troy)
                        {
                            D().roy.NameSucc(text);
                        }
                    }
                }.R(DIR.NW), 0, Body().Y2() + 4);

                AddRelBody(8, DIR.W, new SPRITE.Imp(Icon.S * 2)
                {
                    public override void Render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
                    {
                        D().t.icon.Render(r, X1, X2, Y1, Y2);
                    }
                });

                Pad(6);
            }

            public override void HoverInfoGet(GUI_BOX text)
            {
                base.HoverInfoGet(text);
                if (D().t == D().treg)
                {
                    D().treg.Hover(D().reg, text);
                    text.NL(8);
                    VIEW.World().UI.Regions.Hover(D().reg, text);
                }
                else if (D().t == D().troy)
                {
                    D().troy.Hover(D().roy, text);
                    text.NL(8);
                    VIEW.World().UI.Factions.Hover(text, D().roy.Court.Faction);
                }
            }

            protected override void ClickA()
            {
                if (D().t == D().treg)
                {
                    VIEW.World().Activate();
                    VIEW.World().Panels.AddDontRemove(UIEmissaries.this, VIEW.World().UI.Regions.Get(D().reg));
                }
                else if (D().t == D().troy)
                {
                    VIEW.World().UI.Factions.Open((FactionNPC)D().roy.Court.Faction);
                }
            }

            private Data D()
            {
                return data.Get(ier.Get());
            }
        }

        private class Data
        {
            public EmiType<?> t;
            public EmiTypeRoy troy;
            public EmiType<Region> treg;
            public Royalty roy;
            public Region reg;
        }
    }
}