using System;
using System.Collections.Generic;
using game.boosting;
using game.faction;
using game.time;
using init.paths;
using init.sprite.UI;
using snake2d.util.sets;
using util.keymap;
using util.text;

namespace init.type
{
    public class NEEDS
    {
        private static NEEDS self;

        static NEEDS()
        {
            D.gInit(typeof(NEEDS));
        }

        private readonly BoostableCat bCat = new BoostableCat("RATES_", D.g("Service", "Service Needs"), "", BoostableCat.TYPE_SETT, UI.icons().s.house);
        private readonly BoostableCat bCatE = new BoostableCat("RATES_", D.g("Basic Needs"), "", BoostableCat.TYPE_SETT, UI.icons().s.house);

        private readonly ArrayListGrower<NEED> ALL = new ArrayListGrower<NEED>();
        private readonly ArrayListGrower<NEED> ALLNE = new ArrayListGrower<NEED>();
        private readonly ArrayListGrower<NEED_E> ALLE = new ArrayListGrower<NEED_E>();
        private readonly RMAP<NEED> coll;
        private readonly Types types;
        private readonly ResFolder f = PATHS.STATS().folder("need");

        public NEEDS()
        {
            self = this;

            ResFolder f = PATHS.STATS().folder("need");

            foreach (string k in f.init.getFiles())
            {
                new NEED(k, f, ALL, bCat, null, false);
            }

            types = new Types();

            var events = new ArrayListGrower<NEED>();

            foreach (NEED n in ALL)
            {
                if (n.event > 1)
                    events.add(n);
                if (n is NEED_E)
                    continue;
                ALLNE.add(n);
            }

            {
                int days = events.size() * 2 + 1;
                int day = 0;
                foreach (NEED n in events)
                {
                    new Event(day, days, n);
                    day += 2;
                }
            }

            coll = new RMAP<NEED>("NEED", ALL);
        }

        private static readonly string ¤¤event = "Small Event";
        static NEEDS()
        {
            D.ts(typeof(NEEDS));
        }

        private class Event : BoosterImp
        {
            private readonly int day;
            private readonly int days;

            public Event(int day, int days, NEED need) : base(new BSourceInfo(¤¤event, UI.icons().s.arrowUp), 1, need.event, true)
            {
                this.day = day;
                this.days = days;
                add(need.rate);
            }

            public override double vGet(Faction f)
            {
                return (TIME.days().bitsSinceStart() % days) == day ? 1 : 0;
            }
        }

        public static LIST<NEED> ALL()
        {
            return self.ALL;
        }

        public static LIST<NEED> ALLSIMPLE()
        {
            return self.ALLNE;
        }

        public static LIST<NEED_E> ALLE()
        {
            return self.ALLE;
        }

        public static BoostableCat bCat()
        {
            return self.bCat;
        }

        public static BoostableCat bCatE()
        {
            return self.bCatE;
        }

        public static Types TYPES()
        {
            return self.types;
        }

        public static RMAP<NEED> MAP()
        {
            return self.coll;
        }

        public class Types
        {
            public readonly NEED_E HUNGER = new NEED_E("_HUNGER", f, ALL, ALLE, bCatE);
            public readonly NEED_E THIRST = new NEED_E("_THIRST", f, ALL, ALLE, bCatE);
            public readonly NEED_E SHOPPING = new NEED_E("_SHOPPING", f, ALL, ALLE, bCatE);
            public readonly NEED SKINNYDIP = new NEED("_SKINNYDIP", f, ALL, bCat, UI.icons().s.drop, false);
            public readonly NEED TEMPLE = new NEED("_TEMPLE", f, ALL, bCat, UI.icons().s.temple, false);
            public readonly NEED SHRINE = new NEED("_SHRINE", f, ALL, bCat, UI.icons().s.shrine, false);
        }
    }
}