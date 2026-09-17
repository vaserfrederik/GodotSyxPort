using System;
using System.IO;
using game.boosting;
using game.events.EVENTS;
using game.time;
using init.type;
using settlement.main;
using settlement.stats;
using settlement.stats.standing;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;
using view.sett;
using view.ui.message;

namespace game.events.disaster
{
    public sealed class EventDisease : EventResource
    {
        private static readonly CharSequence ¤¤title = "Outbreak!";
        private static readonly CharSequence ¤¤desc = "Dreadful news! On account of poor health and sanitation, an epidemic of {0} has been discovered. Lets hope our hospitals are well staffed. A curfew can be issued to contain it.";

        static EventDisease()
        {
            D.ts(typeof(EventDisease));
        }

        private readonly double maxTime = 8 * TIME.secondsPerDay();
        private double timer = 1;
        private double ran = RND.rFloat();
        private double spread = 0;
        private double warmup = 0.25;

        public EventDisease() : base("DISEASE")
        {
            Reset();
            IDebugPanelSett.Add("EVENT DISEASE", new ACTION
            {
                exe = () => Set()
            });
        }

        protected override void Save(FilePutter file)
        {
            file.d(timer);
            file.d(spread);
            file.d(ran);
            file.d(warmup);
        }

        protected override void Load(FileGetter file)
        {
            timer = file.d();
            spread = file.d();
            ran = file.d();
            warmup = file.d();
        }

        protected override void Clear()
        {
            Reset();
        }

        protected override void Update(double ds)
        {
            double d = 1 - BOOSTABLES.PHYSICS().HEALTH.Get(HCLASS_RACE.clP());
            if (d < 0)
            {
                timer += ds;
                timer = CLAMP.d(timer, 0, 16.0 * TIME.secondsPerDay());
                return;
            }

            timer -= ds * d;

            if (timer < 0)
            {
                Set();
            }
        }

        private void Set()
        {
            DISEASE de = DISEASES.randomEpidemic(ran);

            if (de == null || SETT.INVADOR().invading())
            {
                timer += 0.1;
                return;
            }

            warmup = CLAMP.d(warmup, 0.25, 1);

            double o = warmup * spread * de.infectRate;
            warmup += warmup;

            if (STATS.DISEASE().outbreak(o, de))
            {
                MessageText te = new MessageText(¤¤title);
                Str.TMP.Clear().Add(¤¤desc).Insert(0, de.info.name);
                te.Paragraph(Str.TMP);
                Str.TMP.Clear().Add(de.info.name).NL().Add(de.info.desc);
                te.Paragraph(Str.TMP);
                te.Send();
                STANDINGS.emergency(HCLASSES.CITIZEN(), TIME.secondsPerDay() * 8);
            }

            Reset();
        }

        private void Reset()
        {
            timer = maxTime;
            timer *= 1 + RND.rFloat() * 2;
            ran = RND.rFloat();
            spread = 0.6 + 0.4 * RND.rFloat();
        }
    }
}