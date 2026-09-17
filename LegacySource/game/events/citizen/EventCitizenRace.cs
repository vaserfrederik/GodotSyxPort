using System;
using System.Collections.Generic;
using System.IO;
using game.faction;
using game.time;
using init.race;
using settlement.entity.humanoid;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.text;
using view.sett;
using view.ui.message;

namespace game.events.citizen
{
    public class EventCitizenRace : EventCitizen.SMALL_EVENT
    {
        private static readonly CharSequence ¤¤title = "Brawls!";
        private static readonly CharSequence ¤¤desc = "A local dispute between a {RACE_A} and {RACE_B} citizen has spread across the whole city. The two species are now at each others throats and fighting each other wherever they meet. We must fix our happiness issues before this spreads any further.";

        private readonly StrInserter<Race> iA = new StrInserter<Race>("RACE_A")
        {
            protected override void Set(Race t, Str str)
            {
                str.Add(t.info.namePosessive);
            }
        };

        private readonly StrInserter<Race> iB = new StrInserter<Race>("RACE_B")
        {
            protected override void Set(Race t, Str str)
            {
                str.Add(t.info.namePosessive);
            }
        };

        private int ra;
        private int rb;
        private double timer;

        static EventCitizenRace()
        {
            D.ts(typeof(EventCitizenRace));

            IDebugPanelSett.Add("Event: race war", new ACTION
            {
                public void Exe()
                {
                    Event(100, FACTIONS.player().race());
                }
            });
        }

        public EventCitizenRace()
        {
            Clear();
        }

        public void Update(double ds)
        {
            timer -= ds;
        }

        public bool Event(int am, Race race)
        {
            ra = -1;
            rb = -1;

            return SpawnRace(race);
        }

        private bool SpawnRace(Race race)
        {
            double max = 0;
            for (int ri = 0; ri < RACES.all().Count; ri++)
            {
                Race r = RACES.all()[ri];
                if (STATS.POP().POP.data(HCLASSES.CITIZEN()).get(r) > 0 && race != r && race.pref().race(r) < 1.0)
                {
                    max += STATS.POP().POP.data(HCLASSES.CITIZEN()).get(r) * (1 - race.pref().race(r));
                }
            }
            if (max == 0)
                return false;

            max *= RND.rFloat();

            for (int ri = 0; ri < RACES.all().Count; ri++)
            {
                Race r = RACES.all()[ri];
                if (STATS.POP().POP.data(HCLASSES.CITIZEN()).get(r) > 0 && race != r && race.pref().race(r) < 1)
                {
                    max -= STATS.POP().POP.data(HCLASSES.CITIZEN()).get(r) * (1 - race.pref().race(r));
                    if (max <= 0)
                    {
                        timer = TIME.secondsPerDay() * (1 + RND.rFloat(2));
                        ra = race.index;
                        rb = r.index;
                        Str s = new Str(¤¤desc);
                        iA.Insert(race, s);
                        iB.Insert(r, s);
                        new MessageText(¤¤title, s).Send();
                    }
                }
            }
            return true;
        }

        public bool IsAtOdds(Humanoid a, Humanoid b)
        {
            if (timer <= 0)
                return false;
            if (ra != -1)
            {
                return (a.race().index == ra && b.race().index == rb) || (a.race().index == rb && b.race().index == ra);
            }
            return false;
        }

        public void Save(FilePutter file)
        {
            file.I(ra);
            file.I(rb);
            file.D(timer);
        }

        public void Load(FileGetter file)
        {
            ra = file.I();
            rb = file.I();
            timer = file.D();
        }

        public void Clear()
        {
            timer = -1;
        }
    }
}