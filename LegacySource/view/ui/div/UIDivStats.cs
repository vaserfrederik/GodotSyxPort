using System;
using game.GAME;
using game.battle.div;
using game.battle.util;
using game.boosting;
using game.faction;
using init.constant;
using init.race;
using init.sprite.UI;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using util.gui.misc;
using util.info;
using util.text;

public static class UIDivStats
{
    private static readonly string ¤¤Config = "Configuration";

    static UIDivStats()
    {
        D.ts(typeof(UIDivStats));
    }

    private static int width = 300;
    private static readonly int height = 20;

    private Div sdiv;

    private readonly DIV_SPEC spec = new DIV_SPEC
    {
        Race = () => sdiv.race(),
        Training = tr => tr.stat.div().getD(sdiv),
        Men = () => sdiv.menNrOf(),
        Equip = e => e.stat().div().getD(sdiv),
        Name = () => sdiv.info.name(),
        Faction = () => sdiv.faction(),
        Experience = () => STATS.BATTLE().COMBAT_EXPERIENCE.div().getD(sdiv),
        BannerI = () => sdiv.info.bannerI()
    };

    private DIV_SPEC div;

    private readonly GuiSection ss = new GuiSection();

    public UIDivStats()
    {
        pair(BOOSTABLES.BATTLE().OFFENCE, BOOSTABLES.BATTLE().DEXTERITY, BOOSTABLES.BATTLE().CHARGE, GMeter.C_YELLOW);
        pair(BOOSTABLES.BATTLE().DEFENCE, BOOSTABLES.BATTLE().PARRY, BOOSTABLES.BATTLE().FORMATION, GMeter.C_GREEN);

        ss.body().incrH(4);

        pair(BOOSTABLES.BATTLE().BLUNT_ATTACK, BOOSTABLES.BATTLE().BLUNT_DEFENCE_DIR, BOOSTABLES.BATTLE().BLUNT_DEFENCE);

        foreach (BDamage bb in BOOSTABLES.BATTLE().DAMAGES)
        {
            pair(bb.attack, bb.defenceDir, bb.defence);
        }

        icon(BOOSTABLES.BATTLE().MORALE.icon);
        ss.addRightC(4, new GaugeBo(BOOSTABLES.BATTLE().MORALE, GMeter.C_BLUE, width, height));

        {
            icon(UI.icons().s.bow);

            HoverableAbs aa = new HoverableAbs(width, height)
            {
                Render = (SPRITE_RENDERER r, float ds, bool isHovered) =>
                {
                    double def = 0;
                    double curr = 0;
                    double max = 0;
                    EquipRange rr = best(div);
                    if (rr != null)
                    {
                        def = rr.projectile.range(0, rr.ref(0, 0));
                        curr = rr.projectile.range(0, rr.ref(div.equip(rr), GAME.battle().boost(div, rr.boostable)));
                        foreach (EquipRange e in STATS.EQUIP().RANGED())
                        {
                            max = Math.Max(max, e.projectile.range(0, rr.ref(1.0, rr.boostable.max(Div.class))));
                        }
                        def /= C.TILE_SIZE;
                        curr /= C.TILE_SIZE;
                        max /= C.TILE_SIZE;
                    }
                    GMeter.renderDelta(r, def / max, curr / max, body.x1(), body.x2(), body.y1(), body.cY(), GMeter.C_YELLOW);
                    if (rr != null)
                    {
                        def = GAME.battle().power.range(rr, 0);
                        curr = GAME.battle().power.range(rr, rr.ref(div.equip(rr), GAME.battle().boost(div, rr.boostable)));
                    }
                    max = GAME.battle().power.bestRangedPower() + 1;
                    GMeter.renderDelta(r, def / max, curr / max, body.x1(), body.x2(), body.cY(), body.y2(), GMeter.C_ORANGE);
                },

                HoverInfoGet = text =>
                {
                    GBox box = (GBox)text;
                    box.title(Dic.¤¤Ammunition);
                    EquipRange b = best(div);

                    if (b != null)
                    {
                        //box.add(GFORMAT.f0(box.text(), b.ref(div.equip(b), rr.boostable.max(Div.class))));
                        b.projectile.range(0, b.ref(0, 0));
                    }
                }
            };

            ss.add(aa, 0, ss.body().y2());
        }

        GuiSection statSection = new GuiSection();
        statSection.add(simple(BOOSTABLES.BATTLE().OFFENCE), 0, statSection.body().y2());
        statSection.add(simple(BOOSTABLES.BATTLE().DEFENCE), 0, statSection.body().y2());
        ss.add(statSection, 0, ss.body().y2());
    }

    public GuiSection getSection()
    {
        return ss;
    }

    private void pair(Boostable b1, Boostable b2, Boostable b3, GMeterCol col)
    {
        ss.add(new GaugeBo(b1, col, width, height), 0, ss.body().y2());
        ss.add(new GaugeBo(b2, col, width, height), 0, ss.body().y2());
        ss.add(new GaugeBo(b3, col, width, height), 0, ss.body().y2());
    }

    private void icon(SPRITE icon)
    {
        ss.add(icon, 0, ss.body().y2());
    }

    private HOVERABLE simple(Boostable bo)
    {
        return new GStat
        {
            Update = text =>
            {
                double v = GAME.battle().boost(div, bo);
                if (sdiv != null)
                    v = bo.get(sdiv);
                GFORMAT.fRel(text, v, div.race().bvalue(bo));
            },

            HoverInfoGet = b => hoverI(div, bo, b)
        }.hh(bo.icon);
    }

    public void hoverI(DIV_SPEC st, Boostable bo, GUI_BOX text)
    {
        GBox b = (GBox)text;
        b.title(bo.name);
        b.text(bo.desc);
        b.sep();

        if (sdiv != null)
        {
            bo.hoverDetailed(text, sdiv, bo.name, true);
        }
        else
        {
            double baseValue = bo.baseValue;
            double tot = GAME.battle().boost(st, bo);
            double withoutRace = tot;

            foreach (BoostSpec s in st.race().boosts.all())
            {
                if (s.boostable == bo)
                {
                    if (s.booster.isMul)
                        withoutRace /= s.booster.to();
                }
            }

            foreach (BoostSpec s in st.race().boosts.all())
            {
                if (s.boostable == bo)
                {
                    if (!s.booster.isMul)
                        withoutRace -= s.booster.to();
                }
            }

            double withOutFaction = withoutRace;
            foreach (Booster ss in bo.fGlobal)
            {
                if (ss.isMul)
                    withOutFaction /= ss.get(st.faction());
            }

            foreach (Booster ss in bo.fGlobal)
            {
                if (!ss.isMul)
                    withOutFaction -= ss.get(st.faction());
            }

            double race = tot - withoutRace;
            double fac = withoutRace - withOutFaction;
            double con = tot - race - fac - baseValue;
            b.textLL(Dic.¤¤Base);
            b.tab(7);
            b.add(GFORMAT.f(b.text(), baseValue));
            b.NL(2);

            b.textLL(RACES.name());
            b.tab(7);
            b.add(GFORMAT.f0(b.text(), race));
            b.NL(2);
            b.textLL(Dic.¤¤Faction);
            b.tab(7);
            b.add(GFORMAT.f0(b.text(), fac));
            b.NL(2);
            b.textLL(¤¤Config);
            b.tab(7);
            b.add(GFORMAT.f0(b.text(), con));
            b.NL(8);
            b.textLL(Dic.¤¤Total);
            b.tab(7);
            b.add(GFORMAT.f0(b.text(), tot));
        }
    }

    private class GaugeBo : Gauge
    {
        private readonly Boostable bo;

        public GaugeBo(Boostable bo, GMeterCol col, int width, int height) : base(col, width, height)
        {
            this.bo = bo;
        }

        protected override void Render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            double max = bo.max(Div.class);
            double rr = div.race().bvalue(bo);
            double d = GAME.battle().boost(div, bo);
            if (sdiv != null)
                d = bo.get(sdiv);

            render(r, rr, d, max);
        }

        public override void HoverInfoGet(GUI_BOX text)
        {
            hoverI(div, bo, text);
        }
    }

    private abstract class Gauge : HoverableAbs
    {
        public readonly GMeterCol col;
        private readonly GText tt = new GText(UI.FONT().S, 4);

        protected Gauge(GMeterCol col, int width, int height) : base(width, height)
        {
            this.col = col;
        }

        protected void render(SPRITE_RENDERER r, double def, double current, double max)
        {
            int X1 = body.x1();
            int X2 = body.x2();
            int Y1 = body.y1();
            int Y2 = body.y2();
            GMeter.renderDelta(r, def / max, current / max, X1, X2, Y1, Y2, col);

            tt.clear();
            GFORMAT.f(tt, current, 1);
            tt.adjustWidth();
            OPACITY.O35.bind();

            X2 -= 4;
            X1 = X2 - tt.width() - 8;
            Y1 += ((Y2 - Y1) - tt.height()) / 2;
            Y2 = Y1 + tt.height();

            COLOR.BLACK.render(r, X1, X2, Y1, Y2);
            OPACITY.unbind();
            tt.render(r, X1 + 4, Y1);
        }
    }
}