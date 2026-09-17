using System;

namespace snake2d.util.map
{
    public interface MAP_BOOLEANE : MAP_BOOLEAN
    {
        MAP_BOOLEANE Set(int tile, bool value);

        MAP_BOOLEANE Set(int tx, int ty, bool value);

        default MAP_BOOLEANE Set(int tx, int ty, DIR d, bool value)
        {
            return Set(tx + d.X(), ty + d.Y(), value);
        }

        default MAP_BOOLEANE Set(COORDINATE c, bool value)
        {
            return Set(c.X(), c.Y(), value);
        }

        default MAP_BOOLEANE Set(COORDINATE c, DIR d, bool value)
        {
            return Set(c.X() + d.X(), c.Y() + d.Y(), value);
        }
    }

    public abstract class BooleanMapE : BooleanMap, MAP_BOOLEANE
    {
        public BooleanMapE(int width, int height) : base(width, height)
        {
        }

        public override MAP_BOOLEANE Set(int tx, int ty, bool value)
        {
            if (body.HoldsPoint(tx, ty))
                Set(tx + ty * width, value);
            return this;
        }

        public void SetAll(bool value)
        {
            int a = body.Height() * body.Width();
            for (int i = 0; i < a; i++)
            {
                Set(i, value);
            }
        }
    }
}