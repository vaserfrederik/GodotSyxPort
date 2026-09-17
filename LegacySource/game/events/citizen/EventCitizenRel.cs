using System;
using System.Collections.Generic;
using System.IO;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.rnd;
using snake2d.util.sets;
using snake2d.util.sprite.text;
using util.text;
using view.sett;
using view.ui.message;

namespace game.events.citizen
{
    final class EventCitizenRel : EventCitizen.SMALL_EVENT
    {
        private static readonly CharSequence ¤¤title = "War of the faiths";
        private static readonly CharSequence ¤¤descRel = "Due to low happiness, a local dispute between two citizens of opposing faiths has spread across the whole city. Followers of {RELIGION_A} and {RELIGION_B} are now at each others throats and fighting each other wherever they meet. We must fix our happiness issues before this spreads any further.";
        private static readonly CharSequence ¤¤descRel2 = "The tension between religious factions have spread. Followers of {RELIGION_A} and {RELIGION_B} are now also at odds, and fighting each other wherever they meet.";

        private readonly StrInserter<Religion> irA = new StrInserter<Religion>("RELIGION_A")
        {
            protected override void Set(Religion t, Str str)
            {
                str.Add(t.diety);
            }
        };

        private readonly StrInserter<Religion> irB = new StrInserter<Religion>("RELIGION_B")
        {
            protected override void Set(Religion t, Str str)
            {
                str.Add(t.diety);
            }
        };

        private double timer;
        private readonly Bitmap1D map = new Bitmap1D(RELIGIONS.ALL().Count, false);

        static EventCitizenRel()
        {
            D.ts(typeof(EventCitizenRel));
        }

        public EventCitizenRel()
        {
            IDebugPanelSett.Add("Event: race war", new ACTION()
            {
                public void Exe()
                {
                    Event(0, FACTIONS.player().race());
                }
            });
            Clear();
        }

        public void Update(double ds)
        {
            timer -= ds;
        }

        public bool Event(int am, Race race)
        {
            if (timer <= 0)
                map.Clear();
            return SpawnRel(race);
        }

        private bool SpawnRel(Race race)
        {
            Religion a = Rel(race);
            if (a == null)
                return false;
            Religion b = RanRel(a);
            if (b == null)
                return false;

            map.Set(a.index(), true);
            map.Set(b.index(), true);
            Str s = new Str(timer > 0 ? ¤¤descRel2 : ¤¤descRel);
            irA.Insert(a, s);
            irB.Insert(b, s);
            timer = TIME.secondsPerDay() * (1 + RND.rFloat(2));
            new MessageText(¤¤title, s).Send();
            return true;
        }

        private Religion Rel(Race race)
        {
            double max = 0;
            for (int ri = 0; ri < RELIGIONS.ALL().Count; ri++)
            {
                Religion r = RELIGIONS.ALL()[ri];
                if (STATS.RELIGION().ALL.Get(r.index()).followers.data(HCLASSES.CITIZEN()).Get(race) > 0)
                {
                    max += STATS.RELIGION().ALL.Get(r.index()).followers.data(HCLASSES.CITIZEN()).Get(race);
                }
            }

            if (max == 0)
                return null;

            max *= RND.rFloat();

            for (int ri = 0; ri < RELIGIONS.ALL().Count; ri++)
            {
                Religion r = RELIGIONS.ALL()[ri];
                if (STATS.RELIGION().ALL.Get(r.index()).followers.data(HCLASSES.CITIZEN()).Get(race) > 0)
                {
                    max -= STATS.RELIGION().ALL.Get(r.index()).followers.data(HCLASSES.CITIZEN()).Get(race);
                    if (max <= 0)
                    {
                        return r;
                    }
                }
            }
            return null;
        }

        private Religion RanRel(Religion other)
        {
            double max = 0;
            for (int ri = 0; ri < RELIGIONS.ALL().Count; ri++)
            {
                Religion r = RELIGIONS.ALL()[ri];
                if (r != other && STATS.RELIGION().ALL.Get(r.index()).followers.data(HCLASSES.CITIZEN()).Get(null) > 0)
                {
                    max += STATS.RELIGION().ALL.Get(r.index()).followers.data(HCLASSES.CITIZEN()).Get(null);
                }
            }

            if (max == 0)
                return null;

            max *= RND.rFloat();

            for (int ri = 0; ri < RELIGIONS.ALL().Count; ri++)
            {
                Religion r = RELIGIONS.ALL()[ri];
                if (r != other && STATS.RELIGION().ALL.Get(r.index()).followers.data(HCLASSES.CITIZEN()).Get(null) > 0)
                {
                    max -= STATS.RELIGION().ALL.Get(r.index()).followers.data(HCLASSES.CITIZEN()).Get(null);
                    if (max <= 0)
                    {
                        return r;
                    }
                }
            }
            return null;
        }

        public bool IsAtOdds(Humanoid a, Humanoid b)
        {
            if (timer <= 0)
                return false;
            Religion ra = STATS.RELIGION().getter.Get(a.indu()).religion;
            Religion rb = STATS.RELIGION().getter.Get(b.indu()).religion;
            return ra != rb && map.Get(ra.index()) && map.Get(rb.index());
        }

        public void Save(FilePutter file)
        {
            map.Save(file);
            file.D(timer);
        }

        public void Load(FileGetter file)
        {
            map.Load(file);
            timer = file.D();
        }

        public void Clear()
        {
            timer = -1;
        }
    }
}