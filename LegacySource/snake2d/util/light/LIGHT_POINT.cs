using System;

namespace Snake2d.Util.Light
{
    public interface ILightPoint
    {
        float GetRed();
        float GetGreen();
        float GetBlue();
        float GetFalloff();
        int GetRadius();
        float Cx();
        float Cy();
        float Cz();
    }
}