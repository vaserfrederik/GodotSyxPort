using System;
using System.IO;

namespace Settlement.Stats.Law
{
    public sealed class Curfew
    {
        public static string ¤¤name = "¤Curfew";
        public static string ¤¤desc = "¤When a curfew is active, subjects will not visit their ordinary jobs, and stay home or inside, except when they need to visit basic services. This deters criminals, and prevents disease from being spread.";

        static Curfew()
        {
            D.ts(typeof(Curfew));
        }

        private double timer;

        public Curfew()
        {
        }

        public void Update(double ds)
        {
            if (timer > 0)
                timer -= ds;
        }

        public bool Is()
        {
            return timer > 0;
        }

        public bool IsSetForADay()
        {
            return timer > 0;
        }

        public void SetForADay(bool set)
        {
            if (set)
            {
                timer = TIME.SecondsPerDay();
            }
            else
            {
                timer = 0;
            }
        }

        public SAVABLE Saver => new SAVABLEImpl();

        private class SAVABLEImpl : SAVABLE
        {
            private readonly Curfew _curfew;

            public SAVABLEImpl()
            {
                _curfew = new Curfew();
            }

            public void Save(FilePutter file)
            {
                file.D(_curfew.timer);
            }

            public void Load(FileGetter file)
            {
                _curfew.timer = file.D();
            }

            public void Clear()
            {
                _curfew.timer = 0;
            }
        }
    }
}