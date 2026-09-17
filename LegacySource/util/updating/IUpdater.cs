using System;
using System.IO;

namespace util.updating
{
    public abstract class IUpdater : SAVABLE
    {
        private readonly int amount;
        private int i;
        private readonly double secondsBetween;
        private readonly double tilesPerSecond;
        private double acc = 0;

        public IUpdater(int amount, double secondsBetween)
        {
            this.amount = amount;
            this.secondsBetween = secondsBetween;
            tilesPerSecond = (amount / secondsBetween);
        }

        public void Update(double ds)
        {
            acc += ds * tilesPerSecond;

            int a = (int)acc;
            acc -= a;
            while (a > 0)
            {
                a--;
                Update(i, secondsBetween);
                i++;
                if (i >= amount)
                    i = 0;
            }
        }

        protected void Backup()
        {
            i--;
        }

        protected abstract void Update(int i, double timeSinceLast);

        public override void Save(FilePutter file)
        {
            file.I(i);
            file.D(acc);
        }

        public override void Load(FileGetter file)
        {
            i = file.I();
            i = i % amount;
            acc = file.D();
        }

        public override void Clear()
        {
            i = 0;
            acc = 0;
        }

        public void Debug()
        {
            LOG.Ln();
            LOG.Ln(this.amount);
            LOG.Ln(this.secondsBetween);
            LOG.Ln(this.tilesPerSecond);
            LOG.Ln(acc);
            LOG.Ln(i);
        }

        public abstract class IUpdaterSer : Serializable
        {
            private readonly int amount;
            private int i;
            private readonly double secondsBetween;
            private readonly double tilesPerSecond;
            private double acc = 0;

            public IUpdaterSer(int amount, double secondsBetween)
            {
                this.amount = amount;
                this.secondsBetween = secondsBetween;
                tilesPerSecond = (amount / secondsBetween);
            }

            public void Update(double ds)
            {
                acc += ds * tilesPerSecond;

                int a = (int)acc;
                acc -= a;
                while (a > 0)
                {
                    a--;
                    Update(i, secondsBetween);
                    i++;
                    if (i >= amount)
                        i = 0;
                }
            }

            protected abstract void Update(int i, double timeSinceLast);

            public void Debug()
            {
                LOG.Ln();
                LOG.Ln(this.amount);
                LOG.Ln(this.secondsBetween);
                LOG.Ln(this.tilesPerSecond);
                LOG.Ln(acc);
                LOG.Ln(i);
            }

            private static readonly long serialVersionUID = 1L;
        }
    }
}