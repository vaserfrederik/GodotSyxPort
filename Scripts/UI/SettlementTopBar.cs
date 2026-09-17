using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Citizens;
using GodotSyxPort.Hauling;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;
using GodotSyxPort.Stats;
using GodotSyxPort.World;

namespace GodotSyxPort.UI;

/// <summary>
/// Settlement top HUD matching UIPanelTopSett: four large class/workforce indicators,
/// six two-row settlement indicators, central time controls and right-side status/actions.
/// </summary>
public sealed partial class SettlementTopBar : ColorRect
{
    private readonly Dictionary<string, Label> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Button> _buttons = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ColorRect> _meters = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Button> _rightActions = new();
    private CitizenSystem _citizens = null!;
    private RoomSystem _rooms = null!;
    private SettlementStatsRuntime _stats = null!;
    private HaulingSystem _hauling = null!;
    private SettlementWorldRuntime _settlementWorld = null!;
    private WorldRaidingRuntime _raiding = null!;

    public event Action? CitizensRequested;
    public event Action? SlavesRequested;
    public event Action? NoblesRequested;
    public event Action? RoomsRequested;
    public event Action? SubjectsRequested;
    public event Action? HousingRequested;
    public event Action? LawRequested;
    public event Action? ArmyRequested;
    public event Action? HealthRequested;
    public event Action? FoodRequested;
    public event Action? GoodsRequested;
    public event Action? TourismRequested;
    public event Action? RaidersRequested;
    public event Action? LevelRequested;
    public event Action? ProfileRequested;
    public event Action? TechnologyRequested;
    public event Action? EconomyRequested;
    public event Action? NotificationsRequested;
    public event Action? WorldLogRequested;
    public event Action? AdviceRequested;
    public event Action? WikiRequested;
    public event Action? WorldRequested;
    public event Action? MainMenuRequested;
    public event Action? BattleRequested;

    public void Initialize(CitizenSystem citizens, RoomSystem rooms,
        SettlementStatsRuntime stats, HaulingSystem hauling,
        SettlementWorldRuntime settlementWorld, WorldRaidingRuntime raiding)
    {
        _citizens = citizens;
        _rooms = rooms;
        _stats = stats;
        _hauling = hauling;
        _settlementWorld = settlementWorld;
        _raiding = raiding;
        Position = Vector2.Zero;
        Size = new Vector2(GetViewportRect().Size.X, 50);
        Color = new Color(0.045f, 0.05f, 0.047f, 0.98f);
        MouseFilter = MouseFilterEnum.Stop;

        var x = 4f;
        AddLargeStat("citizens", OriginalUiIcons.Small(35), "Граждане", ref x, () => CitizensRequested?.Invoke());
        AddLargeStat("slaves", OriginalUiIcons.Small(36), "Рабы", ref x, () => SlavesRequested?.Invoke());
        AddLargeStat("nobles", OriginalUiIcons.Small(37), "Знать", ref x, () => NoblesRequested?.Invoke());
        AddLargeStat("workforce", OriginalUiIcons.Small(15), "Рабочая сила", ref x, () => RoomsRequested?.Invoke());

        x += 4;
        AddSmallPair("population", OriginalUiIcons.Small(14), "Население", x, 1, () => SubjectsRequested?.Invoke());
        AddSmallPair("housing", OriginalUiIcons.Small(59), "Жильё", x, 25, () => HousingRequested?.Invoke());
        x += 76;
        AddSmallPair("law", OriginalUiIcons.Small(26), "Заключённые", x, 1, () => LawRequested?.Invoke());
        AddSmallPair("army", OriginalUiIcons.Small(20), "Армия", x, 25, () => ArmyRequested?.Invoke());
        x += 76;
        AddSmallPair("health", OriginalUiIcons.Small(34), "Здоровье", x, 1, () => HealthRequested?.Invoke());
        AddSmallPair("food", OriginalUiIcons.Small(19), "Еда", x, 25, () => FoodRequested?.Invoke());

        AddSmallPair("stockpile", OriginalUiIcons.Small(11), "Заполненность складов", 812, 1, () => GoodsRequested?.Invoke());
        AddSmallPair("treasury", OriginalUiIcons.Small(21), "Казна", 812, 25, () => EconomyRequested?.Invoke());
        AddSmallPair("tourists", OriginalUiIcons.Small(6), "Туристы", 888, 1, () => TourismRequested?.Invoke());
        AddSmallPair("knowledge", OriginalUiIcons.Small(17), "Знания", 888, 25, () => TechnologyRequested?.Invoke());
        AddSmallPair("raiders", OriginalUiIcons.Small(57), "Рейдеры", 964, 1, () => RaidersRequested?.Invoke());
        AddSplitPair(964, 25);

        x = 1040;
        AddAction(OriginalUiIcons.Medium(54), "Сообщения и события", ref x, () => NotificationsRequested?.Invoke());
        AddAction(OriginalUiIcons.Medium(99), "Журнал мира", ref x, () => WorldLogRequested?.Invoke());
        AddAction(OriginalUiIcons.Medium(106), "Советы", ref x, () => AdviceRequested?.Invoke());
        AddAction(OriginalUiIcons.MainCategory(12), "Глобальная карта", ref x, () => WorldRequested?.Invoke());
        AddAction(OriginalUiIcons.MainCategory(13), "Военный режим", ref x, () => BattleRequested?.Invoke());
        AddAction(OriginalUiIcons.MainCategory(19), "Энциклопедия", ref x, () => WikiRequested?.Invoke());
        AddAction(OriginalUiIcons.MainCategory(11), "Главное меню", ref x, () => MainMenuRequested?.Invoke());
        ApplyResponsiveLayout();
        GetViewport().SizeChanged += ApplyResponsiveLayout;
    }

    public void UpdateState()
    {
        var classes = _citizens.PopulationByClass();
        var citizens = classes.GetValueOrDefault(SocialClass.Citizen);
        var slaves = classes.GetValueOrDefault(SocialClass.Slave);
        var workforce = citizens + slaves;
        var needed = _rooms.All.Sum(room => room.Employment.Needed);
        var employed = _rooms.All.Sum(room => room.Employment.Employed);
        Set("citizens", citizens, Standing(SocialClass.Citizen),
            $"Граждане: {citizens}");
        Set("slaves", slaves, Standing(SocialClass.Slave), $"Рабы: {slaves}");
        Set("nobles", _stats.Nobles, _stats.Nobles > 0 ? 1 : 0,
            $"Активная знать: {_stats.Nobles}");
        Set("workforce", workforce - needed,
            needed == 0 ? 1 : Math.Clamp(workforce / (double)needed, 0, 1),
            $"Рабочая сила: {workforce}\nТребуется: {needed}\nЗанято: {employed}\nРазнорабочие: {workforce - needed}");

        Set("population", _citizens.Count, 1, $"Всего подданных: {_citizens.Count}");
        var homeless = Math.Max(0, _stats.Population - _stats.Housed);
        Set("housing", homeless, _stats.HousingAccess,
            $"Без жилья: {homeless}\nЖильё: {_stats.Housed}/{_stats.HousingCapacity}");
        Set("law", _stats.Prisoners, AverageLaw(), $"Заключённые: {_stats.Prisoners}");
        Set("army", _stats.Recruits, _stats.Divisions > 0 ? 1 : 0,
            $"Дивизии: {_stats.Divisions}\nРекруты: {_stats.Recruits}\nЗаряжено орудий: {_stats.LoadedArtillery}");
        Set("health", _citizens.SickCount, HealthRatio(),
            $"Больные: {_citizens.SickCount}\nРаненые: {_citizens.InjuredCount}");
        Set("food", (int)Math.Floor(_stats.FoodDays), Math.Clamp(_stats.FoodDays / 8.0, 0, 1),
            $"Запас еды: {_stats.FoodDays:0.0} дней\nГолодают: {_stats.Starving}");

        var capacity = Math.Max(0, _rooms.StorageCapacity);
        var stored = Math.Max(0, _hauling.StoredUnits);
        var storageFill = capacity == 0 ? (stored == 0 ? 0 : 1) : Math.Clamp(stored / (double)capacity, 0, 1);
        Set("stockpile", $"{storageFill:P0}", storageFill,
            $"Заполненность складов: {stored}/{capacity}");
        Set("treasury", Compact(_stats.Treasury), _stats.Treasury > 0 ? 1 : 0,
            $"Казна: {_stats.Treasury:0}");
        Set("tourists", _settlementWorld.Tourism.ActiveVisitors, 1,
            $"Туристы в поселении: {_settlementWorld.Tourism.ActiveVisitors}\n" +
            $"Доход от туризма: {_settlementWorld.Tourism.TotalIncome:0}");
        var knowledge = _rooms.Knowledge.Currencies.Values.Sum();
        Set("knowledge", Compact(knowledge), knowledge > 0 ? 1 : 0, $"Знания: {knowledge:0}");
        var activeRaiders = _raiding.Active;
        Set("raiders", activeRaiders.Count, activeRaiders.Count == 0 ? 1 : 0,
            activeRaiders.Count == 0
                ? $"Активных рейдеров нет\nОборона: {_raiding.PlayerPower()}\nПотенциальный выкуп: {_raiding.CurrentRansom():0}"
                : $"Рейдеров на свободе: {activeRaiders.Count}\nОборона: {_raiding.PlayerPower()}\nПотенциальный выкуп: {_raiding.CurrentRansom():0}");
        _buttons["level"].TooltipText = $"Уровень державы: {_rooms.Governance.Progression.Level + 1}";
        _buttons["profile"].TooltipText = "Профиль державы";
    }

    private double Standing(SocialClass socialClass)
    {
        var groups = _stats.Standing.Groups.Where(group => group.Class == socialClass).ToArray();
        return groups.Length == 0 ? 1 : Math.Clamp(groups.Average(group => group.Loyalty), 0, 1);
    }

    private double AverageLaw() => _stats.Law.Count == 0 ? 1 :
        Math.Clamp(_stats.Law.Values.Average(), 0, 1);

    private double HealthRatio() => _citizens.Count == 0 ? 1 :
        Math.Clamp(1.0 - (_citizens.SickCount + _citizens.InjuredCount) / (double)_citizens.Count, 0, 1);

    private void AddLargeStat(string key, Texture2D? icon, string tip, ref float x, Action? action)
    {
        var button = StatButton(new Vector2(x, 1), new Vector2(58, 47), tip, action);
        var meter = new ColorRect
        {
            Position = new Vector2(2, 30), Size = new Vector2(54, 14),
            Color = new Color("315f32"), MouseFilter = MouseFilterEnum.Ignore
        };
        button.AddChild(meter);
        var image = Icon(icon, new Vector2(20, 2), new Vector2(18, 18));
        button.AddChild(image);
        var value = ValueLabel(new Vector2(2, 25), new Vector2(54, 19), 13);
        button.AddChild(value);
        AddChild(button);
        _buttons[key] = button; _values[key] = value; _meters[key] = meter;
        x += 60;
    }

    private void AddSmallPair(string key, Texture2D? icon, string tip,
        float x, float y, Action? action)
    {
        var button = StatButton(new Vector2(x, y), new Vector2(74, 23), tip, action);
        button.AddChild(Icon(icon, new Vector2(3, 3), new Vector2(16, 16)));
        var value = ValueLabel(new Vector2(21, 2), new Vector2(49, 19), 12);
        button.AddChild(value);
        AddChild(button);
        _buttons[key] = button; _values[key] = value;
    }

    private void AddAction(Texture2D? icon, string tip, ref float x, Action? action)
    {
        var button = StatButton(new Vector2(x, 1), new Vector2(32, 47), tip, action);
        button.Icon = icon;
        AddChild(button);
        _rightActions.Add(button);
        x += 34;
    }

    private void ApplyResponsiveLayout()
    {
        var width = GetViewportRect().Size.X;
        Size = new Vector2(width, 50);
        var actionsX = Mathf.Max(620f, width - 240f);
        var statsX = actionsX - 228f;
        _buttons["stockpile"].Position = new Vector2(statsX, 1);
        _buttons["treasury"].Position = new Vector2(statsX, 25);
        _buttons["tourists"].Position = new Vector2(statsX + 76, 1);
        _buttons["knowledge"].Position = new Vector2(statsX + 76, 25);
        _buttons["raiders"].Position = new Vector2(statsX + 152, 1);
        _buttons["level"].Position = new Vector2(statsX + 152, 25);
        _buttons["profile"].Position = new Vector2(statsX + 190, 25);
        for (var i = 0; i < _rightActions.Count; i++)
            _rightActions[i].Position = new Vector2(actionsX + i * 34, 1);
    }

    private void AddSplitPair(float x, float y)
    {
        var level = StatButton(new Vector2(x, y), new Vector2(36, 23),
            "Уровень державы", () => LevelRequested?.Invoke());
        level.Icon = OriginalUiIcons.Small(3);
        AddChild(level);
        _buttons["level"] = level;
        var profile = StatButton(new Vector2(x + 38, y), new Vector2(36, 23),
            "Профиль державы", () => ProfileRequested?.Invoke());
        profile.Icon = OriginalUiIcons.Small(8);
        AddChild(profile);
        _buttons["profile"] = profile;
    }

    private static Button StatButton(Vector2 position, Vector2 size, string tip, Action? action)
    {
        var button = new Button
        {
            Position = position, Size = size, TooltipText = tip,
            FocusMode = FocusModeEnum.None
        };
        button.AddThemeStyleboxOverride("normal", Style(new Color("171a17"), new Color("55584f")));
        button.AddThemeStyleboxOverride("hover", Style(new Color("30352d"), new Color("b5a56d")));
        button.AddThemeStyleboxOverride("pressed", Style(new Color("404539"), new Color("dfc879")));
        if (action is not null) button.Pressed += action;
        return button;
    }

    private static TextureRect Icon(Texture2D? texture, Vector2 position, Vector2 size) => new()
    {
        Texture = texture, Position = position, Size = size,
        ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        MouseFilter = MouseFilterEnum.Ignore
    };

    private static Label ValueLabel(Vector2 position, Vector2 size, int fontSize)
    {
        var label = new Label
        {
            Position = position, Size = size, Text = "0",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", new Color("e1dec8"));
        return label;
    }

    private void Set(string key, int value, double ratio, string tooltip) =>
        Set(key, value.ToString(), ratio, tooltip);

    private void Set(string key, string value, double ratio, string tooltip)
    {
        _values[key].Text = value;
        _buttons[key].TooltipText = tooltip;
        if (!_meters.TryGetValue(key, out var meter)) return;
        var clamped = (float)Math.Clamp(ratio, 0, 1);
        meter.Size = new Vector2(54 * clamped, 14);
        meter.Color = clamped < 0.35f ? new Color("73302c") :
            clamped < 0.7f ? new Color("806d28") : new Color("315f32");
    }

    private static StyleBoxFlat Style(Color background, Color border) => new()
    {
        BgColor = background, BorderColor = border,
        BorderWidthLeft = 1, BorderWidthTop = 1,
        BorderWidthRight = 1, BorderWidthBottom = 1
    };

    private static string Compact(double value) => Math.Abs(value) switch
    {
        >= 1_000_000 => $"{value / 1_000_000:0.#}M",
        >= 10_000 => $"{value / 1_000:0.#}K",
        _ => $"{value:0}"
    };
}
