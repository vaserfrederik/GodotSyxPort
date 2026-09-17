using System;
using System.IO;
using System.Linq;
using game.faction;
using game.time;
using init.resources;
using snake2d.util.file;
using world.army;

namespace settlement.room.military.supply
{
    sealed class Cache : SAVABLE
    {
        private double[] lockedUntil = new double[RESOURCES.ALL().Count];

        private int debugAmount = 0;

        public Cache(ROOM_SUPPLY b)
        {
        }

        public int Needed(RESOURCE res)
        {
            int am = 0;
            foreach (ADSupply a in AD.Supplies().Get(res))
                am += a.Needed(FACTIONS.player());
            return am + debugAmount;
        }

        public int DeliverableSecret(RESOURCE res)
        {
            int needed = 0;
            foreach (ADSupply s in AD.Supplies().Get(res))
            {
                foreach (WArmy e in FACTIONS.player().Armies().All())
                {
                    needed += s.Needed(e);
                }
            }
            needed += debugAmount;
            return needed;
        }

        public int Deliverable(RESOURCE res)
        {
            if (lockedUntil[res.Index()] > TIME.CurrentSecond())
                return 0;
            int needed = DeliverableSecret(res);
            if (needed <= 0)
            {
                lockedUntil[res.Index()] = TIME.CurrentSecond() + TIME.SecondsPerDay() * 0.25;
                return 0;
            }
            return needed;
        }

        public int Deliver(RESOURCE res, int am)
        {
            if (debugAmount != 0)
                return am;

            double needed = Deliverable(res);
            if (needed <= 0)
            {
                return 0;
            }
            int delivered = 0;
            foreach (ADSupply s in AD.Supplies().Get(res))
            {
                foreach (WArmy e in FACTIONS.player().Armies().All())
                {
                    double n = s.Needed(e);
                    n /= needed;
                    int a = (int)Math.Ceiling(am * n);
                    if (a > am)
                        a = am;
                    delivered += a;
                    s.Current().Inc(e, a);
                }
            }
            return delivered;
        }

        public override void Save(FilePutter file)
        {
            file.DsE(lockedUntil);
        }

        public override void Load(FileGetter file)
        {
            file.DsE(lockedUntil);
        }

        public override void Clear()
        {
            lockedUntil = lockedUntil.Select(x => 0).ToArray();
        }
    }
}