using System;
using System.Collections.Generic;
using System.IO;
using game.time;
using init.resources;
using init.trade;
using snake2d.util.file;
using snake2d.util.sets;
using util.statistics;
using util.text;

namespace game.faction
{
    public abstract class FResources : FactionResource
    {
        private static readonly CharSequence ¤¤worn = "¤Furniture";
        private static readonly CharSequence ¤¤theft = "¤Theft";

        static FResources()
        {
            D.ts(typeof(FResources));
        }

        private readonly HistoryTradable[] all;
        public readonly TIMECYCLE time;

        public FResources(int saved, TIMECYCLE time)
        {
            all = new HistoryTradable[RTYPE.all.size() * 2 + 1];
            for (int i = 0; i < all.Length; i++)
                all[i] = new HistoryTradable(saved, time, false);
            this.time = time;
        }

        public abstract int GetAvailable(TRADABLE t);

        public HISTORY_COLLECTION<TRADABLE> In(RTYPE t)
        {
            return all[t.ordinal()];
        }

        public HISTORY_COLLECTION<TRADABLE> Out(RTYPE t)
        {
            return all[RTYPE.all.size() + t.ordinal()];
        }

        public HISTORY_COLLECTION<TRADABLE> Total()
        {
            return all[all.Length - 1];
        }

        public void Inc(TRADABLE res, RTYPE type, int am)
        {
            if (am > 0)
                all[type.ordinal()].Inc(res, am);
            else
                all[RTYPE.all.size() + type.ordinal()].Inc(res, -am);
            all[all.Length - 1].Inc(res, am);
        }

        public void Inc(RESOURCE res, RTYPE type, int am)
        {
            Inc(TR.Get(res), type, am);
        }

        public void Dec(TRADABLE res, RTYPE type, int am)
        {
            Inc(res, type, -am);
        }

        protected override void Save(FilePutter file)
        {
            foreach (HistoryTradable rr in all)
                rr.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            foreach (HistoryTradable rr in all)
                rr.Load(file);
        }

        public override void Clear()
        {
            foreach (HistoryTradable i in all)
            {
                i.Clear();
            }
        }

        protected override void Update(double ds, Faction f)
        {
            // TODO Auto-generated method stub
        }

        public enum RTYPE
        {
            PRODUCED,
            CONSUMED,
            TRADE,
            TAX,
            CONSTRUCTION,
            FURNISH,
            EQUIPPED,
            MAINTENANCE,
            SPOILAGE,
            ARMY_SUPPLY,
            SPOILS,
            DIPLOMACY,
            THEFT,
        }

        public static LIST<RTYPE> All => new ArrayList<RTYPE>(Enum.GetValues(typeof(RTYPE)));

        public readonly CharSequence name;

        private RTYPE(CharSequence name)
        {
            this.name = name;
        }
    }
}