using System;
using System.Collections.Generic;
using System.Linq;
using game.boosting;
using game.faction;
using game.faction.diplomacy;
using game.faction.npc;
using game.faction.royalty.opinion;
using game.faction.trade;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.renderable;
using snake2d.util.misc;
using snake2d.util.sprite;
using util.data;
using util.gui.misc;
using util.info;
using world.region;
using world.region.pop;

class Hoverer
{
    GuiSection s = new GuiSection();
    private readonly SPRITE ss;
    private FactionNPC f;

    private static readonly CharSequence ¤¤powerBalance = "Power Balance compared to you";
    private static readonly CharSequence ¤¤powerD = "The military might of this nation. High powered factions are harder to please.";

    static Hoverer()
    {
        D.ts(typeof(Hoverer));
    }

    public Hoverer()
    {
        s.Add(new GStat(UI.FONT().S)
        {
            public override void update(GText text)
            {
                text.lablifySub().Add(f.nameIntro);
            }
        }.r(DIR.N), 0, 0);

        s.AddDownC(2, new GStat(UI.FONT().H2)
        {
            public override void update(GText text)
            {
                text.lablify().Add(f.name);
            }
        }.r(DIR.N));

        s.AddRelBody(110, DIR.W, new SPRITE.Imp(Icon.L)
        {
            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                f.banner().BIG.render(r, X1, X2, Y1, Y2);
            }
        });

        s.AddRelBody(110, DIR.E, new SPRITE.Imp(Icon.L)
        {
            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                f.banner().BIG.render(r, X1, X2, Y1, Y2);
            }
        });

        s.AddRelBody(4, DIR.S, new GStat()
        {
            public override void update(GText text)
            {
                if (!RD.DIST().reachable(f))
                {
                    if (RD.DIST().factionCanAttackPlayerAllies(f))
                        text.Add(Dic.¤¤FactionBorder);
                    else
                        text.Add(Dic.¤¤Distant);
                }
                else
                    text.Add(DIP.get(f).name);
            }
        }.r(DIR.N));

        GETTER<FactionNPC> g = new GETTER<FactionNPC>()
        {
            public FactionNPC get()
            {
                return f;
            }
        };

        s.Add(facts(g, 3, 100), s.body().x1(), s.body().y2());

        ss = s.asSprite();
    }

    static GuiSection facts(GETTER<FactionNPC> f, int cols, int M)
    {
        GuiSection s = new GuiSection()
        {
            public override void render(SPRITE_RENDERER r, float ds)
            {
                if (f.get() == null)
                    return;
                base.render(r, ds);
            }

            public override void hoverInfoGet(GUI_BOX text)
            {
                if (f.get() == null)
                    return;
                base.hoverInfoGet(text);
            }
        };

        int i = 0;

        {
            GuiSection ss = new GuiSection()
            {
                public override void hoverInfoGet(GUI_BOX text)
                {
                    if (f.get() == null || f.get().court().king() == null)
                        return;
                    text.title(f.get().court().king().roy().induvidual.race().info.namePosessive);
                }
            };
            ss.Add(UI.icons().s.crown, 0, 0);
            ss.AddRightC(4, new RENDEROBJ.RenderImp(Icon.M)
            {
                public override void render(SPRITE_RENDERER r, float ds)
                {
                    f.get().court().king().roy().induvidual.race().appearance().icon.render(r, body);
                }
            });

            s.AddGridD(ss, i++, cols, M, 20, DIR.W);
        }

        s.AddGridD(new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.f0(text, (int)(100 * ROPINION.get(f.get())) / 100.0);
            }

            public override void hoverInfoGet(GBox b)
            {
                b.title(ROPINION.¤¤name);
                b.text(ROPINION.¤¤desc);
                b.sep();
                ROPINION.BOOST().hoverDetailed(b, f.get().court().king().roy());
            }

        }.hh(ROPINION.BOOST().bo.icon), i++, cols, M, 20, DIR.W);

        s.AddGridD(new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.perc100(text, ROPINION.trust().get(f.get()), 0);
            }

            public override void hoverInfoGet(GBox b)
            {
                b.title(ROPINION.trust().bo.name);
                b.text(ROPINION.trust().bo.desc);
                b.sep();
                RTrust.BOOST().hoverDetailed(b, f.get().court().king().roy());
            }

        }.hh(RTrust.BOOST().bo.icon), i++, cols, M, 20, DIR.W);

        s.AddGridD(new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.i(text, RD.DIST().distance(f.get()));
            }

            public override void hoverInfoGet(GBox b)
            {
                b.add(RD.DIST().distance().info());
                b.NL();

                if (f.get() == null)
                    return;

                b.textLL(Dic.¤¤Toll);
                b.tab(6);
                b.add(GFORMAT.f(b.text(), TradeManager.toll(f.get())));
                b.NL();

                b.textLL(Dic.¤¤Tariff);
                b.tab(6);
                b.add(GFORMAT.f(b.text(), ROPINION.tradeCost(f.get())));
                b.NL();
            }

        }.hh(Icon.S), i++, cols, M, 20, DIR.W);

        s.AddGridD(new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.i(text, FACTIONS.CIVICS().DIPLOMACY.icon);
            }

            public override void hoverInfoGet(GBox b)
            {
                b.title(ROPINION.¤¤wEmmi);
            }

        }.hh(BOOSTABLES.CIVICS().DIPLOMACY.icon), i++, cols, M, 20, DIR.W);

        s.AddGridD(new GStat()
        {
            public override void update(GText text)
            {
                GFORMAT.i(text, RD.RACES().population.faction().get(f.get()));
            }

            public override void hoverInfoGet(GBox b)
            {
                b.title(Dic.¤¤Subject);
                foreach (RDRace rr in RD.RACES().all)
                {
                    b.add(rr.race.appearance().icon);
                    b.text(rr.race.info.names);
                    b.tab(7);
                    b.add(GFORMAT.i(b.text(), rr.pop.faction().get(f.get())));
                    b.NL();
                }
            }

        }.hh(UI.icons().s.human), i++, cols, M, 20, DIR.W);

        SPRITE ss = new SPRITE.Imp(140, Icon.S)
        {
            public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
            {
                int am = DIP.WAR().all(f.get()).size();

                if (am == 0)
                    return;

                int dx = (width() - 24) / am;

                dx = CLAMP.i(dx, 1, 24);

                double x1 = X1;
                foreach (Faction fa in DIP.WAR().all(f.get()))
                {
                    fa.banner().MEDIUM.render(r, (int)x1, Y1);
                    x1 += dx;
                    if (x1 > X2)
                        break;
                }
            }
        };

        s.AddGridD(new GHeader.HeaderHorizontal(UI.icons().s.sword, ss)
        {
            public override void hoverInfoGet(GUI_BOX text)
            {
                GBox b = (GBox)text;
                b.title(Dic.¤¤Enemies);
                foreach (Faction fa in FACTIONS.all())
                {
                    if (fa.isActive() && DIP.WAR().is(fa, f.get()))
                    {
                        b.add(fa.banner().BIG);
                        b.text(fa.name);
                        b.NL();
                    }
                }
            }
        }, i++, cols, M, 20, DIR.W);

        s.body().incrW(Math.Max(M - s.body().width() - 20, 0));
        return s;
    }

    void hover(GUI_BOX box, Faction f)
    {
        GBox b = (GBox)box;

        if (f == null)
        {
            b.title(Dic.¤¤NoRuler);
        }
        else if (f is FactionNPC)
        {
            hoverFF(b, (FactionNPC)f);
        }
        else
        {
            box.title(f.name);
        }
    }

    void hoverFF(GBox b, FactionNPC f)
    {
        this.f = f;
        b.add(ss);
    }
}