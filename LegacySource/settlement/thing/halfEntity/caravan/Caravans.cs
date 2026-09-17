using System;
using System.IO;
using System.Collections.Generic;
using init.resources;
using init.trade;
using settlement.main;
using settlement.thing.halfEntity.Factory;
using settlement.trade;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sets;

namespace settlement.thing.halfEntity.caravan
{
    public class Caravans : Factory<Caravan>
    {
        private readonly Type export = new TypeWithDraw();
        private readonly Type delivery = new TypeDeliver();

        public readonly TradableData[] deliveries;
        public readonly TradableData[] withdrawals;

        public Caravans(LISTE<Factory<?>> all) : base(all)
        {
            deliveries = new TradableData[RESOURCES.ALL().size()];
            withdrawals = new TradableData[RESOURCES.ALL().size()];

            foreach (RESOURCE r in RESOURCES.ALL())
            {
                deliveries[r.index()] = new TradableData();
                withdrawals[r.index()] = new TradableData();
            }
        }

        protected override void save(FilePutter file)
        {
            RESOURCES.map().saver().save(deliveries, file);
            RESOURCES.map().saver().save(withdrawals, file);
        }

        protected override void load(FileGetter file)
        {
            RESOURCES.map().loader().load(deliveries, file);
            RESOURCES.map().loader().load(withdrawals, file);
        }

        protected override void clear()
        {
            foreach (TradableData t in deliveries)
                t.clear();
            foreach (TradableData t in withdrawals)
                t.clear();
        }

        public int deliveries(RESOURCE res, TRADE_TYPE t)
        {
            return deliveries[res.index()].get(t);
        }

        public int withdrawals(RESOURCE res, TRADE_TYPE t)
        {
            return withdrawals[res.index()].get(t);
        }

        protected override Caravan make()
        {
            return new Caravan();
        }

        public bool createFetcher(RESOURCE res, int amount, TRADE_TYPE ttype)
        {
            return create(res, amount, export, ttype);
        }

        public bool createDelivery(RESOURCE res, int amount, TRADE_TYPE ttype)
        {
            if (create(res, amount, delivery, ttype))
                return true;
            return false;
        }

        private bool create(RESOURCE res, int amount, Type type, TRADE_TYPE ttype)
        {
            COORDINATE coo = SETT.ENTRY().points.randomReachable();
            if (coo == null)
                return false;

            Caravan c = create();
            if (c.init(coo.x(), coo.y(), type, res, amount, ttype))
            {
                return true;
            }
            else
            {
                type.cancel(c, false);
            }
            return false;
        }
    }
}