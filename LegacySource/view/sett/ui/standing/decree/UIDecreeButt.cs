using System;
using init.race;
using init.sprite.UI;
using init.type;
using settlement.stats;
using settlement.stats.muls;
using snake2d;
using snake2d.util.color;
using snake2d.util.gui;
using snake2d.util.sprite;
using util.colors;
using util.data;
using util.gui.misc;
using util.text;
using view.interrupter;
using view.main;

public sealed class UIDecreeButt : GButt.ButtPanel
{
    static string ¤¤title = "¤Decrees";
    static string ¤¤desc = "¤Options for a ruler to increase fulfillment.";

    static UIDecreeButt()
    {
        D.ts(typeof(UIDecreeButt));
    }

    private readonly GETTER<Race> racegetter;
    private readonly ISidePanel pp;
    private readonly HCLASS cl;

    public UIDecreeButt(HCLASS cl, GETTER<Race> race) : base(new SPRITE.Imp(180, UI.FONT().H2.height())
    {
        private readonly GText t = new GText(UI.FONT().S, 8);
        public override void render(SPRITE_RENDERER r, int X1, int X2, int Y1, int Y2)
        {
            GCOLOR.T().H1.bind();
            UI.FONT().H2.render(r, ¤¤title, X1, Y1);
            t.clear();
            COLOR.unbind();
        }
    })
    {
        this.racegetter = race;
        this.cl = cl;
        body.incrW(16);
        pp = new DPanel(cl, race);
    }

    protected override void renAction()
    {
        activeSet(race() != null);
        selectedSet(VIEW.s().panels.added(pp));
    }

    protected override void clickA()
    {
        VIEW.s().panels.addDontRemove(VIEW.s().ui.standing, VIEW.s().ui.slaves, pp);
    }

    public override void hoverInfoGet(GUI_BOX text)
    {
        text.title(¤¤title);
        text.text(¤¤desc);
        text.NL(16);
        GBox b = (GBox)text;

        foreach (StatMultiplier m in STATS.MULTIPLIERS().get(cl))
        {
            if (m.key != null)
            {
                b.textL(m.name);
                b.NL();
                b.text(m.desc);
                b.NL();
            }
        }
    }

    protected Race race()
    {
        return racegetter.get();
    }

    public static void hoverP(StatMultiplier m, GUI_BOX box, HCLASS cl, Race race)
    {
        GBox b = (GBox)box;
        b.textL(m.name);
        b.NL();
        b.text(m.desc);
        b.NL();
        {
            m.boosters.hover(b, HCLASS_RACE.clP(race, cl), null);
        }
        b.sep();
    }
}