using System;
using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;

namespace GodotSyxPort.Citizens;

public enum SocialClass : byte { Noble, Citizen, Slave, Other }
public enum HumanoidType : byte
{
    Subject, Retiree, Recruit, Student, Prisoner, Tourist, Soldier, Enemy, Rioter,
    Deranged, Nobility, Slave, Child, Guard, ChildSlave, Parent, ParentSlave
}
public enum CitizenOrigin : byte { Native, Immigrant, FreedSlave, Parole, Soldier, Asylum }

public static class HumanoidTypeRules
{
    public static SocialClass ClassOf(HumanoidType type) => type switch
    {
        HumanoidType.Nobility => SocialClass.Noble,
        HumanoidType.Slave or HumanoidType.ChildSlave or HumanoidType.ParentSlave => SocialClass.Slave,
        HumanoidType.Prisoner or HumanoidType.Tourist or HumanoidType.Enemy or
            HumanoidType.Rioter or HumanoidType.Deranged => SocialClass.Other,
        _ => SocialClass.Citizen
    };

    public static bool Works(HumanoidType type) => type is HumanoidType.Subject or
        HumanoidType.Slave or HumanoidType.Parent or HumanoidType.ParentSlave or HumanoidType.Recruit;
    public static bool Hostile(HumanoidType type) => type is HumanoidType.Enemy or HumanoidType.Rioter;
    public static bool PlayerClass(SocialClass socialClass) => socialClass != SocialClass.Other;
}

public sealed record CitizenIdentity(
    int CitizenId, string Race, SocialClass Class, HumanoidType Type,
    string FirstName, string Surname, byte Gender, CitizenOrigin Origin,
    int ParentId, int BirthDay)
{
    public string FullName => string.IsNullOrWhiteSpace(Surname) ? FirstName : $"{FirstName} {Surname}";
}

/// <summary>Stable identity registry replacing race/type bytes in the source Individual.</summary>
public sealed class CitizenIdentityRuntime
{
    private readonly IReadOnlyDictionary<string, RaceRule> _races;
    private readonly Dictionary<int, CitizenIdentity> _identities = new();
    public IReadOnlyCollection<CitizenIdentity> All => _identities.Values;

    public CitizenIdentityRuntime(IReadOnlyDictionary<string, RaceRule> races) => _races = races;

    public CitizenIdentity Create(
        int citizenId, string raceKey, SocialClass socialClass = SocialClass.Citizen,
        HumanoidType type = HumanoidType.Subject, CitizenOrigin origin = CitizenOrigin.Native,
        int parentId = 0, int birthDay = 0)
    {
        if (_identities.TryGetValue(citizenId, out var existing)) return existing;
        if (!_races.TryGetValue(raceKey, out var race))
            throw new ArgumentException($"Unknown source race '{raceKey}'", nameof(raceKey));
        var gender = (byte)(Hash(citizenId, 11) & 1);
        var first = Pick(race.FirstNames, citizenId, 23, race.Name);
        var surname = Pick(race.Surnames, citizenId, 37, "");
        var identity = new CitizenIdentity(
            citizenId, race.Key, socialClass, type, first, surname, gender, origin, parentId, birthDay);
        _identities.Add(citizenId, identity);
        return identity;
    }

    public CitizenIdentity? Get(int citizenId) => _identities.GetValueOrDefault(citizenId);
    public bool Remove(int citizenId) => _identities.Remove(citizenId);
    public void Clear() => _identities.Clear();
    public CitizenIdentity Restore(CitizenIdentity identity)
    {
        if (!_races.ContainsKey(identity.Race))
            throw new ArgumentException($"Unknown source race '{identity.Race}'", nameof(identity));
        _identities[identity.CitizenId] = identity;
        return identity;
    }

    public void ChangeType(int citizenId, SocialClass socialClass, HumanoidType type)
    {
        if (_identities.TryGetValue(citizenId, out var identity))
            _identities[citizenId] = identity with { Class = socialClass, Type = type };
    }

    public void ChangeType(int citizenId, HumanoidType type) =>
        ChangeType(citizenId, HumanoidTypeRules.ClassOf(type), type);

    public IReadOnlyDictionary<string, int> PopulationByRace() => _identities.Values
        .GroupBy(identity => identity.Race, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.Count(), StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<SocialClass, int> PopulationByClass() => _identities.Values
        .GroupBy(identity => identity.Class).ToDictionary(group => group.Key, group => group.Count());

    public string Pronoun(int citizenId, string form)
    {
        var identity = Get(citizenId);
        if (identity is null || !_races.TryGetValue(identity.Race, out var race)) return "";
        var values = form.ToUpperInvariant() switch
        {
            "HE" => race.PronounHe, "HIM" => race.PronounHim, "HIS" => race.PronounHis,
            "HIMSELF" => race.PronounHimself, "CHILD" => race.PronounChild,
            _ => Array.Empty<string>()
        };
        return values.Count == 0 ? "" : values[Math.Min(identity.Gender, values.Count - 1)];
    }

    private static string Pick(IReadOnlyList<string> values, int id, int salt, string fallback) =>
        values.Count == 0 ? fallback : values[(int)(Hash(id, salt) % (uint)values.Count)];
    private static uint Hash(int value, int salt)
    {
        unchecked
        {
            uint hash = (uint)(value * 0x1f123bb5 ^ salt * 0x5f356495);
            hash ^= hash >> 16; hash *= 0x7feb352d; hash ^= hash >> 15;
            return hash;
        }
    }
}
