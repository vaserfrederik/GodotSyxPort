using System;
using System.IO;
using game.time;
using settlement.main;
using snake2d.util.file;
using snake2d.util.misc;
using util.text;

public sealed class WeatherSnow : WeatherThing
{
    private static readonly string ¤¤name = "Snow";
    private static readonly string ¤¤desc = "Amount of snow on the ground";
    private static readonly double rainspeed = 1.0 / (TIME.secondsPerHour() * 4);
    private static readonly double thawspeed = 1.0 / (4 * TIME.secondsPerHour());
    private double snowCount;

    static WeatherSnow()
    {
        D.ts(typeof(WeatherSnow));
    }

    public WeatherSnow() : base(¤¤name, ¤¤desc)
    {
    }

    public override void update(double ds)
    {
        if (SETT.WEATHER().temp.cold() > 0)
        {
            snowCount -= ds * SETT.WEATHER().temp.cold();
            if (snowCount < 0)
                snowCount = -4;
        }
        else
        {
            snowCount += ds * SETT.WEATHER().temp.heat();
            if (snowCount > 0)
                snowCount = 4;
        }

        double d = getD();
        if (rainIsSnow())
        {
            d += SETT.WEATHER().rain.getD() * rainspeed * ds;
        }
        else
        {
            d -= 2 * SETT.WEATHER().rain.getD() * rainspeed * ds;
        }
        if (SETT.WEATHER().temp.heat() > 0)
        {
            d -= thawspeed * SETT.WEATHER().temp.heat() * ds;
        }
        setD(CLAMP.d(d, 0, 1));
    }

    protected override void init()
    {
        double snow = 0;
        double p = TIME.years().bitPartOf();
        double tmp1 = SETT.WEATHER().temp.average(p - 0.2);
        double tmp2 = SETT.WEATHER().temp.average(p);
        if (tmp1 < 0.5 && tmp2 < 0.5)
        {
            snow = 1;
        }
        else if (tmp1 < 0.5)
        {
            snow = 1 - (tmp2 - 0.5) * 16;
        }
        setD(snow);
    }

    public bool rainIsSnow()
    {
        return snowCount < 0;
    }

    protected override void save(FilePutter file)
    {
        base.save(file);
        file.d(snowCount);
    }

    protected override void load(FileGetter file)
    {
        base.load(file);
        snowCount = file.d();
    }
}