using System;
using System.Linq;

namespace Settlement.Room.Law.Stocks
{
    public class Tile
    {
        private readonly Bits stage = new Bits(0b01111);
        private readonly Bits available = new Bits(0b01111_0000);

        private readonly ROOM_STOCKS b;
        private int x, y;
        private Instance ins;

        public Tile(ROOM_STOCKS b)
        {
            this.b = b;
        }

        public Tile Get(int tx, int ty)
        {
            if (b.Constructor.Service(tx, ty))
            {
                x = tx;
                y = ty;
                ins = b.Get(tx, ty);
                return this;
            }
            return null;
        }

        public enum STATE
        {
            none,
            available,
            reserved,
            used
        }

        private readonly STATE[] states = Enum.GetValues<STATE>().ToArray();

        public STATE State()
        {
            return states[stage.Get(SETT.ROOMS().Data.Get(x, y))];
        }

        public bool Used()
        {
            return State() == STATE.reserved || State() == STATE.used;
        }

        public void Init()
        {
            StateSet(STATE.available);
            AvailableSet(8);
        }

        public void StateSet(STATE state)
        {
            if (State() == STATE.none && state != STATE.none)
                b.Total++;

            if (State() == STATE.reserved || State() == STATE.used)
            {
                b.Used--;
                ins.Available--;
            }

            int d = stage.Set(SETT.ROOMS().Data.Get(x, y), (int)state);
            SETT.ROOMS().Data.Set(ins, x, y, d);

            if (state == STATE.reserved || state == STATE.used)
            {
                b.Used++;
                ins.Available++;
            }
        }

        private void AvailableSet(int am)
        {
            am = Math.Clamp(am, 0, 8);
            ins.Service.Report(service, b.Data, -1, true);

            int d = available.Set(SETT.ROOMS().Data.Get(x, y), am);
            SETT.ROOMS().Data.Set(ins, x, y, d);
            ins.Service.Report(service, b.Data, 1, true);
        }

        public readonly FSERVICE service = new FSERVICE()
        {
            Y = () => y,
            X = () => x,
            FindableReservedIs = () => available.Get(SETT.ROOMS().Data.Get(x, y)) < 8,
            FindableReservedCanBe = () => available.Get(SETT.ROOMS().Data.Get(x, y)) > 0,
            FindableReserveCancel = () => AvailableSet(available.Get(SETT.ROOMS().Data.Get(x, y)) + 1),
            FindableReserve = () => AvailableSet(available.Get(SETT.ROOMS().Data.Get(x, y)) - 1),
            Consume = () => FindableReserveCancel(),
            StartUsing = () => { }
        };
    }
}