using System;
using init.settings;
using snake2d.util.misc;

public sealed class LightShadows
{
    private double dir;
    private double sx, sy;
    private double sLength;
    private double tilt;
    public bool IsNight { get; private set; }
    public bool Rising { get; private set; }

    public LightShadows()
    {
    }

    public void Update(Light light)
    {
        double ttilt = 1.0;
        if (S.Get().LightCycle.Get() == 0)
        {
            dir = 180;
            tilt = 25;
        }
        else
        {
            dir = (360 + 90) - light.Time.GetD() * 360;
            dir %= 360;

            double dayL = TIME.Seasons().CurrentDay.DayLength();
            double nightL = 1.0 - dayL;

            final double nightA = (nightL) / 2;
            final double mid = nightA + dayL / 2;
            final double nightB = nightA + dayL;
            if (light.Time.GetD() < nightA)
            {
                ttilt = 1.0 - light.Time.GetD() / nightA;
                IsNight = true;
                Rising = false;
            }
            else if (light.Time.GetD() < mid)
            {
                ttilt = (light.Time.GetD() - nightA) / (mid - nightA);
                IsNight = false;
                Rising = true;
            }
            else if (light.Time.GetD() < nightB)
            {
                ttilt = 1.0 - (light.Time.GetD() - mid) / (nightB - mid);
                IsNight = false;
                Rising = false;
            }
            else
            {
                ttilt = (light.Time.GetD() - nightB) / (1.0 - nightB);
                IsNight = true;
                Rising = true;
            }

            ttilt = Math.Pow(ttilt, 1);
            tilt = 5 + 35 * ttilt;
        }

        sLength = 0.5 + 3 - 3 * Math.Pow(CLAMP.d(tilt / 50, 0, 1), 1);
        double ra = Math.ToRadians(dir);
        sLength = (1 + S.Get().Shadows.Get()) * sLength;

        sx = -sLength * Math.Cos(ra);
        sy = -sLength * Math.Sin(ra);
    }

    public double Dir()
    {
        return dir;
    }

    public double DirNight()
    {
        return (100 + dir) % 360;
    }

    public double Sx()
    {
        return sx;
    }

    public double Sy()
    {
        return sy;
    }

    public double Tilt()
    {
        return tilt;
    }

    public double Dtilt()
    {
        return (tilt - 5) / 35;
    }
}