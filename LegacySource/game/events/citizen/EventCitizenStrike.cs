using System;
using System.Collections.Generic;
using System.IO;
using game.time;
using init.race;
using init.type;
using settlement.entity.humanoid;
using settlement.main;
using settlement.room.main.employment;
using settlement.stats;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.text;
using view.sett;
using view.ui.message;

namespace game.events.citizen
{
    internal class EventCitizenStrike : EventCitizen.SMALL_EVENT
    {
        private static readonly CharSequence ¤¤strike = "¤Worker Strike!";
        private static readonly CharSequence ¤¤strikeD = "¤Your {RACE} workers have halted all work in our {WORKPLACES}, in protest of what they call your bad judgement. Take measures to increase their loyalty so that it doesn't happen again.";
        private static readonly CharSequence ¤¤strikeOver = "¤Strike Over";
        private static readonly CharSequence ¤¤strikeOverD = "¤Your {WORKPLACES} have resumed work and the strike is over.";

        private RoomEmployment strike = null;
        private double strikeTimer = 0;
        private StrInserter<Race> iRace = new StrInserter<Race>("RACE")
        {
            public override void Set(Race t, Str str)
            {
                str.Add(t.info.namePosessive);
            }
        };

        private StrInserter<RoomEmployment> iWork = new StrInserter<RoomEmployment>("WORKPLACES")
        {
            public override void Set(RoomEmployment t, Str str)
            {
                str.Add(t.blueprint().info.names);
            }
        };

        static EventCitizenStrike()
        {
            D.ts(typeof(EventCitizenStrike));
        }

        public EventCitizenStrike()
        {
            IDebugPanelSett.Add("Event: Strike", new ACTION()
            {
                public override void Exe()
                {
                    int ri = RND.rInt(RACES.all().Count);
                    for (int i = 0; i < RACES.all().Count; i++)
                    {
                        Race r = RACES.all().GetC(ri + i);
                        if (Event(0, r))
                            return;
                    }
                }
            });
        }

        public void Save(FilePutter file)
        {
            file.I(strike == null ? -1 : strike.index());
            file.D(strikeTimer);
        }

        public void Load(FileGetter file) throws IOException
        {
            int i = file.I();
            if (i == -1)
                strike = null;
            else
                strike = SETT.ROOMS().employment.ALL().Get(i);
            strikeTimer = file.D();
        }

        public void Clear()
        {
            strikeTimer = 0;
        }

        public bool Event(int am, Race hr)
        {
            RoomEmployment strike = null;
            int most = 0;
            foreach (RoomEmployment e in SETT.ROOMS().employment.ALL())
            {
                if (e.Employed(WGROUP.Get(HTYPES.SUBJECT(), hr)) > most)
                {
                    strike = e;
                    most = e.Employed(WGROUP.Get(HTYPES.SUBJECT(), hr));
                }
            }

            if (strike == null)
                return false;

            this.strike = strike;
            this.strikeTimer = TIME.secondsPerDay() * 1.5;

            Str s = Str.TMP.Clear();
            s.Add(¤¤strikeD);
            iRace.Insert(hr, s);
            iWork.Insert(strike, s);
            new MessageText(¤¤strike, s).Send();

            return true;
        }

        public void Update(double ds)
        {
            if (strikeTimer <= 0)
                return;

            strikeTimer -= ds;
            if (strikeTimer <= 0)
            {
                Str s = Str.TMP.Clear();
                s.Add(¤¤strikeOverD);
                iWork.Insert(strike, s);
                new MessageText(¤¤strikeOver, s).Send();
            }
        }

        public bool IsStriking(Humanoid h)
        {
            return strikeTimer > 0 && STATS.WORK().EMPLOYED.Get(h) != null && STATS.WORK().EMPLOYED.Get(h).blueprintI().Employment() == strike;
        }
    }
}