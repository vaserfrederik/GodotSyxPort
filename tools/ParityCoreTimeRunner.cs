using GodotSyxPort.Simulation;

var clock = new CoreTimeCompat();
float[] deltas = [0f, 0.25f, 0.75f, 0.2f, 0.8f, 0.4f, 2.4f, 0.6f, -0.3f, -1.5f, 0.1f];
Emit(clock, 0);
for (var i = 0; i < deltas.Length; i++)
{
    clock.Update(deltas[i], 1010L + i * 31L, 999999999L + i * 1000000L);
    Emit(clock, i + 1);
}

static void Emit(CoreTimeCompat clock, int step) =>
    Console.WriteLine($"{{\"step\":{step},\"secondsBits\":{BitConverter.DoubleToInt64Bits(clock.SecondsSinceFirstUpdate)},\"millis\":{clock.NowMillis},\"nanos\":{clock.NowNanos},\"pendulumBits\":{BitConverter.SingleToInt32Bits(clock.Pendulum0To1To0)}}}");
