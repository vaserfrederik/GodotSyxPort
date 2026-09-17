using System;
using Godot;

namespace GodotSyxPort.UI;

/// <summary>Clickable Godot controls for SimulationClock.</summary>
public sealed partial class TimeControlBar : PanelContainer
{
    private Label _time = null!;
    private readonly Button[] _speedButtons = new Button[5];
    public event Action<int>? SpeedSelected;

    public void Initialize()
    {
        Size = new Vector2(336, 48);
        MouseFilter = MouseFilterEnum.Stop;
        var panelStyle = new StyleBoxFlat
        {
            BgColor = new Color("111411"), BorderColor = new Color("77705c"),
            BorderWidthLeft = 2, BorderWidthTop = 1,
            BorderWidthRight = 2, BorderWidthBottom = 2
        };
        AddThemeStyleboxOverride("panel", panelStyle);
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 2);
        AddChild(row);
        _time = new Label
        {
            Text = "Д1 00:00", CustomMinimumSize = new Vector2(94, 44),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _time.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        _time.AddThemeFontSizeOverride("font_size", 13);
        row.AddChild(_time);
        var titles = new[] { "Ⅱ", "›", "»", "»»", "»»»" };
        var tips = new[] { "Пауза", "Скорость ×1", "Скорость ×5", "Скорость ×25", "Скорость ×250" };
        for (var i = 0; i < titles.Length; i++)
        {
            var level = i;
            var button = new Button
            {
                Text = titles[i],
                TooltipText = tips[i],
                CustomMinimumSize = new Vector2(46, 44),
                FocusMode = FocusModeEnum.None,
                ToggleMode = true
            };
            button.AddThemeColorOverride("font_color", new Color("e2cf83"));
            button.AddThemeFontSizeOverride("font_size", 18);
            button.Pressed += () => SpeedSelected?.Invoke(level);
            row.AddChild(button);
            _speedButtons[i] = button;
        }
        ApplyResponsiveLayout();
        GetViewport().SizeChanged += ApplyResponsiveLayout;
    }

    private void ApplyResponsiveLayout() =>
        Position = new Vector2(Mathf.Max(0f, (GetViewportRect().Size.X - Size.X) * 0.5f), 1f);

    public void UpdateState(double playedSeconds, double secondsPerDay, int speedLevel)
    {
        var day = Math.Max(1, (int)(playedSeconds / secondsPerDay) + 1);
        var dayPart = secondsPerDay <= 0 ? 0 : playedSeconds % secondsPerDay / secondsPerDay;
        var hour = (int)(dayPart * 24);
        var minute = (int)(dayPart * 24 * 60) % 60;
        _time.Text = $"Д{day} {hour:00}:{minute:00}";
        for (var i = 0; i < _speedButtons.Length; i++)
            _speedButtons[i].ButtonPressed = i == speedLevel;
    }
}
