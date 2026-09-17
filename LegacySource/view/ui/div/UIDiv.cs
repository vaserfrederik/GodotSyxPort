using System;
using System.Collections.Generic;
using game;
using game.battle.util;
using init.constant;
using init.sprite.UI;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.misc;
using snake2d.util.sets;
using util.colors;

public class UIDiv
{
    private const int WIDTH = 58;
    private const int HEIGHT = 64 + 14;

    public readonly UIDivCardSett settCivic = new UIDivCardSett(this);
    public readonly UIDivCardBasic normal = new UIDivCardBasic(this);
    public readonly UIDivCardWorld world = new UIDivCardWorld(this);
    public readonly UIDivCardBattle battle = new UIDivCardBattle(this);

    private readonly List<EquipBattle> equips = new List<EquipBattle>(STATS.EQUIP().BATTLE_ALL().size());
    private readonly Rec body = new Rec();

    public UIDiv()
    {
        // TODO Auto-generated constructor stub
    }

    private class Comp : IComparer<EquipBattle>
    {
        public DIV_SPEC d;

        public int Compare(EquipBattle o1, EquipBattle o2)
        {
            return o1.sprites[d.race().index].z - o2.sprites[d.race().index].z;
        }
    }

    private readonly Comp comp = new Comp();

    public void renderBasics(SPRITE_RENDERER r, int x1, int y1, int scale, DIV_SPEC d)
    {
        if (d == null)
            return;

        body.set(x1, x1 + WIDTH * scale, y1, y1 + HEIGHT * scale);

        int cx = body.cX();

        GAME.ARMIES().banners.get(d.bannerI()).renderCX(r, cx - 12 * scale, y1 + 4 * scale, scale);
        d.race().appearance().icon.renderCX(r, cx, body.y1() + 18 * scale, scale);

        equips.Clear();
        foreach (EquipBattle e in STATS.EQUIP().BATTLE_ALL())
        {
            if (d.equip(e) > 0)
            {
                equips.Add(e);
            }
        }
        comp.d = d;
        equips.Sort(comp);

        foreach (EquipBattle e in equips)
        {
            DivSprite s = e.sprites[d.race().index()];

            OPACITY.O50.bind();
            COLOR.BLACK.bind();
            s.icon.renderCX(r, cx + s.ox * scale + 2 * scale, y1 + 54 * scale + s.oy * scale, scale);
            OPACITY.unbind();

            ColorImp.TMP.interpolate(s.cols, d.equip(e)).bind();
            s.icon.renderCX(r, cx + s.ox * scale, y1 + 52 * scale + s.oy * scale, scale);
        }

        {
            int k = 0;
            foreach (StatTraining t in STATS.BATTLE().TRAINING_ALL)
            {
                double ds = d.training(t);
                if (ds > 0)
                {
                    int am = (int)(ds * 3);
                    ColorImp.TMP.interpolate(t.room.divCols, ds).bind();
                    for (int i = 0; i <= am; i++)
                    {
                        t.room.divIcon.renderScaled(r, cx - 2 * scale + k * 12 * scale, y1 + 2 * scale + scale * i * 5, scale);
                    }
                    k++;
                    if (k >= 2)
                        break;
                }
            }
        }

        {
            double ds = d.experience();
            int am = (int)(ds * 5);
            GCOLOR.T().H1.bind();
            for (int i = 0; i < am; i++)
            {
                UI.icons().s.smallSkull.renderCScaled(r, cx - 20 * scale, y1 + 30 * scale + scale * 6 * i, scale);
            }
        }

        COLOR.unbind();
    }

    private readonly COLOR[] cPower = new COLOR[]
    {
        new ColorImp(114, 84, 33).shade(0.7),
        new ColorImp(114, 114, 114).shade(0.7),
        new ColorImp(114, 114, 33).shade(0.7),
    };

    public void renderPower(int x1, int y1, SPRITE_RENDERER r, double l)
    {
        l = l / (5.0 * Config.battle().MEN_PER_DIVISION);

        int ci = CLAMP.i((int)(l * 3), 0, 2);
        l -= ci / 3.0;
        int am = (int)(1 + l * 5);
        am = CLAMP.i(am, 1, 6);

        OPACITY.O50.bind();
        COLOR.BLACK.render(r, x1, x1 + 10, y1, y1 + am * 8 + 4);
        OPACITY.unbind();
        y1 += 2;
        x1 += 1;
        cPower[ci].bind();
        for (int i = 0; i < am; i++)
        {
            UI.icons().s.chevron(DIR.N).render(r, x1, y1 + i * 8);
        }
        COLOR.unbind();
    }
}