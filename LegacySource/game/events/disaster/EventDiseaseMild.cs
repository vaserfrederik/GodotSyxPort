using System;
using System.IO;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sprite.text;
using util.text;
using view.ui.message;
using settlement.main;
using settlement.stats;
using init.type;

namespace game.events.disaster
{
    public sealed class EventDiseaseMild : EventResource
    {
        private static readonly CharSequence ¤¤titleF = "Disease at our doorstep!";
        private static readonly CharSequence ¤¤descF = "There is rumour that the lands to the east have been ravaged by a deadly disease, leaving cities in ruins and emptying villages of the living. Let us hope it does not reach our lands.";

        private static readonly CharSequence ¤¤title = "Epidemic!";
        private static readonly CharSequence ¤¤desc = "It has come to us. The dreaded {0}. Nothing could have been done, it was the will of the gods. Lets hope our hospitals are well staffed. A curfew can be issued to contain it.";

        static EventDiseaseMild()
        {
            D.ts(typeof(EventDiseaseMild));
        }

        private readonly double maxTime = 16 * 16 * TIME.secondsPerDay();
        private double timer = 0;
        private double ran = RND.rFloat();

        public EventDiseaseMild() : base("DISEASE_MILD")
        {
            Reset();
        }

        protected override void save(FilePutter file)
        {
            file.d(timer);
            file.d(ran);
        }

        protected override void load(FileGetter file)
        {
            timer = file.d();
            ran = file.d();
        }

        protected override void clear()
        {
            Reset();
        }

        protected override void update(double ds)
        {
            if (POP.tot(null, null) < 1000)
                return;

            double t = timer;

            timer += ds;

            if (t < maxTime - TIME.secondsPerDay() * 4 && timer > maxTime - TIME.secondsPerDay() * 4)
            {
                new MessageText(¤¤titleF).paragraph(¤¤descF).send();
            }

            if (timer < maxTime)
                return;

            DISEASE de = DISEASES.randomEpidemic(ran);
            Reset();

            if (de == null || SETT.INVADOR().invading())
            {
                timer -= TIME.secondsPerDay();
                return;
            }

            double eff = 0.25 + 0.75 * POP.tot(null, null) / 20000.0;

            if (STATS.DISEASE().outbreak(de.infectRate * eff, de))
            {
                MessageText te = new MessageText(¤¤title);
                Str.TMP.clear().add(¤¤desc).insert(0, de.info.name);
                te.paragraph(Str.TMP);
                Str.TMP.clear().add(de.info.name).NL().add(de.info.desc);
                te.paragraph(Str.TMP);
                te.send();
            }
        }

        private void Reset()
        {
            timer = 0;
            timer -= (1 + RND.rFloat() * 2) * TIME.secondsPerDay();
            ran = RND.rFloat();
        }
    }
}