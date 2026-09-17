using System;
using System.Collections.Generic;

namespace snake2d.util.datatypes
{
    public interface RECTANGLE : DIMENSION, Serializable, IEnumerable<COORDINATE>
    {
        int x1();
        int x2();
        int y1();
        int y2();
        int cX();
        int cY();

        bool holdsPoint(double x, double y);
        bool holdsPoint(double x, double y, DIR d);
        bool holdsPoint(COORDINATE coo);
        bool holdsPoint(COORDINATE c, DIR d);
        bool touches(double x, double y);
        bool touches(BODY_HOLDER other);
        bool touches(RECTANGLE other);
        bool touches(int x1, int x2, int y1, int y2);
        bool fitsIn(BODY_HOLDER other);
        bool isWithin(RECTANGLE other);
        bool isWithin(int x1, int x2, int y1, int y2);
        bool isSameAs(BODY_HOLDER other);
        bool isSameAs(RECTANGLE other);
        int getDistance(RECTANGLE b);
        bool isOnEdge(int x, int y);
    }
}