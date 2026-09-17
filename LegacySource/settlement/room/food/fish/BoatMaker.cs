using System;
using System.Collections.Generic;
using snake2d.util.datatypes;

namespace settlement.room.food.fish
{
    class BoatMaker
    {
        public static void Make(FishInstance ins)
        {
            foreach (COORDINATE c in ins.Body())
            {
                if (ins.Is(c))
                {
                    if (SETT.ROOMS().fData.tile.Get(c) != null)
                    {
                        continue;
                    }

                    if (SETT.TERRAIN().WATER.SHALLOW.Is(c) && SETT.TERRAIN().WATER.deepSeaFishSpot.Is(c))
                    {
                        Make(ins, c);
                    }
                }
            }
        }

        private static void Make(FishInstance ins, COORDINATE start)
        {
            for (int di = 0; di < DIR.ALL.Size; di++)
            {
                if (Make(ins, start, DIR.ALL.Get(di)))
                {
                    return;
                }
            }
        }

        private static bool Make(FishInstance ins, COORDINATE start, DIR dir)
        {
            int fx = start.X();
            int fy = start.Y();
            for (int i = 0; i <= RoomInstance.MAX_DIM; i++)
            {
                int tx = fx + dir.X();
                int ty = fy + dir.Y();
                if (ins.Is(fx, fy) && SETT.TERRAIN().WATER.SHALLOW.Is(fx, fy) && !SETT.TERRAIN().WATER.open.Is(fx, fy))
                {
                    bool border = true;
                    foreach (DIR d in DIR.ALL)
                    {
                        if (!ins.Is(fx, fy, d) || Job.isWork.Is(SETT.ROOMS().data.Get(fx, fy, d)))
                        {
                            border = false;
                            break;
                        }
                    }
                    if (border)
                    {
                        int data = Job.isWork.Set(0);
                        data = Job.isShip.Set(data);
                        data = Job.shipDir.Set(data, dir.Perpendicular().Id());
                        SETT.ROOMS().data.Set(ins, fx, fy, data);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (!ins.Body().HoldsPoint(tx, ty))
                {
                    return false;
                }
                if (!SETT.TERRAIN().WATER.is.Is(tx, ty))
                {
                    return false;
                }
                if (!dir.IsOrtho())
                {
                    if (!SETT.TERRAIN().WATER.is.Is(fx, ty) || !SETT.TERRAIN().WATER.is.Is(tx, fy))
                    {
                        return false;
                    }
                }

                fx = tx;
                fy = ty;
            }
            return false;
        }
    }
}