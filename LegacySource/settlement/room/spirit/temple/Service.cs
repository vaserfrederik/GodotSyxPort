using settlement.room.spirit.temple;

namespace Settlement.Room.Spirit.Temple
{
    final class Service
    {
        private TempleInstance ins;
        private int x, y;
        private readonly ROOM_TEMPLE blue;

        Service(ROOM_TEMPLE blue)
        {
            this.blue = blue;
        }

        public FSERVICE Get(int tx, int ty)
        {
            ins = blue.Get(tx, ty);
            if (ins != null && blue.constructor.wo == SETT.ROOMS().fData.tile.Get(tx, ty))
            {
                x = tx;
                y = ty;
                return s;
            }
            return null;
        }

        public void Init(int tx, int ty)
        {
            if (Get(tx, ty) != null)
            {
                SETT.ROOMS().data.Set(ins, x, y, 1);
                s.FindableReserveCancel();
            }
        }

        public void Dispose(int tx, int ty)
        {
            if (Get(tx, ty) != null)
            {
                s.FindableReserve();
            }
        }

        private readonly FSERVICE s = new FSERVICE()
        {
            public override int Y()
            {
                return y;
            }

            public override int X()
            {
                return x;
            }

            public override bool FindableReservedIs()
            {
                return SETT.ROOMS().data.Get(x, y) == 1;
            }

            public override bool FindableReservedCanBe()
            {
                return !FindableReservedIs();
            }

            public override void FindableReserveCancel()
            {
                if (FindableReservedIs())
                {
                    SETT.ROOMS().data.Set(ins, x, y, 0);
                    ins.service.Report(s, blue.service, 1);
                }
            }

            public override void FindableReserve()
            {
                if (!FindableReservedIs())
                {
                    ins.service.Report(s, blue.service, -1);
                    SETT.ROOMS().data.Set(ins, x, y, 1);
                }
            }

            public override void Consume()
            {
                FindableReserveCancel();
            }
        };
    }
}