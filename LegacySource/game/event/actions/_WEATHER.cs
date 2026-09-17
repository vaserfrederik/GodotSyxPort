using game;
using game.event.engine;
using settlement.main;
using snake2d.util.color;
using snake2d.util.file;
using snake2d.util.sets;
using System.Collections.Generic;

namespace game.event.actions
{
    internal sealed class _WEATHER : EventActionConstructor
    {
        //private final ECollision coll = new ECollision();

        public _WEATHER() : base("WEATHER")
        {
        }

        public override EventAction action(Data data)
        {
            return new Imp(key, data.json, data.all);
        }

        public sealed class Imp : EventAction
        {
            private double temperature = double.NaN;
            private double downpour = double.NaN;
            private double wind = double.NaN;
            private double lightning = double.NaN;
            private double clouds = double.NaN;
            //private double destruction = double.NaN;
            private COLOR sky = null;

            public Imp(string key, Json data, LISTE<EventAction> all) : base(key, all)
            {
                temperature = data.dTry("TEMPERATURE", -1, 1, double.NaN);
                downpour = data.dTry("DOWNPOUR", 0, 1, double.NaN);
                wind = data.dTry("WIND", 0, 1, double.NaN);
                lightning = data.dTry("LIGHTNING", 0, 1, double.NaN);
                clouds = data.dTry("CLOUDS", 0, 1, double.NaN);
                if (data.has("SKY"))
                {
                    sky = new ColorImp(data, "SKY");
                }
                //destruction = data.dTry("DESTRUCTION", 0, 1, double.NaN);
                data.checkUnused();
            }

            protected override void setContext(Event e, EContext data)
            {
                acc = 0;
            }

            private double acc = 0;

            public override void update(Event e, EContext context, double ds, double second)
            {
                double dd = 1.0;
                if (second < 10)
                    dd = second / 10.0;
                else if (GAME.EVENT().current().duration.seconds - second < 10)
                    dd = (GAME.EVENT().current().duration.seconds - second) / 10;
                if (double.IsFinite(temperature))
                {
                    SETT.WEATHER().temp.setD((temperature + 1) * 0.5);
                }
                if (double.IsFinite(downpour))
                {
                    SETT.WEATHER().rain.setD(downpour * dd);
                }
                if (double.IsFinite(wind))
                {
                    SETT.WEATHER().wind.setD(wind * dd);
                }
                if (double.IsFinite(lightning))
                {
                    SETT.WEATHER().thunder.setD(lightning * dd);
                }
                if (double.IsFinite(clouds))
                {
                    SETT.WEATHER().clouds.setD(clouds * dd);
                }
                if (sky != null)
                {
                    SETT.WEATHER().lightColor().interpolate(COLOR.WHITE200, sky, dd);
                }

                //if (destruction != double.NaN)
                //{
                //    acc += ds * destruction;

                //    while (acc > 1)
                //    {
                //        acc -= 1;
                //        int x = RND.rInt(SETT.PWIDTH);
                //        int y = RND.rInt(SETT.PHEIGHT);

                //        foreach (ENTITY ee in SETT.ENTITIES().getArroundPoint(x, y, C.TILE_SIZE * 2))
                //        {
                //            double mom = EPHYSICS.MOM_TRESHOLDI + RND.rFloat() * 2 * EPHYSICS.MOM_TRESHOLDI;
                //            coll.dirDot = 1.0;
                //            coll.momentum = mom * ee.physics.getMass();
                //            coll.damageStrength = 0;
                //            coll.norX = SETT.WEATHER().wind.dirX();
                //            coll.norY = SETT.WEATHER().wind.dirY();
                //            coll.leave = CAUSE_LEAVES.getAccident();
                //            coll.other = null;

                //            ee.collide(coll);
                //        }
                //    }
                //}
            }
        }
    }
}