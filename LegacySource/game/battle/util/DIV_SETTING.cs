using System;
using System.IO;
using System.Linq;

namespace Game.Battle.Util
{
    public interface DivSetting
    {
        double Training(StatTraining tr);
        double Equip(EquipBattle e);
        int Men();

        int EquipI(EquipBattle e) => (int)Math.Round(CLAMP.D(Math.Round(Equip(e) * e.EquipMax), 0, e.EquipMax));
    }

    public interface DivSettingE : DivSetting
    {
        void TrainingSet(StatTraining tr, double d);
        void EquipSet(EquipBattle e, double d);
        void MenSet(int men);

        DivSettingE CopySettings(DivSetting other)
        {
            for (int i = 0; i < STATS.EQUIP().BattleAll().Size(); i++)
            {
                EquipBattle b = STATS.EQUIP().BattleAll().Get(i);
                EquipSet(b, other.Equip(b));
            }
            for (int i = 0; i < STATS.BATTLE().TrainingAll.Size(); i++)
            {
                StatTraining b = STATS.BATTLE().TrainingAll.Get(i);
                TrainingSet(b, other.Training(b));
            }
            MenSet(other.Men());
            return this;
        }

        DivSettingE CopySettings(DivSetting other, int men, double e, double t)
        {
            for (int i = 0; i < STATS.EQUIP().BattleAll().Size(); i++)
            {
                EquipBattle b = STATS.EQUIP().BattleAll().Get(i);
                double d = Math.Round(other.Equip(b) * e * b.Max());
                d /= b.Max();
                d = CLAMP.D(d, 0, 1);
                EquipSet(b, d);
            }
            for (int i = 0; i < STATS.BATTLE().TrainingAll.Size(); i++)
            {
                StatTraining b = STATS.BATTLE().TrainingAll.Get(i);
                double d = Math.Round(other.Training(b) * t * b.Stat.Indu().Max(null));
                d /= b.Stat.Indu().Max(null);
                d = CLAMP.D(d, 0, 1);
                TrainingSet(b, d);
            }
            MenSet(men);
            return this;
        }
    }

    [Serializable]
    public class DivSettingImp : Savable, DivSettingE
    {
        private static readonly long SerialVersionUID = 1L;
        public double[] Equip = new double[STATS.EQUIP().BattleAll().Size()];
        public double[] Training = new double[STATS.BATTLE().TrainingAll.Size()];
        public int Men;

        public void Save(FilePutter file)
        {
            file.Ds(Equip);
            file.Ds(Training);
            file.I(Men);
        }

        public void Load(FileGetter file)
        {
            file.Ds(Equip);
            file.Ds(Training);
            Men = file.I();
        }

        public void Clear()
        {
            Equip = Equip.Select(x => 0).ToArray();
            Training = Training.Select(x => 0).ToArray();
            Men = 0;
        }

        public double Training(StatTraining tr)
        {
            if (Training == null || Training.Length != STATS.BATTLE().TrainingAll.Size())
                Training = new double[STATS.BATTLE().TrainingAll.Size()];
            return Training[tr.TIndex];
        }

        public double Equip(EquipBattle e)
        {
            if (Equip == null || Equip.Length != STATS.EQUIP().BattleAll().Size())
                Equip = new double[STATS.EQUIP().BattleAll().Size()];
            return Equip[e.IndexMilitary()];
        }

        public int Men()
        {
            return Men;
        }

        public void TrainingSet(StatTraining tr, double d)
        {
            Training[tr.Index()] = d;
        }

        public void EquipSet(EquipBattle e, double d)
        {
            Equip[e.IndexMilitary()] = d;
        }

        public void MenSet(int men)
        {
            Men = men;
        }
    }
}