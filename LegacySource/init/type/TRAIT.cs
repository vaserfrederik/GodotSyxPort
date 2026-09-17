using System;
using System.Collections.Generic;
using game.battle.div;
using game.boosting;
using game.faction.npc;
using init.race;
using init.race.bio;
using init.sprite.UI;
using settlement.stats;
using settlement.stats.colls.StatsTraits;
using settlement.stats.util;
using snake2d.util.file;
using snake2d.util.sets;
using util.info;
using util.keymap;
using util.text;

public sealed class TRAIT : MAPPED
{
    private static readonly string ¤¤name = "Trait";

    static TRAIT()
    {
        D.ts(typeof(TRAIT));
    }

    private readonly string key;
    private readonly int index;
    public readonly INFO info;
    public readonly string rTitle;
    public readonly BoostSpecs boosters;
    public readonly string[] bios;
    private readonly double[] occRaces = new double[RACES.All().Size()];

    private readonly ArrayListGrower<TRAIT> disables = new ArrayListGrower<TRAIT>();

    public TRAIT(LISTE<TRAIT> all, string key, Json data, Json jtext)
    {
        this.key = key;
        this.index = all.Add(this);
        info = new INFO(jtext);
        rTitle = jtext.Text("TITLE");
        bios = BioLine.Insert.Check(jtext.Texts("BIO_DESC"));
        RACES.Map().ReadFill("DEFAULT_RACE_OCCURANCE", occRaces, data, 0, 1);
        boosters = new BoostSpecs(¤¤name + ": " + info.Name, UI.Icons().S.Alert, true);

        boosters.Read(data, new StatBooster()
        {
            public double VGet(Induvidual indu)
            {
                return Stat().GetD(indu);
            }

            public double VGet(Div div)
            {
                return Stat().GetD(div);
            }

            public double VGet(FactionNPC f)
            {
                return VGet(f.Court().King().Roy().Induvidual);
            }

            public double VGet(HCLASS_RACE popTime)
            {
                return Stat().GetD(popTime.Cl, popTime.Race);
            }
        });
    }

    public int Index()
    {
        return index;
    }

    public string Key()
    {
        return key;
    }

    public LIST<TRAIT> Disables()
    {
        return disables;
    }

    public double Get(Induvidual inDuvidual)
    {
        return STATS.TRAITS().Stat(this).GetD(inDuvidual);
    }

    public double Occurance(Race race)
    {
        return occRaces[race.Index];
    }

    public StatTrait Stat()
    {
        return STATS.TRAITS().Stat(this);
    }
}