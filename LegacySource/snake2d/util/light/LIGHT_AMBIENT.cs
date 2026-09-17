using System;

namespace Snake2d.Util.Light
{
    public interface ILightAmbient : IRGB
    {
        float X { get; }
        float Y { get; }
        float Z { get; }
    }
}