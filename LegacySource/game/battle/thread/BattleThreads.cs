using System;
using System.Collections.Generic;
using System.IO;
using game.GAME;
using game.battle.div;
using game.battle.thread.general;
using game.battle.thread.order;
using game.battle.thread.position;
using game.battle.thread.status;
using game.battle.thread.trajectory;
using game.debug;
using init.constant;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.main;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sets;
using view.interrupter;

namespace game.battle.thread
{
    public class BattleThreads : GameResource
    {
        public readonly BattleStatus status = new BattleStatus();
        public readonly DivCentres centres = new DivCentres();
        public readonly BattleTrajectories trajs = new BattleTrajectories();
        public readonly BattleOrders orders = new BattleOrders();
        public readonly Strategos2000 strat = new Strategos2000();
        private readonly ArrayList<BattleThread> threads = new ArrayList<BattleThread>(orders, centres, status, trajs, strat);
        private bool started = false;

        public BattleThreads(GAME game) : base("BATTLE_THREADS")
        {
            game.AddBeforeGameStarts(new ACTION
            {
                exe = () =>
                {
                    if (!started)
                        Unpause(true);
                }
            });

            game.Saver().OnAfterLoad(new ACTION_O<Path>
            {
                exe = t =>
                {
                    bool s = started;

                    ((BattleThread)centres).Init();
                    ((BattleThread)status).Init();

                    if (s)
                    {
                        Unpause(false);
                    }
                }
            });

            IDebugPanel.Add("battle threads pause", new ACTION
            {
                exe = () =>
                {
                    Pause();
                }
            });

            IDebugPanel.Add("battle threads unpause", new ACTION
            {
                exe = () =>
                {
                    if (started)
                        Pause();
                    Unpause(true);
                }
            });
        }

        public void InitAndTeleport(LIST<Div> divs)
        {
            bool s = started;
            Pause();
            foreach (Div d in divs)
            {
                orders.Init(d);
            }

            Bitmap1D map = new Bitmap1D(Config.battle().DIVISIONS_PER_BATTLE, false);
            foreach (Div d in divs)
            {
                map.Set(d.Index(), true);
            }

            foreach (ENTITY e in SETT.ENTITIES().GetAllEnts())
            {
                if (e is Humanoid)
                {
                    Humanoid a = (Humanoid)e;
                    if (a.Division() != null && map.Get(a.Division().Index()))
                        a.TeleportAndInitInDiv();
                }
            }

            ((BattleThread)centres).Init();
            ((BattleThread)status).Init();
            ((BattleThread)trajs).Init();
            ((BattleThread)strat).Init();

            if (s)
                Unpause(false);
        }

        protected override void Update(double ds, Profiler prof)
        {
            // TODO Auto-generated method stub
        }

        protected override void Save(FilePutter file)
        {
            bool started = this.started;
            Pause();
            foreach (BattleThread t in threads)
            {
                t.Save(file);
            }

            file.Bool(started);
            if (started)
                Unpause(false);
        }

        protected override void Load(FileGetter file)
        {
            foreach (BattleThread t in threads)
            {
                t.Load(file);
            }

            started = file.Bool();
        }

        public void Pause()
        {
            started = false;
            foreach (BattleThread t in threads)
            {
                t.Stop();
            }
        }

        public void Unpause(bool init)
        {
            started = true;
            foreach (BattleThread t in threads)
            {
                if (init)
                    t.Init();
                t.Start();
            }
        }

        protected override void LoadFail()
        {
            bool s = started;
            Pause();
            foreach (BattleThread t in threads)
            {
                t.Init();
            }

            if (s)
                Unpause(true);
        }
    }
}