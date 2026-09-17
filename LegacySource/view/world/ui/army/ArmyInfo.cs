using System;
using System.Collections.Generic;
using System.Text;

namespace view.world.ui.army
{
    class ArmyInfo : GuiSection
    {
        private static string ¤¤MoraleDesc = "Morale is gained by keeping the army well supplied and by winning battles. Morale affects your army's performance on the battlefield.";
        private static string ¤¤HealthDesc = "Health is gained by keeping the army well supplied. Poor health will lead to desertion.";

        private static string ¤¤CreditsD = "The amount of money needed to upkeep this army daily.";

        static ArmyInfo()
        {
            D.ts(typeof(ArmyInfo));
        }

        public static GuiSection Info(GETTER<WArmy> army)
        {
            GuiSection ss = new GuiSection();

            ss.Add(new GStat
            {
                Update = (text) =>
                {
                    GFORMAT.I(text, AD.Men(null).Get(army.Get()));
                },
                HoverInfoGet = (b) =>
                {
                    b.Title(Dic.¤¤Soldiers);
                    b.Add(GFORMAT.IofKInv(b.Text(), AD.Men(null).Get(army.Get()), AD.MenTarget(null).Get(army.Get())));
                }
            }.Hh(SPRITES.Icons().s.Human));

            ss.AddRightC(80, new GStat
            {
                Update = (text) =>
                {
                    army.Get().State().Info(army.Get(), text);
                    text.LablifySub();
                }
            }.R(DIR.NW));

            ss.Add(new GStat
            {
                Update = (text) =>
                {
                    GFORMAT.Perc(text, AD.Supplies().Health(army.Get()));
                }
            }.Hh(SPRITES.Icons().s.Pluses).HoverTitleSet(Dic.¤¤Health).HoverInfoSet(¤¤HealthDesc), 0, ss.Body().Y2() + 2);

            ss.AddRightC(80, new GStat
            {
                Update = (text) =>
                {
                    GFORMAT.PercInc(text, AD.Morale(army.Get()), 0);
                },
                HoverInfoGet = (b) =>
                {
                    b.Title(Dic.¤¤Morale);
                    b.Text(¤¤MoraleDesc);
                    b.Sep();
                    BHoverer.HoverDetailed(b, AD.MoraleFactors(), army.Get(), Dic.¤¤Factors, 1, true);
                }
            }.Hh(SPRITES.Icons().s.Standard));

            ss.AddRightC(80, new GStat
            {
                Update = (text) =>
                {
                    GFORMAT.I(text, (int)GAME.Battle().Power.Get(army.Get()));
                    if (S.Get().Developer)
                    {
                        text.S();
                        GFORMAT.I(text, (int)AD.Power().Get(army.Get()));
                    }
                }
            }.Hh(SPRITES.Icons().s.Fist));

            ss.AddRightC(80, new GStat
            {
                Update = (text) =>
                {
                    double needed = 0;
                    double total = 0;
                    foreach (ADSupply s in AD.Supplies().All)
                    {
                        needed += s.Current().Get(army.Get());
                        total += s.TargetAmount(army.Get());
                    }
                    if (total == 0)
                        needed = 1;
                    else
                        needed /= total;
                    GFORMAT.Perc(text, needed);
                }
            }.Hh(SPRITES.Icons().s.Storage).HoverTitleSet(Dic.¤¤Supplies).HoverInfoSet(Dic.¤¤SuppliesD));

            ss.Body().IncrW(64);

            return ss;
        }

        public static GuiSection Supplies(GETTER<WArmy> army)
        {
            GuiSection s = new GuiSection();
            int i = 0;

            foreach (ADSupply su in AD.Supplies().All)
            {
                RENDEROBJ g = Supply(army, su);

                s.Add(g, (i % 4) * (g.Body().Width() + 16), (i / 4) * (g.Body().Height() + 4));
                i++;
            }

            RECTANGLE ee = s.GetLast();

            s.Add(new GStat
            {
                Update = (text) =>
                {
                    GFORMAT.I(text, AD.Supplies().Credits().Get(army.Get()));
                }
            }.Hh(SPRITES.Icons().s.Money.Resized(Icon.M)).HoverInfoSet(¤¤CreditsD), (i % 4) * (ee.Width() + 16), (i / 4) * (ee.Height() + 4));
            i++;

            return s;
        }

        private static RENDEROBJ Supply(GETTER<WArmy> army, ADSupply su)
        {
            int w = 60;
            int h = 14;

            SPRITE s = new SPRITE.Imp(w, h)
            {
                Render = (r, X1, X2, Y1, Y2) =>
                {
                    if (su.TargetAmount(army.Get()) == 0)
                    {
                        GMeter.Render(r, GMeter.C_GREEN_DARK, 0, X1, X2, Y1, Y2);
                        return;
                    }

                    double now = (double)su.MinimumAmount(army.Get()) / su.TargetAmount(army.Get());
                    double needed = (double)su.Current().Get(army.Get()) / su.TargetAmount(army.Get());

                    if (su.Current().Get(army.Get()) >= su.MinimumAmount(army.Get()))
                        GMeter.Render(r, GMeter.C_BLUE, needed, X1, X2, Y1, Y2);
                    else
                        GMeter.Render(r, GMeter.C_REDORANGE, needed, X1, X2, Y1, Y2);

                    X1 += 3 + now * (X2 - X1 - 6);

                    GCOLOR.UI().Border().Render(r, X1 - 1, X1 + 1, Y1, Y2);

                    if (!SETT.ROOMS().Supply.Has(su.Res))
                        UI.Icons().s.Alert.Render(r, X2 - 8, Y1 - 2);
                }
            };

            RENDEROBJ o = new GHeader.HeaderHorizontal(su.Res.Icon(), s)
            {
                HoverInfoGet = (text) =>
                {
                    GBox b = (GBox)text;
                    su.Hover(b, army.Get());
                }
            };

            if (S.Get().Developer)
            {
                GuiSection ss = new GuiSection()
                {
                    Rec = new STRING_RECIEVER
                    {
                        AcceptString = (string str) =>
                        {
                            try
                            {
                                double d = Double.Parse(str);
                                su.Current().Set(army.Get(), (int)(su.TargetAmount(army.Get()) * d));
                            }
                            catch (Exception e)
                            {
                            }
                        }
                    },
                    ClickA = () =>
                    {
                        VIEW.Inters().Input.RequestInput(Rec, "set");
                        base.ClickA();
                    }
                };
                ss.Add(o);
                return ss;
            }
            else
            {
                return o;
            }
        }
    }
}