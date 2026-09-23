using GodotSyxPort.LegacyCompat;

int[][] integers = { [-3, 0, 10], [0, 0, 10], [7, 0, 10], [30, 0, 10], [2, 5, -5] };
for (var i = 0; i < integers.Length; i++)
{
    var c = integers[i];
    Emit($"int-{i}", SourceClamp.Integer(c[0], c[1], c[2]));
}
int[][] bytes = { [-1, 0, 10], [-8, -5, 5], [5, -5, 5], [127, -5, 5], [-128, -200, 20] };
for (var i = 0; i < bytes.Length; i++)
{
    var c = bytes[i];
    Emit($"byte-{i}", SourceClamp.Byte((sbyte)c[0], c[1], c[2]));
}
double[][] doubles = { [double.NaN, 1, 10], [double.NegativeInfinity, 1, 10],
    [double.PositiveInfinity, 1, 10], [-3.5, 1, 10], [5.25, 1, 10], [30, 1, 10] };
for (var i = 0; i < doubles.Length; i++)
{
    var c = doubles[i];
    Emit($"double-{i}", BitConverter.DoubleToInt64Bits(SourceClamp.Double(c[0], c[1], c[2])));
}
double[][] cycles = { [2.25, 5], [5, 5], [7.25, 5], [12.25, 5], [-2, 5] };
for (var i = 0; i < cycles.Length; i++)
{
    var c = cycles[i];
    Emit($"cycle-{i}", BitConverter.DoubleToInt64Bits(SourceClamp.Cycle(c[0], c[1])));
}
int[] masks = [0, 0xF0, 0x0F, 0xA0, int.MinValue];
for (var i = 0; i < masks.Length; i++)
{
    var bits = new PackedBits(masks[i]);
    var data = 0x5A5A5A5A;
    var value = bits.Mask & 5;
    var set = bits.Set(data, value);
    Console.WriteLine($"{{\"case\":\"bits-{i}\",\"shift\":{bits.Shift},\"mask\":{bits.Mask},\"set\":{set},\"get\":{bits.Get(set)},\"inc\":{bits.Increment(set, 2)},\"max\":{bits.IsMaximum(set).ToString().ToLowerInvariant()},\"distance\":{PackedBits.Distance(5, 1, bits.Mask)}}}");
}

static void Emit(string name, long value) => Console.WriteLine($"{{\"case\":\"{name}\",\"value\":{value}}}");
