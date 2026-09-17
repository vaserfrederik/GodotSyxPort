using System;
using System.Text;
using System.Collections.Generic;

namespace Game.Battle.Util
{
    using Game;
    using Game.Faction;
    using Init.Constant;
    using Init.Race;
    using Settlement.Stats;
    using Settlement.Stats.Colls.StatsBattle;
    using Settlement.Stats.Equip;
    using Snake2D.Util.Rnd;
    using Snake2D.Util.Sprite.Text;
    using Util.Text;

    public interface IDivSpec : IDivSetting, IDivSimple
    {
        double Experience { get; }
        Faction Faction { get; }
        string Name { get; }
        int BannerIndex { get; }

        public interface IDivSpece : IDivSpec, IDivSettingE
        {
            void RaceSet(Race race);
            void ExperienceSet(double experience);
            Str NameE { get; }
            void BannerIndexSet(int bannerIndex);

            void FactionSet(Faction faction);

            default IDivSpece CopyFrom(IDivSpec other)
            {
                for (int i = 0; i < STATS.BATTLE().TRAINING_ALL.Count; i++)
                {
                    StatTraining t = STATS.BATTLE().TRAINING_ALL[i];
                    TrainingSet(t, other.Training(t));
                }

                for (int i = 0; i < STATS.EQUIP().BATTLE_ALL().Count; i++)
                {
                    EquipBattle t = STATS.EQUIP().BATTLE_ALL()[i];
                    EquipSet(t, other.Equip(t));
                }

                MenSet(other.Men());
                ExperienceSet(other.Experience());
                RaceSet(other.Race());
                NameE.Clear().Add(other.Name());
                BannerIndexSet(other.BannerIndex());
                FactionSet(other.Faction());
                return this;
            }

            default void Generate()
            {
                double tr = RND.rFloat();
                double eq = RND.rFloat();

                Race r = RACES.all().Rnd();
                RaceSet(r);
                MenSet(10 + RND.rInt(Config.Battle().MEN_PER_DIVISION - 10));
                ExperienceSet(RND.rFloat());
                NameE.Clear().Add(r.Info.ArmyNames.Rnd());
                BannerIndexSet(RND.rInt(GAME.ARMIES().Banners.Size()));

                DivType dd = GAME.Battle().Types.Rnd(r, FACTIONS.Player(), RND.rFloat());

                for (int i = 0; i < STATS.EQUIP().BATTLE_ALL().Count; i++)
                {
                    EquipBattle t = STATS.EQUIP().BATTLE_ALL()[i];
                    EquipSet(t, dd.Equip(STATS.EQUIP().BATTLE_ALL()[i]) * eq);
                }
                for (int i = 0; i < STATS.BATTLE().TRAINING_ALL.Count; i++)
                {
                    StatTraining t = STATS.BATTLE().TRAINING_ALL[i];
                    TrainingSet(t, dd.Training(STATS.BATTLE().TRAINING_ALL[i]) * tr);
                }
            }
        }

        public class DivSpecImp : IDivSpece
        {
            private readonly double[] training = new double[STATS.BATTLE().TRAINING_ALL.Count];
            private readonly double[] gear = new double[STATS.EQUIP().BATTLE_ALL().Count];
            private int men = 10;
            private double experience = 0;
            private int race = 0;
            private Str name = new Str(24).Add(Dic.¤¤Rename);
            private int bannerIndex = 0;
            private int faction = 0;

            public DivSpecImp()
            {

            }

            void Clear(Race race)
            {
                Array.Fill(training, 0);
                Array.Fill(gear, 0);
                men = 10;
                experience = 0;
                this.race = race.Index;
                name.Clear().Add(Dic.¤¤Rename);
                bannerIndex = 0;
                faction = 0;
            }

            public double Training(StatTraining tr)
            {
                return training[tr.TIndex];
            }

            public double Equip(EquipBattle e)
            {
                return gear[e.IndexMilitary()];
            }

            public int Men()
            {
                return men;
            }

            public Race Race()
            {
                return RACES.all().Get(race);
            }

            public void RaceSet(Race race)
            {
                this.race = race.Index;
            }

            public double Experience()
            {
                return experience;
            }

            public Faction Faction()
            {
                return FACTIONS.GetByIndex(faction);
            }

            public string Name()
            {
                return name.ToString();
            }

            public int BannerIndex()
            {
                return bannerIndex;
            }

            public void MenSet(int men)
            {
                this.men = men;
            }

            public void ExperienceSet(double experience)
            {
                this.experience = experience;
            }

            public Str NameE()
            {
                return name;
            }

            public void BannerIndexSet(int bannerIndex)
            {
                this.bannerIndex = bannerIndex;
            }

            public void TrainingSet(StatTraining tr, double d)
            {
                training[tr.TIndex] = d;
            }

            public void EquipSet(EquipBattle e, double d)
            {
                gear[e.IndexMilitary()] = d;
            }

            public void FactionSet(Faction faction)
            {
                this.faction = faction.Index();
            }
        }
    }
}