using System;

namespace snake2d.util.datatypes
{
    public interface RECTANGLEE : RECTANGLE
    {
        RECTANGLEE incr(double x, double y);
        RECTANGLEE incrX(double amount);
        RECTANGLEE incrY(double amount);
        RECTANGLEE incr(COORDINATE vector);
        RECTANGLEE incr(COORDINATE vector, double factor);

        RECTANGLEE moveX1Y1(double X, double Y);
        RECTANGLEE moveX1Y1(COORDINATE vector);
        RECTANGLEE moveX1Y1(RECTANGLE other);
        RECTANGLEE moveX1(double X1);
        RECTANGLEE moveX2(double X2);
        RECTANGLEE moveY1(double Y1);
        RECTANGLEE moveY2(double Y2);
        // RECTANGLEE scale(double scale);

        RECTANGLEE moveC(COORDINATE c);
        RECTANGLEE moveC(double X, double Y);
        RECTANGLEE moveCX(double X);
        RECTANGLEE moveCY(double Y);
        RECTANGLEE centerIn(BODY_HOLDER b);
        RECTANGLEE centerIn(RECTANGLE other);
        RECTANGLEE centerIn(double x1, double x2, double y1, double y2);
        RECTANGLEE centerX(double x1, double x2);
        RECTANGLEE centerX(BODY_HOLDER b);
        RECTANGLEE centerX(RECTANGLE other);
        RECTANGLEE centerY(double y1, double y2);
        RECTANGLEE centerY(BODY_HOLDER b);
        RECTANGLEE centerY(RECTANGLE other);

        RECTANGLEE fitIn(RECTANGLE other);
    }
}