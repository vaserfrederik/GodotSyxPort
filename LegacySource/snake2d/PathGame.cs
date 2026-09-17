using System;
using System.IO;

namespace snake2d
{
    public interface PathGame : COORDINATE, SAVABLE
    {
        int getCapacity();
        int length();
        bool isStart();
        void setStart();
        bool isDest();
        bool hasNext();
        bool setNext();
        int getCurrentI();
        void setCurrentI(int i);
        bool hasPrev();
        bool setPrev();
    }

    public interface COST
    {
        double BLOCKED { get; }
        double SKIP { get; }

        double getCost(int fromX, int fromY, int toX, int toY);
    }

    public abstract class DEST
    {
        protected abstract bool isDest(int x, int y);
        protected abstract float getOptDistance(int x, int y);

        public abstract class CLOSEST : DEST
        {
            protected override float getOptDistance(int x, int y)
            {
                return 0;
            }
        }
    }

    public class PathSimple : PathGame
    {
        private const int bitA = 2;
        private const int tilesPerInt = 8;
        private const long mask = 0x0000000000000003;
        private const long maskI = 0x00000000FFFFFFFC;

        private readonly int[] bits;
        private int length;
        private int tilesI = 0;
        private short currentX, currentY;

        public PathSimple(int size)
        {
            int ints = size / tilesPerInt;
            if (size % tilesPerInt > 0)
                ints++;
            this.bits = Alloc.ii(ints);
        }

        public void save(FilePutter file)
        {
            file.is(bits);
            file.i(length);
            file.i(tilesI);
            file.i(currentX).i(currentY);
        }

        public void load(FileGetter file)
        {
            file.is(bits);

            length = file.i();
            tilesI = file.i();
            currentX = (short)file.i();
            currentY = (short)file.i();
        }

        public void clear()
        {
            tilesI = 0;
            length = 0;
        }

        public void reverse()
        {
            while (hasPrev())
                setPrev();
        }

        public void copyTo(PathSimple other)
        {
            if (other.getCapacity() < getCapacity())
                throw new Exception();
            other.length = Math.Clamp(length, 0, other.getCapacity());
            other.tilesI = Math.Clamp(tilesI, 0, other.getCapacity());
            other.currentX = currentX;
            other.currentY = currentY;
            for (int i = 0; i < bits.Length; i++)
            {
                other.bits[i] = bits[i];
            }
        }

        private int get(int index)
        {
            if (index < 0 || index >= bits.Length * tilesPerInt * bitA)
                throw new Exception("outof");

            index *= bitA;
            int i1 = index / 32;

            index += bitA;

            int i2 = index / 32;
            int b2 = index % 32;
            if (b2 == 0)
                b2 = 32;

            int res = bits[i1];

            if (i1 == i2 || b2 == 32)
            {
                res = res >> (32 - b2);
                res &= (int)mask;
            }
            else
            {
                res = res << b2;
                res &= (int)mask;
                int res2 = bits[i2];
                res2 = res2 >> (32 - b2);
                res2 &= (int)(mask >> (b2));
                res |= res2;
                res &= (int)mask;
            }

            return res;
        }

        private void set(int index, int value)
        {
            if (index < 0 || index >= bits.Length * tilesPerInt * bitA)
                throw new Exception("outof");

            value &= (int)mask;

            index *= bitA;
            int i1 = index / 32;

            index += bitA;

            int i2 = index / 32;
            int b2 = index % 32;
            if (b2 == 0)
                b2 = 32;

            bits[i1] &= (int)(maskI >> (b2 * 2));
            bits[i1] |= (int)(value << (b2 * 2));

            if (b2 != 0)
            {
                bits[i2] &= (int)(maskI >> (b2 * 2));
                bits[i2] |= (int)(value >> (32 - (b2 * 2)));
            }
        }

        public int getCapacity()
        {
            return bits.Length * tilesPerInt;
        }

        public int length()
        {
            return length;
        }

        public bool isStart()
        {
            return tilesI == 0;
        }

        public void setStart()
        {
            while (hasPrev())
                setPrev();
        }

        public bool isDest()
        {
            return tilesI >= length - 1;
        }

        public bool hasNext()
        {
            return tilesI < length - 1;
        }

        public bool nextIsLast()
        {
            return tilesI == length - 2;
        }

        public bool setNext()
        {
            if (hasNext())
            {
                int x = get(tilesI * 2 + 1);
                int y = get(tilesI * 2);
                if (x == 3)
                    x = -1;
                if (y == 3)
                    y = -1;
                currentX -= (short)x;
                currentY -= (short)y;
                tilesI++;
                return true;
            }
            return false;
        }

        public bool hasPrev()
        {
            return tilesI > 0;
        }

        public bool setPrev()
        {
            if (hasPrev())
            {
                tilesI--;
                int y = get(tilesI * 2);
                int x = get(tilesI * 2 + 1);

                if (x == 3)
                    x = -1;
                if (y == 3)
                    y = -1;
                currentX += (short)x;
                currentY += (short)y;
                return true;
            }
            return false;
        }

        public int x()
        {
            if (length > 0)
                return currentX;
            return -1;
        }

        public int y()
        {
            if (length > 0)
                return currentY;
            return -1;
        }

        public int getCurrentI()
        {
            return tilesI;
        }

        public void setCurrentI(int i)
        {
            if (i < 0 || i >= length)
                throw new Exception(i + " " + length);
            while (tilesI < i)
                setNext();
            while (tilesI > i)
                setPrev();
        }

        public void debug()
        {
            int iold = getCurrentI();
            setCurrentI(0);
            Printer.ln();
            Printer.ln("l:" + length());
            for (int i = 0; i < length; i++)
            {
                setCurrentI(i);
                Printer.ln("\t" + "(" + x() + " " + y() + ")");
            }
            setCurrentI(iold);
            Printer.ln();
        }

        protected void setLength(int length)
        {
            this.length = Math.Min(length, this.length);
        }
    }

    public class PathFancy : PathSimple
    {
        private bool compleate;
        private double totalCost;
        private int totalLength;

        public PathFancy(int size) : base(size)
        {
        }

        public override void save(FilePutter file)
        {
            base.save(file);
            file.bool(compleate);
            file.d(totalCost);
            file.i(totalLength);
        }

        public override void load(FileGetter file)
        {
            base.load(file);
            compleate = file.bool();
            totalCost = file.d();
            totalLength = file.i();
        }

        public override void clear()
        {
            base.clear();
            compleate = false;
        }

        public override void copyTo(PathFancy other)
        {
            base.copyTo(other);
            if (other.getCapacity() < getCapacity())
                throw new Exception();
            other.compleate = compleate;
            other.totalCost = totalCost;
            other.totalLength = totalLength;
        }

        public override bool set(PathTile dest)
        {
            base.set(dest);
            totalCost = dest.accCost;

            int i = 1;
            PathTile t = dest.pathParent;
            while (t != null)
            {
                i++;
                t = t.pathParent;
            }
            this.totalLength = i;

            compleate = base.set(dest);
            return compleate;
        }

        public override void setOne(int destX, int destY)
        {
            base.setOne(destX, destY);
            totalCost = 0;
            compleate = true;
            totalLength = 1;
        }

        public override void setTwo(int x1, int y1, int x2, int y2)
        {
            base.setTwo(x1, y1, x2, y2);
            totalCost = 0;
            totalLength = 2;
            compleate = true;
        }

        public double getTotalCost()
        {
            return totalCost;
        }

        public int lengthTotal()
        {
            return totalLength;
        }

        public bool isCompleate()
        {
            return compleate;
        }
    }
}