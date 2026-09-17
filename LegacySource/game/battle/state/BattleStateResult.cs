using System;
using System.Collections.Generic;
using game;
using game.battle.div;
using init.constant;
using init.race;
using init.type;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using world.battle;

public sealed class BattleStateResult
{
    public readonly Induvidual[][] PlayerSurvivors;
    public readonly int[] EnemySurvivors;
    public readonly int[] EnemyCaptured;
    public readonly BATTLE_RESULT Result;
    public readonly int PlayerLosses;
    public readonly int EnemyLosses;

    public BattleStateResult(BATTLE_RESULT result, int enemydead, int playerdead)
    {
        Result = result;
        int[] count = Alloc.Ii(Config.Battle().DIVISIONS_PER_ARMY);
        PlayerLosses = playerdead;
        EnemyLosses = enemydead;
        PlayerSurvivors = new Induvidual[Config.Battle().DIVISIONS_PER_ARMY][];
        EnemySurvivors = Alloc.Ii(Config.Battle().DIVISIONS_PER_ARMY);
        EnemyCaptured = Alloc.Ii(RACES.All().Size());

        if (Result != BATTLE_RESULT.VICTORY)
        {
            int losses = (int)Math.Ceiling(GAME.ARMIES().Enemy().Men() * WBattles.RetreatPenalty);
            double dlosses = (double)losses / (GAME.ARMIES().Player().Men() + 1.0);
            foreach (Div d in GAME.ARMIES().Player().Divisions())
            {
                int am = (int)(STATS.BATTLE().DIV.Stat().Div().Get(d) * dlosses);
                if (d.Status().IsFighting())
                    am += (int)(STATS.BATTLE().DIV.Stat().Div().Get(d) * 0.75);
                am = CLAMP.I(am, 0, STATS.BATTLE().DIV.Stat().Div().Get(d));
                count[d.IndexArmy()] = am;
            }
            foreach (Div d in GAME.ARMIES().Player().Divisions())
            {
                PlayerSurvivors[d.IndexArmy()] = new Induvidual[STATS.BATTLE().DIV.Stat().Div().Get(d) - count[d.IndexArmy()]];
                count[d.IndexArmy()] = 0;
            }
        }
        else
        {
            foreach (Div d in GAME.ARMIES().Player().Divisions())
            {
                PlayerSurvivors[d.IndexArmy()] = new Induvidual[STATS.BATTLE().DIV.Stat().Div().Get(d)];
            }
        }

        ENTITY[] es = SETT.ENTITIES().GetAllEnts();
        foreach (ENTITY e in es)
        {
            if (e is Humanoid)
            {
                Humanoid h = (Humanoid)e;
                Div d = STATS.BATTLE().DIV.Get(h);
                if (d == null)
                {
                    if (h.Indu().HType() == HTYPES.ENEMY())
                        EnemyCaptured[h.Race().Index]++;
                }
                else
                {
                    if (d.Index() >= Config.Battle().DIVISIONS_PER_ARMY)
                    {
                        if (Result == BATTLE_RESULT.VICTORY && RND.rBoolean())
                            EnemyCaptured[h.Race().Index]++;
                        else
                            EnemySurvivors[d.IndexArmy()]++;
                    }
                    else
                    {
                        if (count[d.IndexArmy()] >= PlayerSurvivors[d.IndexArmy()].Length)
                        {
                        }
                        else
                        {
                            PlayerSurvivors[d.IndexArmy()][count[d.IndexArmy()]++] = h.Indu();
                        }
                    }
                }
            }
        }

        Wash();
    }

    private void Wash()
    {
        foreach (Div d in GAME.ARMIES().Player().Divisions())
        {
            Wash(d);
        }
    }

    private void Wash(Div div)
    {
        Induvidual[] ins = PlayerSurvivors[div.IndexArmy()];
        int am = 0;
        foreach (Induvidual ii in ins)
        {
            if (ii != null)
                am++;
        }
        if (am == ins.Length)
            return;

        Induvidual[] nins = new Induvidual[ins.Length];
        am = 0;
        System.Console.Error.WriteLine("BattleResult");
        System.Console.Error.WriteLine(Result);
        System.Console.Error.WriteLine(div.IndexArmy());
        System.Console.Error.WriteLine(STATS.BATTLE().DIV.Stat().Div().Get(div));
        System.Console.Error.WriteLine(ins.Length + " " + am);

        foreach (Induvidual ii in ins)
        {
            if (ii != null)
                nins[am++] = ii;
        }
        PlayerSurvivors[div.IndexArmy()] = nins;
    }
}