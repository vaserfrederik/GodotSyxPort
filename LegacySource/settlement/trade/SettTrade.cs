using System;
using System.IO;
using System.Linq;
using game;
using game.debug;
using game.faction.npc.stockpile;
using game.time;
using init.race;
using init.resources;
using init.trade;
using settlement.entity.humanoid;
using settlement.main;
using snake2d.util.file;
using util.updating;

namespace settlement.trade
{
    public class SettTrade : SettResource
    {
        private readonly PBuyer[] buyers;
        private readonly PSeller[] sellers;
        private readonly int[] slavesReserved;

        private readonly IUpdater updater;

        public SettTrade() : base("TRADE", false)
        {
            buyers = new PBuyer[TR.ALL().Count()];
            sellers = new PSeller[TR.ALL().Count()];
            slavesReserved = Alloc.ii(RACES.all().Count());

            updater = new IUpdater(TR.ALL().Count(), TIME.secondsPerDay() / 8)
            {
                Update = (i, timeSinceLast) =>
                {
                    buyers[i].deliver();
                    sellers[i].extract();
                }
            };

            foreach (TRADABLEO<RESOURCE> rr in TR.RES())
            {
                buyers[rr.index()] = new PBuyerRes(rr);
                sellers[rr.index()] = new PSellerRes(rr);
            }

            foreach (TRADABLEO<Race> ss in TR.SLAVES())
            {
                buyers[ss.index()] = new PBuyerSlave(ss);
                sellers[ss.index()] = new PSellerSlave(ss);
            }

            if (buyers.Any(b => b == null))
            {
                throw new Exception("no trade implementation of trade type");
            }

            clear();
        }

        public PBuyer buyer(TRADABLE t)
        {
            return buyers[t.index()];
        }

        public PSeller seller(TRADABLE t)
        {
            return sellers[t.index()];
        }

        protected override void update(double ds, Profiler profiler)
        {
            updater.update(ds);
            base.update(ds, profiler);
        }

        protected override void save(FilePutter file)
        {
            TR.MAP().saver().save(buyers, file);
            TR.MAP().saver().save(sellers, file);
            updater.save(file);
            RACES.map().saver().save(slavesReserved, file);
        }

        protected override void load(FileGetter file)
        {
            TR.MAP().loader().load(buyers, file);
            TR.MAP().loader().load(sellers, file);
            updater.load(file);
            if (VERSION.versionIsBefore(71, 6))
            {
                var slaveAttempting = new TradableData[RACES.all().Count()];
                foreach (Race r in RACES.all())
                {
                    slaveAttempting[r.index()] = new TradableData();
                }
                RACES.map().loader().load(slaveAttempting, file);
            }
            else
            {
                RACES.map().loader().load(slavesReserved, file, 0);
            }
        }

        protected override void clear()
        {
            foreach (TRADABLE rr in TR.ALL())
            {
                buyers[rr.index()].clear();
                sellers[rr.index()].clear();
            }
            updater.clear();

            slavesReserved.Fill(0);
        }

        public bool shouldLeave(Humanoid h)
        {
            return seller(TR.get(h.race())).promised().get(null) > slavesReserved[h.race().index];
        }

        public bool reserveLeave(Humanoid h)
        {
            if (shouldLeave(h))
            {
                slavesReserved[h.race().index]++;
                return true;
            }
            return false;
        }

        public void reserveLeaveCancel(Humanoid h)
        {
            slavesReserved[h.race().index]--;
        }

        public void leave(Humanoid h)
        {
            reserveLeaveCancel(h);
            PSeller ss = seller(TR.get(h.race()));

            foreach (TRADE_TYPE t in TRADE_TYPE.all)
            {
                if (ss.promised.get(t) > 0)
                {
                    ss.promised.inc(t, -1);
                    break;
                }
            }
        }

        public int tradeCredits(double price, double rate)
        {
            if (rate == 0)
                return 10000;
            return (int)(price / rate);
        }

        public double tradeValue(double price, double rate)
        {
            if (rate == 0)
                return 10000;
            return price / (rate * NPCStockpile.AVERAGE_PRICE);
        }
    }
}