using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Bootstrap;
using GodotSyxPort.Citizens;
using GodotSyxPort.Hauling;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;
using GodotSyxPort.World;

namespace GodotSyxPort.UI;

/// <summary>
/// Godot adapter for view.ui.manage.IManager. The original seven manager entries retain
/// separate pages and use the same simulation owners as their Java counterparts.
/// </summary>
public sealed partial class SettlementManagerPanel : ColorRect
{
    private enum Page { Goods, Economy, Tourism, Technology, Raiders, Level, Profile }
    private readonly Dictionary<Page, List<Control>> _pages = new();
    private ResourceLedger _resources = null!;
    private EconomyTracker _economy = null!;
    private RoomSystem _rooms = null!;
    private HaulingSystem _hauling = null!;
    private SettlementWorldRuntime _settlementWorld = null!;
    private WorldRaidingRuntime _raiding = null!;
    private CitizenSystem _citizens = null!;
    private Page _page;
    private ItemList _goods = null!, _tourists = null!, _raiders = null!;
    private Label _goodsDetails = null!, _economyDetails = null!, _touristDetails = null!;
    private Label _raiderDetails = null!, _levelDetails = null!, _profileDetails = null!;
    private ResourceKind[] _goodsOrder = Array.Empty<ResourceKind>();
    private int[] _touristIds = Array.Empty<int>();
    private int[] _raiderIds = Array.Empty<int>();
    private CheckButton _showAllRaiders = null!;
    public event Action? TechnologyRequested;
    public event Action? TradeRequested;

    public void Initialize(ResourceLedger resources, EconomyTracker economy, RoomSystem rooms,
        HaulingSystem hauling, SettlementWorldRuntime settlementWorld,
        WorldRaidingRuntime raiding, CitizenSystem citizens)
    {
        _resources = resources;
        _economy = economy;
        _rooms = rooms;
        _hauling = hauling;
        _settlementWorld = settlementWorld;
        _raiding = raiding;
        _citizens = citizens;
        Position = new Vector2(150, 58);
        Size = new Vector2(980, 590);
        Color = new Color("101410f8");
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;
        foreach (var page in Enum.GetValues<Page>()) _pages[page] = new List<Control>();

        var title = LabelAt("Управление державой", 18, 12, 20);
        title.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        var x = 18f;
        AddTab("Товары", Page.Goods, OriginalUiIcons.Small(11), ref x);
        AddTab("Экономика", Page.Economy, OriginalUiIcons.Small(21), ref x);
        AddTab("Туристы", Page.Tourism, OriginalUiIcons.Small(6), ref x);
        AddTab("Технологии", Page.Technology, OriginalUiIcons.Small(17), ref x);
        AddTab("Рейдеры", Page.Raiders, OriginalUiIcons.Small(57), ref x);
        AddTab("Уровень", Page.Level, OriginalUiIcons.Small(3), ref x);
        AddTab("Профиль", Page.Profile, OriginalUiIcons.Small(8), ref x);
        var close = new Button { Text = "×", Position = new Vector2(932, 10), Size = new Vector2(34, 32) };
        close.Pressed += () => Visible = false;
        AddChild(close);

        BuildGoods();
        BuildEconomy();
        BuildTourism();
        BuildTechnology();
        BuildRaiders();
        BuildLevel();
        BuildProfile();
        ShowPage(Page.Goods);
    }

    public void OpenGoods() => Open(Page.Goods);
    public void OpenEconomy() => Open(Page.Economy);
    public void OpenTourism() => Open(Page.Tourism);
    public void OpenTechnology() => TechnologyRequested?.Invoke();
    public void OpenRaiders() => Open(Page.Raiders);
    public void OpenLevel() => Open(Page.Level);
    public void OpenProfile() => Open(Page.Profile);

    public void Refresh()
    {
        if (!Visible) return;
        switch (_page)
        {
            case Page.Goods: RefreshGoods(); break;
            case Page.Economy: RefreshEconomy(); break;
            case Page.Tourism: RefreshTourism(); break;
            case Page.Raiders: RefreshRaiders(); break;
            case Page.Level: RefreshLevel(); break;
            case Page.Profile: RefreshProfile(); break;
        }
    }

    private void BuildGoods()
    {
        _goods = ItemList(Page.Goods, 18, 96, 430, 452);
        _goodsOrder = Enum.GetValues<ResourceKind>();
        foreach (var kind in _goodsOrder)
            _goods.AddItem(ResourceTitle(kind), OriginalUiIcons.Resource(ResourceSheet(kind)));
        _goods.ItemSelected += index => RefreshGood((int)index);
        _goodsDetails = PageLabel(Page.Goods, 466, 96, 496, 348);
        var trade = PageButton(Page.Goods, "Импорт и экспорт", 466, 462, 210);
        trade.Pressed += () => TradeRequested?.Invoke();
    }

    private void BuildEconomy() =>
        _economyDetails = PageLabel(Page.Economy, 18, 96, 944, 452);

    private void BuildTourism()
    {
        _tourists = ItemList(Page.Tourism, 18, 96, 430, 452);
        _tourists.ItemSelected += index => RefreshTourist((int)index);
        _touristDetails = PageLabel(Page.Tourism, 466, 96, 496, 452);
    }

    private void BuildTechnology()
    {
        var description = PageLabel(Page.Technology, 18, 96, 944, 180);
        description.Text = "Дерево технологий открывается отдельной кнопкой, как полный экран UITechTree оригинала.";
        var open = PageButton(Page.Technology, "Открыть технологии", 18, 292, 240);
        open.Pressed += () => TechnologyRequested?.Invoke();
    }

    private void BuildRaiders()
    {
        _showAllRaiders = new CheckButton
        {
            Text = "Показывать всех", Position = new Vector2(18, 94), Size = new Vector2(220, 34),
            ButtonPressed = true
        };
        AddChild(_showAllRaiders);
        _pages[Page.Raiders].Add(_showAllRaiders);
        _showAllRaiders.Toggled += _ => RefreshRaiders();
        _raiders = ItemList(Page.Raiders, 18, 134, 430, 414);
        _raiders.ItemSelected += index => RefreshRaider((int)index);
        _raiderDetails = PageLabel(Page.Raiders, 466, 96, 496, 452);
    }

    private void BuildLevel() =>
        _levelDetails = PageLabel(Page.Level, 18, 96, 944, 452);

    private void BuildProfile() =>
        _profileDetails = PageLabel(Page.Profile, 18, 96, 944, 452);

    private void Open(Page page)
    {
        ShowPage(page);
        Visible = true;
        Refresh();
    }

    private void ShowPage(Page page)
    {
        _page = page;
        foreach (var pair in _pages)
            foreach (var control in pair.Value) control.Visible = pair.Key == page;
    }

    private void RefreshGoods()
    {
        var selected = Math.Clamp(_goods.GetSelectedItems().FirstOrDefault(), 0, _goodsOrder.Length - 1);
        for (var i = 0; i < _goodsOrder.Length; i++)
            _goods.SetItemText(i, $"{ResourceTitle(_goodsOrder[i])}   {_resources.Get(_goodsOrder[i])}");
        RefreshGood(selected);
    }

    private void RefreshGood(int index)
    {
        if ((uint)index >= (uint)_goodsOrder.Length) return;
        var kind = _goodsOrder[index];
        var produced = _resources.CaptureTotalProduced()[(int)kind];
        var consumed = _resources.CaptureTotalConsumed()[(int)kind];
        var policy = _rooms.Trade.Policy(kind);
        _goodsDetails.Text = $"{ResourceTitle(kind)}\n\n" +
            $"В наличии: {_resources.Get(kind)}\nПроизведено: {produced}\nИзрасходовано: {consumed}\n\n" +
            $"Импорт: {policy.ImportLevel:P0}, максимальная цена {policy.MaximumImportPrice}\n" +
            $"Экспорт: оставлять {policy.RetainedExportFraction:P0}, минимальная цена {policy.MinimumExportPrice}\n\n" +
            $"Склад: {_hauling.StoredUnits}/{_rooms.StorageCapacity}";
    }

    private void RefreshEconomy()
    {
        var treasury = _rooms.Governance.Treasury;
        var latest = treasury.History.GroupBy(value => value.Category)
            .Select(group => (Category: group.Key, Income: group.Sum(value => value.Income),
                Expense: group.Sum(value => value.Expense)))
            .OrderBy(value => value.Category).ToArray();
        _economyDetails.Text = $"Казна: {treasury.Balance:0}\n" +
            $"Годовая прибыль: {treasury.YearlyProfit:0}\nОборот: {treasury.YearlyTurnover:0}\n\n" +
            string.Join("\n", latest.Select(value =>
                $"{value.Category}: +{value.Income:0} / −{value.Expense:0}"));
    }

    private void RefreshTourism()
    {
        var visits = _settlementWorld.Tourism.Visits.OrderBy(value => value.Id).ToArray();
        var ids = visits.Select(value => value.Id).ToArray();
        if (!_touristIds.SequenceEqual(ids))
        {
            _tourists.Clear();
            foreach (var visit in visits)
                _tourists.AddItem($"#{visit.Id} · {visit.Race} · {visit.State}");
            _touristIds = ids;
        }
        var selected = _tourists.GetSelectedItems().FirstOrDefault();
        if (visits.Length > 0) RefreshTourist(Math.Clamp(selected, 0, visits.Length - 1));
        else _touristDetails.Text = "Туристов сейчас нет.";
    }

    private void RefreshTourist(int index)
    {
        var visits = _settlementWorld.Tourism.Visits.OrderBy(value => value.Id).ToArray();
        if ((uint)index >= (uint)visits.Length) return;
        var visit = visits[index];
        _touristDetails.Text = $"Турист #{visit.Id}\n\nРаса: {visit.Race}\nСостояние: {visit.State}\n" +
            $"Регион происхождения: {visit.OriginRegionId}\nПостоялый двор: #{visit.InnRoomId}\n" +
            $"Бюджет: {visit.Budget:0}\nПотрачено: {visit.Spent:0}\n" +
            $"Качество жилья: {visit.InnQuality:P0}\nДостопримечательности: {visit.LandmarkQuality:P0}\n" +
            $"Безопасность: {visit.Safety:P0}\nУслуги: {visit.ServiceQuality:P0}";
    }

    private void RefreshRaiders()
    {
        var visible = _raiding.All.Where(value => _showAllRaiders.ButtonPressed ||
            _raiding.StatsVisible(value)).ToArray();
        var ids = visible.Select(value => value.Id).ToArray();
        if (!_raiderIds.SequenceEqual(ids))
        {
            _raiders.Clear();
            foreach (var raider in visible) _raiders.AddItem("");
            _raiderIds = ids;
        }
        for (var i = 0; i < visible.Length; i++)
        {
            var raider = visible[i];
            var known = _raiding.StatsVisible(raider);
            _raiders.SetItemText(i, $"{raider.Name} · {RaiderStatus(_raiding.Status(raider))} · " +
                $"выкуп {(known ? $"{raider.Worth:0}" : "?")} · сила {(known ? $"{raider.Power}" : "?")}");
        }
        var selected = _raiders.GetSelectedItems().FirstOrDefault();
        if (visible.Length > 0) RefreshRaider(Math.Clamp(selected, 0, visible.Length - 1));
        else _raiderDetails.Text = "Сведения о рейдерах отсутствуют.";
    }

    private void RefreshRaider(int index)
    {
        var visible = _raiding.All.Where(value => _showAllRaiders.ButtonPressed ||
            _raiding.StatsVisible(value)).ToArray();
        if ((uint)index >= (uint)visible.Length) return;
        var raider = visible[index];
        var known = _raiding.StatsVisible(raider);
        _raiderDetails.Text = $"Оборона: {_raiding.PlayerPower()}\n" +
            $"Безопасность от рейдов: 1.0\nПотенциальный выкуп: {_raiding.CurrentRansom():0}\n\n" +
            $"{raider.Name}\nРаса: {raider.Race}\n{RaiderDescription(_raiding.Status(raider))}\n\n" +
            $"Сила: {(known ? $"{raider.Power}" : "?")}\n" +
            $"Выкуп: {(known ? $"{raider.Worth:0}" : "?")}\nНабеги: {raider.Raids}";
    }

    private static string RaiderStatus(RaiderVisibility status) => status switch
    {
        RaiderVisibility.Raiding => "нападает",
        RaiderVisibility.Defeated => "погиб",
        RaiderVisibility.Distant => "далеко",
        RaiderVisibility.Hiding => "скрывается",
        RaiderVisibility.AtLarge => "на свободе",
        _ => status.ToString()
    };

    private static string RaiderDescription(RaiderVisibility status) => status switch
    {
        RaiderVisibility.Raiding => "Этот разбойник сейчас нападает на державу.",
        RaiderVisibility.Defeated => "Этот разбойник был повержен и остался лишь воспоминанием.",
        RaiderVisibility.Distant => "Держава пока слишком мала и бедна, чтобы заинтересовать этого разбойника.",
        RaiderVisibility.Hiding => "Сейчас у разбойника недостаточно сил для нападения.",
        RaiderVisibility.AtLarge => "Разбойник на свободе и обдумывает следующий набег.",
        _ => ""
    };

    private void RefreshLevel()
    {
        var progression = _rooms.Governance.Progression;
        var current = progression.CurrentLevel;
        _levelDetails.Text = $"Уровень державы: {progression.Level + 1}\n" +
            $"Ступень: {current?.Key ?? "—"}\n\nТребования ступени:\n" +
            (current is null ? "—" : string.Join("\n", current.Requirements.Select(pair =>
                $"{pair.Key}: {pair.Value:0.##}"))) +
            $"\n\nОткрытые титулы: {string.Join(", ", progression.Titles.DefaultIfEmpty("—"))}";
    }

    private void RefreshProfile()
    {
        var session = GameSession.Current;
        var profile = session?.Profile;
        var byRace = _citizens.PopulationByRace();
        _profileDetails.Text = $"Раса игрока: {session?.PlayerRace ?? "—"}\n" +
            $"Фракция: {profile?.FactionName ?? "—"}\n" +
            $"Правитель: {profile?.RulerName ?? "—"}\n" +
            $"Прозвания: {(profile is null ? "—" : string.Join(", ", profile.Titles.DefaultIfEmpty("—")))}\n" +
            $"Регион столицы: {session?.SelectedRegionId ?? -1}\n" +
            $"Население: {_citizens.Count}\n\nНаселение по расам:\n" +
            string.Join("\n", byRace.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key}: {pair.Value}"));
    }

    private void AddTab(string text, Page page, Texture2D? icon, ref float x)
    {
        var button = new Button
        {
            Text = text, Icon = icon, Position = new Vector2(x, 50), Size = new Vector2(130, 36),
            FocusMode = FocusModeEnum.None
        };
        button.Pressed += () => { ShowPage(page); Refresh(); };
        AddChild(button);
        x += 132;
    }

    private ItemList ItemList(Page page, float x, float y, float width, float height)
    {
        var list = new ItemList { Position = new Vector2(x, y), Size = new Vector2(width, height) };
        AddChild(list); _pages[page].Add(list); return list;
    }

    private Label PageLabel(Page page, float x, float y, float width, float height)
    {
        var label = LabelAt("", x, y, 14);
        label.Size = new Vector2(width, height);
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _pages[page].Add(label); return label;
    }

    private Button PageButton(Page page, string text, float x, float y, float width)
    {
        var button = new Button { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, 36) };
        AddChild(button); _pages[page].Add(button); return button;
    }

    private Label LabelAt(string text, float x, float y, int size)
    {
        var label = new Label { Text = text, Position = new Vector2(x, y) };
        label.AddThemeFontSizeOverride("font_size", size);
        AddChild(label); return label;
    }

    private static string ResourceTitle(ResourceKind kind) => kind switch
    {
        ResourceKind.Wood => "Дерево", ResourceKind.Stone => "Камень",
        ResourceKind.Grain => "Зерно", ResourceKind.Food => "Еда",
        ResourceKind.Tools => "Инструменты", ResourceKind.Furniture => "Мебель",
        ResourceKind.Beer => "Пиво", ResourceKind.Wine => "Вино",
        ResourceKind.Coal => "Уголь", ResourceKind.Ore => "Руда",
        ResourceKind.Metal => "Металл", ResourceKind.Ration => "Рационы",
        _ => kind.ToString()
    };

    private static string ResourceSheet(ResourceKind kind) => kind switch
    {
        ResourceKind.Wood => "Wood", ResourceKind.Stone => "Stone", ResourceKind.Grain => "Grain",
        ResourceKind.Food => "Bread", ResourceKind.Tools => "Tool", ResourceKind.Furniture => "Furniture",
        ResourceKind.Beer or ResourceKind.Wine => "Alcohol", ResourceKind.CutStone => "StoneCut",
        ResourceKind.ArmourLeather => "Armour_leather", ResourceKind.ArmourPlate => "Armour_plate",
        ResourceKind.WeaponHammer => "Weapon_hammer", ResourceKind.WeaponMount => "Weapon_mount",
        ResourceKind.WeaponShield => "Weapon_shield", ResourceKind.WeaponShort => "Weapon_short",
        ResourceKind.WeaponSlash => "Weapon_slash", ResourceKind.WeaponSpear => "Weapon_spear",
        _ => kind.ToString()
    };
}
