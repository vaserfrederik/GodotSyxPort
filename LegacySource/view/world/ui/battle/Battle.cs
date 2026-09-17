using System;
using System.Collections.Generic;
using System.Linq;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.Hoverable;
using snake2d.util.sprite;
using snake2d.util.sprite.text;
using util.colors;
using util.data;
using util.gui.misc;
using util.gui.table;
using util.info;
using util.text;
using world;
using world.army;
using world.army.ADSupplies;
using world.battle.spec;
using world.map.landmark;
using world.map.regions;

abstract class Battle
{
    private static string ¤¤battleOf = "Battle of {0}";
    private static string ¤¤battle = "Battle";
    public static string ¤¤Annihilation = "¤Annihilation";
    public static string ¤¤Command = "¤Command";
    public static string ¤¤autoD = "¤Auto resolve this battle. The result will be {0}. You will lose about {1} men and inflict about {2} casualties on the enemy.";
    public static string ¤¤AutoResolve = "¤Auto";
    public static string ¤¤defence = "¤This unit is defending and is given extra power due to their defensive position.";

    static Battle()
    {
        D.ts(typeof(Battle));
    }

    private readonly int WIDTH = 300;

    private bool hovRetreat = false;
    private bool hovAuto = false;
    protected WBattleSpec g;

    private readonly GuiSection sec = new GuiSection
    {
        Render = (SPRITE_RENDERER r, float ds) =>
        {
            base.Render(r, ds);
            hovRetreat = false;
            hovAuto = false;
            string title = title(g).ToString();
            int w = UI.FONT().H2.width(title);
            UI.PANEL().titleBoxes[1].renderCY(r, body().cX() - w / 2, body().y1() - 16, w);
            GCOLOR.T().H1.bind();
            UI.FONT().H2.renderC(r, body().cX(), body().y1() - 16, title);
            COLOR.unbind();
        }
    };

    public Battle(string desc)
    {
        GETTER<WBattleSide> pg = () => g.player;
        sec.add(side(pg, GMeter.C_REDGREEN));
        GETTER<WBattleSide> sg = () => g.enemy;
        sec.addRightC(8, side(sg, GMeter.C_REDORANGE));

        sec.addRelBody(16, DIR.N, balance(pg, sg));
        sec.addRelBody(8, DIR.N, new RENDEROBJ.RenderDummy(10, 8));

        string[] descs = UI.FONT().M.getRows(desc, WIDTH * 2);

        foreach (var d in descs)
        {
            GText t = new GText(UI.FONT().M, d);
            t.warnify();
            sec.addRelBody(4, DIR.S, t);
        }
        sec.addRelBody(8, DIR.S, buttons());
    }

    protected abstract RENDEROBJ buttons();

    public static GuiSection balance(GETTER<WBattleSide> player, GETTER<WBattleSide> enemy)
    {
        GuiSection s = new GuiSection();

        RENDEROBJ gg = new HOVERABLE.HoverableAbs(200, Icon.M)
        {
            Render = (SPRITE_RENDERER r, float ds, bool isHovered) =>
            {
                double d = player.get().powerBalance();
                if (d < 0.5)
                    GMeter.render(r, GMeter.C_RED, d, body);
                else
                    GMeter.render(r, GMeter.C_BLUE, d, body);
            },
            hoverInfoGet = (GUI_BOX text) => text.title(Dic.¤¤Balance)
        };

        s.addDownC(4, gg);
        s.addC(UI.icons().l.rebel, s.body().cX(), s.body().cY());

        int y1 = s.body().cY() - 12;

        s.add(new GStat(UI.FONT().M)
        {
            update = (GText text) =>
            {
                GFORMAT.iBig(text, player.get().men());
                text.normalify2();
            }
        }.r(DIR.NE), s.body().x1() - 80, y1);

        s.add(new GStat(UI.FONT().M)
        {
            update = (GText text) =>
            {
                GFORMAT.iBig(text, enemy.get().men());
                text.warnify();
            }
        }.r(DIR.NW), s.body().x2() + 80, y1);

        return s;
    }

    void setCas(bool hovRetreat, bool hovFight)
    {
        this.hovRetreat = hovRetreat;
        this.hovAuto = hovFight;
    }

    public GuiSection get(WBattleSpec spec)
    {
        this.g = spec;
        return sec;
    }

    private GuiSection side(GETTER<WBattleSide> g, GMeterCol col)
    {
        GuiSection s = new GuiSection();

        {
            HoverableAbs h = new HoverableAbs(WIDTH, Icon.S)
            {
                Render = (SPRITE_RENDERER r, float ds, bool isHovered) =>
                {
                    int am = 0;
                    foreach (var a in AD.supplies().arts())
                    {
                        am += g.get().artillery(a);
                    }

                    if (am <= 0)
                        return;

                    int d = (body().width() - 50) / am;
                    d = CLAMP.i(d, 1, 16);

                    int i = 0;
                    foreach (var a in AD.supplies().arts())
                    {
                        for (int k = 0; k < g.get().artillery(a); k++)
                        {
                            a.art.icon.small.render(r, body.x1() + i * d, body().y1());
                            i++;
                        }
                    }
                },
                hoverInfoGet = (GUI_BOX text) =>
                {
                    GBox b = (GBox)text;
                    foreach (var a in AD.supplies().arts())
                    {
                        b.text($"{a} - {g.get().artillery(a)}");
                    }
                }
            };

            s.add(h);
        }

        s.add(new Row(g, null, col));

        SPRITE frame = GCOLOR.UI().border().makeFrame(s.body().width() + 12, s.body().height() + 12, 1);
        s.addC(frame, s.body().cX(), s.body().cY());

        return s;
    }

    private class Row : HoverableAbs
    {
        private readonly GETTER<WBattleSide> g;
        private readonly GETTER<int> ier;
        private readonly GMeterCol col;

        public Row(GETTER<WBattleSide> g, GETTER<int> ier, GMeterCol col) : base(WIDTH, Icon.M)
        {
            this.g = g;
            this.ier = ier;
            this.col = col;
        }

        protected override void render(SPRITE_RENDERER r, float ds, bool isHovered)
        {
            WBattleUnit u = u();
            if (u == null)
                return;
            u.icon().renderCY(r, body.x1(), body.cY());

            int X1 = body.x1() + Icon.M + 4;
            int WI = body.x2() - X1 - 8;

            double dmen = Math.Sqrt((double)u.men() / Config.battle().MEN_PER_ARMY);
            int X2 = (int)(X1 + WI * dmen);

            int losses = 0;

            if (hovRetreat)
                losses = u.lossesRetreat();
            if (hovAuto)
            {
                losses = u.losses();
            }

            double d = (double)(u.men() - losses) / u.men();

            GMeter.render(r, col, d, X1, X2, body.y1() + 2, body.y2() - 2);

            GMeter.renderDelta(r, 1.0, d, X1, X2, body.y1() + 2, body.y2() - 2, col);
        }

        public override void hoverInfoGet(GUI_BOX text)
        {
            WBattleUnit u = u();
            if (u == null)
                return;
            u.hover(text);
            base.hoverInfoGet(text);

            if (u.defences() >= 1)
            {
                GBox b = (GBox)text;
                b.sep();
                b.text(¤¤defence);
            }
        }

        private WBattleUnit u()
        {
            WBattleSide s = g.get();
            if (s == null)
                return null;
            int ui = ier == null ? 0 : ier.get() + 1;
            WBattleUnit u = s.units().get(ui);
            if (u == null)
                return null;
            return u;
        }
    }

    static class Butt : GButt.ButtPanel
    {
        public Butt(SPRITE icon, string label) : base(label)
        {
            icon(UI.icons().s.arrow_left);
            body.setWidth(200);
        }
    }
}