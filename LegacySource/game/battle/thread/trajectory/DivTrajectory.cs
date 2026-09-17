using System;
using System.IO;
using System.Linq;

namespace Game.Battle.Thread.Trajectory
{
    using Init.Constant;
    using Settlement.Entity.Humanoid;
    using Settlement.Thing.Projectiles;
    using Snake2D.Util.File;

    internal sealed class DivTrajectory
    {
        public int Targets { get; private set; }
        public bool Potential { get; private set; }
        private readonly float[] data = new float[Config.Battle().MenPerDivision * 3];
        private static Trajectory Tra = new Trajectory();

        public DivTrajectory()
        {
        }

        public void Set(int pos, Trajectory t)
        {
            int i = pos * 3;
            Targets++;
            data[i] = (float)t.Vx();
            data[i + 1] = (float)t.Vy();
            data[i + 2] = (float)t.Vz();
        }

        public bool Has(int pos)
        {
            return !float.IsNaN(data[pos * 3]);
        }

        public Trajectory Get(int pos, Humanoid a)
        {
            int i = pos * 3;
            if (float.IsNaN(data[i]))
                return null;

            Tra.Set(data[i], data[i + 1], data[i + 2]);
            return Tra;
        }

        public void Save(FilePutter file)
        {
            file.Fs(data);
            file.I(Targets);
            file.Bool(Potential);
        }

        public void Load(FileGetter file)
        {
            file.Fs(data);
            Targets = file.I();
            Potential = file.Bool();
        }

        public void Clear()
        {
            data.Fill(float.NaN);
            Targets = 0;
            Potential = false;
        }
    }
}