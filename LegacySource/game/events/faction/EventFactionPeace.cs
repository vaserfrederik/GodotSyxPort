using System;
using System.IO;
using System.Collections.Generic;

namespace game.events.faction
{
    public class EventFactionPeace : EventResource
    {
        private double[] secondWhenWarEnds = new double[FACTIONS.MAX];

        public EventFactionPeace() : base("FACTION_PEACE")
        {
            new DIP.DipActivityListener()
            {
                public void Change(Faction a, Faction b, DipStance old, DipStance nn)
                {
                    if (nn == DIP.WAR())
                    {
                        secondWhenWarEnds[b.Index] = PeaceTime();
                        secondWhenWarEnds[a.Index] = PeaceTime();
                    }
                }
            };
        }

        private double PeaceTime()
        {
            return TIME.PlayedGame + TIME.SecondsPerDay + RND.rFloat() * TIME.SecondsPerDay * 32.0;
        }

        private readonly IUpdater updater = new IUpdater(FACTIONS.MAX, TIME.SecondsPerDay / 2)
        {
            protected override void Update(int i, double timeSinceLast)
            {
                Faction f = FACTIONS.GetByIndex(i);
                if (f.IsActive && f is FactionNPC)
                {
                    Up((FactionNPC)f);
                }
            }
        };

        protected override void Update(double ds)
        {
            updater.Update(ds);
        }

        private void Up(FactionNPC f)
        {
            if (DIP.WAR.All(f).Count == 0)
                return;

            if (DIP.WAR.Is(f))
                return;

            if (DIP.ALLY.Is(f))
                return;

            if (TIME.PlayedGame > secondWhenWarEnds[f.Index])
            {
                secondWhenWarEnds[f.Index] = PeaceTime() / 2.0;
                Faction e = DIP.WAR.All(f).Rnd();
                if (e == null)
                    return;

                if (e == FACTIONS.Player())
                    return;

                if (DIP.WAR.Is((FactionNPC)e) && DIP.ALLY.Is(f))
                    return;

                DIP.NEUTRAL.Set(f, e);
            }
            else
            {
            }
        }

        protected override void Save(FilePutter file)
        {
            updater.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            updater.Load(file);
        }

        protected override void Clear()
        {
            updater.Clear();
        }
    }
}