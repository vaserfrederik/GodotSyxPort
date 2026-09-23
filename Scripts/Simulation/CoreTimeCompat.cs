namespace GodotSyxPort.Simulation;

/// <summary>Frame time and one-second pendulum from snake2d.CoreTime.</summary>
public sealed class CoreTimeCompat
{
    private double _secondsSinceFirstUpdate;
    private long _nowMillis;
    private long _nowNanos;
    private float _pendulum;
    private float _direction = 1f;

    public double SecondsSinceFirstUpdate => _secondsSinceFirstUpdate;
    public long NowMillis => _nowMillis;
    public long NowNanos => _nowNanos;
    public float Pendulum0To1To0 => _pendulum;

    public void Update(float deltaSeconds, long nowMillis, long nowNanos)
    {
        _secondsSinceFirstUpdate += deltaSeconds;
        _nowMillis = nowMillis;
        _nowNanos = nowNanos;

        _pendulum += _direction * deltaSeconds;
        if (_pendulum > 1f)
        {
            _pendulum -= (int)_pendulum;
            _pendulum = 1f - _pendulum;
            _direction = -1f;
        }
        else if (_pendulum < 0f)
        {
            _pendulum *= -1f;
            _direction = 1f;
        }
    }
}
