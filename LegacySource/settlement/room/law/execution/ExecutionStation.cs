using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Settlement.Room.Law.Execution
{
    public static class ExecutionStation
    {
        public const int TYPE_CHOP = 1;
        public const int TYPE_HANG = 2;
        public const int TYPE_GIBBET = 3;
        public const int TYPE_CROSS = 4;

        private static readonly int services = 8;

        private static readonly int STATE_UNINITED = 0;
        private static readonly int STATE_RESERVABLE = 1;
        private static readonly int STATE_RESERVED = 2;
        private static readonly int STATE_USED = 3;
        private static readonly int STATE_EXECUTING = 4;
        private static readonly int STATE_DEAD = 5;

        private readonly ROOM_EXECTUTION b;
        private readonly RoomAreaWrapper aa = new RoomAreaWrapper();
        private readonly Coo coo = new Coo();
        private readonly RoomBits bState;
        private readonly RoomBits bServices;
        private readonly Client client = new Client();
        private readonly Guard guard = new Guard();
        private ArrayCooShort available = new ArrayCooShort(128);
        private int total = 0;

        public ExecutionStation(ROOM_EXECTUTION b)
        {
            this.b = b;
            this.bState = new BB(coo, new Bits(0x000000FF));
            this.bServices = new BB(coo, new Bits(0x0000FF00));
        }

        public Client ExecutionReserve()
        {
            if (available.Count == 0)
                return null;

            int m = available.Count;
            if (m == 0)
                return null;

            available.Set(RND.rInt(m));
            int x = available.Get().X();
            int y = available.Get().Y();

            if (!IsInit(x, y) || bState.Get() != STATE_RESERVABLE)
                throw new RuntimeException();

            bState.Set(aa.Area(), STATE_RESERVED);

            available.Swap(available.Count, m - 1);
            available.Set(m - 1);

            return client;
        }

        public int Total()
        {
            return total;
        }

        public int Available()
        {
            return available.Count;
        }

        public bool DeadORDying(int tx, int ty)
        {
            if (IsInit(tx, ty))
                return bState.Get() >= STATE_USED;
            return false;
        }

        void Save(FilePutter f)
        {
            f.Object(available);
            f.I(total);
        }

        void Load(FileGetter f) throws IOException
        {
            available = (ArrayCooShort)f.Object();
            total = f.I();
        }

        void Clear()
        {
            available = new ArrayCooShort(128);
            total = 0;
        }

        void Init(int tx, int ty)
        {
            if (IsInit(tx, ty))
            {
                total++;
                available.Get().Set(tx, ty);
                bState.Set(aa.Area(), STATE_RESERVABLE);
            }
        }

        void Dispose(int tx, int ty)
        {
            if (IsInit(tx, ty))
            {
                total--;
                int m = available.Count;
                for (int i = 0; i < m; i++)
                {
                    if (available.Set(i).IsSameAs(tx, ty))
                    {
                        available.Swap(i, m - 1);
                        available.Set(m - 1);
                        break;
                    }
                }
                bState.Set(aa.Area(), STATE_UNINITED);
            }
        }

        private bool IsInit(int tx, int ty)
        {
            Room r = ROOMS().Map.Get(tx, ty);
            if (r != SETT.ROOMS().EXECUTION.Instance)
                return false;

            int c = SETT.ROOMS().FData.TileData.Get(tx, ty);
            if (c <= 0)
                return false;
            aa.Done();
            aa.Init(r, tx, ty);
            coo.Set(tx, ty);
            return true;
        }

        public int Type(int tx, int ty)
        {
            return SETT.ROOMS().FData.TileData.Get(tx, ty);
        }

        public FSERVICE Service(int tx, int ty)
        {
            if (IsInit(tx, ty))
                return service;
            return null;
        }

        public Client Client(int tx, int ty)
        {
            if (IsInit(tx, ty))
                return client;
            return null;
        }

        public Guard Guard(int tx, int ty)
        {
            if (IsInit(tx, ty))
                return guard;
            return null;
        }

        public class Client
        {
            public bool ClientReserved()
            {
                return bState.Get() >= STATE_RESERVED;
            }

            public bool ClientPresent()
            {
                return bState.Get() > STATE_RESERVED;
            }

            public void ClientUse()
            {
                bState.Set(aa.Area(), STATE_USED);
            }

            public void ClientCancel()
            {
                if (SETT.THINGS().Corpses.TGet.Get(coo) != null)
                    bState.Set(aa.Area(), STATE_DEAD);
                else
                    bState.Set(aa.Area(), STATE_RESERVABLE);
            }

            public bool ClientBeingExecuted()
            {
                return bState.Get() == STATE_EXECUTING;
            }

            public DIR ClientDir()
            {
                return DIR.ORTHO.Get(SETT.ROOMS().FData.Item.Get(coo).Rotation);
            }

            public COORDINATE Coo()
            {
                return coo;
            }
        }

        public class Guard
        {
            public bool Active()
            {
                if (bState.Get() >= STATE_RESERVABLE && bState.Get() < STATE_DEAD)
                    return true;
                return false;
            }

            public bool ShouldExecute()
            {
                return bState.Get() == STATE_USED;
            }

            public bool WorkExecute()
            {
                if (bState.Get() == STATE_USED)
                {
                    bState.Set(aa.Area(), STATE_EXECUTING);

                    if (Type(coo.X(), coo.Y()) == TYPE_CHOP)
                    {
                        foreach (ENTITY e in SETT.ENTITIES().GetAtTile(coo.X(), coo.Y()))
                        {
                            if (e is Humanoid)
                            {
                                Humanoid a = (Humanoid)e;
                                STATS.NEEDS().INJURIES.COUNT.Indu().IncD(a.Indu(), 0.2 + RND.rFloat());
                                SETT.THINGS().Gore.Cloud(a, a.Race().Appearance().Colors.Blood);
                                SETT.THINGS().Gore.Flesh(a, a.Race().Appearance().Colors.Blood);
                                if (STATS.NEEDS().INJURIES.COUNT.Indu().GetD(a.Indu()) > 0.75)
                                {
                                    GAME.Count().EXECUTIONS.Inc(1);
                                    STATS.NEEDS().INJURIES.COUNT.Indu().SetD(a.Indu(), 1.0);
                                    a.Kill(false, CAUSE_LEAVES.EXECUTED());
                                    client.ClientCancel();
                                }
                                return true;
                            }
                        }
                    }
                }
                return false;
            }

            public COORDINATE Coo()
            {
                return coo;
            }
        }

        private readonly FSERVICE service = new FSERVICE()
        {
            public int Y()
            {
                return coo.Y();
            }

            public int X()
            {
                return coo.X();
            }

            public bool FindableReservedIs()
            {
                return bServices.Get() > 0;
            }

            public bool FindableReservedCanBe()
            {
                return bState.Get() == STATE_RESERVABLE;
            }

            public void Report(int change)
            {
                b.Data.Report(service, change);
            }
        };

        private class BB : RoomBits
        {
            public BB(Coo coo, Bits bits) : base(coo, bits) { }

            public override void Set(Room r, int t)
            {
                bool av = bState.Get() == STATE_RESERVABLE;
                bool requested = bState.Get() == STATE_RESERVED;
                if (service.FindableReservedCanBe())
                    b.data.Report(service, -1);
                base.Set(r, t);
                if (service.FindableReservedCanBe())
                    b.data.Report(service, 1);
                if (!av && bState.Get() == STATE_RESERVABLE)
                {
                    if (available.Count - 1 >= available.Size)
                    {
                        ArrayCooShort nn = new ArrayCooShort(available.Size + 128);
                        for (int i = available.Count - 1; i >= 0; i--)
                        {
                            nn.Set(i).Set(available.Set(i));
                        }
                        available = nn;
                    }
                    available.Get().Set(coo.X(), coo.Y());
                    available.Inc();
                }
                if (!requested && bState.Get() == STATE_RESERVED)
                {
                    int tx = coo.X();
                    int ty = coo.Y();
                    SETT.ROOMS().GUARD.Reporter.ReportExecution(tx, ty);
                    IsInit(tx, ty);
                }
            }
        }

        public void Update(int tx, int ty)
        {
            if (IsInit(tx, ty))
            {
                if (bState.Get() == STATE_DEAD)
                {
                    if (SETT.THINGS().Corpses.TGet.Get(tx, ty) == null)
                        bState.Set(aa.Area(), STATE_RESERVABLE);
                }
            }
        }

        int Sevices(int tx, int ty)
        {
            if (Service(tx, ty) != null)
            {
                if (bState.Get() == STATE_DEAD)
                {
                    return services - bServices.Get();
                }
            }
            return 0;
        }

        int State(int tx, int ty)
        {
            if (Service(tx, ty) != null)
            {
                return bState.Get();
            }
            return 0;
        }
    }
}