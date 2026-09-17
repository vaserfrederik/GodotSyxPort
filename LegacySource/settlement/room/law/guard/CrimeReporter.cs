using System;
using System.IO;
using System.Linq;
using settlement.entity;
using settlement.entity.humanoid;
using settlement.entity.humanoid.ai.main;
using settlement.main;
using settlement.room.law.execution;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.rnd;

namespace settlement.room.law.guard
{
    public class CrimeReporter
    {
        private static int EE = 5;
        private int[] data;

        private static int tCrime = 0;
        private static int tExecution = 1;

        private readonly ROOM_GUARD b;

        public CrimeReporter(ROOM_GUARD b)
        {
            this.b = b;
            data = new int[256 * 2];
        }

        public void Save(FilePutter file)
        {
            file.IsE(data);
        }

        public void Load(FileGetter file)
        {
            file.IsE(data);
        }

        public void Clear()
        {
            Array.Fill(data, 0);
        }

        public int[] MakeData()
        {
            return new int[EE * 2 + 2];
        }

        public int[] Data(GuardInstance ins)
        {
            if (ins.cdata == null || ins.cdata.Length != EE * 2 + 2)
                ins.cdata = new int[EE * 2 + 2];
            return ins.cdata;
        }

        public void ReportCriminal(Humanoid a)
        {
            if (RND.RBoolean())
                return;
            Report(tCrime, a.Tc().X, a.Tc().Y, 90, a.Id());
        }

        public void ReportExecution(int tx, int ty)
        {
            int payload = ((tx << 16) & ~0x0FFFF) | (ty & 0x0FFFF);
            Report(tExecution, tx, ty, 180, payload);
        }

        public int Crimes(GuardInstance ins)
        {
            if (ins != null)
                return Data(ins)[tCrime];
            return data[tCrime];
        }

        public int Executions(GuardInstance ins)
        {
            if (ins != null)
                return Data(ins)[tExecution];
            return data[tExecution];
        }

        private void Report(int type, int sx, int sy, int radius, int payload)
        {
            COORDINATE c = b.finder.Reserve(sx, sy, radius);

            if (c != null)
            {
                GuardInstance ins = b.getter.Get(c);
                int[] data = Data(ins);

                bool av = Available(data);
                Push(type, payload, data);

                if (av && !Available(data))
                    b.finder.Report(b.service.Get(ins), -1);
            }
            else if (RND.OneIn(4))
            {
                Push(type, payload, data);
            }
        }

        private bool Push(int stride, int payload, int[] data)
        {
            int length = (data.Length - 2) / 2;
            int count = data[stride];
            if (count >= length)
                return false;
            data[2 + length * stride + count] = payload;
            data[stride]++;
            return true;
        }

        public Humanoid PollCriminal(GuardInstance ins)
        {
            if (ins != null)
            {
                bool av = Available(data);

                int id = Pop(tCrime, Data(ins));
                if (!av && Available(data))
                    b.finder.Report(b.service.Get(ins), 1);
                while (id >= 0)
                {
                    ENTITY e = SETT.ENTITIES().GetByID(id);
                    if (e != null && e is Humanoid && !e.IsRemoved())
                    {
                        Humanoid a = (Humanoid)e;
                        if (AI.modules().IsCriminal(a))
                            return a;
                    }
                    id = Pop(tCrime, ins.cdata);
                }
            }

            int id = Pop(tCrime, data);
            while (id >= 0)
            {
                ENTITY e = SETT.ENTITIES().GetByID(id);
                if (e != null && e is Humanoid && !e.IsRemoved())
                {
                    Humanoid a = (Humanoid)e;
                    if (AI.modules().IsCriminal(a))
                        return a;
                }
                id = Pop(tCrime, data);
            }
            return null;
        }

        public Guard PollExecution(GuardInstance ins)
        {
            if (ins != null)
            {
                bool av = Available(data);

                int id = Pop(tExecution, Data(ins));
                if (!av && Available(data))
                    b.finder.Report(b.service.Get(ins), 1);
                while (id >= 0)
                {
                    int tx = (id >> 16) & 0x0FFFF;
                    int ty = id & 0x0FFFF;

                    Guard g = SETT.ROOMS().EXECUTION.stations.Guard(tx, ty);
                    if (g != null && g.Active())
                        return g;
                    id = Pop(tCrime, ins.cdata);
                }
            }

            int id = Pop(tExecution, data);
            while (id >= 0)
            {
                int tx = (id >> 16) & 0x0FF;
                int ty = id & 0x0FF;

                Guard g = SETT.ROOMS().EXECUTION.stations.Guard(tx, ty);
                if (g != null && g.Active())
                    return g;
                id = Pop(tCrime, data);
            }
            return null;
        }

        public bool Available(GuardInstance ins)
        {
            return Available(Data(ins));
        }

        private bool Available(int[] data)
        {
            int length = (data.Length - 2) / 2;
            if (data[tCrime] >= length || data[tExecution] >= length)
                return false;
            return true;
        }

        private int Pop(int stride, int[] data)
        {
            int length = (data.Length - 2) / 2;
            int count = data[stride];
            if (count == 0)
                return -1;
            data[stride]--;
            return data[2 + length * stride + count - 1];
        }
    }
}