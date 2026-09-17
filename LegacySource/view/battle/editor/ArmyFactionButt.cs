using System;
using game;
using game.battle.util;
using game.faction;
using init.sprite.UI;
using snake2d;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.gui.clickable;
using snake2d.util.sprite.text;
using util.colors;
using util.data.GETTER;
using util.gui.misc;
using util.info;
using util.text;
using view.main;
using view.ui.profile;

class ArmyFactionButt : GuiSection
{
    private readonly ArmySide divs;
    private readonly GETTERE<ArmySide> g;

    private static readonly CharSequence ¤¤player = "player";
    private static readonly CharSequence ¤¤enemy = "enemy";

    static
    {
        D.ts(typeof(ArmyFactionButt));
    }

    public ArmyFactionButt(Faction f, ArmySide divs, GETTERE<ArmySide> g)
    {
        if (f == FACTIONS.player())
        {
            add(new GText(UI.FONT().H2, ¤¤player).normalify2(), 0, 0);
        }
        else
        {
            add(new GText(UI.FONT().H2, ¤¤enemy).errorify(), 0, 0);
        }

        CLICKABLE title = new GInput(new StringInputSprite(24, UI.FONT().S)
        {
            public override void renAction()
            {
                text().clear().add(f.name);
            }

            protected override void change()
            {
                f.name.clear().add(text());
            }
        });

        addDown(4, title);

        {
            GuiSection s = new GuiSection();
            s.add(new GStat()
            {
                public override void update(GText text)
                {
                    int m = 0;
                    foreach (DIV_SPEC s in divs.divs)
                    {
                        if (s != null)
                            m += s.men();
                    }
                    GFORMAT.i(text, m);
                }
            }.hh(UI.icons().s.human));

            s.addRightC(68, new GStat()
            {
                public override void update(GText text)
                {
                    int m = 0;
                    foreach (DIV_SPEC s in divs.divs)
                    {
                        if (s != null)
                            m += GAME.battle().power.get(s);
                    }
                    GFORMAT.i(text, m);
                }
            }.hh(UI.icons().s.fist));

            addRelBody(4, DIR.S, s);
        }

        CLICKABLE banner = new GButt.ButtPanel(f.banner().HUGE)
        {
            protected override void clickA()
            {
                VIEW.inters().popup.show(new UIFactionBanner(f), this);
            }
        };

        addRelBody(8, DIR.W, banner);

        body().pad(16, 8);
        this.divs = divs;
        this.g = g;
    }

    public override void render(SPRITE_RENDERER r, float ds)
    {
        GButt.ButtPanel.renderBG(r, true, g.get() == divs, hoveredIs(), body());
        base.render(r, ds);
        GCOLOR.UI().border().renderFrame(r, body(), 0, 1);
    }

    protected override void clickA()
    {
        g.set(divs);
        base.clickA();
    }
}