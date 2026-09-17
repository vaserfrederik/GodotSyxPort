using System;
using System.IO;
using game.battle.div;
using game.battle.thread;
using init.constant;
using snake2d.util.file;

namespace game.battle.thread.position
{
    public sealed class DivCentres : BattleThread
    {
        private Context current = new Context();

        private Context[] next = new Context[] {
            new Context(),
            new Context(),
        };
        private volatile int ci = 0;
        private readonly Updater updater = new Updater();

        public DivCentres() : base(1.0 / 60)
        {
            new Tests(this);
        }

        public DivCentre centre(Div d)
        {
            return current.statuses[d.index()];
        }

        public DivCentre centre(int di)
        {
            return current.statuses[di];
        }

        protected override void stop()
        {
            updater.stop = true;
            base.stop();
            updater.stop = false;
        }

        protected override void init()
        {
            updater.init(current);
        }

        public void init(Div div)
        {
            bool started = thread.working();
            stop();
            updater.init(current, div);
            if (started)
                start();
        }

        protected override void load(FileGetter file)
        {
            throw new NotImplementedException();
        }

        protected override void doThreadJob()
        {
            updater.init(next[ci]);
            if (updater.stop)
                return;
            Context c = current;
            current = next[ci];
            next[ci] = c;
            ci++;
            ci %= next.Length;
        }

        private class Context
        {
            public readonly DivCentre[] statuses = new DivCentre[Config.battle().DIVISIONS_PER_BATTLE];

            public Context()
            {
                for (int i = 0; i < statuses.Length; i++)
                    statuses[i] = new DivCentre();
            }
        }
    }
}