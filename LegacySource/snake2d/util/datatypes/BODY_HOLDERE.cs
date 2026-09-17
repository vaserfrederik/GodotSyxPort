using System;

namespace Snake2D.Util.DataTypes
{
    public interface BodyHolderE : BodyHolder
    {
        new RectangleE Body();
    }
}