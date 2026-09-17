using System;
using System.Collections.Generic;
using System.Threading;

namespace Settlement.Path.Thread
{
    public sealed class FinderThread
    {
        private readonly List<ThreadPathJob> queueJob = new List<ThreadPathJob>(1024);
        private readonly List<ThreadPath> queueJobPath = new List<ThreadPath>(1024);
        private volatile bool lockFlag;
        private volatile bool stopForUp = false;

        private static readonly Worker[] works = new Worker[1];
        private readonly Thread[] threads = new Thread[works.Length];

        public FinderThread(SCOMPONENTS comps)
        {
            foreach (Worker w in works)
            {
                if (w != null)
                {
                    w.Working = false;
                }
            }

            for (int i = 0; i < threads.Length; i++)
            {
                works[i] = new Worker(this, comps);
                Thread t = new Thread(works[i].Run);
                t.IsBackground = true;
                t.Name = "Path offloade #" + i;
                threads[i] = t;
                t.Start();
            }
            lockFlag = false;
        }

        private void Lock()
        {
            while (lockFlag)
                ;
            lockFlag = true;
        }

        public void SetStop()
        {
            stopForUp = true;
        }

        public void Stop()
        {
            stopForUp = true;

            while (!AllAreStopped())
                ;
        }

        private bool AllAreStopped()
        {
            for (int i = 0; i < threads.Length; i++)
            {
                if (!threads[i].IsAlive)
                    throw new Exception("dead!");
                if (!works[i].Stopped)
                    return false;
            }
            return true;
        }

        public void Start()
        {
            stopForUp = false;
        }

        public void Prep(SPath p, int sx, int sy, int dx, int dy, bool full)
        {
            Prep(p, Resume, sx, sy, dx, dy, full);
        }

        public void Prep(SPath p, ThreadPathJob job, int sx, int sy, int dx, int dy, bool full)
        {
            ThreadPath t = p.thread;
            if (t.Status == 1)
                return;
            t.Status = 1;
            t.Sx = (short)sx;
            t.Sy = (short)sy;
            t.Dx = (short)dx;
            t.Dy = (short)dy;
            t.Full = full;
            Lock();
            if (queueJob.Count < 1024)
            {
                queueJob.Add(job);
                queueJobPath.Add(t);
            }
            else
                t.Status = 0;
            lockFlag = false;
        }

        private class Worker
        {
            private readonly PathUtilOnline pather;
            private readonly SPathFinderThread fin;
            private volatile bool working = true;
            private volatile bool stopped;
            private readonly FinderThread tt;

            public Worker(FinderThread tt, SCOMPONENTS comps)
            {
                this.tt = tt;
                pather = new PathUtilOnline(SETT.TWIDTH);
                fin = new SPathFinderThread(comps, pather, 13);
            }

            public void Run()
            {
                while (working)
                {
                    if (tt.stopForUp)
                    {
                        stopped = true;
                        if (!working)
                            break;
                        while (tt.stopForUp)
                        {
                            if (!working)
                                break;
                            Sleep();
                            continue;
                        }
                        continue;
                    }
                    stopped = false;
                    tt.Lock();
                    if (queueJob.Count == 0)
                    {
                        tt.lockFlag = false;
                        tt.stopForUp = true;
                        stopped = true;
                        Thread.Yield();
                        continue;
                    }

                    ThreadPath t = queueJobPath[queueJobPath.Count - 1];
                    queueJobPath.RemoveAt(queueJobPath.Count - 1);
                    ThreadPathJob j = queueJob[queueJob.Count - 1];
                    queueJob.RemoveAt(queueJob.Count - 1);
                    tt.lockFlag = false;
                    if (j.DoJob(pather, fin, t))
                        t.Status = 3;
                    else
                        t.Status = 2;
                }
                Console.WriteLine("Pathworker is dead");
            }

            private void Sleep()
            {
                try
                {
                    Thread.Sleep(1);
                }
                catch (InterruptedException)
                {
                }
            }
        }

        public class ThreadPath
        {
            private volatile byte status;
            public readonly PathFancy Path = new PathFancy(SPath.Size);
            public short Sx, Sy, Dx, Dy;
            public bool Full;
            public volatile short DestX;
            public volatile short DestY;

            public bool IsProcessed(int sx, int sy, int dx, int dy)
            {
                if (this.Sx == sx && this.Sy == sy && this.Dx == dx && this.Dy == dy)
                    return status > 1;
                return false;
            }

            public bool IsBeingProcessed()
            {
                return status >= 1;
            }

            public bool IsSuccess()
            {
                return status == 3;
            }

            public void Debug(int sx, int sy, int dx, int dy)
            {
                if (status > 1)
                    Console.WriteLine($"{status} {this.Sx - sx} {this.Sy - sy} {this.Dx - dx} {this.Dy - dy}");
            }
        }

        public interface ThreadPathJob
        {
            bool DoJob(PathUtilOnline p, SPathFinderThread fin, ThreadPath pp);
        }

        private static readonly ThreadPathJob Resume = new ThreadPathJob()
        {
            public bool DoJob(PathUtilOnline p, SPathFinderThread fin, ThreadPath t)
            {
                PathTile tile = fin.Find(t.Sx, t.Sy, t.Dx, t.Dy, t.Full);

                if (tile != null)
                {
                    t.DestX = (short)tile.X();
                    t.DestY = (short)tile.Y();
                    t.Path.Set(tile);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        };
    }
}