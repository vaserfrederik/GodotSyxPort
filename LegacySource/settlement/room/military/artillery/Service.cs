using System;
using game.battle;
using init.constant;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path.finders;
using snake2d.util.datatypes;

namespace settlement.room.military.artillery
{
    class Service
    {
        private readonly ROOM_ARTILLERY blue;
        private int x, y;
        private ArtilleryInstance ins;

        Service(ROOM_ARTILLERY blue)
        {
            this.blue = blue;
        }

        public FINDABLE_MANNING get(int tx, int ty)
        {
            ins = blue.get(tx, ty);
            if (ins != null && ins.mustered())
            {
                if (SETT.ROOMS().fData.tileData.get(tx, ty) == Constructor.SERVICE)
                {
                    x = tx;
                    y = ty;
                    return ser;
                }
            }
            return null;
        }

        public void activate(int tx, int ty)
        {
            if (get(tx, ty) != null)
            {
                SETT.ROOMS().data.set(ins, x, y, 0);
                blue.service(tx, ty).report(tx, ty, 1);
            }
        }

        public void deactivate(int tx, int ty)
        {
            if (get(tx, ty) != null)
            {
                if (get(tx, ty).findableReservedCanBe())
                    blue.service(tx, ty).report(tx, ty, -1);
                SETT.ROOMS().data.set(ins, x, y, 0);
            }
        }

        private readonly FINDABLE_MANNING ser = new FINDABLE_MANNING()
        {
            public int y()
            {
                return y;
            }

            public int x()
            {
                return x;
            }

            public bool findableReservedIs()
            {
                return SETT.ROOMS().data.get(x, y) == 1;
            }

            public bool findableReservedCanBe()
            {
                return !findableReservedIs();
            }

            public void findableReserveCancel()
            {
                if (findableReservedIs())
                {
                    blue.service(x, y).report(x, y, 1);
                    SETT.ROOMS().data.set(ins, x, y, 0);
                    ins.men--;
                }
            }

            public void findableReserve()
            {
                if (!findableReservedIs())
                {
                    blue.service(x, y).report(x, y, -1);
                    SETT.ROOMS().data.set(ins, x, y, 1);
                    ins.men++;
                }
            }

            public DIR faceDIR()
            {
                return DIR.get((x << C.T_SCROLL) + C.TILE_SIZEH, (y << C.T_SCROLL) + C.TILE_SIZEH, ins.centre());
            }

            public void work(double time, Humanoid a)
            {
                ins.work(time, a);
            }

            public bool needsWork()
            {
                return ins.needsWork();
            }

            public Army army()
            {
                return ins.army();
            }
        };
    }
}