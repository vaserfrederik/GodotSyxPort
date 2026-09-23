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
    public double PendingSeconds => _accumulator;

    public int ConsumeTicks(double realDelta, int maxTicksPerFrame = 32)
    {
        if (!double.IsFinite(realDelta) || realDelta < 0)
            throw new ArgumentOutOfRangeException(nameof(realDelta));
        if (maxTicksPerFrame < 1)
            throw new ArgumentOutOfRangeException(nameof(maxTicksPerFrame));
        // A pause stops execution immediately, including work queued by a fast frame.
        // The queue is kept for when the player resumes.
        if (IsPaused) return 0;

        _accumulator += realDelta * Speed;
        var count = (int)Math.Min(_accumulator / FixedStep, maxTicksPerFrame);
        _accumulator -= count * FixedStep;
        // Keep every unprocessed step. The cap limits work in this frame, but
        // cannot make the simulation run at the requested speed when it stays
        // slower than real time for many consecutive frames.
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
    }
}
