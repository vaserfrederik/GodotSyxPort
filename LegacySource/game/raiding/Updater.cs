using System;
using System.IO;
using game;
using game.time;
using snake2d.util.file;
using util.updating;

namespace game.raiding
{
    class Updater : IUpdater
    {
        private static double raidingInterval = 6 * 16 * TIME.secondsPerDay();
        private double timer = 0;

        public Updater(RAIDING r) : base(r.AMOUNT, TIME.secondsPerDay())
        {
        }

        protected override void update(int i, double timeSinceLast)
        {
            if (GAME.raiders().current.current() != null)
                return;

            Raider r = GAME.raiders().ALL().get(i);
            if (!r.defeated && r.hasInterrest() && !r.isScared())
            {
                timer += timeSinceLast;

                if (timer >= raidingInterval)
                {
                    r.text.set(r, r.raids == 0);
                    GAME.raiders().current.raid(r);
                    timer -= raidingInterval * Math.Sqrt(GAME.raiders().ALL().size());
                }
            }
        }

        public override void save(FilePutter file)
        {
            file.d(timer);
            base.save(file);
        }

        public override void load(FileGetter file)
        {
            timer = file.d();
            base.load(file);
        }

        public override void clear()
        {
            timer = 0;
            base.clear();
        }
    }
}