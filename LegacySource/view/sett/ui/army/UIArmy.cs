using System;
using System.Collections.Generic;
using game;
using game.battle;
using init.constant;
using snake2d.util.datatypes;
using snake2d.util.sets;
using util.text;
using view.interrupter;
using view.sett;

public sealed class UIArmy : ISidePanel
{
    public UIArmy(Armies m)
    {
        titleSet(Dic.¤¤Conscripts);

        ArrayList<Div> selection = new ArrayList<Div>(Config.battle().DIVISIONS_PER_ARMY);

        this.section.add(new Info());
        this.section.addRelBody(8, DIR.S, new Actions(selection));
        this.section.addRelBody(8, DIR.S, new DivList(HEIGHT - this.section.body().height() - 16, selection));

        IDebugPanelSett.add(new FormationDebugPlacer(GAME.ARMIES().player()));
        IDebugPanelSett.add(new FormationDebugPlacer(GAME.ARMIES().enemy()));
    }
}