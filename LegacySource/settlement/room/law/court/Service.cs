using System;

namespace Settlement.Room.Law.Court
{
    using Settlement.Main;
    using Settlement.Misc.Util;
    using Settlement.Room.Main.Util;
    using Snake2D.Util.DataTypes;

    public sealed class Service : FSERVICE
    {
        private readonly Coo coo = new Coo();
        private CourtInstance ins;
        private static readonly Service self = new Service();

        private readonly RoomBits breservable = new RoomBits(coo, 0b0000_0000_0001)
        {
            protected override void Remove()
            {
                if (Get() == 1)
                    ins.Service().Report(this, ins.BlueprintI().Data, -1);
            }

            protected override void Add()
            {
                if (Get() == 1)
                    ins.Service().Report(this, ins.BlueprintI().Data, 1);
            }
        };

        public static Service Init(int tx, int ty)
        {
            CourtInstance ins = SETT.ROOMS().COURT.Get(tx, ty);
            if (ins == null)
                return null;

            if (SETT.ROOMS().FData.TileData.Get(tx, ty) == Constructor.CodeSpectator)
            {
                self.ins = ins;
                self.coo.Set(tx, ty);
                return self;
            }
            return null;
        }

        public static void InitInit(int tx, int ty, CourtInstance ins)
        {
            Service s = Init(tx, ty);

            if (s != null)
            {
                s.breservable.Set(ins, 1);
            }
        }

        public override bool FindableReservedCanBe()
        {
            return !FindableReservedIs();
        }

        public override void FindableReserve()
        {
            breservable.Set(ins, 0);
        }

        public override bool FindableReservedIs()
        {
            return breservable.Get() == 0;
        }

        public override void FindableReserveCancel()
        {
            breservable.Set(ins, 1);
        }

        public override int X()
        {
            return coo.X();
        }

        public override int Y()
        {
            return coo.Y();
        }

        public override void Consume()
        {
            FindableReserveCancel();
        }

        internal void Activate()
        {
            FindableReserveCancel();
        }

        internal void Deactivate()
        {
            FindableReserve();
        }
    }
}