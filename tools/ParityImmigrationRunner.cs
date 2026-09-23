using System.Globalization;
using GodotSyxPort.Data;
using GodotSyxPort.Settlement;

foreach (var line in File.ReadAllLines(args[0]).Skip(1))
{
    var v = line.Split(',');
    var race = new RaceRule("fixture", double.Parse(v[3], CultureInfo.InvariantCulture));
    var wanted = ImmigrationRuntime.WantedUltimately(
        race, double.Parse(v[1], CultureInfo.InvariantCulture),
        double.Parse(v[2], CultureInfo.InvariantCulture), int.Parse(v[4]),
        bool.Parse(v[5]), int.Parse(v[6]), double.Parse(v[7], CultureInfo.InvariantCulture));
    Console.WriteLine($"{{\"case\":\"{v[0]}\",\"wanted\":{wanted}}}");
}

namespace GodotSyxPort.Data
{
    public sealed record RaceRule(string Key, double PopulationMaximum);
}

namespace GodotSyxPort.Citizens { }
