using System;
using System.IO;
using System.Threading;
using game;
using game.battle.div;
using game.battle.thread;
using init.sprite.UI;
using settlement.main;
using snake2d;
using snake2d.util.file;
using snake2d.util.misc;
using snake2d.util.sprite.text;
using util.rendering;
using view.interrupter;

namespace game.battle.thread.status
{
    public sealed class BattleStatus : BattleThread
    {
        private BattleContext current = new BattleContext();
        private volatile BattleContext nextnext = new BattleContext();
        private volatile BattleContext next = nextnext;
        private readonly Updater updater = new Updater();

        public BattleStatus() : base(0.5)
        {
            IDebugPanel.Add("Battle Status Debug", new ACTION
            {
                bool debug = false;
                ON_TOP_RENDERABLE ren = new ON_TOP_RENDERABLE
                {
                    public void Render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
                    {
                        RenderIterator it = data.OnScreenTiles();
                        if (current == null)
                            return;

                        while (it.Has())
                        {
                            int p = current.Map.Soldiers(GAME.ARMIES().Player()).Get(it.Tile());
                            int e = current.Map.Soldiers(GAME.ARMIES().Enemy()).Get(it.Tile());
                            if (p != 0 || e != 0)
                            {
                                Str.TMP.Clear().Add(p).Add('/').Add(e);
                                UI.FONT().S.Render(r, Str.TMP, it.X(), it.Y());
                            }
                            it.Next();
                        }
                        if (!debug)
                            Remove();
                    }
                };

                public void Exe()
                {
                    debug = !debug;
                    ren.Add();
                }
            });

            new Tests(this);
        }

        protected override void Stop()
        {
            updater.stop = true;
            base.Stop();
            updater.stop = false;
        }

        protected override void Save(FilePutter file)
        {
        }

        protected override void Load(FileGetter file) throws IOException
        {
        }

        protected override void Init()
        {
            updater.Init(current);
        }

        protected override void DoThreadJob()
        {
            if (next == null)
            {
                next = nextnext;
                return;
            }

            updater.Init(next);
            BattleContext c = current;
            current = next;
            nextnext = c;
            next = null;
        }

        public static DivStatus Status(Div d)
        {
            return GAME.BATTLE_THREADS().status.current.statuses[d.Index()];
        }

        public static DivsTileMap Map()
        {
            return GAME.BATTLE_THREADS().status.current.map;
        }

        public static DivsQuadMap Quads()
        {
            return GAME.BATTLE_THREADS().status.current.quads;
        }

        public static DivsSpaceMap Space()
        {
            return GAME.BATTLE_THREADS().status.current.space;
        }

        public static DivArmyMap Army()
        {
            return GAME.BATTLE_THREADS().status.current.army;
        }
    }
}