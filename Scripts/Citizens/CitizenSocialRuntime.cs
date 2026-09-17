using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotSyxPort.Citizens;

public readonly record struct CitizenContact(
    int First, int Second, int Meetings, double Relation, double LastMeetingDay);

/// <summary>
/// Non-rendering consolidation of idle interaction plans and their relationship state.
/// Relationships are stored once per unordered pair, while race preference controls the
/// initial affinity. Animation, speech bubbles and sound remain presentation adapters.
/// </summary>
public sealed class CitizenSocialRuntime
{
    public const int SearchDistance = 64;
    public const double InteractionMinimumSeconds = 5;
    public const double InteractionMaximumSeconds = 15;
    public const double DailyRelationDecay = 0.01;
    private readonly Dictionary<(int First, int Second), CitizenContact> _contacts = new();

    public IReadOnlyCollection<CitizenContact> Contacts => _contacts.Values;

    public CitizenContact Meet(int first, int second, double day, double racePreference)
    {
        if (first == second) throw new ArgumentException("A citizen cannot meet itself.");
        var key = Pair(first, second);
        var previous = _contacts.GetValueOrDefault(key,
            new CitizenContact(key.First, key.Second, 0,
                Math.Clamp(racePreference, 0, 1), day));
        var relation = Math.Clamp(previous.Relation +
            (1.0 - previous.Relation) * 0.08, 0, 1);
        var contact = previous with
        {
            Meetings = previous.Meetings + 1,
            Relation = relation,
            LastMeetingDay = day
        };
        _contacts[key] = contact;
        return contact;
    }

    public int BestFriend(int citizenId, IEnumerable<int> available)
    {
        var candidates = available.ToHashSet();
        return _contacts.Values
            .Where(contact => contact.First == citizenId || contact.Second == citizenId)
            .Select(contact => (Contact: contact,
                Other: contact.First == citizenId ? contact.Second : contact.First))
            .Where(value => candidates.Contains(value.Other))
            .OrderByDescending(value => value.Contact.Relation)
            .ThenByDescending(value => value.Contact.Meetings)
            .Select(value => value.Other).FirstOrDefault();
    }

    public double Relation(int first, int second) => first == second ? 1 :
        _contacts.GetValueOrDefault(Pair(first, second)).Relation;

    public void BeginDay(double day, IReadOnlySet<int> alive)
    {
        foreach (var key in _contacts.Keys.ToArray())
        {
            var contact = _contacts[key];
            if (!alive.Contains(contact.First) || !alive.Contains(contact.Second))
            {
                _contacts.Remove(key);
                continue;
            }
            var absentDays = Math.Max(0, day - contact.LastMeetingDay - 1);
            if (absentDays <= 0) continue;
            _contacts[key] = contact with
            {
                Relation = Math.Max(0, contact.Relation - DailyRelationDecay * absentDays),
                LastMeetingDay = day - 1
            };
        }
    }

    public double AverageRelation(int citizenId)
    {
        var contacts = _contacts.Values.Where(contact =>
            contact.First == citizenId || contact.Second == citizenId).ToArray();
        return contacts.Length == 0 ? 0 : contacts.Average(contact => contact.Relation);
    }

    private static (int First, int Second) Pair(int first, int second) =>
        first < second ? (first, second) : (second, first);
}
