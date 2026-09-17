using System;
using System.Collections.Generic;
using System.IO;
using game.audio;
using game.battle;
using game.battle.thread;
using game.battle.util;
using game.boosting.superb;
using game.boosting.tmp;
using game.debug;
using game.event.engine;
using game.events;
using game.faction;
using game.faction.player;
using game.nobility;
using game.raiding;
using game.save;
using game.time;
using game.tourism;
using game.values;
using init;
using init.paths;
using init.race;
using init.settings;
using init.sprite;
using script;
using settlement.main;
using snake2d;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using util;
using util.spritecomposer;
using view.interrupter;
using view.main;
using world;

namespace game
{
    public class GAME
    {
        private static TextureHolder texture;
        private static GAME game;
        public readonly static GameSpeed SPEED = new GameSpeed();

        private readonly ArrayListGrower<ACTION> onUpdateFinish = new ArrayListGrower<ACTION>();
        private readonly ArrayListGrower<ACTION> onGameInited = new ArrayListGrower<ACTION>();
        private readonly ArrayListGrower<ACTION> onViewInited = new ArrayListGrower<ACTION>();
        private readonly ArrayListGrower<ACTION> onBeforeGameStarts = new ArrayListGrower<ACTION>();

        private readonly GameSaver saver;
        private readonly Intervals intervals;
        private readonly AUDIO audio;

        private readonly TmpBoosting boostingTmp;
        private readonly SuperBoostables boostingSuper;

        private readonly Armies battle;
        private readonly BattleThreads battleThreads;
        private readonly BattleUtil battleutil;

        private readonly SETT settlement;
        private readonly WORLD world;

        private readonly FACTIONS factions;
        private readonly EVENTS events;
        private readonly EVENT_HANDLER event;
        private readonly RAIDING raiders;
        private readonly ScriptEngine script;
        private readonly NOBLES nobilities;
        private readonly GCOUNTS counts;

        private int updateI = 0;
        private readonly int version;
        private Profiler profiler = Profiler.DUMMY;
        private bool achieving = true;

        private readonly VIEW view;

        private GAME(GameSpec spec) : this(spec, new ArrayListGrower<ACTION>(), new ArrayListGrower<ACTION>(), new ArrayListGrower<ACTION>(), new ArrayListGrower<ACTION>())
        {
        }

        private GAME(GameSpec spec, ArrayListGrower<ACTION> onUpdateFinish, ArrayListGrower<ACTION> onGameInited, ArrayListGrower<ACTION> onViewInited, ArrayListGrower<ACTION> onBeforeGameStarts)
        {
            CORE.disposeClient();
            GameDisposable.disposeAll();
            this.version = spec.version;
            this.saver = new GameSaver(this);
            this.saver.add(new Savable("GAME")
            {
                protected override void save(FilePutter file)
                {
                    file.Bool(game.achieving);
                    SPEED.save(file);
                    file.Int(updateI());
                }

                protected override void load(FileGetter file)
                {
                    achieving = file.Bool();

                    SPEED.load(file);

                    updateI = file.Int();
                }
            });

            game = this;
            CORE.checkIn();
            script = new ScriptEngine(spec.scripts);
            CORE.checkIn();

            audio = new AUDIO(null);

            INIT init = new INIT();

            new SPRITES(this);

            new GUTIL();

            CORE.checkIn();
            new TIME();
            CORE.checkIn();

            audio.init();

            intervals = new Intervals();
            CORE.checkIn();

            boostingTmp = new TmpBoosting(this);
            boostingSuper = new SuperBoostables(this);
            CORE.checkIn();

            settlement = new SETT();
            CORE.checkIn();

            battle = new Armies(this);
            battleThreads = new BattleThreads(game);

            battleutil = new BattleUtil(this);

            CORE.checkIn();
            counts = new GCOUNTS();
            CORE.checkIn();

            RACES.expand();
            world = new WORLD(spec.wx, spec.wy);

            raiders = new RAIDING();

            CORE.checkIn();
            events = new EVENTS();
            new TOURISM();
            CORE.checkIn();
            nobilities = new NOBLES();
            factions = new FACTIONS();
            CORE.checkIn();
            SPEED.clear();
            CORE.checkIn();

            script.init.initBeforeGameInited();

            event = new EVENT_HANDLER();

            for (Savable s in init.finish())
            {
                saver.addSpecialSaver(s);
            }

            foreach (ACTION a in onGameInited)
                a.exe();

            view = new VIEW(GAME.this);

            script.init(null);

            IDebugPanel.Add("Profile", new ACTION()
            {
                public override void exe()
                {
                    if (profiler == Profiler.DUMMY)
                        profiler = Profiler.LIVE;
                    else
                        profiler = Profiler.DUMMY;
                }
            });

            foreach (ACTION a in onViewInited)
                a.exe();
        }

        public static VIEW Create(string[] scripts)
        {
            return Create(GameSpec.Get(scripts));
        }

        public static VIEW Create(GameSpec spec)
        {
            LOG.Ln("NEW GAME " + "Game version: " + VERSION.VERSION_STRING);
            new GlJob()
            {
                public override void DoJob()
                {
                    texture = new Initer()
                    {
                        public override void CreateAssets()
                        {
                            CORE.GetSoundCore().StopAllSounds();
                            CORE.GetSoundCore().DisposeSounds();
                            CORE.CheckIn();

                            game = new GAME(spec);
                        }
                    }.GetTexture();
                }
            }.Run();

            return game.view;
        }

        public static void Update(double ds)
        {
            game.profiler.Start();

            foreach (GameResource resource in GameResource.All)
            {
                resource.Update(ds, game.profiler);
            }

            game.profiler.Stop();
        }

        public static void AddAfterUpdate(ACTION action)
        {
            game.onUpdateFinish.Add(action);
        }

        public static void AddOnInit(ACTION action)
        {
            game.onGameInited.Add(action);
        }

        public static void AddOnViewInit(ACTION action)
        {
            game.onViewInited.Add(action);
        }

        public static void AddBeforeGameStarts(ACTION action)
        {
            game.onBeforeGameStarts.Add(action);
        }

        public static int Version()
        {
            return game.version;
        }

        public static bool Achieving()
        {
            return game.achieving;
        }

        public static void Achieve(bool a)
        {
            game.achieving = a;
        }

        public static void Notify(string s)
        {
            if (S.Get().developer && S.Get().debug)
            {
                SPEED.SpeedSet(0);
                Console.WriteLine();
                Console.WriteLine("SYX NOTIFICATION: " + s);
                StackTraceElement[] trace = new System.Diagnostics.StackTrace().GetFrames();
                int l = trace.Length - 1;
                for (; l >= 0; l--)
                {
                    StackTraceElement e = trace[l];
                    if (e.GetClassName() == typeof(GAME).FullName)
                        break;
                    if (e.GetClassName().StartsWith("snake2d"))
                        break;
                }
                for (int i = 1; i <= l; i++)
                {
                    Console.WriteLine("    " + trace[i].ToString());
                }

                Console.WriteLine();
            }
        }

        public static void Notify(object s)
        {
            Notify(s.ToString());
        }

        public static void Error(string s)
        {
            if (S.Get().developer || S.Get().debug)
            {
                Warn(s);
            }
            else
            {
                throw new Exception(s);
            }
        }

        public static void Warn(string s)
        {
            if (S.Get().developer || S.Get().debug)
            {
                SPEED.SpeedSet(0);
                LOG.Err("SYX WARNING: " + s);
                StackTraceElement[] trace = new System.Diagnostics.StackTrace().GetFrames();
                foreach (StackTraceElement e in trace)
                {
                    if (e.GetClassName() == typeof(GAME).FullName)
                        continue;
                    if (e.GetClassName().StartsWith("snake2d"))
                        continue;
                    LOG.Err("    " + e.ToString());
                }
            }
        }

        public static void WarnLight(string s)
        {
            if (S.Get().developer || S.Get().debug)
            {
                LOG.Err("SYX WARNING: " + s);
            }
        }

        public static void Error(object s)
        {
            Error(s.ToString());
        }

        public static class GameResource
        {
            protected readonly bool isBattle;

            private static readonly ArrayList<GameResource> All = new ArrayList<GameResource>(16);
            static GameResource()
            {
                new GameDisposable()
                {
                    protected override void Dispose()
                    {
                        All.Clear();
                    }
                };
            }

            protected GameResource(string key) : this(key, true)
            {
            }

            protected GameResource(string key, bool isBattle)
            {
                base(key);
                GAME.saver().Add(this);
                All.Add(this);
                this.isBattle = isBattle;
            }

            protected virtual void AfterTick()
            {
            }

            protected abstract void Update(double ds, Profiler prof);
        }

        public static class Cache
        {
            int upI;
            private readonly int ticks;

            public Cache(int ticks)
            {
                upI = -ticks;
                this.ticks = ticks;
            }

            public bool ShouldAndReset()
            {
                if (Math.Abs(GAME.updateI() - upI) > ticks)
                {
                    upI = GAME.updateI();
                    return true;
                }
                return false;
            }

            public void Reset()
            {
                upI = GAME.updateI() - ticks;
            }
        }
    }
}