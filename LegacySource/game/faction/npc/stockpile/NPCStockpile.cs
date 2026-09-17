using System;
using System.Collections.Generic;
using System.IO;
using game;
using game.faction.npc;
using init.resources;
using init.trade;
using settlement.entity;
using snake2d.util.file;
using util.data;
using util.statistics;
using world.region;
using world.region.pop;

namespace game.faction.npc.stockpile
{
    public class NPCStockpile : NPCResource
    {
        public static readonly int AVERAGE_PRICE = 400;
        public static readonly int GAME_THEORY = (int)(0.05 * AVERAGE_PRICE);
        private const double PRICE_MAX = 10.0;
        private const double PRICE_MIN = 1.0 / PRICE_MAX;
        private const double PILE_SIZE = 9 * ENTETIES.MAX / 40000.0;

        private static Updater updater;

        static NPCStockpile()
        {
            new GameDisposable
            {
                Dispose = () => updater = null
            };
        }

        private readonly NPCRes[] resses;
        private readonly FactionNPC f;
        private readonly DOUBLE credits;
        private double workforce = 1;

        public readonly HistoryTradable price = new HistoryTradable(16, TIME.seasons(), true);
        public readonly HistoryTradable forSale = new HistoryTradable(16, TIME.seasons(), true);

        public NPCStockpile(FactionNPC f, LISTE<NPCResource> all, DOUBLE credits) : base(all)
        {
            this.f = f;

            if (updater == null)
            {
                updater = new Updater();
            }

            this.credits = credits;
            for (int i = 0; i < TR.ALL().size(); i++)
            {
                resses[i] = new NPCRes(this, TR.ALL()[i]);
            }
        }

        public double CreditScore()
        {
            double aa = workforce * AVERAGE_PRICE * RESOURCES.ALL().size();
            aa = (aa + credits.getD()) / (aa + 1);
            aa = CLAMP.d(aa, PRICE_MIN, PRICE_MAX);
            return aa;
        }

        public double Credit()
        {
            return (workforce * AVERAGE_PRICE * RESOURCES.ALL().size() + credits.getD());
        }

        public double Workforce()
        {
            return workforce;
        }

        protected override SAVABLE Saver()
        {
            return new SAVABLE
            {
                Save = file =>
                {
                    TR.MAP().saver().save(resses, file);
                    file.d(workforce);
                    price.save(file);
                    forSale.save(file);
                },
                Load = file =>
                {
                    TR.MAP().loader().load(resses, file);
                    workforce = file.d();
                    price.load(file);
                    forSale.load(file);
                },
                Clear = () =>
                {
                    foreach (NPCRes r in resses)
                        r.clear();
                    workforce = 1;
                    price.clear();
                    forSale.clear();
                }
            };
        }

        public override void Update(FactionNPC faction, double seconds)
        {
            Update(faction, seconds, RD.RACES().population.get(faction.capitolRegion()) * 0.25 + 0.15 * RD.RACES().population.faction().get(faction));
        }

        public void Update(FactionNPC faction, double seconds, double wf)
        {
            workforce = wf * PILE_SIZE / TR.ALL().size();

            foreach (TRADABLE res in TR.ALL())
            {
                NPCRes rr = resses[res.index()];
                rr.update(res, f);
            }

            updater.update(this, seconds * TIME.secondsPerDayI());

            foreach (TRADABLE res in TR.ALL())
            {
                price.set(res, (int)Math.Round(res(res).priceAt(0)));
                forSale.set(res, (int)Math.Round(res(res).amount()));
            }
        }

        protected override void Generate(RDRace race, FactionNPC faction, bool init)
        {
            Saver().clear();
            Update(faction, 0);
        }

        public NPCRes Res(TRADABLE tr)
        {
            return resses[tr.index()];
        }
    }
}