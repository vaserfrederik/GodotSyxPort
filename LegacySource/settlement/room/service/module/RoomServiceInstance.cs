using System;

namespace Settlement.Room.Service.Module
{
    [Serializable]
    public class RoomServiceInstance
    {
        private static readonly long SerialVersionUID = 1L;
        private short available;
        private short reserved = 0;
        private readonly short total;
        public byte CurrentHigh;
        public byte LastHigh;

        public RoomServiceInstance(int total, RoomService data)
        {
            this.total = (short)total;
            data.IncreServices(this.total, 0);
        }

        public void Report(FSERVICE s, RoomService data, int delta)
        {
            Report(s, data, delta, true);
        }

        public void Report(FSERVICE s, RoomService data, int delta, bool load)
        {
            if (s.FindableReservedCanBe())
            {
                available += (short)delta;
                data.IncreServices(0, delta);
                if (delta < 0)
                {
                    data.Finder.Report(s, -1);
                }
                else
                {
                    data.Finder.Report(s, 1);
                }
            }
            else if (s.FindableReservedIs())
            {
                reserved += (short)delta;
            }

            if (load)
            {
                byte h = (byte)(127 * (Total() - Available()) / (double)Total());
                if (h > CurrentHigh)
                {
                    CurrentHigh = h;
                }
                if (h > LastHigh)
                {
                    LastHigh = h;
                }
            }
        }

        public void Report(FSERVICE s, RoomService data, int delta, int reservable, int reserved)
        {
            this.reserved += (short)(reserved * delta);
            available += (short)(reservable * delta);
            data.IncreServices(0, reservable * delta);

            if (s.FindableReservedCanBe())
            {
                if (delta < 0)
                {
                    data.Finder.Report(s, -1);
                }
                else
                {
                    data.Finder.Report(s, 1);
                }
            }

            byte h = (byte)(127 * (Total() - Available()) / (double)Total());
            if (h > CurrentHigh)
            {
                CurrentHigh = h;
            }
            if (h > LastHigh)
            {
                LastHigh = h;
            }
        }

        public int Available()
        {
            return available;
        }

        public int Total()
        {
            return total;
        }

        public int Reserved()
        {
            return reserved;
        }

        public double Load()
        {
            return LastHigh / 127.0;
        }

        public void UpdateDay()
        {
            LastHigh = CurrentHigh;
            CurrentHigh = 0;
        }

        public void ClearLoad()
        {
            LastHigh = 0;
            CurrentHigh = 0;
        }

        public void Dispose(RoomService data)
        {
            data.IncreServices(-this.total, -available);
            available = 0;
        }
    }
}