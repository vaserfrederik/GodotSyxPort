using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.boosting;
using game.faction;
using game.time;
using init.resources;
using settlement.entity.humanoid;
using settlement.room.industry.module;
using settlement.room.main;
using snake2d.util.file;
using snake2d.util.sets;
using util.data;
using util.statistics;

namespace settlement.room.industry.module.consumption
{
    public class RoomConsumptionAbs : SAVABLE, IndustryRate
    {
        protected readonly ArrayListGrower<IndustryResource> allIns = new ArrayListGrower<IndustryResource>();
        protected readonly ArrayListGrower<IndustryResource> allRes = new ArrayListGrower<IndustryResource>();
        protected IndustryResourceIn[] inMap = new IndustryResourceIn[RESOURCES.ALL().size()];

        private readonly Boostable bonus;
        public readonly ArrayListGrower<RoomBoost> roomBoosts = new ArrayListGrower<RoomBoost>();
        public readonly RoomBlueprintImp blue;
        protected readonly INT_OE<ROOM_IDATA_INSTANCE> pday;
        public Boostable conBonus = null;

        protected readonly DataOSimple<ROOM_IDATA_INSTANCE> data = new DataOSimple<ROOM_IDATA_INSTANCE>()
        {
            protected override long[] data(ROOM_IDATA_INSTANCE t)
            {
                return t.productionData();
            }
        };

        public RoomConsumptionAbs(RoomBlueprintImp blue, Boostable bonus)
        {
            pday = data.new DataNibble();
            this.blue = blue;
            this.bonus = bonus;
        }

        public IndustryResource In(RESOURCE res)
        {
            return inMap[res.index()];
        }

        public void UpdateRoom(ROOM_IDATA_INSTANCE r)
        {
            if (pday.Get(r) != (TIME.days().bitCurrent() & 0x0F))
            {
                pday.Set(r, (TIME.days().bitCurrent() & 0x0F));
                bool year = TIME.days().bitsSinceStart() % TIME.years().bitConversion(TIME.days()) == 0;
                foreach (IndustryResource i in allRes)
                {
                    int v = (int)i.day.Get(r);
                    i.dayPrev.Set(r, v);
                    i.day.IncD(r, -v);
                    if (year)
                    {
                        i.yearPrev.Set(r, i.year.Get(r));
                        i.year.Set(r, 0);
                    }
                }
            }
        }

        public LIST<IndustryResource> Ins()
        {
            return allIns;
        }

        public Boostable Bonus()
        {
            return bonus;
        }

        public LIST<RoomBoost> Boosts()
        {
            return roomBoosts;
        }

        public long[] MakeData()
        {
            return new long[data.longCount()];
        }

        public long[] MakeDataFix(long[] old)
        {
            if (old.Length != data.longCount())
                return new long[data.longCount()];
            return old;
        }

        public void Save(FilePutter file)
        {
            file.I(allRes.size());
            foreach (IndustryResource r in allRes)
                r.Save(file);
        }

        public void Load(FileGetter file) throws IOException
        {
            int am = file.I();
            if (am != allRes.size())
            {
                HistoryInt history = new HistoryInt(48, TIME.days(), false);
                for (int i = 0; i < am; i++)
                    history.Load(file);
                Clear();
            }
            else
            {
                foreach (IndustryResource r in allRes)
                    r.Load(file);
            }
        }

        public void Clear()
        {
            foreach (IndustryResource r in allRes)
                r.Clear();
        }

        protected class IndustryResourceIn : IndustryResource
        {
            public IndustryResourceIn(DataOSimple<ROOM_IDATA_INSTANCE> data, RESOURCE res, double rate, double AI, double AIRate)
                : base(data, allIns.size(), res, rate, AI, AIRate)
            {
                allIns.Add(this);
                inMap[resource.index()] = this;
                allRes.Add(this);
            }

            public override int Inc(ROOM_IDATA_INSTANCE r, double amount, bool record)
            {
                int old = (int)day.Get(r);
                day.IncD(r, amount);
                int now = (int)day.Get(r);
                int d = now - old;
                if (record)
                    GAME.player().res().Inc(resource, RTYPE.PRODUCED, -d);
                year.Inc(r, d);
                history.Inc(d);
                return d;
            }

            protected override double GetEffort(Humanoid skill, ROOM_IDATA_INSTANCE r, double workSeconds)
            {
                return IndustryUtil.CalcConsumptionRate(rateSeconds * workSeconds, skill, (RoomInstance)r, RoomConsumptionAbs.this);
            }
        }

        public double ConsumptionRate(RoomInstance ins, Humanoid h, IndustryResource oo)
        {
            return ins.employees().totEfficiency() * IndustryUtil.CalcConsumptionRate(oo.rate, h, ins, this);
        }

        public double ConBonus(BOOSTABLE_O bo)
        {
            if (conBonus == null)
                return 1;
            return conBonus.Get(bo);
        }
    }
}