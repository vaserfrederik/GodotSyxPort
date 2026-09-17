using System;
using System.Collections.Generic;

namespace game.battle.util
{
    class Boosts
    {
        private readonly Dictionary<int, Entry> map = new Dictionary<int, Entry>();

        public double Get(DIV_SPEC div, Boostable bo)
        {
            if (!map.ContainsKey(bo.Index()))
                map.Add(bo.Index(), new Entry(bo));

            Entry e = map[bo.Index()];
            return e.Get(div);
        }

        public double Max(Boostable bo)
        {
            if (!map.ContainsKey(bo.Index()))
                map.Add(bo.Index(), new Entry(bo));
            return map[bo.Index()].Max();
        }

        private class Entry
        {
            private int checkI = GAME.UpdateI() - 1201;
            private readonly Boostable bo;
            private readonly TmpBoost badd = new TmpBoost();
            private readonly TmpBoost bmul = new TmpBoost();

            private double add = 0;
            private double sub = 0;
            private double mul = 0;

            public Entry(Boostable bo)
            {
                this.bo = bo;
            }

            public double Get(DIV_SPEC div)
            {
                if (Math.Abs(checkI - GAME.UpdateI()) > 1200)
                {
                    checkI = GAME.UpdateI();
                    badd.Set(bo, false);
                    bmul.Set(bo, true);
                }

                add = bo.BaseValue;
                sub = 0;
                mul = 1;

                foreach (BoostSpec ss in div.Race().All(bo))
                {
                    if (ss.Booster.IsMul)
                        mul *= ss.Booster.GetValue(1.0);
                    else
                        Add(ss.Booster.GetValue(1.0));
                }

                if (div.Faction() != null)
                {
                    foreach (Booster ss in bo.FGlobal)
                    {
                        if (ss.IsMul)
                            mul *= ss.Get(div.Faction());
                        else
                            Add(ss.Get(div.Faction()));
                    }
                }

                Add(div.Experience(), badd.Experience, bmul.Experience);

                for (int i = 0; i < STATS.BATTLE().TrainingAll.Count; i++)
                {
                    StatTraining tr = STATS.BATTLE().TrainingAll[i];
                    double v = tr.BValue(div.Training(STATS.BATTLE().TrainingAll[i]));
                    Add(v, badd.Training[i], bmul.Training[i]);
                }

                for (int i = 0; i < STATS.EQUIP().BattleAll().Count; i++)
                {
                    EquipBattle e = STATS.EQUIP().BattleAll()[i];
                    double v = e.BValue(div.Equip(e));
                    Add(v, badd.Equip[i], bmul.Equip[i]);
                }

                return Math.Clamp(mul * add + sub, 0, double.MaxValue);
            }

            public double Max()
            {
                double add = bo.BaseValue;
                double mul = 1;

                double a = 0;
                double m = 0;

                for (int ri = 0; ri < RACES.Playable().Count; ri++)
                {
                    Race r = RACES.Playable()[ri];
                    a = Math.Max(a, badd.Race[r.Index()]);
                    m = Math.Max(m, bmul.Race[r.Index()]);
                }
                add += a;
                mul *= m;

                a = 0;
                m = 0;

                for (int i = 0; i < STATS.BATTLE().TrainingAll.Count; i++)
                {
                    a = Math.Max(a, badd.Training[i]);
                    m = Math.Max(a, bmul.Training[i]);
                }

                add += a;
                mul *= m;

                a = 0;
                m = 0;

                for (int i = 0; i < STATS.EQUIP().BattleAll().Count; i++)
                {
                    a = Math.Max(a, badd.Equip[i]);
                    m = Math.Max(a, bmul.Equip[i]);
                }

                add += a;
                mul *= m;

                return add * mul;
            }

            private void Add(double v, double a, double m)
            {
                a *= v;
                m = 1 + (m - 1) * v;
                Add(a);
                mul *= m;
            }

            private void Add(double a)
            {
                if (a < 0)
                    sub += a;
                else
                    add += a;
            }

            public int Index()
            {
                return bo.Index();
            }
        }

        private class TmpBoost
        {
            private double[] training;
            private double[] equip;
            private double[] race;
            private double experience;

            public TmpBoost()
            {
                training = new double[STATS.BATTLE().TrainingAll.Count];
                equip = new double[STATS.EQUIP().All().Count];
                race = new double[RACES.All().Count];
            }

            public void Set(Boostable bo, bool isMul)
            {
                experience = Get(STATS.BATTLE().CombatExperience.Boosters, bo, isMul);
                for (int ri = 0; ri < RACES.All().Count; ri++)
                    race[ri] = Get(RACES.All()[ri].Boosts, bo, isMul);
                for (int i = 0; i < STATS.BATTLE().TrainingAll.Count; i++)
                {
                    StatTraining t = STATS.BATTLE().TrainingAll[i];
                    training[i] = Get(t.Stat.Boosters, bo, isMul);
                }
                for (int i = 0; i < STATS.EQUIP().BattleAll().Count; i++)
                {
                    EquipBattle t = STATS.EQUIP().BattleAll()[i];
                    equip[i] = Get(t.Stat().Boosters, bo, isMul);
                }
            }

            private double Get(BoostSpecs bos, Boostable bo, bool isMul)
            {
                if (isMul)
                {
                    double res = 1;
                    for (int si = 0; si < bos.All().Count; si++)
                    {
                        BoostSpec s = bos.All()[si];
                        if (s.Booster.IsMul == isMul && s.Boostable == bo)
                        {
                            res *= s.Booster.To();

                        }
                    }
                    return res;
                }
                else
                {
                    double res = 0;
                    for (int si = 0; si < bos.All().Count; si++)
                    {
                        BoostSpec s = bos.All()[si];
                        if (s.Booster.IsMul == isMul && s.Boostable == bo)
                        {
                            res += s.Booster.To();

                        }
                    }
                    return res;
                }
            }
        }
    }
}