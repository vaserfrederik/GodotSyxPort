using System;
using System.Collections.Generic;
using init.constant;
using init.race;
using settlement.stats;
using settlement.stats.colls;
using settlement.stats.equip;
using snake2d.util.sets;

namespace game.battle.util
{
    public sealed class DivType : DIV_SETTING
    {
        public readonly double occurence;
        public readonly double[] roccurence;

        private readonly double[] equip;
        private readonly double[] training;

        public DivType()
        {
            occurence = 0;
            roccurence = new double[RACES.all().Size()];
            equip = new double[STATS.EQUIP().BATTLE_ALL().Size()];
            training = new double[STATS.BATTLE().TRAINING_ALL.Size()];
        }

        public DivType(double occ, LIST<StatTraining> trs, LIST<EquipBattle> eqps)
        {
            occurence = occ;
            roccurence = new double[RACES.all().Size()];

            equip = new double[STATS.EQUIP().BATTLE_ALL().Size()];
            training = new double[STATS.BATTLE().TRAINING_ALL.Size()];

            foreach (var tr in trs)
                training[tr.tIndex] = 1.0;

            foreach (var eq in eqps)
                equip[eq.indexMilitary()] = 1.0;
        }

        public override double training(StatTraining tr)
        {
            return training[tr.tIndex];
        }

        public override double equip(EquipBattle e)
        {
            return equip[e.indexMilitary()];
        }

        public override int men()
        {
            return Config.battle().MEN_PER_DIVISION;
        }

        public bool valid(Race race)
        {
            for (int i = 0; i < equip.Length; i++)
            {
                if (equip[i] > 0 && !STATS.EQUIP().BATTLE_ALL().get(i).allowed(race))
                    return false;
            }
            return true;
        }
    }
}