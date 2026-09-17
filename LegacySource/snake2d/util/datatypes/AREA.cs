using System;

namespace Snake2D.Util.Datatypes
{
    public interface Area : BodyHolder, MapBoolean
    {
        int Area { get; }
    }
}