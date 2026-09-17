using System;
using System.Collections.Generic;

namespace Settlement.Room.Knowledge.School
{
    using Game.Audio;
    using Game.Time;
    using Init.Resources;
    using Settlement.Entity.Humanoid;
    using Settlement.Main;
    using Settlement.Misc.Job;
    using Settlement.Misc.Util;
    using Snake2D.Util.Bit;
    using Snake2D.Util.Bit;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.DataTypes;
    using Snake2D.Util.DataTypes;

    internal sealed class SchoolStation
    {
        private SchoolInstance Ins;
        private readonly RoomSchool B;
        private readonly Work Work = new Work();
        private readonly Service Service = new Service();

        public SchoolStation(RoomSchool b)
        {
            this.B = b;
        }

        public FSERVICE Service(int tx, int ty)
        {
            if (B.Is(tx, ty))
            {
                int i = SETT.Rooms().FData.TileData.Get(tx, ty);
                if (i == SchoolConstructor.ISERVICE)
                {
                    Ins = B.Get(tx, ty);
                    Service.Data = SETT.Rooms().Data.Get(tx, ty);
                    Service.X = tx;
                    Service.Y = ty;
                    for (int di = 0; di < DIR.ORTHO.Size; di++)
                    {
                        int dx = DIR.ORTHO.Get(di).X + tx;
                        int dy = DIR.ORTHO.Get(di).Y + ty;
                        if (Ins.Is(dx, dy) && SETT.Rooms().FData.TileData.Get(dx, dy) == SchoolConstructor.IWORK)
                        {
                            Work.Coo.Set(dx, dy);
                            Work.Data = SETT.Rooms().Data.Get(dx, dy);
                            return Service;
                        }
                    }
                }
            }
            return null;
        }

        public DIR ServiceDir(int tx, int ty)
        {
            if (B.Is(tx, ty))
            {
                int i = SETT.Rooms().FData.TileData.Get(tx, ty);
                if (i == SchoolConstructor.ISERVICE)
                {
                    Ins = B.Get(tx, ty);
                    for (int di = 0; di < DIR.ORTHO.Size; di++)
                    {
                        int dx = DIR.ORTHO.Get(di).X + tx;
                        int dy = DIR.ORTHO.Get(di).Y + ty;
                        if (Ins.Is(dx, dy) && SETT.Rooms().FData.TileData.Get(dx, dy) == SchoolConstructor.IWORK)
                        {
                            return DIR.ORTHO.Get(di);
                        }
                    }
                }
            }
            return null;
        }

        public SETT_JOB Job(int tx, int ty)
        {
            if (B.Is(tx, ty))
            {
                int i = SETT.Rooms().FData.TileData.Get(tx, ty);
                if (i == SchoolConstructor.IWORK)
                {
                    Ins = B.Get(tx, ty);
                    Work.Data = SETT.Rooms().Data.Get(tx, ty);
                    Work.Coo.Set(tx, ty);
                    for (int di = 0; di < DIR.ORTHO.Size; di++)
                    {
                        int dx = DIR.ORTHO.Get(di).X + tx;
                        int dy = DIR.ORTHO.Get(di).Y + ty;
                        if (Ins.Is(dx, dy) && SETT.Rooms().FData.TileData.Get(dx, dy) == SchoolConstructor.ISERVICE)
                        {
                            Service.X = dx;
                            Service.Y = dy;
                            Service.Data = SETT.Rooms().Data.Get(dx, dy);
                            return Work;
                        }
                    }
                }
            }
            return null;
        }

        public void Dispose(int tx, int ty)
        {
            if (Job(tx, ty) == null)
                return;
            if (Service.FindableReservedCanBe())
                Service.FindableReserve();
            if (Work.Paper.Get(Work.Data) > 0)
                SETT.Things().Resources.Create(Work.JobCoo(), B.Industry.Ins()[0].Resource, Work.Paper.Get(Work.Data));
        }

        internal sealed class Work : SETT_JOB
        {
            private readonly Bit Reserved = new Bit(0b0000_0000_0001);
            private readonly Bits Dones = new Bits(0b0000_0000_0110);
            private readonly Bits Paper = new Bits(0b0000_1111_0000);
            private readonly Bits FetchFree = new Bits(0b0001_0000_0000);
            private readonly Coo Coo = new Coo();
            private int Data;

            private int Wt = (int)(TIME.WorkSeconds() / 40);

            public void JobReserve(RESOURCE r)
            {
                if (!JobReserveCanBe())
                    throw new Exception();
                Data = Reserved.Set(Data);
                Save();
            }

            public bool JobReservedIs(RESOURCE r)
            {
                return Reserved.Is(Data);
            }

            public void JobReserveCancel(RESOURCE r)
            {
                Data = Reserved.Clear(Data);
                Save();
            }

            public bool JobReserveCanBe()
            {
                if (Reserved.Is(Data))
                    return false;
                if (Dones.Get(Data) < 3)
                {
                    return true;
                }
                return false;
            }

            public RBIT JobResourceBitToFetch()
            {
                if (Paper.Get(Data) < 1)
                    return B.Industry.Ins()[0].Resource.Bit;
                return null;
            }

            public double JobPerformTime(Humanoid skill)
            {
                if (FetchFree.Get(Data) == 1)
                    return 0;
                return Wt;
            }

            public void JobStartPerforming()
            {
            }

            public RESOURCE JobPerform(Humanoid skill, RESOURCE r, int ram)
            {
                if (r == B.Industry.Ins()[0].Resource)
                {
                    Data = Paper.Inc(Data, ram);
                }
                else
                {
                    Data = Dones.Inc(Data, 1);
                    Data = FetchFree.Set(Data, 0);
                    Service.SetReserveable();
                }

                if (FetchFree.Get(Data) == 0 && Ins.Employees().FetchBonusConsume(Wt))
                {
                    Data = FetchFree.Set(Data, 1);
                }

                JobReserveCancel(r);
                return null;
            }

            public int JobResourcesNeeded(Humanoid skill)
            {
                return Paper.Mask;
            }

            public COORDINATE JobCoo()
            {
                return Coo;
            }

            public string JobName()
            {
                return B.Employment().Verb.ToString();
            }

            public bool JobUseTool()
            {
                return false;
            }

            public SoundRace JobSound()
            {
                return B.Employment().Sound();
            }

            private void Save()
            {
                int c = Data;
                Data = SETT.Rooms().Data.Get(Coo);

                Data = c;

                SETT.Rooms().Data.Set(Ins, Coo, Data);
            }

            internal void Consume(bool day)
            {
                Data = Dones.Inc(Data, -1);
                if (day)
                {
                    int p = B.Industry.Ins()[0].IncDay(Ins);
                    if (p > 0)
                    {
                        Data = Paper.Inc(Data, -p);
                    }
                }
                Save();
                Ins.Jobs.SearchAgain();
            }
        }

        internal sealed class Service : FSERVICE
        {
            public int X, Y;
            public int Data;

            private readonly Bit Reserved = new Bit(0b0000_0000_0001);
            private readonly Bit Reservable = new Bit(0b0000_0000_0010);
            private readonly Bit Used = new Bit(0b0000_0000_0100);

            public int Y()
            {
                return Y;
            }

            public int X()
            {
                return X;
            }

            public bool FindableReservedIs()
            {
                return Reserved.Is(Data);
            }

            public bool FindableReservedCanBe()
            {
                return !Reserved.Is(Data) && Reservable.Is(Data);
            }

            public void FindableReserveCancel()
            {
                Data = Reserved.Clear(Data);
                Save();
            }

            public void FindableReserve()
            {
                if (FindableReservedCanBe())
                {
                    Data = Reserved.Set(Data);
                    Save();
                }
            }

            public void StartUsing()
            {
                if (FindableReservedIs())
                {
                    Data = Used.Set(Data);
                    Save();
                    Work.Consume(false);
                }
            }

            public void Consume()
            {
                Data = 0;
                Work.Consume(true);
                if (Work.Dones.Get(Work.Data) > 0)
                {
                    Data = Reservable.Set(Data);
                }
                Save();
            }

            internal void SetReserveable()
            {
                Data = Reservable.Set(Data);
                Save();
            }

            private void Save()
            {
                int c = Data;
                Data = SETT.Rooms().Data.Get(this);
                Ins.Service().Report(this, Ins.BlueprintI().Service(), -1);
                Data = c;
                Ins.Service().Report(this, Ins.BlueprintI().Service(), 1);
                SETT.Rooms().Data.Set(Ins, X, Y, Data);
            }
        }
    }
}