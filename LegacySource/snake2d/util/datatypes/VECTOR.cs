using System;

namespace Snake2D.Util.DataTypes
{
    public interface IVector
    {
        double Magnitude();
        double NX();
        double NY();
        Dir Dir();
        double X();
        double Y();
    }
}