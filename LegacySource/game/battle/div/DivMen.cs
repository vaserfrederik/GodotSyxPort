using System;
using System.IO;
using init.constant;
using snake2d.util.file;

namespace game.battle.div
{
    sealed class DivMen : SAVABLE
    {
        private readonly short[] order;
        private readonly short[] orderR;
        private short orderI = 0;

        public DivMen()
        {
            orderR = new short[Config.battle().MEN_PER_DIVISION];
            order = new short[Config.battle().MEN_PER_DIVISION];
            Clear();
        }

        public short GetSpot(int i)
        {
            return orderR[i];
        }

        public short SpotTranslate(int i)
        {
            return order[i];
        }

        public int FreeSpots()
        {
            return order.Length - orderI;
        }

        public int Men()
        {
            return orderI;
        }

        public short GetNewSpot()
        {
            if (FreeSpots() <= 0)
                throw new Exception();
            short spot = order[orderI];
            orderR[spot] = orderI;
            orderI++;

            return spot;
        }

        public void ReturnSpot(short spot)
        {
            if (orderI == 0)
                throw new Exception();
            if (orderI == 1)
            {
                orderI = 0;
                return;
            }
            int spotLast = order[orderI - 1];
            int orderIA = orderR[spot];
            if (orderIA >= orderI)
                throw new Exception(orderIA + " " + orderI);

            order[orderIA] = (short)spotLast;
            order[orderI - 1] = spot;

            orderR[spot] = -1;
            orderR[spotLast] = (short)orderIA;

            orderI--;
        }

        public void Save(FilePutter file)
        {
            file.ss(order);
            file.ss(orderR);
            file.i(orderI);
        }

        public void Load(FileGetter file)
        {
            file.ss(order);
            file.ss(orderR);
            orderI = (short)file.i();
        }

        public void Clear()
        {
            for (int i = 0; i < order.Length; i++)
            {
                order[i] = (short)i;
                orderR[i] = -1;
            }
            orderI = 0;
        }
    }
}