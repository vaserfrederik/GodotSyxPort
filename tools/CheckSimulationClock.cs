using GodotSyxPort.Simulation;

var clock = new SimulationClock();
clock.SetSpeedLevel(4);
var executed = clock.ConsumeTicks(1.0 / 60.0);
Require(executed == 32, "fast frame respects the per-frame cap");
var pending = clock.PendingSeconds;
Require(pending > 2.5, "remaining fast-frame time stays queued");

clock.SetSpeedLevel(0);
Require(clock.ConsumeTicks(10) == 0, "pausing does not execute queued work");
Require(clock.PendingSeconds == pending, "pausing does not discard queued work");

clock.SetSpeedLevel(1);
for (var frame = 0; frame < 4; frame++)
    executed += clock.ConsumeTicks(0);
Require(executed == 83, "all complete steps from the original frame are eventually executed");
Require(clock.PendingSeconds >= 0 && clock.PendingSeconds < SimulationClock.FixedStep,
    "only a fractional step remains after catch-up");
Require(clock.Tick == 83 && Math.Abs(clock.PlayedSeconds - 4.15) < 1e-12,
    "tick and played time advance only for executed steps");

clock.Restore(120, 6);
Require(clock.PendingSeconds == 0 && clock.Tick == 120 && clock.PlayedSeconds == 6,
    "restore starts with its saved completed time and no queued frame work");
Require(clock.ConsumeTicks(SimulationClock.FixedStep) == 1,
    "normal speed continues after restore");

Console.WriteLine("SimulationClock: cap, backlog, pause, catch-up and restore passed");

static void Require(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
