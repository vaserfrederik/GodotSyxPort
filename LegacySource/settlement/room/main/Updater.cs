using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Main
{
    class Updater : SAVABLE
    {
        private int dayCurrent = TIME.Days().BitsSinceStart();
        private bool day;
        private const double UPDATE_INTERVAL = 64;
        private const double UPDATE_INTERVALI = 1.0 / UPDATE_INTERVAL;
        private double acc = 0;
        private int ii = 0;

        public Updater(LIST<RoomBlueprintIns<?>> all)
        {
        }

        public void Update(double ds)
        {
            final int max = SETT.ROOMS().Map.Max();
            double n = acc += ds * max * UPDATE_INTERVALI;
            int am = (int)n;
            acc = n - am;

            for (int i = 0; i < am; i++)
            {
                if (ii >= max)
                {
                    day = false;
                    if (dayCurrent != TIME.Days().BitsSinceStart())
                        day = true;
                    dayCurrent = TIME.Days().BitsSinceStart();
                    ii = 0;
                    return;
                }

                Room a = SETT.ROOMS().Map.GetByIndex(ii);
                if (a != null && a is RoomInstance)
                {
                    RoomInstance ins = (RoomInstance)a;
                    ins.Update(UPDATE_INTERVAL, day);
                }

                ii++;
            }
        }

        public override void Save(FilePutter file)
        {
            file.Bool(day);
            file.I(dayCurrent);
            file.I(ii);
            file.D(acc);
        }

        public override void Load(FileGetter file)
        {
            day = file.Bool();
            dayCurrent = file.I();
            ii = file.I();
            acc = file.D();
        }

        public override void Clear()
        {
            dayCurrent = TIME.Days().BitsSinceStart();
            day = false;
            ii = 0;
            acc = 0;
        }
    }
}