namespace GodotSyxPort.Core;

public readonly record struct GridCoord(int X, int Z)
{
    public static readonly GridCoord[] Cardinal =
    {
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
    };

    public static readonly GridCoord[] AllDirections =
    {
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)
    };

    public static GridCoord operator +(GridCoord a, GridCoord b) =>
        new(a.X + b.X, a.Z + b.Z);
}
