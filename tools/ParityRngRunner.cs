using System;
using System.Globalization;
using GodotSyxPort.Simulation;

if (args.Length != 3) throw new ArgumentException("seed count bound");
var seed = int.Parse(args[0], CultureInfo.InvariantCulture);
var count = int.Parse(args[1], CultureInfo.InvariantCulture);
var bound = int.Parse(args[2], CultureInfo.InvariantCulture);
if (count < 1 || count > 10000 || bound < 1) throw new ArgumentOutOfRangeException(nameof(args));
var rng = new JavaRandomCompat(seed);
for (var tick = 0; tick < count; tick++)
{
    var integer = rng.NextInt();
    var bounded = rng.NextInt(bound);
    var flag = rng.NextBoolean();
    var bits = BitConverter.SingleToInt32Bits(rng.NextFloat());
    var longValue = rng.NextLong();
    Console.WriteLine($"{{\"tick\":{tick},\"int\":{integer},\"bounded\":{bounded},\"boolean\":{flag.ToString().ToLowerInvariant()},\"float_bits\":{bits},\"long\":{longValue}}}");
}
