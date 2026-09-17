using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using init.race;
using settlement.entity.humanoid;
using settlement.main;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.text;
using view.sett;
using view.ui.message;

namespace game.events.citizen
{
    final class EventCitizenEmmigrate : EventCitizen.SMALL_EVENT
    {
        private static readonly CharSequence ¤¤emigration = "¤Mass Emigrantion!";
        private static readonly CharSequence ¤¤emigrationD = "¤A large group of {RACE} have decided to leave your city, renouncing their citizenship, and your rule. This is a sign of weakness. Make sure you increase loyalty so that this will not happen again!";
        private static readonly StrInserter<Race> iRace = new StrInserter<Race>("RACE")
        {
            public override void Set(Race t, Str str)
            {
                str.Add(t.info.names);
            }
        };

        static EventCitizenEmmigrate()
        {
            D.ts(typeof(EventCitizenEmmigrate));
        }

        private readonly int[] emmigrations;

        public EventCitizenEmmigrate()
        {
            emmigrations = new int[RACES.all().size()];
            IDebugPanelSett.add("Event: Emmigration", new ACTION
            {
                public override void Exe()
                {
                    int ri = RND.rInt(RACES.all().size());
                    for (int i = 0; i < RACES.all().size(); i++)
                    {
                        Race r = RACES.all().getC(ri + i);
                        int am = (int)Math.Ceiling(STATS.POP().POP.data(HCLASSES.CITIZEN()).get(r) * RND.rFloat());
                        if (am > 0)
                        {
                            Event(am, r);
                            return;
                        }
                    }
                }
            });
        }

        public override void Save(FilePutter file)
        {
            file.isE(emmigrations);
        }

        public override void Load(FileGetter file) => file.isE(emmigrations);

        public override void Clear() => Array.Fill(emmigrations, 0);

        public bool ShouldEmigrate(Race r)
        {
            if (STATS.POP().POP.data(HCLASSES.CITIZEN()).get(r) == 0)
            {
                emmigrations[r.index()] = 0;
                return false;
            }
            return emmigrations[r.index()] > 0;
        }

        public void Emigrate(Humanoid h)
        {
            emmigrations[h.race().index()]--;
            if (emmigrations[h.race().index()] < 0)
            {
                emmigrations[h.race().index()] = 0;
            }
        }

        public override bool Event(int amH, Race hr)
        {
            if (SETT.ENTRY().IsClosed())
                return false;

            emmigrations[hr.index()] = amH;

            Str t = Str.TMP;
            t.Clear();
            t.Add(¤¤emigrationD);
            iRace.Insert(hr, t);
            new MessageText(¤¤emigration, t).Send();
            return true;
        }

        void Inc(int amH, Race hr) => emmigrations[hr.index()] += amH;

        public override void Update(double ds) { }
    }
}