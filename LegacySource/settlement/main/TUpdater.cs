using System;
using System.Collections.Generic;
using game;
using game.boosting;
using game.debug;
using game.faction;
using game.time;
using init.resources;
using init.type;
using settlement.main;
using settlement.misc.util;
using settlement.room.main;
using settlement.thing;
using settlement.thing.ThingsResources;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.misc;
using util.updating;
using view.sett;
using view.tool;

namespace settlement.main
{
    class TUpdater : SettResource
    {
        private readonly double[] counts;
        private readonly double degradePerYear;

        private readonly TileUpdater updater;

        public TUpdater() : base("TUPDATER", true)
        {
            counts = new double[RESOURCES.ALL().size()];
            degradePerYear = 1.0 / TIME.years().bitConversion(TIME.days());

            updater = new TileUpdater(TWIDTH, THEIGHT, TIME.secondsPerDay())
            {
                Update = (tx, ty, i, timeSinceLast) => Update(tx, ty, i)
            };

            IDebugPanelSett.Add(new PlacableMulti("update sett tile")
            {
                Place = (tx, ty, area, type) => Update(tx, ty, tx + ty * TWIDTH),
                IsPlacable = (tx, ty, area, type) => null
            });
        }

        private void Update(int tx, int ty, int now)
        {
            SETT.TILE_MAP().UpdateTileDay(tx, ty, now);
            SETT.MAINTENANCE().UpdateTileDay(tx, ty, now);
            Degrade(tx, ty, now);
        }

        private void Degrade(int tx, int ty, int now)
        {
            double baseDegrade = degradePerYear;
            double bonus = Math.Clamp(1.0 / BOOSTABLES.CIVICS().SPOILAGE.Get(HCLASS_RACE.clP(null, null)), 0, 10);

            foreach (Thing t in SETT.THINGS().Get(tx, ty))
            {
                double b = baseDegrade;
                if (SETT.TERRAIN().Get(now).RoofIs())
                    b *= 0.75;

                if (t is ScatteredResource)
                    Degrade((RESOURCE_TILE)t, b * 2);
            }

            Room r = SETT.ROOMS().Map.Get(tx, ty);

            if (r != null)
            {
                r.UpdateTileDay(tx, ty);

                RESOURCE_TILE t = r.ResourceTile(tx, ty);

                if (t != null)
                    Degrade(t, bonus * baseDegrade * t.SpoilRate() * 0.75);
            }
        }

        private void Degrade(RESOURCE_TILE t, double value)
        {
            int r = t.Reservable();
            if (r == 0)
                return;

            if (t.Resource().DegradeSpeed() == 0)
                return;

            value *= t.Resource().DegradeSpeed();
            if (value == 0)
                return;

            RESOURCE res = t.Resource();

            counts[res.BIndex()] += value * t.Reservable();
            int am = (int)counts[res.BIndex()];
            counts[res.BIndex()] -= am;

            int dec = 0;
            while (am > 0)
            {
                t.FindableReserve();
                t.ResourcePickup();
                dec++;
                am--;
            }
            if (dec > 0)
                GAME.Player().Res().Inc(t.Resource(), RTYPE.SPOILAGE, -dec);
        }

        protected override void Update(double ds, Profiler profiler)
        {
            updater.UpdateRandom(ds);
        }

        protected override void Save(FilePutter file)
        {
            updater.Save(file);
            file.Ds(counts);
            base.Save(file);
        }

        protected override void Load(FileGetter file)
        {
            updater.Load(file);
            file.Ds(counts);
            base.Load(file);
        }

        protected override void Clear()
        {
            updater.Clear();
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = 0;
            }
        }
    }
}