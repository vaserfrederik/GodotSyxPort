using System;
using System.IO;
using System.Collections.Generic;
using init.constant;
using init.sprite;
using settlement.entity.humanoid;
using settlement.main;
using settlement.path.finders;
using settlement.path.thread;
using snake2d;
using snake2d.util.color;
using snake2d.util.datatypes;
using snake2d.util.file;
using snake2d.util.sprite;
using snake2d.util.sprite.text;

namespace settlement.path.path
{
    public class SPath : PathGame.PathFancy
    {
        protected short destX, destY;
        protected bool successful = false;
        protected bool arrived = false;
        private bool full = false;
        public static readonly int size = 256;
        private int sx, sy;

        public SPath() : base(size)
        {
        }

        private static Text t = UI.FONT().M.getText(5).AdjustWidth();
        public readonly ThreadPath thread = new ThreadPath();

        public override void Save(FilePutter file)
        {
            file.i(destX).i(destY).i(sx).i(sy);
            file.bool(successful).bool(arrived).bool(full);
            base.Save(file);
        }

        public override void Load(FileGetter file) throws IOException
        {
            destX = (short)file.i();
            destY = (short)file.i();
            sx = file.i();
            sy = file.i();
            successful = file.bool();
            arrived = file.bool();
            full = file.bool();
            base.Load(file);
        }

        public void Render(SPRITE_RENDERER r, int offsetX, int offsetY)
        {
            if (!successful)
                return;
            if (length() == 0)
                return;

            COLOR.YELLOW100.bind();

            int i = 0;
            t.clear();

            SPRITE s = SPRITES.cons().ICO.crosshair;

            int tileI = getCurrentI();

            int x = x() * C.TILE_SIZE + offsetX;
            int y = y() * C.TILE_SIZE + offsetY;
            int d = 0; //(C.TILE_SIZE - Icon.M) / 2;
            s.render(r, x + d, y + d);
            t.clear().add(i++);
            t.render(r, x, y);

            while (super.setNext())
            {
                t.clear().add(i++);
                x = super.x() * C.TILE_SIZE + offsetX;
                y = super.y() * C.TILE_SIZE + offsetY;
                s.render(r, x + d, y + d);
                t.render(r, x, y);
            }

            COLOR.unbind();

            setCurrentI(tileI);
        }

        public bool Request(int startX, int startY, SFINDER f, int maxDistance)
        {
            this.full = false;
            this.sx = startX;
            this.sy = startY;
            this.destX = -1;
            this.destY = -1;
            this.successful = false;
            if (setNear(startX, startY, f))
            {
                SPathFinder finder = SETT.PATH().finders.finder();
                PathTile t = finder.find(new Coordinate(startX, startY), new Coordinate(destX, destY), f);
                if (t != null)
                {
                    sett(t);
                    return true;
                }
            }
            return false;
        }

        public bool Request(COORDINATE start, RECTANGLE body)
        {
            if (!successful)
                return false;

            if (start.isSameAs(this))
                return !PATH().solidity.is(this);

            if (PATH().solidity.is(this))
            {
                return Request(start.x(), start.y(), destX, destY, full);
            }

            int x = x();
            int y = y();
            if (setPrev() && start.isSameAs(this))
            {
                if (PATH().coster.player.getCost(start.x(), start.y(), x, y) >= 0)
                    return true;
            }

            return Request(start.x(), start.y(), destX, destY, full);
        }

        public override bool SetNext()
        {
            if (!successful)
                throw new Exception();

            if (IsDest())
                return false;

            if (!super.hasNext())
            {
                resumeThreaded(x(), y());
                super.setNext();
            }
            else
            {
                super.setNext();
            }

            return successful;
        }

        public int GetSettCX()
        {
            return (x() << C.T_SCROLL) + C.TILE_SIZEH;
        }

        public int GetSettCY()
        {
            return (y() << C.T_SCROLL) + C.TILE_SIZEH;
        }

        public override bool IsDest()
        {
            if (!successful)
                return false;
            if (super.hasNext())
                return false;
            if (full)
                return x() == destX && y() == destY;

            return (Math.Abs(x() - destX) + Math.Abs(y() - destY) == 1);
        }

        public bool IsSuccessful()
        {
            return successful;
        }

        public short DestX()
        {
            return destX;
        }

        public short DestY()
        {
            return destY;
        }

        public bool IsFull()
        {
            return full;
        }

        public string ToDebugString()
        {
            return "(" + "(" + sx + "," + sy + ") --> (" + destX + "," + destY + ") succ:" + IsSuccessful() + ", dest:" + IsDest() + "\n "
                + x() + " " + y() + " " + length() + " " + getCurrentI();
        }

        public static double LAST_DISTANCE()
        {
            return SETT.PATH().finders.finder().lastDistance;
        }

        public override void Clear()
        {
            successful = false;
            full = false;
            base.Clear();
        }

        private void sett(PathTile t)
        {
            set(t);

            if ((full && !t.isSameAs(destX, destY)) || (!full && (Math.Abs(t.x() - destX) + Math.Abs(t.y() - destY) > 1)))
            {
                SETT.PATH().thread.prep(this, t.x(), t.y(), destX, destY, full);
            }

            while (t != null)
            {
                PATH().huristics.set(t.x(), t.y());
                t = t.getParent();
            }
        }

        public void SetDirect(int sx, int sy, int destX, int destY, PathTile t, bool full)
        {
            successful = true;
            this.destX = (short)destX;
            this.destY = (short)destY;
            this.sx = sx;
            this.sy = sy;
            this.full = full;
            sett(t);
            arrived = IsCompleate();
        }

        public void Copy(PathFancy other, int destX, int destY, bool full)
        {
            other.setCurrentI(0);
            other.copyTo(this);
            successful = true;

            this.full = full;
            sx = other.x();
            sy = other.y();
            while (other.hasNext())
            {
                PATH().huristics.set(other.x(), other.y());
                other.setNext();
            }

            this.destX = (short)destX;
            this.destY = (short)destY;
            arrived = other.IsCompleate();
        }

        public override void SetLength(int ll)
        {
            base.SetLength(ll);
            int iold = getCurrentI();
            while (super.setNext())
            {
                destX = (short)x();
                destY = (short)y();
            }
            setCurrentI(iold);
        }

        public override void Debug()
        {
            int iold = getCurrentI();
            setCurrentI(0);
            Console.WriteLine();
            Console.WriteLine("l:" + length());
            for (int i = 0; i < length(); i++)
            {
                setCurrentI(i);
                Console.WriteLine("\t" + "(" + x() + " " + y() + ")");
            }
            setCurrentI(iold);
            Console.WriteLine(destX + " " + destY);
        }
    }
}