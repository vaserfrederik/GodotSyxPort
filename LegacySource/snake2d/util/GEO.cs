using System;

namespace snake2d.util
{
    public class GEO
    {
        public static bool Collides(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            // calculate the distance to intersection point
            double uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
            double uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

            // if uA and uB are between 0-1, lines are colliding
            if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
            {
                return true;
            }
            return false;
        }

        /**
         * 
         * @param a
         * @param b
         * @param c
         * @return > 0 left, < 0 right
         */
        public static double LeftOrRight(double ax, double ay, double bx, double by, double cx, double cy)
        {
            return ((bx - ax) * (cy - ay) - (by - ay) * (cx - ax));
        }

        public static double LeftOrRight(double px, double py, COORDINATE l1, COORDINATE l2)
        {
            return ((l1.x() - px) * (l2.y() - py) - (l1.y() - py) * (l2.x() - px));
        }
    }
}