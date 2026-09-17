using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Settlement.Room.Military.Training
{
    public abstract class RoomMTrainer<T> : RoomBlueprintIns<T> where T : RoomInstance
    {
        private static readonly List<RoomMTrainer<T>> all = new List<RoomMTrainer<T>>();
        private static readonly string ¤¤Speed = "¤Speed of:";

        static RoomMTrainer()
        {
            GameDisposable.Add(new GameDisposable
            {
                Dispose = () => all.Clear()
            });
            D.Ts(typeof(RoomMTrainer<T>));
        }

        int trainingLimit = 10000;
        public readonly int INDEX_TRAINING;
        public readonly int TRAINING_DAYS;
        public readonly double TRAINING_RATE;
        public readonly EmployerSimple emp;
        public BoostSpecs boosters;
        public readonly INFO tInfo;

        public readonly SPRITE divIcon;
        public readonly List<ColorImp> divCols;

        protected RoomMTrainer(int typeIndex, RoomInitData data, string key) : base(typeIndex, data, key, data.m.CATS.MILITARY)
        {
            tInfo = new INFO(data.text().json("TRAINING"));

            var name = info.name;
            var desc = $"{¤¤Speed} {info.name}";

            var d = data.data().json("TRAINING");

            PushBo(d, name, desc, type, true);

            TRAINING_DAYS = d.i("FULL_TRAINING_IN_DAYS");

            INDEX_TRAINING = all.Add(this);
            TRAINING_RATE = 1.0 / TRAINING_DAYS;
            boosters = new BoostSpecs(info.name, icon, false);
            boosters.read(d, null);

            var json = data.data().json("DIV_SPRITE");

            divIcon = UI.icons().get(json, UI.icons().s.cancel);
            divCols = ColorImp.cols(json);
        }

        public StatTraining training()
        {
            return STATS.BATTLE().TRAINING_ALL.get(INDEX_TRAINING);
        }

        public static double BasicTrainingTimedays()
        {
            return 1.0 / 0.1;
        }

        public void Train(Humanoid a, RoomInstance room, double delta)
        {
            double b = delta * IndustryUtil.RoomBonus(room, null);
            b *= bonus().get(a.indu());

            if (!STATS.BATTLE().basicTraining.isMax(a.indu()))
            {
                STATS.BATTLE().basicTraining.incFraction(a.indu(), delta * STATS.BATTLE().basicTraining.max(a.indu()) * 0.1);
            }
            else
            {
                b *= TRAINING_RATE;
                training().inc(a.indu(), b);
            }
        }

        public int TrainingDays()
        {
            return (int)Math.Ceiling(TRAINING_DAYS / bonus().get(HCLASS_RACE.clP(null, null)));
        }

        protected override void saveP(FilePutter f)
        {
            f.i(trainingLimit);
        }

        protected override void loadP(FileGetter f)
        {
            trainingLimit = f.i();
        }

        protected override void clearP()
        {
            trainingLimit = 10000;
        }

        public int limit()
        {
            return trainingLimit;
        }

        public override SFinderFindable service(int tx, int ty)
        {
            return null;
        }

        protected override void update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public override void appendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this));
        }

        public static List<RoomMTrainer<T>> ALL()
        {
            return all;
        }

        public int employable()
        {
            int e = emp.employable();
            int l = trainingLimit - employment().employed();
            return Math.Min(e, l);
        }
    }
}