using System;
using System.Collections.Generic;
using System.IO;

namespace game.faction.royalty
{
    public class Royalty : SuperBoostableObj
    {
        public readonly Induvidual induvidual;
        public readonly NPCCourt court;
        private readonly int deathDay;
        private bool event;
        public short eventMark;

        private readonly SuperData bdata = ROPINION.BOOST().makeData();

        private static string ¤¤Ruler = "¤Ruler of {0}";
        private static string ¤¤Heir = "¤First Heir of {0}";
        private static string ¤¤Heir2 = "¤Second Heir of {0}";
        private static string ¤¤Heir3 = "¤Third Heir of {0}";
        static Royalty()
        {
            D.ts(typeof(Royalty));
        }
        private readonly string[] sss = new string[] {
            ¤¤Ruler,
            ¤¤Heir,
            ¤¤Heir2,
            ¤¤Heir3
        };
        public readonly List<TRAIT> traits = new List<TRAIT>();

        Royalty(NPCCourt court, Race race)
        {
            induvidual = new Induvidual(HTYPES.NOBILITY(), race);
            this.court = court;

            int ls = STATS.POP().age.lifespan(induvidual);
            int min = ls / 4;
            int dd = ls - 2 * min;
            int days = (min + RND.rInt(dd));
            STATS.POP().age.DAYS.set(induvidual, days);

            deathDay = (int)(TIME.days().bitsSinceStart() + 1 + (ls - days) * RND.rFloat());
            setTitles();
        }

        private void setTitles()
        {
            traits.Clear();
            foreach (TRAIT t in TRAITS.tmp(induvidual, 3))
            {
                if (traits.Count == 0 || Math.Abs(t.get(induvidual) - 0.5) > 0.2)
                    traits.Add(t);
            }
        }

        Royalty(NPCCourt court, FileGetter file) : this(court, file)
        {
            this.court = court;
            induvidual = new Induvidual(file);
            deathDay = file.i();
            bdata.load(file);
            event = file.bool();
            eventMark = file.s();
            setTitles();
        }

        public void update(double seconds)
        {
            ROPINION.BOOST().update(this, seconds);
        }

        void save(FilePutter file)
        {
            induvidual.save(file);
            file.i(deathDay);
            bdata.save(file);
            file.bool(event);
            file.s(eventMark);
        }

        public void kill(bool sendMessage)
        {
            court.kill(this);
        }

        public bool isKing()
        {
            return court.king().roy() == this;
        }

        public string name()
        {
            if (isKing())
                return court.king().name;
            return STATS.APPEARANCE().nameLast.name(induvidual);
        }

        public Str nameFull(Str s)
        {
            if (isKing())
            {
                s.add(court.king().name);
                return court.king().name;
            }
            else
                s.add(STATS.APPEARANCE().name(induvidual));
            return s;
        }

        public Str nameSucc(Str s)
        {
            s.add(sss[successionI()]).insert(0, court.faction.name);
            return s;
        }

        public int successionI()
        {
            return court.all.IndexOf(this);
        }

        public bool @event()
        {
            return event;
        }

        public void eventSet(bool b)
        {
            event = b;
        }

        public override SuperData boostingData()
        {
            return bdata;
        }
    }
}