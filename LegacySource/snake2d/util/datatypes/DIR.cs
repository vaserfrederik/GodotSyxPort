using System;
using System.Collections.Generic;
using System.IO;

namespace Snake2D.Util.DataTypes
{
    public enum DIR : byte
    {
        N,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW,
        C
    }

    public interface COORDINATE
    {
        int X { get; }
        int Y { get; }
        double TileDistance { get; }
    }

    public static class DIRExtensions
    {
        private static readonly List<DIR> All = new List<DIR> { DIR.N, DIR.NE, DIR.E, DIR.SE, DIR.S, DIR.SW, DIR.W, DIR.NW };
        private static readonly List<DIR> AllC = new List<DIR> { DIR.N, DIR.NE, DIR.E, DIR.SE, DIR.S, DIR.SW, DIR.W, DIR.NW, DIR.C };
        private static readonly List<DIR> Ortho = new List<DIR> { DIR.N, DIR.E, DIR.S, DIR.W };
        private static readonly List<DIR> OrthoC = new List<DIR> { DIR.C, DIR.N, DIR.E, DIR.S, DIR.W };
        private static readonly List<DIR> Northo = new List<DIR> { DIR.NE, DIR.SE, DIR.SW, DIR.NW };

        private static readonly Dictionary<DIR, (int X, int Y, int Mask, string Name, int BoxID, double NormalizedX, double NormalizedY, double Distance)> dirData = new Dictionary<DIR, (int X, int Y, int Mask, string Name, int BoxID, double NormalizedX, double NormalizedY, double Distance)>
        {
            { DIR.N, (0, -1, 1, "north", 1, 0, -1, 1) },
            { DIR.NE, (1, -1, 1, "north-east", 2, 1, -1, Math.Sqrt(0.5)) },
            { DIR.E, (1, 0, 2, "east", 5, 1, 0, 1) },
            { DIR.SE, (1, 1, 2, "south-east", 8, 1, 1, Math.Sqrt(0.5)) },
            { DIR.S, (0, 1, 4, "south", 7, 0, 1, 1) },
            { DIR.SW, (-1, 1, 4, "south-west", 6, -1, 1, Math.Sqrt(0.5)) },
            { DIR.W, (-1, 0, 8, "west", 3, -1, 0, 1) },
            { DIR.NW, (-1, -1, 16, "north-west", 0, -1, -1, Math.Sqrt(0.5)) },
            { DIR.C, (0, 0, 0, "centre", 4, 0, 0, 0) }
        };

        public static DIR Get(COORDINATE coo)
        {
            return Get(coo.X, coo.Y);
        }

        public static DIR Get(COORDINATE from, COORDINATE to)
        {
            return Get(to.X - from.X, to.Y - from.Y);
        }

        public static DIR Get(int fx, int fy, COORDINATE to)
        {
            return Get(to.X - fx, to.Y - fy);
        }

        public static DIR Get(COORDINATE from, int tx, int ty)
        {
            return Get(tx - from.X, ty - from.Y);
        }

        public static DIR Get(RECTANGLE a, RECTANGLE b)
        {
            double dx = b.CX - a.CX;
            double dy = b.CY - a.CY;
            return Get(dx, dy);
        }

        public static DIR Get(int fx, int fy, int tx, int ty)
        {
            return Get(tx - fx, ty - fy);
        }

        public static DIR Get(double norX2, double norY2)
        {
            if (norX2 == 0 && norY2 == 0)
                return DIR.C;
            if (norX2 == 0)
                return norY2 < 0 ? DIR.N : DIR.S;
            if (norY2 == 0)
                return norX2 < 0 ? DIR.W : DIR.E;

            double ratio = Math.Abs(norX2 / norY2);

            if (ratio < 0.38)
                return norY2 < 0 ? DIR.N : DIR.S;
            if (ratio > 2.43)
                return norX2 < 0 ? DIR.W : DIR.E;

            if (norY2 < 0)
            {
                if (norX2 < 0)
                    return DIR.NW;
                return DIR.NE;
            }

            if (norX2 < 0)
                return DIR.SW;
            return DIR.SE;
        }

        public static int ToBoxID(int orthoMask)
        {
            // Implement the toBoxID logic here
            return 0; // Placeholder
        }

        public static DIR Next(this DIR dir, int nr)
        {
            return All[(int)dir + nr & 7];
        }

        public static DIR Perpendicular(this DIR dir)
        {
            return All[(int)dir + All.Count / 2 % All.Count];
        }

        public static void PositionWithin(this DIR dir, RECTANGLEE target, RECTANGLE reference)
        {
            int x = reference.X1 + reference.Width / 2 * ((int)dir + 1);
            int y = reference.Y1 + reference.Height / 2 * ((int)dir + 1);
            target.MoveX2(x);
            target.MoveY2(y);
        }

        public static void PositionCentered(this DIR dir, RECTANGLEE target, RECTANGLE reference)
        {
            int x = reference.X1 + reference.Width / 2 * ((int)dir + 1);
            int y = reference.Y1 + reference.Height / 2 * ((int)dir + 1);
            target.MoveC(x, y);
        }

        public static void PositionEdge(this DIR dir, Rec target, RECTANGLE reference)
        {
            int x = reference.X1 + reference.Width / 2 * ((int)dir + 1);
            int y = reference.Y1 + reference.Height / 2 * ((int)dir + 1);
            target.MoveX1Y1(x, y);
        }

        public static void Reposition(this DIR dir, Rec old, int nWidth, int nHeight)
        {
            if ((int)dir < 0)
            {
                old.SetWidth(nWidth);
            }
            else if ((int)dir > 0)
            {
                int x2 = old.X2;
                old.SetWidth(nWidth);
                old.MoveX2(x2);
            }
            else
            {
                int cx = old.CX;
                old.SetWidth(nWidth);
                old.MoveCX(cx);
            }

            if ((int)dir < 0)
            {
                old.SetHeight(nHeight);
            }
            else if ((int)dir > 0)
            {
                int y2 = old.Y2;
                old.SetHeight(nHeight);
                old.MoveY2(y2);
            }
            else
            {
                int cY = old.CY;
                old.SetHeight(nHeight);
                old.MoveCY(cY);
            }
        }

        public static bool IsOrtho(this DIR dir)
        {
            return Math.Abs((int)dir) == 1;
        }

        public static int OrthoID(this DIR dir)
        {
            return (int)dir >> 1;
        }

        public static void Save(this DIR dir, FilePutter file)
        {
            byte b = dir == DIR.C ? (byte)-1 : (byte)dir;
            file.Write(b);
        }

        public static DIR Load(FileGetter file)
        {
            byte b = file.Read();
            if (b < 0)
                return DIR.C;
            return All[b];
        }

        private static DIR Get(int fx, int fy)
        {
            // Implement the logic to determine the direction based on fx and fy
            return DIR.C; // Placeholder
        }
    }

    public class RECTANGLE
    {
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public double CX => X1 + Width / 2;
        public double CY => Y1 + Height / 2;
    }

    public class RECTANGLEE : RECTANGLE
    {
        public void MoveX2(int x)
        {
            // Implement the logic to move the rectangle based on X2
        }

        public void MoveY2(int y)
        {
            // Implement the logic to move the rectangle based on Y2
        }

        public void MoveC(int x, int y)
        {
            // Implement the logic to move the rectangle based on its center
        }
    }

    public class Rec
    {
        public int X2 { get; set; }
        public int Y2 { get; set; }

        public void SetWidth(int width)
        {
            // Implement the logic to set the width of the rectangle
        }

        public void SetHeight(int height)
        {
            // Implement the logic to set the height of the rectangle
        }

        public void MoveX2(int x)
        {
            // Implement the logic to move the rectangle based on X2
        }

        public void MoveY2(int y)
        {
            // Implement the logic to move the rectangle based on Y2
        }

        public void MoveCX(int x)
        {
            // Implement the logic to move the rectangle based on its center X
        }

        public void MoveCY(int y)
        {
            // Implement the logic to move the rectangle based on its center Y
        }
    }

    public class FilePutter
    {
        private readonly Stream stream;

        public FilePutter(Stream stream)
        {
            this.stream = stream;
        }

        public void Write(byte b)
        {
            stream.WriteByte(b);
        }
    }

    public class FileGetter
    {
        private readonly Stream stream;

        public FileGetter(Stream stream)
        {
            this.stream = stream;
        }

        public byte Read()
        {
            return (byte)stream.ReadByte();
        }
    }
}