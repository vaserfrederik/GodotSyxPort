using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Data;
using GodotSyxPort.Rooms;

namespace GodotSyxPort.UI;

/// <summary>
/// Cascading room menu matching view.sett.ui.bottom.BuildMain: a 350x44 source-style
/// category column opens a second 350x44 room column beside it.
/// </summary>
public sealed partial class RoomBuildPalette : ColorRect
{
    private enum PaletteMode { Rooms, Construction, Jobs }
    private const float ColumnWidth = 350f;
    private const float RowHeight = 44f;
    private const float MenuHeight = RowHeight * 8f;
    private RoomBlueprintCatalog _catalog = null!;
    private RoomPlanner _planner = null!;
    private Func<string, bool> _available = null!;
    private ScrollContainer _categoryScroll = null!;
    private ScrollContainer _roomScroll = null!;
    private VBoxContainer _categoryColumn = null!;
    private VBoxContainer _roomColumn = null!;
    private Label _status = null!;
    private string _currentMain = "Работы";
    private string _currentSub = "";
    private PaletteMode _mode = PaletteMode.Rooms;
    private static readonly Color Gold = new("d5c79d");
    private static readonly Color MutedGold = new("9e987d");
    private static readonly Color Panel = new("202120");

    private static readonly (string Main, string Sub, string[] Prefixes, int Icon)[] Categories =
    {
        ("Сельское хозяйство", "Фермы", new[] { "FARM_", "ORCHARD_" }, 11),
        ("Сельское хозяйство", "Животноводство", new[] { "PASTURE_" }, 10),
        ("Сельское хозяйство", "Аквакультура", new[] { "FISHERY_" }, 12),
        ("Сельское хозяйство", "Прочее", new[] { "HUNTER_" }, 0),
        ("Работы", "Рудники", new[] { "MINE_" }, 9),
        ("Работы", "Переработка", new[] { "REFINER_" }, 13),
        ("Работы", "Производство", new[] { "WORKSHOP_" }, 14),
        ("Работы", "Прочее", new[] { "_WOODCUTTER", "_CANNIBAL" }, 0),
        ("Службы", "Религия", new[] { "TEMPLE_", "SHRINE_" }, 22),
        ("Службы", "Распределение", new[] { "CANTEEN_", "EATERY_", "TAVERN_", "MARKET_" }, 23),
        ("Службы", "Здоровье", new[] { "_HEARTH", "_ASYLUM", "_HOSPITAL", "BATH_", "WELL_", "BARBER_", "LAVATORY_", "PHYSICIAN_" }, 24),
        ("Службы", "Развлечения", new[] { "SPEAKER_", "STAGE_", "FIGHTPIT_", "ARENAG_", "PLEASURE_" }, 25),
        ("Службы", "Загробный мир", new[] { "_DUMP_CORPSE", "GRAVEYARD_", "TOMB_" }, 26),
        ("Службы", "Жильё", new[] { "_HOME", "RESTHOME_" }, 27),
        ("Управление", "Администрация", new[] { "_EMBASSY", "_INN", "LABORATORY_", "LIBRARY_", "UNIVERSITY_", "ADMIN_" }, 17),
        ("Управление", "Закон", new[] { "_STOCKADE", "_GUARD", "_PRISON", "_EXECUTION", "_POLICE", "_STOCKS", "_COURT" }, 15),
        ("Управление", "Военное дело", new[] { "_MILITARY_SUPPLY", "BARRACKS_", "ARCHERY_", "GATEHOUSE_", "ARTILLERY_" }, 16),
        ("Управление", "Размножение", new[] { "SCHOOL_", "NURSERY_", "BREEDER_" }, 18),
        ("Управление", "Логистика", new[] { "_STOCKPILE", "_EXPORT", "_IMPORT", "_HAULER", "_TRANSPORT", "_STATION" }, 20),
        ("Управление", "Вода", new[] { "_WATER", "POOL_" }, 21)
    };

    private static readonly Dictionary<string, string> RussianRoomNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["_HOME"] = "Дом",
        ["_HOME_CHAMBER"] = "Покои знати",
        ["HUNTER_NORMAL"] = "Охотничий лагерь",
        ["FISHERY_NORMAL"] = "Рыболовня",
        ["_WOODCUTTER"] = "Лесосека",
        ["_CANNIBAL"] = "Каннибал",
        ["REFINER_BAKERY"] = "Пекарня",
        ["REFINER_BREWERY"] = "Пивоварня",
        ["REFINER_COALER"] = "Углевыжигательная",
        ["REFINER_SMELTER"] = "Плавильня",
        ["REFINER_WEAVER"] = "Ткач"
    };

    public event Action<string>? RoomSelected;
    public event Action<string>? BuildActionSelected;

    public void Initialize(RoomBlueprintCatalog catalog, RoomPlanner planner, Func<string, bool> available)
    {
        _catalog = catalog;
        _planner = planner;
        _available = available;
        Color = new Color(0.055f, 0.058f, 0.055f, 0.98f);
        Size = new Vector2(ColumnWidth + 8f, MenuHeight + 8f);
        ZIndex = 100;
        ClipContents = true;
        MouseFilter = MouseFilterEnum.Stop;

        AddChild(VerticalRule(2, 2, 2, MenuHeight + 4));
        AddChild(VerticalRule(ColumnWidth + 3, 2, 2, MenuHeight + 4));
        AddChild(VerticalRule(ColumnWidth * 2f + 4, 2, 2, MenuHeight + 4));
        (_categoryScroll, _categoryColumn) = CreateColumn(5);
        (_roomScroll, _roomColumn) = CreateColumn(ColumnWidth + 6);
        _roomScroll.Visible = false;
        _status = new Label { Visible = false };
        AddChild(_status);
        OpenCategory("Работы");
        ApplyResponsiveLayout();
        GetViewport().SizeChanged += ApplyResponsiveLayout;
    }

    private void ApplyResponsiveLayout()
    {
        var viewport = GetViewportRect().Size;
        // Keep the main column over the same toolbar position. Source Inter.exp
        // positions only the expansion panel; opening it never moves its parent.
        var mainX = Mathf.Max(8f, (viewport.X - ColumnWidth) * 0.5f);
        Position = new Vector2(mainX,
            Mathf.Max(84f, viewport.Y - MenuHeight - 68f));
    }

    public void OpenCategory(string main)
    {
        _mode = PaletteMode.Rooms;
        _currentMain = main;
        // BuildMain.BMain opens only its first panel. BExp opens the room panel
        // later, when the pointer actually enters a subcategory row.
        _currentSub = "";
        BuildCategoryColumn();
        BuildRoomColumn();
        Visible = true;
        MoveToFront();
    }

    /// <summary>
    /// Exact BuildMain construction order: throne placer, MAIN_INFRA.misc,
    /// fences, roads, structures, DECOR and fortifications.
    /// </summary>
    public void OpenConstruction()
    {
        _mode = PaletteMode.Construction;
        _currentMain = "Строительство";
        _currentSub = "";
        BuildCategoryColumn();
        BuildRoomColumn();
        Visible = true;
        MoveToFront();
    }

    /// <summary>
    /// Exact JobClears order after forage and hunt. Return-water and cave-fill
    /// are deliberately absent here, as they are in BuildMain.java.
    /// </summary>
    public void OpenJobs()
    {
        _mode = PaletteMode.Jobs;
        _currentMain = "Задания";
        _currentSub = "";
        BuildCategoryColumn();
        BuildRoomColumn();
        Visible = true;
        MoveToFront();
    }

    public void SetStatus(string status) => _status.Text = status;

    public void RefreshDraft()
    {
        if (!_planner.UsesDefinition || string.IsNullOrWhiteSpace(_planner.PlacementStatus)) return;
        _status.Text = _planner.PlacementStatus;
    }

    private void BuildCategoryColumn()
    {
        Clear(_categoryColumn);
        if (_mode == PaletteMode.Construction)
        {
            _categoryColumn.AddChild(ActionButton("Переместить трон", "MOVE_THRONE", OriginalUiIcons.Room("_THRONE")));
            foreach (var room in RoomsMatchingPrefixes("_JANITOR", "_THRONE", "_BUILDER"))
                _categoryColumn.AddChild(RoomButton(room));
            _categoryColumn.AddChild(ActionButton("Заборы", "FENCES", OriginalUiIcons.Medium(11)));
            _categoryColumn.AddChild(ActionButton("Дороги", "ROADS", OriginalUiIcons.MainCategory(3)));
            _categoryColumn.AddChild(ActionButton("Конструкции", "STRUCTURES", OriginalUiIcons.Medium(96)));
            var decor = MenuButton("Украшения", OriginalUiIcons.Category(19), true,
                _currentSub == "Украшения");
            decor.MouseEntered += () => SelectSubcategory("Украшения");
            decor.Pressed += () => SelectSubcategory("Украшения");
            _categoryColumn.AddChild(decor);
            _categoryColumn.AddChild(ActionButton("Укрепления", "FORTIFICATION", OriginalUiIcons.Category(16)));
            return;
        }
        if (_mode == PaletteMode.Jobs)
        {
            _categoryColumn.AddChild(ActionButton("Собирать съедобные растения", "JOB_FORAGE", OriginalUiIcons.Category(11)));
            _categoryColumn.AddChild(ActionButton("Охота", "JOB_HUNT", OriginalUiIcons.Room("HUNTER_NORMAL")));
            _categoryColumn.AddChild(ActionButton("Рубить деревья", "JOB_CLEAR_WOOD", OriginalUiIcons.Category(0)));
            _categoryColumn.AddChild(ActionButton("Убирать камни", "JOB_CLEAR_STONE", OriginalUiIcons.Category(28)));
            _categoryColumn.AddChild(ActionButton("Убирать деревья и камни", "JOB_CLEAR_ALL", OriginalUiIcons.Category(28)));
            _categoryColumn.AddChild(ActionButton("Осушать воду", "JOB_CLEAR_WATER", OriginalUiIcons.Category(21)));
            _categoryColumn.AddChild(ActionButton("Прокладывать тоннель", "JOB_CLEAR_MOUNTAIN", OriginalUiIcons.Category(9)));
            return;
        }
        foreach (var room in RoomsFor(_currentMain, "Прочее"))
            _categoryColumn.AddChild(RoomButton(room));
        foreach (var category in Categories.Where(value => value.Main == _currentMain && value.Sub != "Прочее"))
        {
            var button = MenuButton(category.Sub, OriginalUiIcons.Category(category.Icon), true,
                category.Sub == _currentSub);
            button.MouseEntered += () => SelectSubcategory(category.Sub);
            button.Pressed += () => SelectSubcategory(category.Sub);
            _categoryColumn.AddChild(button);
        }
    }

    private void BuildRoomColumn()
    {
        Clear(_roomColumn);
        if (string.IsNullOrWhiteSpace(_currentSub))
        {
            _categoryScroll.Position = new Vector2(5, 4);
            _roomScroll.Visible = false;
            Size = new Vector2(ColumnWidth + 8f, MenuHeight + 8f);
            ApplyResponsiveLayout();
            return;
        }
        var rooms = _mode == PaletteMode.Construction && _currentSub == "Украшения"
            ? RoomsMatchingPrefixes("MONUMENT_", "_BENCH")
            : RoomsFor(_currentMain, _currentSub);
        foreach (var room in rooms)
            _roomColumn.AddChild(RoomButton(room));
        // BuildMain.Inter.exp keeps the parent fixed and begins the child at panel.x2.
        _categoryScroll.Position = new Vector2(5, 4);
        _roomScroll.Position = new Vector2(ColumnWidth + 6, 4);
        _roomScroll.Visible = true;
        _roomScroll.MoveToFront();
        Size = new Vector2(ColumnWidth * 2f + 8f, MenuHeight + 8f);
        ApplyResponsiveLayout();
    }

    private void SelectSubcategory(string sub)
    {
        if (_currentSub == sub) return;
        _currentSub = sub;
        BuildRoomColumn();
    }

    private IEnumerable<RoomBlueprintRuntime> RoomsFor(string main, string sub)
    {
        var category = Categories.FirstOrDefault(value => value.Main == main && value.Sub == sub);
        // BuildMain categories do not all have a direct "misc" section. Services
        // and management consist only of expandable rows; asking for their absent
        // misc group must yield an empty list, not abort opening the whole menu.
        if (category.Prefixes is null) return Enumerable.Empty<RoomBlueprintRuntime>();
        return _catalog.All.Where(room => MatchesCategory(room, main, sub))
            // ROOMS.java registers fixed blueprints and RoomsCreator families in
            // category prefix order.  Alphabetically sorting translated names changed
            // the source menu on every locale.
            .OrderBy(room => Array.FindIndex(category.Prefixes, prefix =>
                room.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ThenBy(room => room.Key, StringComparer.OrdinalIgnoreCase);
    }

    private IEnumerable<RoomBlueprintRuntime> RoomsMatchingPrefixes(params string[] prefixes) =>
        _catalog.All.Where(room => prefixes.Any(prefix =>
                room.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(room => Array.FindIndex(prefixes, prefix =>
                room.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ThenBy(room => room.Key, StringComparer.OrdinalIgnoreCase);

    private static bool MatchesCategory(RoomBlueprintRuntime room, string main, string sub)
    {
        var category = Categories.FirstOrDefault(value => value.Prefixes.Any(prefix =>
            room.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));
        return category.Main == main && category.Sub == sub;
    }

    private Button RoomButton(RoomBlueprintRuntime room)
    {
        var unlocked = _available(room.Key);
        var button = MenuButton(RoomName(room), OriginalUiIcons.Room(room.Key), false, false);
        button.Disabled = !unlocked;
        button.TooltipText = unlocked ? Describe(room) : $"{Describe(room)}\nТребуется технология";
        button.Pressed += () =>
        {
            if (unlocked) RoomSelected?.Invoke(room.Key);
        };
        return button;
    }

    private Button ActionButton(string text, string key, Texture2D? icon)
    {
        var button = MenuButton(text, icon, false, false);
        button.Pressed += () => BuildActionSelected?.Invoke(key);
        return button;
    }

    private static Button MenuButton(string text, Texture2D? icon, bool arrow, bool selected)
    {
        var button = new Button
        {
            Text = arrow ? $"{text}     →" : text,
            Icon = icon,
            Alignment = HorizontalAlignment.Left,
            IconAlignment = HorizontalAlignment.Left,
            CustomMinimumSize = new Vector2(ColumnWidth - 4f, RowHeight),
            FocusMode = FocusModeEnum.None,
            TooltipText = text
        };
        button.AddThemeFontSizeOverride("font_size", 21);
        button.AddThemeColorOverride("font_color", Gold);
        button.AddThemeColorOverride("font_hover_color", new Color("f1dfaa"));
        button.AddThemeColorOverride("font_disabled_color", MutedGold.Darkened(0.45f));
        button.AddThemeConstantOverride("icon_max_width", 32);
        button.AddThemeStyleboxOverride("normal", ButtonStyle(selected ? new Color("343531") : Panel, new Color("575950")));
        button.AddThemeStyleboxOverride("hover", ButtonStyle(new Color("363732"), new Color("a39a73")));
        button.AddThemeStyleboxOverride("pressed", ButtonStyle(new Color("41423b"), new Color("c6b879")));
        button.AddThemeStyleboxOverride("disabled", ButtonStyle(new Color("171817"), new Color("383a35")));
        return button;
    }

    private static StyleBoxFlat ButtonStyle(Color background, Color border) => new()
    {
        BgColor = background,
        BorderColor = border,
        BorderWidthLeft = 2,
        BorderWidthTop = 2,
        BorderWidthRight = 2,
        BorderWidthBottom = 2,
        ContentMarginLeft = 4,
        ContentMarginRight = 4
    };

    private (ScrollContainer Scroll, VBoxContainer Column) CreateColumn(float x)
    {
        var scroll = new ScrollContainer
        {
            Position = new Vector2(x, 4),
            Size = new Vector2(ColumnWidth - 4f, MenuHeight),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto,
            MouseFilter = MouseFilterEnum.Stop
        };
        AddChild(scroll);
        var column = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(ColumnWidth - 20f, MenuHeight)
        };
        column.AddThemeConstantOverride("separation", 0);
        scroll.AddChild(column);
        return (scroll, column);
    }

    private static ColorRect VerticalRule(float x, float y, float width, float height) => new()
    {
        Position = new Vector2(x, y),
        Size = new Vector2(width, height),
        Color = new Color("55564f"),
        MouseFilter = MouseFilterEnum.Ignore
    };

    private static void Clear(Node parent)
    {
        foreach (var child in parent.GetChildren()) child.QueueFree();
    }

    private static string RoomName(RoomBlueprintRuntime room) =>
        RussianRoomNames.GetValueOrDefault(room.Key, room.Rule.Name);

    private static string Describe(RoomBlueprintRuntime room)
    {
        var construction = room.Rule.Construction.Resources.Count == 0 ? "Материалы не нужны" :
            "Стоимость: " + string.Join(", ", room.Rule.Construction.Resources.Select((resource, index) =>
                $"{RussianResourceName(resource)} ×{(index < room.Rule.Construction.AreaCosts.Count ? room.Rule.Construction.AreaCosts[index] : 0):0.##}/клетку"));
        var recipe = room.Rule.Recipes.Count == 0 ? "" : "\n" + string.Join("; ", room.Rule.Recipes.Take(3).Select(value =>
            $"{string.Join(" + ", value.Inputs.Select(input => RussianResourceName(input.Resource)))} → " +
            string.Join(" + ", value.Outputs.Select(output => RussianResourceName(output.Resource)))));
        return $"{RoomName(room)}\n{room.Key}\n{construction}{recipe}";
    }

    private static string RussianResourceName(string resource) => resource.ToUpperInvariant() switch
    {
        "WOOD" => "Дерево",
        "STONE" => "Камень",
        "FURNITURE" => "Мебель",
        "FABRIC" => "Ткань",
        "TOOLS" or "TOOL" => "Инструменты",
        "FISH" => "Рыба",
        "MEAT" => "Мясо",
        "LEATHER" => "Кожа",
        _ => resource
    };
}
