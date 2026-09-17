using System;
using System.Threading;

namespace snake2d
{
    public sealed class SlaveThread : CORE_RESOURCE
    {
        private readonly Thread thread;
        private readonly long time;
        public readonly double ds;
        private volatile ACTION job;
        private volatile bool working = false;
        private volatile bool shouldWork = false;
        private volatile bool shouldDie = false;
        private volatile bool doOnce = false;
        public readonly string name;
        private volatile long sleepTime;

        private volatile double utilization = 0;
        private long sleepDigTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private volatile long lastResponseTime = 0;
        private volatile bool hasWarnedOfSlowResponse = false;

        public SlaveThread(string name, double interval)
        {
            this.ds = interval;
            this.name = name;
            thread = new Thread(Runner);
            thread.Name = name;
            thread.IsBackground = true;
            time = (long)(1000 * interval);
            thread.UnhandledException += (s, e) =>
            {
                working = false;
                shouldWork = false;
                shouldDie = true;
                Console.Error.WriteLine(e.ExceptionObject);
                CORE.Annihilate((Exception)e.ExceptionObject);
            };
            CORE.AddDisposable(this);
            thread.Start();
            lastResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            hasWarnedOfSlowResponse = false;
        }

        public bool Working()
        {
            return shouldWork && !shouldDie;
        }

        public void Start(ACTION job)
        {
            if (shouldWork)
                throw new InvalidOperationException(thread.Name + " is already started");
            this.job = job;
            if (shouldDie)
                throw new InvalidOperationException("thread is dead");
            lastResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            shouldWork = true;
        }

        public void DoOnce(ACTION job)
        {
            if (shouldWork || doOnce)
                throw new InvalidOperationException("already started");
            this.job = job;
            lastResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            doOnce = true;
        }

        public void SetStopFlag()
        {
            shouldWork = false;
            lastResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public void WaitUntilStopped()
        {
            if (Thread.CurrentThread.ManagedThreadId == thread.ManagedThreadId)
                throw new InvalidOperationException("can't stop yourself");
            shouldWork = false;
            while (working || doOnce)
            {
                CheckForUnresponsiveness();
                Sleep(1);
            }
        }

        public void Kill()
        {
            shouldDie = true;
            if (thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                thread.Interrupt();
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                while (thread.IsAlive)
                {
                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - now > 10000)
                    {
                        Console.Error.WriteLine(thread.Name + " refuses to die");
                        foreach (var e in thread.GetStackTrace())
                        {
                            Console.Error.WriteLine(e);
                        }
                        return;
                    }
                    Sleep(0);
                }
            }
        }

        public bool IsDead()
        {
            return thread.IsAlive;
        }

        public double GetUtilization()
        {
            return utilization;
        }

        private void Sleep(long milis)
        {
            if (milis < 0)
                milis = 0;
            try
            {
                Thread.Sleep(milis);
            }
            catch (ThreadInterruptedException)
            {
                // ignored
            }
        }

        public void CheckForUnresponsiveness()
        {
            if (shouldDie && !thread.IsAlive)
                return;
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long tmp = now - lastResponseTime;
            if (tmp > time * 1000)
            {
                if (hasWarnedOfSlowResponse)
                {
                    throw new InvalidOperationException(thread.Name + " is stuck and will now die!");
                }
                else
                {
                    Console.Error.WriteLine(thread.Name + " is stuck!");
                    foreach (var e in thread.GetStackTrace())
                    {
                        Console.Error.WriteLine(e);
                    }
                    lastResponseTime = now;
                    hasWarnedOfSlowResponse = true;
                    thread.Interrupt();
                }
            }
        }

        protected override void Dis()
        {
            Kill();
        }

        private readonly Action Runner = () =>
        {
            while (!shouldDie)
            {
                lastResponseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                hasWarnedOfSlowResponse = false;
                if (doOnce)
                {
                    job.Exe();
                    job = null;
                    doOnce = false;
                    shouldWork = false;
                }

                if (shouldWork)
                {
                    working = true;
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    job.Exe();

                    long workTime = now;
                    now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    workTime = now - workTime;

                    if (workTime > time)
                        continue;

                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - sleepDigTimer > 1000)
                    {
                        long d = now - sleepDigTimer;
                        utilization = d <= 0 ? 0 : 100.0 * (1.0 - ((double)sleepTime) / d);
                        sleepTime = 0;
                        sleepDigTimer = now;
                    }
                    long st = time - workTime;
                    sleepTime += st;
                    working = false;
                    Sleep(st - 1);
                }
                else
                {
                    working = false;
                    Sleep(1);
                }
            }
            Printer.Ln(thread.Name + " is dead.");
        };
    }
}