using System;
using System.IO;

namespace Game.Boosting.Superb
{
    public class SuperData : SAVABLE
    {
        private readonly SuperBoostable<?> daddy;
        private double[] values;
        private double[] times;
        private double[] state;

        public SuperData(SuperBoostable<?> daddy)
        {
            this.daddy = daddy;
        }

        private void Init()
        {
            if (values == null || values.Length != daddy.ups.Count)
            {
                values = new double[daddy.ups.Count];
                times = new double[daddy.ups.Count];
                state = new double[daddy.ups.Count];
            }
        }

        public double[] Values()
        {
            Init();
            return values;
        }

        public double[] Times()
        {
            Init();
            return times;
        }

        public double[] States()
        {
            Init();
            return state;
        }

        public override void Save(FilePutter file)
        {
            Init();
            for (int i = 0; i < values.Length; i++)
            {
                file.d(values[i]);
                file.d(times[i]);
                file.d(state[i]);
            }
        }

        public override void Load(FileGetter file)
        {
            Init();
            Clear();

            int[] so = daddy.SaveOrder();
            for (int i = 0; i < so.Length; i++)
            {
                double v = file.d();
                double t = file.d();
                double s = file.d();
                if (so[i] >= 0 && so[i] < values.Length)
                {
                    values[so[i]] = v;
                    times[so[i]] = t;
                    state[so[i]] = s;
                }
            }
        }

        public override void Clear()
        {
            Init();
            Array.Fill(values, 0);
            Array.Fill(times, 0);
            Array.Fill(state, 0);
        }
    }
}