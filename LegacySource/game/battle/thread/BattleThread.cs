using System;
using System.IO;
using System.Threading;

namespace game.battle.thread
{
    public abstract class BattleThread
    {
        protected readonly Thread thread;
        private readonly Action job;

        protected BattleThread(double interval)
        {
            job = DoThreadJob;
            thread = new Thread(JobRunner);
            thread.Name = this.GetType().Name;
            thread.Start(interval);
        }

        private void JobRunner(object interval)
        {
            while (true)
            {
                if (thread.IsBackground || thread.ThreadState == ThreadState.AbortRequested)
                {
                    break;
                }

                job?.Invoke();
                Thread.Sleep((int)interval);
            }
        }

        protected void Stop()
        {
            thread.Abort();
            thread.Join();
        }

        protected void Start()
        {
            if (thread.ThreadState == ThreadState.Unstarted || thread.ThreadState == ThreadState.Stopped)
            {
                thread.Start();
            }
        }

        protected abstract void DoThreadJob();

        protected virtual void Save(BinaryWriter file)
        {
            // TODO Auto-generated method stub
        }

        protected void Init()
        {
        }

        protected virtual void Load(BinaryReader file) throws IOException
        {
            Init();
        }
    }
}