using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Simulation;
using GodotSyxPort.Bootstrap;

namespace GodotSyxPort.Citizens;

/// <summary>Source EventAccident debt scheduler, detached from its message/UI layer.</summary>
public sealed class WorkAccidentRuntime
{
    private readonly RoomSystem _rooms;
    private readonly CitizenSystem _citizens;
    private readonly Dictionary<string, double> _timers = new(StringComparer.OrdinalIgnoreCase);
    private double _scanTime;
    private int _scanIndex;
    public int AccidentCount { get; private set; }
    public int InjuredCount { get; private set; }
    public int DeathCount { get; private set; }

    public WorkAccidentRuntime(RoomSystem rooms, CitizenSystem citizens)
    {
        _rooms = rooms;
        _citizens = citizens;
    }

    public void Tick(double delta, double playedSeconds, ResourceLedger resources, JobBoard jobs)
    {
        _scanTime += Math.Max(0, delta);
        while (_scanTime >= 1)
        {
            _scanTime -= 1;
            var candidates = _rooms.AccidentCandidates().OrderBy(candidate => candidate.Key).ToArray();
            if (candidates.Length == 0) return;
            _scanIndex %= candidates.Length;
            var candidate = candidates[_scanIndex++];
            var employed = candidate.Employed - 150.0;
            if (employed < 0) continue;
            var dayFraction = playedSeconds / OriginalGameData.Current.SecondsPerDay % 1.0;
            var relative = (dayFraction - candidate.ShiftOffset + 1.0) % 1.0;
            if (relative < 0.1 || relative > 0.6) continue;
            var coefficient = candidates.Length /
                (OriginalGameData.Current.SecondsPerDay * 16.0);
            var chanceDebt = GameSession.TitleBonuses.Apply("CIVIC_ACCIDENT",
                coefficient * candidate.AccidentsPerYear * Math.Pow(employed, 1.2));
            _timers[candidate.Key] = _timers.GetValueOrDefault(candidate.Key) -
                                     Math.Clamp(chanceDebt, 0, 1);
            if (_timers[candidate.Key] >= -10) continue;
            var result = _citizens.CreateWorkAccident(
                candidate.RoomId, candidate.Cell, resources, jobs);
            _timers[candidate.Key] += result.Injured + result.Deaths;
            if (result.Injured + result.Deaths <= 0) continue;
            AccidentCount++;
            InjuredCount += result.Injured;
            DeathCount += result.Deaths;
        }
    }
}
