using System;
using GodotSyxPort.Data;

namespace GodotSyxPort.Simulation;

/// <summary>Simulation-only consolidation of settlement.weather; renderers and UI stay deferred.</summary>
public sealed class SettlementWeatherRuntime
{
    private readonly ClimateRule _climate;
    private readonly double _secondsPerHour;
    private readonly double _secondsPerDay;
    private int _day = -1;
    private double _temperatureTarget;
    private double _downfallTarget;
    private double _cloudTarget;
    private double _thunderTarget;
    private double _lastSnow;

    public double Temperature { get; private set; }
    public double Downfall { get; private set; }
    public double Moisture { get; private set; } = 0.75;
    public double Snow { get; private set; }
    public double Ice { get; private set; }
    public double Wind { get; private set; } = 0.5;
    public double Clouds { get; private set; }
    public double Thunder { get; private set; }
    public double Growth { get; private set; }
    public double Ripeness { get; private set; }
    public bool CropsAreRipe => Ripeness >= 1.0 && Moisture > 0.25;
    public double Heat => Temperature > 0.5 ? 2 * (Temperature - 0.5) : 0;
    public double Cold => Temperature < 0.5 ? 2 * (0.5 - Temperature) : 0;
    public double CropGrowthMultiplier => Math.Clamp(Moisture * 4.0, 0, 1);

    public SettlementWeatherRuntime(ClimateRule climate, double secondsPerHour, double secondsPerDay)
    {
        _climate = climate;
        _secondsPerHour = secondsPerHour;
        _secondsPerDay = secondsPerDay;
        Temperature = Math.Clamp(AverageTemperature(0), 0, 1);
        _temperatureTarget = Temperature;
    }

    public void Tick(double delta, double playedSeconds)
    {
        var day = (int)(playedSeconds / _secondsPerDay);
        var yearPart = playedSeconds / (_secondsPerDay * 16.0) % 1.0;
        var dayPart = playedSeconds / _secondsPerDay % 1.0;
        if (day != _day)
        {
            _day = day;
            var variation = (Hash01(day, 17) - 0.5) * (4.0 / 60.0);
            _temperatureTarget = Math.Clamp(AverageTemperature(yearPart) + variation, 0, 1);
            _downfallTarget = Hash01(day, 31) > 0.68 ? Hash01(day, 47) : 0;
            _cloudTarget = Math.Clamp(_downfallTarget + Hash01(day, 61) * 0.25, 0, 1);
            _thunderTarget = Heat * _downfallTarget;
        }

        var nightCooling = dayPart is < 0.25 or > 0.75 ? 1.0 - 4.0 / 60.0 : 1.0;
        Temperature = Towards(Temperature, _temperatureTarget * nightCooling, delta / _secondsPerDay);
        Downfall = Towards(Downfall, _downfallTarget, delta * 0.5 / _secondsPerHour);
        Clouds = Towards(Clouds, _cloudTarget, delta / (1.5 * _secondsPerHour));
        Thunder = Towards(Thunder, _thunderTarget, delta / _secondsPerHour);
        Wind = Towards(Wind, 0.25 + 0.75 * Hash01(day, 73), delta * 0.1);

        if (Cold > 0) Snow = Clamp01(Snow + Downfall * delta / (4 * _secondsPerHour));
        else Snow = Clamp01(Snow - (2 * Downfall + Heat) * delta / (4 * _secondsPerHour));
        Ice = Clamp01(Ice - (Temperature - 0.5) * 2 * delta / (8 * _secondsPerHour));
        if (Cold <= 0) Moisture += Downfall * delta * 2 / _secondsPerHour;
        Moisture += Math.Max(0, _lastSnow - Snow);
        if (Heat > 0) Moisture -= delta / (8 * _secondsPerDay);
        Moisture = Clamp01(Moisture);
        _lastSnow = Snow;

        var autumn = yearPart > 0.5;
        if (!autumn && Heat > 0) Growth += delta / _secondsPerDay;
        else if (autumn && Heat < 0.25) Growth -= delta / (2 * _secondsPerDay);
        Growth = Clamp01(Growth);
        Ripeness = yearPart > 5.0 / 8.0
            ? Clamp01(1 - (yearPart - 5.0 / 8.0) * 4)
            : yearPart > 1.0 / 8.0 ? Clamp01((yearPart - 1.0 / 8.0) * 4) : 0;
    }

    public double AverageTemperature(double yearPart)
    {
        var winter = 0.5 + 0.5 * Math.Cos(yearPart * Math.PI * 2);
        var seasonal = winter * _climate.SeasonalChange;
        return _climate.TempWarm + (_climate.TempCold - _climate.TempWarm) * seasonal;
    }

    private static double Towards(double current, double target, double speed) =>
        current < target ? Math.Min(target, current + speed) : Math.Max(target, current - speed);
    private static double Clamp01(double value) => Math.Clamp(value, 0, 1);
    private static double Hash01(int value, int salt)
    {
        unchecked
        {
            uint hash = (uint)(value * 0x1f123bb5 ^ salt * 0x5f356495);
            hash ^= hash >> 16; hash *= 0x7feb352d; hash ^= hash >> 15;
            return hash / (double)uint.MaxValue;
        }
    }
}
