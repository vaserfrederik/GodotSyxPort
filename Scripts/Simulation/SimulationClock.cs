using System;

namespace GodotSyxPort.Simulation;

public sealed class SimulationClock
{
    public const double FixedStep = 1.0 / 20.0;
    private static readonly double[] Speeds = { 0.0, 1.0, 5.0, 25.0, 250.0 };
    private double _accumulator;
    private int _speedIndex = 1;

    public ulong Tick { get; private set; }
    public double PlayedSeconds { get; private set; }
    public double Speed => Speeds[_speedIndex];
    public int SpeedLevel => _speedIndex;
    public bool IsPaused => _speedIndex == 0;
    public long DroppedTicks { get; private set; }

    public int ConsumeTicks(double realDelta, int maxTicksPerFrame = 32)
    {
        _accumulator += realDelta * Speed;
        var available = (int)(_accumulator / FixedStep);
        var count = Math.Min(available, maxTicksPerFrame);
        _accumulator -= count * FixedStep;
        // A capped fixed-step loop must not retain an ever-growing backlog. At 250x,
        // one 60 FPS frame asks for about 83 source ticks while the safety cap is 32;
        // retaining the other 51 made every later frame hit the cap, even after the
        // player returned to normal speed. Java's updater distributes/merges fast-time
        // work instead of replaying an unbounded queue. Keep only the fractional part.
        if (available > maxTicksPerFrame)
        {
            DroppedTicks += available - maxTicksPerFrame;
            _accumulator %= FixedStep;
        }
        Tick += (ulong)count;
        PlayedSeconds += count * FixedStep;
        return count;
    }

    public void TogglePause() => _speedIndex = _speedIndex == 0 ? 1 : 0;
    public void SetSpeedLevel(int level) => _speedIndex = Math.Clamp(level, 0, Speeds.Length - 1);
    public void Restore(ulong tick, double playedSeconds, int speedLevel = 1)
    {
        Tick = tick;
        PlayedSeconds = playedSeconds;
        _speedIndex = Math.Clamp(speedLevel, 0, Speeds.Length - 1);
        _accumulator = 0;
        DroppedTicks = 0;
    }
}
