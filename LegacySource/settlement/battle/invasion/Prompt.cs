using System;
using System.Collections.Generic;
using settlement.battle.invasion;
using game.battle.util;
using settlement.main;
using snake2d.util.datatypes;
using snake2d.util.gui;
using snake2d.util.sprite.text;
using util.gui.misc;
using util.text;
using view.main;
using view.ui.message;

class Prompt : MessageSection
{
    private static readonly long serialVersionUID = 1L;
    static readonly string ¤¤invasion = "¤Invasion!";
    static readonly string ¤¤invasionD = "¤An enemy host stands gathered {0} of the city, preparing for an assault. They will attack at any moment. If they reach the throne, it is all over.";
    static readonly string ¤¤count = "Our scouts report {0} soldiers.";
    static readonly string ¤¤attack = "There is still a window of opportunity to attack them before they begin their assault. If we don't, we'll fight in the city.";

    static Prompt()
    {
        D.ts(typeof(Prompt));
    }

    private readonly int men;
    private readonly int refValue;
    private readonly string dir;

    public Prompt(InvasionSpec spec, DIR dd) : base(¤¤invasion)
    {
        dir = dd.getName();
        refValue = spec.ref;
        int men = 0;
        foreach (DivGeneration g in spec.divs)
        {
            men += g.indus.Length;
        }
        this.men = men;
    }

    protected override void make(GuiSection section)
    {
        paragraph(Str.TMP.clear().add(¤¤invasionD).insert(0, dir));

        paragraph(Str.TMP.clear().add(¤¤count).insert(0, men));

        paragraph(¤¤attack);

        section.addRelBody(16, DIR.S, new GButt.ButtPanel(Dic.¤¤Attack)
        {
            protected override void renAction()
            {
                InvasionSpec sp = SETT.INVADOR().spec(refValue);
                activeSet(sp != null && sp.canBeAttacked);
            }

            protected override void clickA()
            {
                InvasionSpec sp = SETT.INVADOR().spec(refValue);
                if (sp != null && sp.canBeAttacked)
                {
                    VIEW.inters().messages.hide();
                    new Attack(sp);
                }
            }
        });
    }
}