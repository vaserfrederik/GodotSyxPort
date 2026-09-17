using System.Collections.Generic;
using System.Globalization;

namespace GodotSyxPort.Data;

public sealed class SyxDataNode
{
    public Dictionary<string, SyxDataNode>? Fields { get; init; }
    public List<SyxDataNode>? Items { get; init; }
    public string? Scalar { get; init; }

    public SyxDataNode? Get(string key) =>
        Fields is not null && Fields.TryGetValue(key, out var value) ? value : null;

    public string Text(string fallback = "") => Scalar ?? fallback;

    public double Number(double fallback = 0) =>
        double.TryParse(Scalar, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;

    public int Integer(int fallback = 0) =>
        int.TryParse(Scalar, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : fallback;

    public bool Boolean(bool fallback = false) => Scalar?.ToLowerInvariant() switch
    {
        "true" => true,
        "false" => false,
        _ => fallback
    };
}
