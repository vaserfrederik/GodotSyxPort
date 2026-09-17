using System;
using System.IO;
using game.time;
using snake2d.util.file;
using util.data;
using util.info;
using util.keymap;

namespace util.statistics
{
    public class HistoryObject<T> : HISTORY_COLLECTION<T>, INT_OE<T>, SAVABLE where T : MAPPED
    {
        private readonly RMAPS<T> map;
        private readonly HistoryInt total;
        private readonly HistoryInt[] histories;
        private readonly INFO info;

        public HistoryObject(int size, TIMECYCLE time, bool keep, RMAPS<T> map) : this(null, size, time, keep, map) { }

        public HistoryObject(INFO info, int size, TIMECYCLE time, bool keep, RMAPS<T> map)
        {
            this.map = map;
            total = new HistoryInt(size, time, keep);
            histories = new HistoryInt[map.all().Count];
            for (int i = 0; i < map.all().Count; i++)
            {
                histories[i] = new H(map.all()[i], size, time, keep);
            }
            this.info = info;
        }

        public HistoryObject(int size, TIMECYCLE time, bool keep, string name, string desc, RMAPS<T> map) : this(new INFO(name, desc), size, time, keep, map) { }

        public INFO info()
        {
            return info;
        }

        public HISTORY_INT total()
        {
            return total;
        }

        public void save(FilePutter file)
        {
            map.saver().save(histories, file);
            total.save(file);
        }

        public void load(FileGetter file)
        {
            map.loader().load(histories, file);
            total.load(file);
            total.set(0);
            foreach (HistoryInt ii in histories)
                total.inc(ii.get());
        }

        public void clear()
        {
            foreach (HistoryInt i in histories)
                i.clear();
            total.clear();
        }

        public HISTORY_INT.HISTORY_INTE get(int rI)
        {
            return histories[rI];
        }

        public HISTORY_INT.HISTORY_INTE history(T r)
        {
            if (r == null)
                return total;
            return get(r.index());
        }

        protected void change(T r, int old, int current)
        {
        }

        private class H : HistoryInt
        {
            private readonly T r;

            public H(T r, int size, TIMECYCLE c, bool keep) : base(size, c, keep)
            {
                this.r = r;
            }

            protected override void change(int old, int current)
            {
                total.inc(-old);
                total.inc(current);
                HistoryObject<T>.this.change(r, old, current);
            }
        }

        public int get(T t)
        {
            return history(t).get();
        }

        public void set(T t, int i)
        {
            history(t).set(i);
        }

        public int min(T t)
        {
            return int.MinValue;
        }

        public int max(T t)
        {
            return int.MaxValue;
        }
    }
}