using System;
using init.constant;
using init.resources;
using settlement.main;
using snake2d;
using snake2d.util.datatypes;
using util.rendering;

namespace settlement.room.service.food.canteen
{
    class SChair
    {
        public const int I = 3;

        private readonly ROOM_CANTEEN b;
        private readonly Coo res = new Coo();

        public SChair(ROOM_CANTEEN b)
        {
            this.b = b;
        }

        public COORDINATE Get(int sx, int sy)
        {
            CanteenInstance ins = b.getter.Get(sx, sy);
            if (ins == null)
                return null;

            int tx = ins.tableX;
            int ty = ins.tableY;

            if (tx == -1)
                return null;

            int a = ins.body().Width() * ins.body().Height();

            for (int i = 0; i < a; i++)
            {
                if (ins.Is(tx, ty))
                {
                    if (SETT.ROOMS().fData.tileData.Get(tx, ty) == I)
                    {
                        if (SETT.ROOMS().data.Get(tx, ty) == 0)
                        {
                            ins.tableX = (short)tx;
                            ins.tableY = (short)ty;
                            return Ret(ins, tx, ty);
                        }
                    }
                }
                tx++;
                if (tx >= ins.body().X2())
                {
                    tx = ins.body().X1();
                    ty++;
                    if (ty >= ins.body().Y2())
                    {
                        ty = ins.body().Y1();
                    }
                }
            }

            ins.tableX = -1;
            ins.tableY = -1;
            return null;
        }

        COORDINATE Ret(CanteenInstance ins, int x, int y)
        {
            ins.tableX = (short)x;
            ins.tableY = (short)y;
            SETT.ROOMS().data.Set(ins, x, y, 1);
            res.Set(x, y);
            return res;
        }

        public DIR Set(int tx, int ty, int mealData)
        {
            CanteenInstance ins = b.getter.Get(tx, ty);
            if (ins == null)
                return null;
            if (SETT.ROOMS().fData.tileData.Get(tx, ty) != I)
            {
                return null;
            }

            COORDINATE u = SETT.ROOMS().fData.ItemX1Y1(tx, ty, Coo.TMP);
            int ux = u.X();
            int uy = u.Y();

            for (int di = 0; di < DIR.ORTHO.Size(); di++)
            {
                DIR d = DIR.ORTHO.Get(di);
                if (ins.Is(tx, ty, d) && SETT.ROOMS().fData.tileData.Get(tx, ty, d) != I && SETT.ROOMS().fData.Tile.Get(tx, ty, d) != null)
                {
                    u = SETT.ROOMS().fData.ItemX1Y1(tx, ty, d, Coo.TMP);
                    if (u != null && u.IsSameAs(ux, uy))
                    {
                        SETT.ROOMS().data.Set(ins, tx, ty, d, mealData);
                        return d;
                    }
                }
            }

            return null;
        }

        void ReturnTable(int tx, int ty)
        {
            CanteenInstance ins = b.getter.Get(tx, ty);
            if (ins == null)
                return;
            if (SETT.ROOMS().fData.tileData.Get(tx, ty) != I)
            {
                return;
            }
            SETT.ROOMS().data.Set(ins, tx, ty, 0);
            COORDINATE u = SETT.ROOMS().fData.ItemX1Y1(tx, ty, Coo.TMP);
            int ux = u.X();
            int uy = u.Y();

            for (int di = 0; di < DIR.ORTHO.Size(); di++)
            {
                DIR d = DIR.ORTHO.Get(di);
                if (SETT.ROOMS().fData.tileData.Get(tx, ty, d) != I && SETT.ROOMS().fData.Tile.Get(tx, ty, d) != null)
                {
                    u = SETT.ROOMS().fData.ItemX1Y1(tx, ty, d, Coo.TMP);
                    if (u != null && u.IsSameAs(ux, uy))
                    {
                        SETT.ROOMS().data.Set(ins, tx, ty, d, 0);
                        if (ins.tableX == -1)
                        {
                            ins.tableX = (short)ins.body().X1();
                            ins.tableY = (short)ins.body().Y1();
                        }
                        return;
                    }
                }
            }
        }

        void Render(SPRITE_RENDERER r, ShadowBatch s, int rotMask, RenderIterator it, int am, RESOURCE res)
        {
            rotMask &= 0b01111;

            DIR d = DIR.N;
            for (int i = 0; i < DIR.ORTHO.Size(); i++)
            {
                if ((rotMask | d.Mask()) == 0b1111)
                    break;
                d = d.Next(2);
            }

            int x1 = C.TILE_SIZEH / 2 * d.X();
            int y1 = C.TILE_SIZEH / 2 * d.Y();

            int dd = 0;

            while (am-- > 0)
            {
                int ddd = dd / 3;
                int dddd = dd % 3;
                int x = -d.X() * ddd + d.Y() * (-1 + dddd);
                int y = -d.Y() * ddd + d.X() * (-1 + dddd);
                am--;
                dd++;
                it.SetOff(x1 + x * C.TILE_SIZEH / 2, y1 + y * C.TILE_SIZEH / 2);
                b.constructor.RenderDish(r, s, res, it, it.Ran());
            }
        }
    }
}