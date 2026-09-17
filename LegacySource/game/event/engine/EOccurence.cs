using System;
using game.faction;
using game.time;
using init.race;
using init.type;
using init.value;
using settlement.main;
using settlement.stats;
using snake2d.util.file;

namespace game.event.engine
{
    sealed class EOccurence
    {
        public readonly Lockable<Faction> plockable = GVALUES.FACTION.LOCK.push();
        public readonly double[] coccurence = new double[CLIMATES.ALL().Count];
        public readonly double[] roccurence = new double[RACES.all().Count];
        public readonly double[] toccurence = new double[TERRAINS.ALL().Count];
        public readonly int maxSpawns;
        public double onlyAfterTime;

        public EOccurence(Json data, EventCollection engine, Event parent)
        {
            if (data.Has("OCCURENCE"))
            {
                data = data.Json("OCCURENCE");
                data.Value("TYPE", "");
                CLIMATES.MAP().readFill("CLIMATE", coccurence, data, 0, 10000000);
                RACES.map().readFill("RACE", roccurence, data, 0, 10000000);
                TERRAINS.MAP().readFill("TERRAIN", toccurence, data, 0, 100000);
                maxSpawns = data.i("MAX_SPAWNS", 0, int.MaxValue, int.MaxValue);

                onlyAfterTime = data.dTry("ONLY_AFTER_DAYS", 0, 100000, 0) * TIME.secondsPerDay();
                plockable.push(data);
                data.CheckUnused();

            }
            else
            {
                maxSpawns = 10;
            }
        }

        public double occurence(Race race)
        {
            double occ = 0;

            foreach (TERRAIN t in TERRAINS.ALL())
            {
                occ += toccurence[t.index()] * SETT.WORLD_AREA().info.get(t).getD();
            }
            CLIMATE climate = SETT.ENV().climate();
            occ *= coccurence[climate.index()] * roccurence[race.index()];
            return occ;
        }

        public double race(Race race)
        {
            return roccurence[race.index()];
        }

        public double occurence()
        {
            double occ = 0;

            foreach (TERRAIN t in TERRAINS.ALL())
            {
                double d = toccurence[t.index()] * SETT.WORLD_AREA().info.get(t).getD();
                if (d > occ)
                    occ = d;
            }

            double raM = 0;
            double tot = 1 + STATS.POP().POP.data(null).get(null);
            foreach (Race r in RACES.all())
            {
                double d = roccurence[r.index()] * STATS.POP().POP.data().get(r) / tot;
                if (d > raM)
                    raM = d;
            }
            occ *= coccurence[SETT.ENV().climate().index()] * raM;
            return occ;
        }
    }
}