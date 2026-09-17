using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Game.Battle.Thread.General
{
    public class Strategos2000 : BattleThread
    {
        private readonly StrategosUtil context;
        private Strategos2000Updater current;
        private readonly Strategos2000Updater offense;
        private int oldDivs;
        private bool debug = false;
        private double prevUpdate = 0;

        public Strategos2000() : base(1.0 / 120.0)
        {
            this.context = new StrategosUtil();

            offense = new Strategos2000UpdaterOffense(context);
            current = offense;

            IDebugPanel.Add("Battle General Debug", new Action(() =>
            {
                ON_TOP_RENDERABLE ren = new ON_TOP_RENDERABLE()
                {
                    public void Render(Renderer r, ShadowBatch shadowBatch, RenderData data, double ds)
                    {
                        RenderIterator it = data.OnScreenTiles();

                        if (current == null)
                            return;

                        while (it.Has())
                        {
                            current.Render(r, it);
                            it.Next();
                        }

                        current.Render(r, shadowBatch, data);

                        if (!debug)
                            Remove();
                    }
                };
                debug = !debug;
                ren.Add();
            }));

            IDebugPanel.Add("battle general pause", new Action(() =>
            {
                Stop();
            }));

            IDebugPanel.Add("battle general unpause", new Action(() =>
            {
                Stop();
                Start();
            }));
        }

        public override void Save(FilePutter file)
        {
            file.Mark(offense);
            offense.Save(file);
            file.Mark(offense);
        }

        public override void Load(FileGetter file)
        {
            file.Check(offense);
            offense.Load(file);
            file.Check(offense);
        }

        public enum State
        {
            ATTACK
        }

        protected override void Init()
        {
            offense.Clear();
        }

        protected override void DoThreadJob()
        {
            int newDivs = 0;
            for (int di = 0; di < context.GetArmy().Divisions().Size(); di++)
            {
                if (context.GetArmy().Divisions().Get(di).Active())
                    newDivs++;
            }
            if (newDivs == 0)
            {
                if (oldDivs > 0)
                {
                    offense.Clear();
                }
            }
            oldDivs = newDivs;

            if (prevUpdate - TIME.CurrentSecond() > -2)
                return;

            prevUpdate = TIME.CurrentSecond() + 2;

            while (thread.Working())
            {
                if (!offense.Update())
                    break;
            }
        }
    }
}