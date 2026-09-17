using System;
using System.IO;
using System.Collections.Generic;

namespace Util.Statistics
{
    public class HistoryRace : HISTORY_COLLECTION<Race>, INT_OE<Race>, SAVABLE
    {
        private readonly HistoryInt total;
        private readonly HistoryInt[] histories;
        private readonly INFO info;

        public HistoryRace(int size, TIMECYCLE time, bool keep)
        {
            total = new HistoryInt(size, time, keep);
            histories = new HistoryInt[RACES.All().Count];
            for (int i = 0; i < RACES.All().Count; i++)
            {
                Race r = RACES.All()[i];
                histories[i] = new HistoryInt(size, time, keep)
                {
                    Change = (oldValue, currentValue) =>
                    {
                        total.Inc(-oldValue);
                        total.Inc(currentValue);
                        Change(r, oldValue, currentValue);
                    },
                    Max = () => Max(r),
                    Min = () => Min(r)
                };
            }
            info = null;
        }

        public HistoryRace(int size, TIMECYCLE time, bool keep, string name, string desc)
        {
            total = new HistoryInt(size, time, keep);
            histories = new HistoryInt[RACES.All().Count];
            for (int i = 0; i < RACES.All().Count; i++)
            {
                Race r = RACES.All()[i];
                histories[i] = new HistoryInt(size, time, keep)
                {
                    Change = (oldValue, currentValue) =>
                    {
                        total.Inc(-oldValue);
                        total.Inc(currentValue);
                        Change(r, oldValue, currentValue);
                    }
                };
            }
            info = new INFO(name, desc);
        }

        public HISTORY_INT.HISTORY_INTE History(Race r)
        {
            if (r == null)
                return total;
            return histories[r.Index];
        }

        public HISTORY_INT.HISTORY_INTE Total()
        {
            return total;
        }

        public void Save(FilePutter file)
        {
            RACES.Map().Saver().Save(histories, file);
        }

        public void Load(FileGetter file)
        {
            RACES.Map().Loader().Load(histories, file);
            total.Clear();
            foreach (Race r in RACES.All())
                total.Add(histories[r.Index]);
        }

        public void Clear()
        {
            foreach (Race r in RACES.All())
                histories[r.Index].Clear();
            total.Clear();
        }

        public int Get(Race t)
        {
            if (t == null)
                return total.Get();
            return History(t).Get();
        }

        protected void Change(Race r, int old, int current)
        {
        }

        public INFO Info()
        {
            return info;
        }

        public int Min(Race t)
        {
            return 0;
        }

        public int Max(Race t)
        {
            return int.MaxValue;
        }

        public void Set(Race t, int i)
        {
            History(t).Set(i);
        }
    }
}