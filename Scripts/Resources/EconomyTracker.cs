using System;
using System.Collections.Generic;

namespace GodotSyxPort.Resources;

public sealed record EconomySample(double Time, int[] Produced, int[] Consumed);

/// <summary>Keeps bounded one-minute resource flow history independent of frame rate.</summary>
public sealed class EconomyTracker
{
    public const int MaxSamples = 120;
    public const double SampleSeconds = 60.0;
    private readonly List<EconomySample> _history = new();
    private double _sampleAccumulator;
    private double _elapsed;

    public IReadOnlyList<EconomySample> History => _history;
    public EconomySample? Latest => _history.Count == 0 ? null : _history[^1];

    public void Tick(double delta, ResourceLedger ledger)
    {
        _elapsed += delta;
        _sampleAccumulator += delta;
        if (_sampleAccumulator < SampleSeconds) return;
        _sampleAccumulator %= SampleSeconds;
        var flow = ledger.TakePeriodFlow();
        _history.Add(new EconomySample(_elapsed, flow.Produced, flow.Consumed));
        if (_history.Count > MaxSamples) _history.RemoveAt(0);
    }

    public EconomySample[] Capture() => _history.ToArray();

    public void Restore(IEnumerable<EconomySample> samples)
    {
        _history.Clear();
        foreach (var sample in samples)
        {
            var produced = new int[ResourceLedger.KindCount];
            var consumed = new int[ResourceLedger.KindCount];
            Array.Copy(sample.Produced, produced, Math.Min(sample.Produced.Length, produced.Length));
            Array.Copy(sample.Consumed, consumed, Math.Min(sample.Consumed.Length, consumed.Length));
            _history.Add(new EconomySample(sample.Time, produced, consumed));
            if (_history.Count > MaxSamples) _history.RemoveAt(0);
            _elapsed = Math.Max(_elapsed, sample.Time);
        }
        _sampleAccumulator = 0;
    }
}
