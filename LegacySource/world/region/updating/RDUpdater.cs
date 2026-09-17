using System;
using System.Collections.Generic;
using System.IO;
using game.faction;
using game.time;
using snake2d.util.file;
using snake2d.util.rnd;
using snake2d.util.sets;
using util.updating;
using world;
using world.map.regions;
using world.region.RD;

namespace world.region.updating
{
    public sealed class RDUpdater
    {
        private readonly LIST<RDUpdatable> all;
        private float[] timers;

        private readonly Shipper shipper = new Shipper();
        private readonly Builder builder = new Builder();

        private readonly double upD = TIME.secondsPerDay() / 4;
        private readonly double ship = TIME.secondsPerDay();
        private readonly double build = TIME.secondsPerDay() * 2;

        public RDUpdater(RDInit init)
        {
            this.all = init.upers;
            timers = new float[WREGIONS.MAX];
            for (int i = 0; i < timers.Length; i++)
            {
                timers[i] = (float)(RND.rFloat() * build);
            }
        }

        private readonly IUpdater uper = new IUpdater(WREGIONS.MAX, 16)
        {
            protected override void update(int i, double timeSinceLast)
            {
                Region r = WORLD.REGIONS().getByIndex(i);
                if (r != null && r.active())
                {
                    if (r.faction() == FACTIONS.player())
                    {
                        timers[r.index()] += (float)timeSinceLast;
                        if (timers[r.index()] > ship)
                        {
                            shipper.ship(r, ship);
                            timers[r.index()] -= (float)ship;
                        }

                        foreach (RDUpdatable u in all)
                            u.update(r, timeSinceLast);
                    }
                    else
                    {
                        float next = timers[r.index()] + (float)timeSinceLast;

                        if ((int)(timers[r.index()] / upD) != (int)(next / upD))
                        {
                            foreach (RDUpdatable u in all)
                                u.update(r, upD);
                        }
                        if (next >= build)
                        {
                            if (r.faction() != null)
                                builder.build(r);
                            shipper.ship(r, build);
                            next -= (float)build;
                        }
                        timers[r.index()] = next;
                    }
                }
            }
        };

        public readonly SAVABLE saver = new SAVABLE()
        {
            public void save(FilePutter file)
            {
                file.fs(timers);
                uper.save(file);
            }

            public void load(FileGetter file)
            {
                file.fs(timers);
                uper.load(file);
            }

            public void clear()
            {
            }
        };

        public void update(double ds)
        {
            uper.update(ds);
        }

        public void BUILD(Region reg)
        {
            this.builder.build(reg);
        }

        public void BUILD(Region reg, RealmBuilder bu)
        {
            this.builder.build(reg, bu);
        }

        public void init(Region reg)
        {
            this.builder.build(reg);
        }

        public void shipAll(Faction f, double days)
        {
            shipper.shipAll(f, days);
        }
    }
}