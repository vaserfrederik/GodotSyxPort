using System;
using System.Collections.Generic;
using System.IO;
using game;
using init.sprite.UI;
using settlement.entity;
using settlement.main;
using settlement.room.main;
using snake2d;
using snake2d.util.misc;
using snake2d.util.sets;
using util.data;

namespace util
{
    public class GUTIL
    {
        public static Data data;

        static GUTIL()
        {
            new GameDisposable
            {
                protected override void Dispose()
                {
                    if (data != null)
                        data.humans.Clear();
                }
            };
        }

        public class Data
        {
            private readonly StatsDebugger debugger;
            private readonly CircleCooIterator circleIterator;
            private readonly AreaTmp areaTmp = new AreaTmp();
            private readonly ArrayCooShort coos = new ArrayCooShort(Room.MAX_SIZE + 1);
            private readonly PathUtilOnline pathOnline;
            private readonly RANMAP ran1 = new RANMAP();
            private readonly RANMAP ran2 = new RANMAP();
            private ArrayList<object> humans = new ArrayList<object>(ENTETIES.MAX);

            public TextureHolder texture;

            private Data() : this(null)
            {
            }

            private Data(Path path) : this(path, null)
            {
            }

            private Data(Path path, Action action) : this(path, action, null)
            {
            }

            private Data(Path path, Action action, Action action2)
            {
                GUTIL.data = this;

                debugger = new StatsDebugger(UI.FONT().M);

                pathOnline = new PathUtilOnline(SETT.TWIDTH);
                circleIterator = new CircleCooIterator(120, pathOnline.getFlooder());
                debugger.Add(new Value("Ents", 0, Formatter.Amount)
                {
                    protected override double GetValue()
                    {
                        if (SETT.ENTITIES() == null)
                            return 0;
                        return SETT.ENTITIES().size();
                    }
                });
                debugger.Add(new Value("Speed", 0, Formatter.Amount)
                {
                    double t;
                    double am;

                    protected override double GetValue()
                    {
                        t += GAME.SPEED.speed();
                        am++;
                        if (am > 30)
                        {
                            t = t / am;
                            am = 1;
                        }
                        return t / am;
                    }
                });

                IDebugPanel.Add("Reload Assets", new ACTION
                {
                    public void Exe()
                    {
                        Path p = GAME.saver().save("debugReload");
                        if (p != null)
                            CORE.setCurrentState(new GameLoader(p));
                    }
                });
            }
        }

        public GUTIL() : this(null)
        {
        }

        public static StatsDebugger debugger()
        {
            return data.debugger;
        }

        public static Flooder flooder()
        {
            return data.pathOnline.getFlooder();
        }

        public static PathUtilOnline.Marker marker()
        {
            return data.pathOnline.marker;
        }

        public static PathUtilOnline.Filler filler()
        {
            return data.pathOnline.filler;
        }

        public static PathUtilOnline.AStar astar()
        {
            return data.pathOnline.astar;
        }

        public static PathUtilOnline pathTools()
        {
            return data.pathOnline;
        }

        public static CircleCooIterator circle()
        {
            return data.circleIterator;
        }

        public static ArrayCooShort coos()
        {
            return data.coos;
        }

        public static RANMAP ran1()
        {
            return data.ran1;
        }

        public static RANMAP ran2()
        {
            return data.ran2;
        }

        public static AreaTmp AREA()
        {
            return data.areaTmp;
        }

        public static ArrayList<object> hList()
        {
            return data.humans;
        }
    }
}