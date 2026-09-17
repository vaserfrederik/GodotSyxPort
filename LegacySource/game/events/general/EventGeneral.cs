using System;
using System.Collections.Generic;
using System.IO;

namespace game.events.general
{
    public sealed class EventGeneral : EventResource
    {
        private readonly EventCollection coll;

        private readonly double eventsPerSecondLow;
        private readonly double eventsPerSecondHigh;
        private readonly SortedSet<Event> spawnSort;

        private double timer = 0;

        public EventGeneral() : base("ENGINE")
        {
            double d = Json.ReadJson(PATHS.EVENT().init.GetFile("_CONFIG")).GetDouble("DAYS_BETWEEN_EVENTS_POP_0", 0, 1000);
            eventsPerSecondLow = 1.0 / (d * TIME.SecondsPerDay());

            d = Json.ReadJson(PATHS.EVENT().init.GetFile("_CONFIG")).GetDouble("DAYS_BETWEEN_EVENTS_POP_5000", 0, 1000);
            eventsPerSecondHigh = 1.0 / (d * TIME.SecondsPerDay());

            coll = new EventCollection(PATHS.EVENT());

            spawnSort = new SortedSet<Event>(new EventComparer());

            IDebugPanel.Add("Spawn next event", new ACTION
            {
                Exe = () => spawn()
            });
        }

        protected override void Save(BinaryWriter file)
        {
            file.Write(timer);
        }

        protected override void Load(BinaryReader file)
        {
            timer = file.ReadDouble();
        }

        protected override void Clear()
        {
            timer = 0;
        }

        protected override void Update(double ds)
        {
            if (GAME.EVENT().Current != null)
                return;

            if (timer >= coll.All.Count)
            {
                if (!spawn())
                    timer -= 5;
                else
                    timer -= (int)timer;
            }

            int no = (int)timer;

            double d = POP.Tot(null, null) / 10000.0;
            d = Math.Clamp(d, 0, 1);
            d *= (eventsPerSecondHigh - eventsPerSecondLow);

            d = eventsPerSecondLow + d;

            timer += ds * d * coll.All.Count;
            int nn = (int)timer;

            for (; no < nn && no < coll.All.Count; no++)
            {
                Event e = coll.All[no];
                GAME.EVENT().AccInc(e);
            }
        }

        private bool spawn()
        {
            spawnSort.Clear();
            foreach (Event e in coll.All)
            {
                if (GAME.EVENT().Acc(e) > 0)
                {
                    spawnSort.Add(e);
                }
            }

            int m = 0;

            while (m++ < 5 && spawnSort.Count > 0)
            {
                Event e = spawnSort.Max;
                spawnSort.Remove(e);
                if (GAME.EVENT().TrySet(e))
                {
                    return true;
                }
            }
            return spawnSort.Count == 0;
        }
    }

    internal class EventComparer : IComparer<Event>
    {
        public int Compare(Event x, Event y)
        {
            return GAME.EVENT().Acc(y).CompareTo(GAME.EVENT().Acc(x));
        }
    }
}