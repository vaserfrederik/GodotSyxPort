using System;

namespace Settlement.Room.Water
{
    public abstract class RoomPumpable
    {
        protected abstract void Drain(int tx, int ty);

        protected abstract void Pump(int tx, int ty, DIR from, int dirmask);

        protected virtual void PumpFail(int tx, int ty, int dirmask)
        {
        }

        /**
         * 
         * @param tx
         * @param ty
         * @return a number that is asked before being drained. This number then goes to pump, if pumped
         */
        protected abstract int Dirmask(int tx, int ty);
        protected virtual int Radius()
        {
            return 0;
        }

        public static void ReportChange(int tx, int ty, int radius)
        {
            SETT.ROOMS().WATER.Updater.ReportChange(tx, ty, radius);
        }

        protected abstract bool PumpsTo(int fromX, int fromY, int tx, int ty);

        public virtual double SuckAmount(int tx, int ty)
        {
            return 1;
        }

        public abstract double Irrigation(int tx, int ty);

        public interface ROOM_PUMPABLE
        {
            public RoomPumpable Pumpable(int tx, int ty);
        }
    }
}