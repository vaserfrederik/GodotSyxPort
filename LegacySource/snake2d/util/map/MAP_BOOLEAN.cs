using System;

namespace snake2d.util.map
{
    public interface MAP_BOOLEAN
    {
        public bool Is(int tile);

        public bool Is(int tx, int ty);

        public bool Is(int tx, int ty, DIR d)
        {
            return Is(tx + d.X, ty + d.Y);
        }

        public bool Is(COORDINATE c)
        {
            return Is(c.X, c.Y);
        }

        public bool Is(COORDINATE c, DIR d)
        {
            return Is(c.X + d.X, c.Y + d.Y);
        }

        public abstract class BooleanMap : MAP_BOOLEAN
        {
            public readonly int Width;
            public readonly RECTANGLE Body;

            public BooleanMap(int width, int height)
            {
                this.Width = width;
                this.Body = new Rec(width, height);
            }

            public BooleanMap(DIMENSION dim)
            {
                this.Width = dim.Width();
                this.Body = new Rec(dim.Width(), dim.Height());
            }

            public override bool Is(int tx, int ty)
            {
                return Body.HoldsPoint(tx, ty) && Is(tx + ty * Width);
            }
        }

        public abstract class MAP_BOOLEAN_IMP : MAP_BOOLEAN
        {
            private readonly int Width;

            public MAP_BOOLEAN_IMP(int width)
            {
                this.Width = width;
            }

            public override bool Is(int tx, int ty)
            {
                return Is(tx + ty * Width);
            }
        }
    }

    public class COORDINATE
    {
        public int X { get; }
        public int Y { get; }

        public COORDINATE(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class DIMENSION
    {
        public int Width()
        {
            // Implementation
            return 0;
        }

        public int Height()
        {
            // Implementation
            return 0;
        }
    }

    public class DIR
    {
        public int X { get; }
        public int Y { get; }

        public DIR(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    public class RECTANGLE
    {
        public bool HoldsPoint(int x, int y)
        {
            // Implementation
            return false;
        }
    }

    public class Rec : RECTANGLE
    {
        public Rec(int width, int height)
        {
            // Implementation
        }
    }
}