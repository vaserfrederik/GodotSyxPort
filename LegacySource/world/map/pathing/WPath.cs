using System;
using System.IO;
using System.Collections.Generic;
using Init.Constant;
using Snake2D;
using Snake2D.Util.DataTypes;
using Snake2D.Util.File;
using Snake2D.Util.Misc;
using Snake2D.Util.Rnd;
using World;
using World.Entity;
using World.Map.Pathing;
using World.Map.Road;

namespace World.Map.Pathing
{
    public abstract class WPath : SAVABLE
    {
        private short destX, destY = -1;
        private byte dir;
        private float movement = 0;
        private readonly PathGame.PathSimple tilePath = new PathGame.PathSimple(256);
        private const double sqrt = Math.Sqrt(2);

        public WPath()
        {
            Clear();
            dir = (byte)RND.RInt(DIR.ALL.Size);
        }

        public override void Save(FilePutter file)
        {
            tilePath.Save(file);
            file.S(destX);
            file.S(destY);
            file.B(dir);
        }

        public override void Load(FileGetter file)
        {
            tilePath.Load(file);
            destX = file.S();
            destY = file.S();
            dir = file.B();
        }

        public override void Clear()
        {
            tilePath.Clear();
            destX = -1;
            destY = -1;
        }

        public void CopyTo(WPath tmp)
        {
            tmp.destX = destX;
            tmp.destY = destY;
            tmp.dir = dir;
            tilePath.CopyTo(tmp.tilePath);
        }

        public int DestX()
        {
            return destX;
        }

        public int DestY()
        {
            return destY;
        }

        public bool IsValid()
        {
            return destX >= 0 && destY >= 0;
        }

        public DIR Dir()
        {
            return DIR.ALL.Get(dir);
        }

        public bool Arrived()
        {
            return destX == tilePath.X() && destY == tilePath.Y();
        }

        public int X()
        {
            return tilePath.X();
        }

        public int Y()
        {
            return tilePath.Y();
        }

        public bool Find(int sx, int sy, int destX, int destY)
        {
            Clear();
            if (sx == destX && sy == destY)
            {
                this.destX = (short)destX;
                this.destY = (short)destY;
                tilePath.SetOne(destX, destY);
                return true;
            }

            if (!WORLD.PATH().Map.Is.Is(destX, destY))
            {
                return false;
            }

            if (!WORLD.PATH().Map.Is.Is(sx, sy))
            {
                DIR d = DIR.Get(sx, sy, destX, destY);
                if (WORLD.PATH().Map.Is.Is(sx, sy, d))
                {
                    ;
                }
                else if (WORLD.PATH().Map.Is.Is(sx, sy, d.Next(1)))
                {
                    d = d.Next(1);
                }
                else if (WORLD.PATH().Map.Is.Is(sx, sy, d.Next(-1)))
                {
                    d = d.Next(-1);
                }
                else
                {
                    return false;
                }
                sx += d.X();
                sy += d.Y();
            }

            PathTile t = WORLD.PATH().Path(sx, sy, destX, destY, Treaty());

            if (t != null)
            {
                tilePath.Set(t);
                this.destX = (short)destX;
                this.destY = (short)destY;

                return true;
            }

            return false;
        }

        public bool Move(WEntity ent, double speed)
        {
            RECTANGLEE e = ent.Body();

            speed *= WPATHING.MovementSpeed(ent.Ctx(), ent.Cty());
            if (!WORLD.WATER().Is(ent.Ctx(), ent.Cty()) && WORLD.WATER().IsBig.Is(X(), Y()))
            {
                speed /= WTRAV.PORT_PENALTY * 2;
            }

            return Move(e, speed);
        }

        public bool Move(RECTANGLEE e, double speed)
        {
            if (!Moving(e))
                return false;

            movement += (float)speed;

            while (movement > sqrt && IsValid())
                if (!Move(e))
                {
                    movement = 0;
                    return false;
                }

            return true;
        }

        private bool Move(RECTANGLEE e)
        {
            int cx = e.CX();
            int cy = e.CY();

            int destx = X() * C.TILE_SIZE + C.TILE_SIZEH;
            int desty = Y() * C.TILE_SIZE + C.TILE_SIZEH;

            int x = 0;
            int y = 0;
            while (movement > sqrt)
            {
                if (cx == destx && cy == desty)
                {
                    return SetNext();
                }
                x = destx - cx;
                y = desty - cy;
                x = CLAMP.I(x, -1, 1);
                y = CLAMP.I(y, -1, 1);

                if (Math.Abs(x) != Math.Abs(y))
                {
                    movement -= 1;
                }
                else
                {
                    movement -= (float)sqrt;
                }

                cx += x;
                cy += y;
            }

            dir = (byte)DIR.Get(x, y).Id();
            e.MoveC(cx, cy);
            return true;
        }

        public bool Moving(RECTANGLEE e)
        {
            if (destX == -1 || destY == -1)
            {
                movement = 0;
                return false;
            }

            if (X() == destX && Y() == destY)
            {
                int cx = e.CX();
                int cy = e.CY();
                int dx = X() * C.TILE_SIZE + C.TILE_SIZEH;
                int dy = Y() * C.TILE_SIZE + C.TILE_SIZEH;
                if (cx == dx && cy == dy)
                    return false;
            }
            return true;
        }

        public int Remaining()
        {
            return tilePath.Length() - tilePath.GetCurrentI();
        }

        public bool SetNext()
        {
            if (tilePath.SetNext())
            {
                return true;
            }

            if (destX == tilePath.X() && destY == tilePath.Y())
                return false;

            if (Find(tilePath.X(), tilePath.Y(), destX, destY))
            {
                tilePath.SetNext();
                return true;
            }

            return false;
        }

        public abstract Treaty Treaty();
    }
}