using System;
using settlement.main;
using game.audio;
using init.resources;
using settlement.entity.humanoid;
using settlement.room.main;
using settlement.thing;
using snake2d.util.bit;
using snake2d.util.datatypes;

namespace settlement.room.spirit.grave
{
    final class Grave
    {
        static readonly int ITEM_MARK = 1;
        static readonly int DIG_MARK = 2;
        private static readonly Bits id = new Bits(0x000000FFFF);
        private static readonly Bits state = new Bits(0x00000F0000);
        private static readonly Bit reserved = new Bit(0x0000100000);
        private static readonly Bits time = new Bits(0x00FF000000);

        private const int UNUSED = 0;
        private const int RESERVED = 2;
        private const int USED = 3;
        private int data;
        private readonly Coo coo = new Coo();
        private GraveInstance ins;

        private readonly RoomBlueprintIns<GraveInstance> b;
        private readonly GraveData d;

        public Grave(RoomBlueprintIns<GraveInstance> b, GraveData d)
        {
            this.b = b;
            this.d = d;
        }

        public int DaysTillDecompose(int tx, int ty)
        {
            if (Get(tx, ty) != null)
            {
                return time.Get(data);
            }
            return 0;
        }

        public bool Init(int tx, int ty, int i)
        {
            if (b.Is(tx, ty))
            {
                if (SETT.ROOMS().fData.tileData.Get(tx, ty) == ITEM_MARK)
                {
                    SETT.ROOMS().data.Set(ins, tx, ty, id.Set(0, i));
                    return true;
                }
            }
            return false;
        }

        public Grave Get(int tx, int ty)
        {
            if (b.Is(tx, ty))
            {
                if (SETT.ROOMS().fData.tileData.Get(tx, ty) == ITEM_MARK)
                {
                    int data = SETT.ROOMS().data.Get(tx, ty);
                    this.data = data;
                    coo.Set(tx, ty);
                    ins = b.Get(tx, ty);
                    return this;
                }
            }
            return null;
        }

        public void UpdateDay2()
        {
            if (state.Get(data) == USED)
            {
                data = time.Inc(data, -1);
                SETT.ROOMS().data.Set(ins, coo, data);
                if (time.Get(data) == 0)
                {
                    GraveInfo.Get(ins, id.Get(data)).Clear();
                    data = state.Set(data, UNUSED);
                    Save();
                }
            }
        }

        public FSERVICE Service(int tx, int ty)
        {
            Grave g = Get(tx, ty);
            if (g != null)
                return g.service;
            return null;
        }

        public GRAVE_JOB Job(int tx, int ty)
        {
            Grave g = Get(tx, ty);
            if (g != null)
                return g.job;
            return null;
        }

        public bool IsUsable()
        {
            return state.Get(data) == UNUSED;
        }

        public static bool IsUsed(int tx, int ty)
        {
            int i = state.Get(SETT.ROOMS().data.Get(tx, ty));
            return i == USED;
        }

        private void Save()
        {
            int old = SETT.ROOMS().data.Get(coo);

            if (old != data)
            {
                int current = data;
                data = old;
                if (service.FindableReservedCanBe())
                    SETT.ROOMS().graveServiceSpots.Report(service, -1);

                if (state.Get(data) == UNUSED)
                {
                    ins.Count(-1);
                }

                data = current;
                if (service.FindableReservedCanBe())
                    SETT.ROOMS().graveServiceSpots.Report(service, 1);

                if (state.Get(data) == UNUSED)
                {
                    ins.Count(1);
                }

                SETT.ROOMS().data.Set(ins, coo, data);
            }
        }

        public readonly FSERVICE service = new FSERVICE
        {
            public int X()
            {
                return coo.X();
            }

            public int Y()
            {
                return coo.Y();
            }

            public bool FindableReservedCanBe()
            {
                return (state.Get(data) == USED) && !reserved.Is(data);
            }

            public void FindableReserve()
            {
                if (!FindableReservedCanBe())
                    throw new Exception();
                data = reserved.Set(data);
                Save();
            }

            public bool FindableReservedIs()
            {
                return reserved.Is(data);
            }

            public void FindableReserveCancel()
            {
                data = reserved.Clear(data);
                data = state.Set(data, USED);
                Save();
            }

            public void Consume()
            {
                data = reserved.Clear(data);
                Save();
            }
        };

        private readonly GRAVE_JOB job = new GRAVE_JOB
        {
            public bool JobUseTool()
            {
                return true;
            }

            public void JobStartPerforming()
            {
                // TODO Auto-generated method stub
            }

            public SoundRace JobSound()
            {
                return ins.BlueprintI().Employment().Sound();
            }

            public RBIT JobResourceBitToFetch()
            {
                return null;
            }

            public bool JobReservedIs(RESOURCE r)
            {
                if (state.Get(data) == RESERVED)
                {
                    return true;
                }
                return false;
            }

            public void JobReserveCancel(RESOURCE r)
            {
                if (JobReservedIs(r))
                {
                    data = state.Set(data, UNUSED);
                    Save();
                }
            }

            public bool JobReserveCanBe()
            {
                if (state.Get(data) == UNUSED)
                {
                    return true;
                }
                return false;
            }

            public void JobReserve(RESOURCE r)
            {
                data = state.Set(data, RESERVED);
                Save();
            }

            public double JobPerformTime(Humanoid skill)
            {
                return 10;
            }

            public string JobName()
            {
                return null;
            }

            public COORDINATE JobCoo()
            {
                return coo;
            }

            public void BuryAndPerform(Corpse c)
            {
                if (c != null)
                {
                    d.Get(c.Indu().Clas()).Burry(c);
                    GraveInfo.Get(ins, id.Get(data)).Burry(c);
                    c.Remove();
                    data = state.Set(data, USED);
                    data = time.Set(data, d.ComposeTime);
                }
                else
                {
                    data = state.Set(data, UNUSED);
                }

                Save();
            }
        };

        public bool Reuse()
        {
            if (state.Get(data) == USED)
            {
                data = state.Set(data, UNUSED);
                Save();
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (state.Get(data) == UNUSED || state.Get(data) == RESERVED)
            {

            }

            if (service.FindableReservedCanBe())
                SETT.ROOMS().graveServiceSpots.Report(service, -1);

            data = state.Set(data, UNUSED);
            SETT.ROOMS().data.Set(ins, coo, data);
        }

        public void Deactivate()
        {
            if (state.Get(data) == UNUSED || state.Get(data) == RESERVED)
            {

            }
        }
    }
}