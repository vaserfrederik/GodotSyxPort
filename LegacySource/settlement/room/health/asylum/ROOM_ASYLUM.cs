using System;
using System.Collections.Generic;
using System.IO;

namespace Settlement.Room.Health.Asylum
{
    public sealed class RoomAsylum : RoomBlueprintIns<AsylumInstance>, INDUSTRY_HASER
    {
        private readonly Constructor _constructor;
        private int _prisonersCurrent;
        private int _prisonersMax;
        private readonly Industry _consumption;
        private readonly List<Industry> _industries;

        public RoomAsylum(RoomInitData init, RoomCategorySub block) : base(0, init, "_ASYLUM", block)
        {
            _constructor = new Constructor(this, init);
            _consumption = new Industry(this, init.Data, null);

            _industries = new List<Industry> { _consumption };
        }

        protected override void Update(double ds)
        {
            // TODO Auto-generated method stub
        }

        public int Prisoners => _prisonersCurrent;

        public int PrisonersMax => _prisonersMax;

        public override Furnisher Constructor => _constructor;

        public void IncPrisoners(int p, int total)
        {
            _prisonersCurrent += p;
            _prisonersMax += total;
        }

        protected override void SaveP(FilePutter f)
        {
            f.WriteInt(_prisonersCurrent);
            f.WriteInt(_prisonersMax);
            IndustryUtil.Save(f, _industries);
        }

        protected override void LoadP(FileGetter f)
        {
            _prisonersCurrent = f.ReadInt();
            _prisonersMax = f.ReadInt();

            _prisonersCurrent = 0;
            foreach (AsylumInstance i in All())
                _prisonersCurrent += i.Prisoners;

            IndustryUtil.Load(f, _industries);
        }

        protected override void ClearP()
        {
            _prisonersCurrent = 0;
            _prisonersMax = 0;
            IndustryUtil.Clear(_industries);
        }

        public override SFinderRoomService Service(int tx, int ty)
        {
            // TODO Auto-generated method stub
            return null;
        }

        public COORDINATE RegisterPrisoner(Humanoid h)
        {
            if (_prisonersCurrent >= _prisonersMax)
                return null;
            if (Is(h.Tc()))
            {
                AsylumInstance ins = Get(h.Tc().X, h.Tc().Y);
                if (ins.Active && ins.Prisoners < ins.PrisonersMax)
                {
                    return ins.RegisterPrisoner(h);
                }
            }

            int i = RND.RInt(InstancesSize());
            for (int k = 0; k < InstancesSize(); k++)
            {
                AsylumInstance ins = GetInstance((k + i) % InstancesSize());
                if (ins.Active && ins.Prisoners < ins.PrisonersMax)
                {
                    return ins.RegisterPrisoner(h);
                }
            }

            for (int k = 0; k < InstancesSize(); k++)
            {
                AsylumInstance ins = GetInstance((k + i) % InstancesSize());
                Console.WriteLine($"{ins.Active} {ins.Prisoners} {ins.PrisonersMax}");
            }

            throw new InvalidOperationException($"{_prisonersCurrent} {_prisonersMax}");
        }

        public void UnregisterPrisoner(COORDINATE c)
        {
            if (Is(c) && Getter.Get(c).Active)
            {
                Getter.Get(c).RemovePrisoner(c.X, c.Y);
            }
        }

        public bool EatFood(COORDINATE cell)
        {
            if (Is(cell))
            {
                for (int di = 0; di < DIR.ORTHO.Size; di++)
                {
                    DIR dir = DIR.ORTHO.Get(di);
                    Food f = Food.Init(cell.X + dir.X, cell.Y + dir.Y);
                    if (f != null && f.Food > 0)
                    {
                        f.Consume();
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsWithinCell(int nx, int ny, COORDINATE cell)
        {
            if (Is(nx, ny) && Is(cell.X, cell.Y))
            {
                return _constructor.IsWithinCell(nx, ny, cell.X, cell.Y);
            }
            return false;
        }

        public bool IsDoor(COORDINATE cell)
        {
            return Is(cell) && SETT.ROOMS().fData.TileData.Get(cell) == Constructor.CODE_ENTRANCE;
        }

        public bool IsReserved(COORDINATE cell)
        {
            return Is(cell) && Getter.Get(cell).Active && Getter.Get(cell).IsReserved(cell.X, cell.Y);
        }

        public double TreatmentFactor(COORDINATE cell)
        {
            AsylumInstance i = Get(cell.X, cell.Y);
            return TreatmentFactor(i);
        }

        private double TreatmentFactor(AsylumInstance i)
        {
            if (i != null)
                return Math.Clamp(0.25 + 0.75 * (1.0 - i.Degrade) * i.Employees.Employed / i.Employees.Max, 0, 1);
            return 0;
        }

        public override void AppendView(List<UIRoomModule> mm)
        {
            mm.Add(new Gui(this).Make());
        }

        public override List<Industry> Industries => _industries;
    }
}