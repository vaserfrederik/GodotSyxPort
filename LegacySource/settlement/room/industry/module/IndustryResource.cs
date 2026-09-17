using System;
using System.IO;
using game.time;
using init.resources;
using settlement.entity.humanoid;
using snake2d.util.file;
using snake2d.util.sets;
using util.data;
using util.statistics;

namespace settlement.room.industry.module
{
    public abstract class IndustryResource : INDEXED, SAVABLE
    {
        public readonly RESOURCE resource;
        public readonly double rate;
        public readonly double AIRate;
        public readonly double AIRecovery;
        public readonly double rateSeconds;
        private readonly int index;

        protected readonly HistoryInt history = new HistoryInt(48, TIME.days(), false);
        public readonly INT_OE<ROOM_IDATA_INSTANCE> year;
        public readonly INT_OE<ROOM_IDATA_INSTANCE> yearPrev;
        public readonly DOUBLE_OE<ROOM_IDATA_INSTANCE> day;
        public readonly INT_OE<ROOM_IDATA_INSTANCE> dayPrev;

        protected IndustryResource(DataOSimple<ROOM_IDATA_INSTANCE> data, int li, RESOURCE res, double rate, double AIRate, double AIRecovery)
        {
            this.resource = res;
            this.rate = rate;
            this.AIRate = AIRate;
            this.AIRecovery = AIRecovery;
            rateSeconds = Humanoid.WORK_PER_DAYI * rate / TIME.secondsPerDay();
            this.index = li;
            year = data.new DataInt();
            yearPrev = data.new DataInt();
            day = data.new DataFloat();
            dayPrev = data.new DataInt();
        }

        public HISTORY_INT history()
        {
            return history;
        }

        public void save(FilePutter file)
        {
            history.save(file);
        }

        public void load(FileGetter file)
        {
            history.load(file);
        }

        public void clear()
        {
            history.clear();
        }

        public int inc(ROOM_IDATA_INSTANCE r, double amount)
        {
            return inc(r, amount, true);
        }

        public abstract int inc(ROOM_IDATA_INSTANCE r, double amount, bool record);

        public int work(Humanoid skill, ROOM_IDATA_INSTANCE r, double workSeconds)
        {
            double e = getEffort(skill, r, workSeconds);
            int a = inc(r, e);
            return a;
        }

        public int incDay(ROOM_IDATA_INSTANCE r)
        {
            return inc(r, rate);
        }

        protected abstract double getEffort(Humanoid skill, ROOM_IDATA_INSTANCE r, double workSeconds);

        public int index()
        {
            return index;
        }
    }
}