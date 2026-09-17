using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Bootstrap;
using GodotSyxPort.Data;

namespace GodotSyxPort.UI;

/// <summary>
/// C# adaptation of StagePickRace, StageVisuals and StagePickTitles.
/// Profile title unlocks are deliberately separated from per-game selection.
/// </summary>
public sealed partial class NewGameProfileScreen : Control
{
    private enum Stage { Race, Faction, Titles }
    private readonly HashSet<string> _selectedTitles = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<TitleEntry> _titles = new();
    private PanelContainer _panel = null!;
    private VBoxContainer _content = null!;
    private ScrollContainer _stageScroll = null!;
    private string _race = "HUMAN";
    private string _factionName = "Новое государство";
    private string _rulerName = "Правитель";
    private int _bannerType;
    private Color _bannerBackground = new("315b84");
    private Color _bannerForeground = new("d5bb55");
    private Color _bannerBorder = new("554521");
    private Color _bannerPole = new("77684d");
    private readonly bool[] _bannerPixels = new bool[BannerPreview.PixelCount];
    private bool _bannerInitialized;

    public event Action<PlayerStartProfile>? Completed;
    public event Action? BackRequested;

    public void Initialize(PlayerStartProfile? profile)
    {
        if (profile is null) return;
        _race = profile.Race;
        _factionName = profile.FactionName;
        _rulerName = profile.RulerName;
        _bannerType = profile.BannerType;
        _bannerBackground = ParseColor(profile.BannerBackground, _bannerBackground);
        _bannerForeground = ParseColor(profile.BannerForeground, _bannerForeground);
        _bannerBorder = ParseColor(profile.BannerBorder, _bannerBorder);
        _bannerPole = ParseColor(profile.BannerPole, _bannerPole);
        DecodeBanner(profile.BannerPixels);
        _selectedTitles.Clear();
        foreach (var title in profile.Titles.Take(5)) _selectedTitles.Add(title);
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(FullBackground());
        _panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(760, 824)
        };
        _panel.SetAnchorsPreset(LayoutPreset.TopLeft);
        _panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.055f, 0.058f, 0.055f, 0.98f),
            BorderColor = new Color("756b4d"), BorderWidthLeft = 2, BorderWidthTop = 2,
            BorderWidthRight = 2, BorderWidthBottom = 2
        });
        AddChild(_panel);
        _stageScroll = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        _panel.AddChild(_stageScroll);
        _content = new VBoxContainer();
        _content.AddThemeConstantOverride("separation", 8);
        _content.CustomMinimumSize = new Vector2(728, 0);
        _stageScroll.AddChild(_content);
        GetViewport().SizeChanged += LayoutPanel;
        LayoutPanel();
        LoadTitles();
        if (!_bannerInitialized) LoadSourceBanner(0);
        ShowRace();
    }

    private void ShowRace()
    {
        ClearContent();
        Heading("Выбор расы");
        AddText("Каждая раса обладает уникальным стилем игры, предпочтениями и антипатиями.");
        var buttons = new GridContainer { Columns = 6, CustomMinimumSize = new Vector2(720, 118) };
        foreach (var race in OriginalGameData.Current.Races.Values
                     .Where(value => value.Playable).OrderBy(value => value.Key))
        {
            var selectedRace = race;
            var button = new Button
            {
                Text = "",
                Icon = OriginalUiIcons.FromAtlas(ToAtlasName(race.Key), 0),
                TooltipText = LocalizedRaceName(race.Key, race.Name),
                CustomMinimumSize = new Vector2(116, 112),
                ExpandIcon = true,
                ToggleMode = true,
                ButtonPressed = race.Key.Equals(_race, StringComparison.OrdinalIgnoreCase)
            };
            button.Pressed += () => { _race = selectedRace.Key; ShowRace(); };
            buttons.AddChild(button);
        }
        _content.AddChild(buttons);
        var presentation = RacePresentation(_race);
        if (presentation is not null)
        {
            AddText(presentation.Description, 174);
            AddText($"Начальная сложность: {presentation.Challenge}", 38);
            var columns = new HBoxContainer { CustomMinimumSize = new Vector2(720, 190) };
            columns.AddChild(new Label
            {
                Text = string.Join("\n", presentation.Pros.Select(value => $"+ {value}")),
                CustomMinimumSize = new Vector2(350, 180), AutowrapMode = TextServer.AutowrapMode.WordSmart
            });
            var cons = new Label
            {
                Text = string.Join("\n", presentation.Cons.Select(value => $"− {value}")),
                CustomMinimumSize = new Vector2(350, 180), AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            cons.AddThemeColorOverride("font_color", new Color("d17868"));
            columns.AddChild(cons);
            _content.AddChild(columns);
        }
        Navigation("Назад", () => BackRequested?.Invoke(), "Подтвердить", ShowFaction);
    }

    private void ShowFaction()
    {
        ClearContent();
        Heading("Настроить фракцию");
        var faction = AddLine("Название фракции", _factionName);
        faction.TextChanged += value => _factionName = value;
        var ruler = AddLine("Имя правителя", _rulerName);
        ruler.TextChanged += value => _rulerName = value;
        var preview = new BannerPreview
        {
            CustomMinimumSize = new Vector2(260, 250),
            Pattern = _bannerType,
            BackgroundColor = _bannerBackground,
            ForegroundColor = _bannerForeground,
            BorderColor = _bannerBorder,
            PoleColor = _bannerPole,
            Pixels = _bannerPixels
        };
        _content.AddChild(preview);
        var pattern = new SpinBox
        {
            Prefix = "Знамя ", MinValue = 0, MaxValue = 7, Step = 1,
            Value = _bannerType, CustomMinimumSize = new Vector2(240, 36)
        };
        pattern.ValueChanged += value => { _bannerType = (int)value; preview.Pattern = _bannerType; preview.QueueRedraw(); };
        _content.AddChild(pattern);
        var editorRow = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        editorRow.AddChild(new BannerBitmapEditor
        {
            Pixels = _bannerPixels,
            Changed = preview.QueueRedraw,
            CustomMinimumSize = new Vector2(300, 300)
        });
        _content.AddChild(editorRow);
        AddColor("Фон", _bannerBackground, value => { _bannerBackground = value; preview.BackgroundColor = value; preview.QueueRedraw(); });
        AddColor("Передний план", _bannerForeground, value => { _bannerForeground = value; preview.ForegroundColor = value; preview.QueueRedraw(); });
        AddColor("Рамка", _bannerBorder, value => { _bannerBorder = value; preview.BorderColor = value; preview.QueueRedraw(); });
        AddColor("Древко", _bannerPole, value =>
        {
            _bannerPole = value; preview.PoleColor = value; preview.QueueRedraw();
        });
        Navigation("Назад", ShowRace, "Подтвердить",
            () => { if (_titles.Any(value => value.Unlocked)) ShowTitles(); else Complete(); });
    }

    private void ShowTitles()
    {
        ClearContent();
        Heading("Выбор прозваний");
        AddText($"Выберите до 5 разблокированных прозваний: {_selectedTitles.Count}/5");
        var scroll = new ScrollContainer
        {
            CustomMinimumSize = new Vector2(640, 420),
            VerticalScrollMode = ScrollContainer.ScrollMode.Auto
        };
        var rows = new VBoxContainer { CustomMinimumSize = new Vector2(610, 0) };
        scroll.AddChild(rows);
        foreach (var title in _titles)
        {
            var entry = title;
            var button = new Button
            {
                Text = $"{title.Name}\n{title.Description}",
                Icon = OriginalUiIcons.PlayerTitle(title.IconIndex),
                TooltipText = title.Unlocked
                    ? $"{title.BoostDescription}\nРазблокировано расами: {string.Join(", ", title.Races)}\n" +
                      $"Сила бонуса: {title.BoostValue:P0}"
                    : "Недоступно: требуется достижение из оригинального профиля.",
                Disabled = !title.Unlocked,
                ToggleMode = true,
                ButtonPressed = _selectedTitles.Contains(title.Key),
                Alignment = HorizontalAlignment.Left,
                CustomMinimumSize = new Vector2(600, 62)
            };
            button.Pressed += () => ToggleTitle(entry);
            rows.AddChild(button);
        }
        _content.AddChild(scroll);
        Navigation("Назад", ShowFaction, "Подтвердить", ConfirmTitles);
    }

    private void ConfirmTitles()
    {
        var available = Math.Min(5, _titles.Count(value => value.Unlocked));
        if (_selectedTitles.Count >= available) { Complete(); return; }
        var dialog = new ConfirmationDialog
        {
            Title = "Выбор прозваний",
            DialogText = "Доступны ещё прозвания. Начать игру, не выбирая все?"
        };
        dialog.Confirmed += Complete;
        dialog.Canceled += dialog.QueueFree;
        dialog.Confirmed += dialog.QueueFree;
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(520, 180));
    }

    private void ToggleTitle(TitleEntry title)
    {
        if (!title.Unlocked) return;
        if (!_selectedTitles.Remove(title.Key) && _selectedTitles.Count < 5)
            _selectedTitles.Add(title.Key);
        ShowTitles();
    }

    private void Complete()
    {
        Completed?.Invoke(new PlayerStartProfile(
            _race, _factionName.Trim(), _rulerName.Trim(), _bannerType,
            _bannerBackground.ToHtml(), _bannerForeground.ToHtml(),
            _bannerBorder.ToHtml(), _bannerPole.ToHtml(), _selectedTitles.ToArray(), EncodeBanner()));
    }

    private void LoadTitles()
    {
        _titles.Clear();
        foreach (var file in DirAccess.GetFilesAt("res://Data/Original/init/player/titles")
                     .Where(value => value.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)).OrderBy(value => value))
        {
            var key = file[..^4];
            var init = SyxDataParser.Parse(FileAccess.GetFileAsString($"res://Data/Original/init/player/titles/{file}"));
            var text = SyxDataParser.Parse(FileAccess.GetFileAsString($"res://Data/Original/text/player/titles/{file}"));
            var unlockedRaces = PlayerProfileProgressRuntime.TitleRaces(key).ToArray();
            var unlocked = init.Get("REQUIRES") is null || unlockedRaces.Length > 0 || _selectedTitles.Contains(key);
            if (unlocked && unlockedRaces.Length == 0) unlockedRaces = new[] { _race };
            var boostValue = unlockedRaces.Length == 0 ? 0 : 0.5 + 0.5 * unlockedRaces.Length /
                Math.Max(1, OriginalGameData.Current.Races.Values.Count(value => value.Playable));
            var iconIndex = init.Get("ICON_I")?.Integer() ?? 0;
            var boost = init.Get("BOOST")?.Fields?.Select(value => $"{value.Key}: {value.Value.Text()}") ??
                        Enumerable.Empty<string>();
            _titles.Add(new TitleEntry(key, text.Get("NAME")?.Text(key) ?? key,
                text.Get("DESC")?.Text("") ?? "", string.Join(" · ", boost), unlocked, iconIndex,
                unlockedRaces, boostValue));
        }
    }

    private void ClearContent()
    {
        foreach (var child in _content.GetChildren()) child.QueueFree();
        _stageScroll.ScrollVertical = 0;
    }

    private void LayoutPanel()
    {
        var viewport = GetViewportRect().Size;
        var size = new Vector2(Math.Min(760, Math.Max(640, viewport.X - 32)),
            Math.Min(824, Math.Max(560, viewport.Y - 32)));
        _panel.CustomMinimumSize = Vector2.Zero;
        _panel.Position = (viewport - size) / 2f;
        _panel.Size = size;
        _stageScroll.Size = size;
        _content.CustomMinimumSize = new Vector2(Math.Max(608, size.X - 32), 0);
    }

    private void Heading(string text)
    {
        var label = new Label { Text = text, HorizontalAlignment = HorizontalAlignment.Center };
        label.AddThemeFontSizeOverride("font_size", 25);
        _content.AddChild(label);
    }

    private void AddText(string text, float height = 54)
    {
        _content.AddChild(new Label
        {
            Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(640, height)
        });
    }

    private LineEdit AddLine(string label, string value)
    {
        var row = new HBoxContainer();
        row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2(190, 36) });
        var edit = new LineEdit { Text = value, CustomMinimumSize = new Vector2(430, 36) };
        row.AddChild(edit); _content.AddChild(row); return edit;
    }

    private void AddColor(string label, Color value, Action<Color> changed)
    {
        var row = new HBoxContainer();
        row.AddChild(new Label { Text = label, CustomMinimumSize = new Vector2(190, 34) });
        var picker = new ColorPickerButton { Color = value, CustomMinimumSize = new Vector2(180, 34) };
        picker.ColorChanged += color => changed(color);
        row.AddChild(picker); _content.AddChild(row);
    }

    private void Navigation(string backText, Action back, string nextText, Action next)
    {
        var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        var previous = new Button { Text = backText, CustomMinimumSize = new Vector2(180, 42) };
        previous.Pressed += back;
        var following = new Button { Text = nextText, CustomMinimumSize = new Vector2(180, 42) };
        following.Pressed += next;
        row.AddChild(previous); row.AddChild(following); _content.AddChild(row);
    }

    private static ColorRect FullBackground()
    {
        var background = new ColorRect { Color = new Color("101419"), MouseFilter = MouseFilterEnum.Ignore };
        background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect); return background;
    }

    private static Color ParseColor(string html, Color fallback)
        => Color.HtmlIsValid(html) ? new Color(html) : fallback;

    private string EncodeBanner() => new(_bannerPixels.Select(value => value ? '1' : '0').ToArray());

    private void DecodeBanner(string encoded)
    {
        if (encoded.Length != _bannerPixels.Length) return;
        for (var i = 0; i < _bannerPixels.Length; i++) _bannerPixels[i] = encoded[i] == '1';
        _bannerInitialized = true;
    }

    private void LoadSourceBanner(int index)
    {
        const string path = "res://Data/Original/assets/sprite/ui/FactionBanners.png";
        var image = GD.Load<Texture2D>(path)?.GetImage();
        if (image is null) return;
        var columns = Math.Max(1, (image.GetWidth() - 2) / 14);
        var count = columns * Math.Max(1, (image.GetHeight() - 2) / 14);
        index = Math.Clamp(index, 0, count - 1);
        var originX = 2 + index % columns * 14;
        var originY = 2 + index / columns * 14;
        for (var y = 0; y < BannerPreview.PixelSide; y++)
        for (var x = 0; x < BannerPreview.PixelSide; x++)
        {
            var color = image.GetPixel(originX + x, originY + y);
            _bannerPixels[y * BannerPreview.PixelSide + x] =
                color.R < 0.01f && color.G < 0.01f && color.B < 0.01f;
        }
        _bannerInitialized = true;
    }

    private static string BestValues(IReadOnlyDictionary<string, double> values) => string.Join(", ",
        values.OrderByDescending(value => value.Value).Take(3).Select(value => $"{value.Key} {value.Value:P0}"));

    private static string ToAtlasName(string key) => key.ToUpperInvariant() switch
    {
        // The source race key/text file is Q_AMEVIA, while the distributed appearance
        // atlas follows the race appearance name used by the sidebar: Amevia.png.
        "AMEVIA" or "Q_AMEVIA" => "Amevia",
        _ => char.ToUpperInvariant(key[0]) + key[1..].ToLowerInvariant()
    };
    private static RacePresentationData? RacePresentation(string key)
    {
        if (GameLanguage.IsRussian && RussianRacePresentation(key) is { } translated)
            return translated;
        var path = $"res://Data/Original/text/race/{(key == "AMEVIA" ? "Q_AMEVIA" : key)}.txt";
        if (!FileAccess.FileExists(path)) return null;
        var source = SyxDataParser.Parse(FileAccess.GetFileAsString(path));
        return new RacePresentationData(
            source.Get("DESC_LONG")?.Text(source.Get("DESC")?.Text(key) ?? key) ?? key,
            source.Get("CHALLENGE")?.Text("—") ?? "—",
            source.Get("PROS")?.Items?.Select(value => value.Text()).ToArray() ?? Array.Empty<string>(),
            source.Get("CONS")?.Items?.Select(value => value.Text()).ToArray() ?? Array.Empty<string>());
    }

    private static string LocalizedRaceName(string key, string fallback) =>
        !GameLanguage.IsRussian ? fallback : key.ToUpperInvariant() switch
        {
            "HUMAN" => "Люди", "CRETONIAN" => "Кретонцы", "DONDORIAN" => "Дондорийцы",
            "GARTHIMI" => "Гартими", "TILAPI" => "Тилапи", "AMEVIA" or "Q_AMEVIA" => "Амевии",
            _ => fallback
        };

    private static RacePresentationData? RussianRacePresentation(string key) => key.ToUpperInvariant() switch
    {
        "HUMAN" => new("Люди — гибкая и разносторонняя раса. Они превосходны в исследованиях и управлении, неплохо занимаются земледелием, но требуют комфортного окружения и подвержены преступности и безумию.",
            "Средняя", new[] { "Сильные исследователи и администраторы", "Универсальная рабочая сила", "Неплохие земледельцы" },
            new[] { "Высокие требования к окружению", "Склонность к преступности и безумию" }),
        "CRETONIAN" => new("Кретонцы — мирные дети Кратора, тесно связанные с землёй. Они любят земледелие, деревянные постройки и гармоничное окружение, хорошо переносят монотонный труд, но слабы в бою.",
            "Лёгкая", new[] { "Превосходные земледельцы", "Хорошие переработчики", "Невысокие требования к разнообразию труда" },
            new[] { "Слабые бойцы", "Предпочитают растительную пищу" }),
        "DONDORIAN" => new("Дондорийцы считаются созданиями самих богов. Они живут столетиями, великолепно работают руками, добывают руду и уверенно сражаются, но уступают другим в воображении и стрельбе.",
            "Сложная", new[] { "Отличные ремесленники и шахтёры", "Сильная тяжёлая пехота", "Долгая жизнь" },
            new[] { "Слабее в стрельбе", "Низкая приспособленность к сельскому хозяйству" }),
        "GARTHIMI" => new("Гартими — быстро размножающаяся раса с прочным панцирем и строгим коллективным порядком. Они хороши в шахтах и камнерезном деле, но их неловкие конечности мешают большинству производств.",
            "Лёгкая", new[] { "Быстро размножаются", "Устойчивы к климату", "Сильные шахтёры и пехотинцы" },
            new[] { "Слабы в тонком производстве", "Для армии требуется длительная подготовка" }),
        "TILAPI" => new("Тилапи были созданы хранителями древнего леса. Они территориальны, превосходно стреляют, разводят животных, охотятся и работают с древесиной, но враждебно относятся к другим расам.",
            "Средняя", new[] { "Отличные лучники с высокой моралью", "Сильные охотники, пастухи и лесорубы", "Хорошо выращивают фруктовые сады" },
            new[] { "Не любят другие расы", "Стремятся к каннибализму" }),
        "AMEVIA" or "Q_AMEVIA" => new("Амевии любят воду и лучше всего живут среди рек и каналов. Их размеры и природная броня делают их сильными солдатами; они превосходно рыбачат и разводят глобдиенов.",
            "Средняя", new[] { "Превосходные рыбаки", "Могут разводить глобдиенов", "Сильные и хорошо защищённые бойцы" },
            new[] { "Нуждаются в воде", "Особенно требовательны к рыбе и яйцам" }),
        _ => null
    };
    private sealed record RacePresentationData(string Description, string Challenge,
        IReadOnlyList<string> Pros, IReadOnlyList<string> Cons);
    private sealed record TitleEntry(string Key, string Name, string Description,
        string BoostDescription, bool Unlocked, int IconIndex,
        IReadOnlyList<string> Races, double BoostValue);
}

public sealed partial class BannerPreview : Control
{
    public const int PixelSide = 12;
    public const int PixelCount = PixelSide * PixelSide;
    public int Pattern { get; set; }
    public Color BackgroundColor { get; set; }
    public Color ForegroundColor { get; set; }
    public Color BorderColor { get; set; }
    public Color PoleColor { get; set; }
    public bool[] Pixels { get; set; } = new bool[PixelCount];

    public override void _Draw()
    {
        var rect = new Rect2(190, 8, 230, 230);
        var cloth = OriginalUiIcons.FromAtlas("_BANNER", Math.Clamp(Pattern, 0, 7));
        if (cloth is not null) DrawTextureRect(cloth, rect, false, BackgroundColor);
        else DrawRect(rect, BackgroundColor);
        DrawRect(new Rect2(rect.Position + new Vector2(-8, 0), new Vector2(7, rect.Size.Y + 24)), PoleColor);
        var size = 12f;
        var origin = rect.Position + new Vector2((rect.Size.X - PixelSide * size) / 2f, 30);
        for (var y = 0; y < PixelSide; y++)
        for (var x = 0; x < PixelSide; x++)
        {
            if (!Pixels[y * PixelSide + x]) continue;
            var pixel = new Rect2(origin + new Vector2(x * size, y * size), new Vector2(size, size));
            DrawRect(pixel.Grow(1), BorderColor);
            DrawRect(pixel, ForegroundColor);
        }
    }
}

/// <summary>Godot adaptation of BitmapSpriteEditor for the original 12x12 faction mark.</summary>
public sealed partial class BannerBitmapEditor : Control
{
    public bool[] Pixels { get; set; } = new bool[BannerPreview.PixelCount];
    public Action? Changed { get; set; }

    public override void _Draw()
    {
        var side = Math.Min(Size.X, Size.Y);
        var cell = side / BannerPreview.PixelSide;
        for (var y = 0; y < BannerPreview.PixelSide; y++)
        for (var x = 0; x < BannerPreview.PixelSide; x++)
        {
            var color = Pixels[y * BannerPreview.PixelSide + x]
                ? new Color("242424") : new Color("747474");
            DrawRect(new Rect2(x * cell, y * cell, cell - 1, cell - 1), color);
        }
        DrawRect(new Rect2(Vector2.Zero, new Vector2(side, side)), new Color("b9aa78"), false, 2);
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouse mouse ||
            !Input.IsMouseButtonPressed(MouseButton.Left) &&
            !Input.IsMouseButtonPressed(MouseButton.Right)) return;
        var side = Math.Min(Size.X, Size.Y);
        var cell = side / BannerPreview.PixelSide;
        var x = (int)(mouse.Position.X / cell);
        var y = (int)(mouse.Position.Y / cell);
        if ((uint)x >= (uint)BannerPreview.PixelSide || (uint)y >= (uint)BannerPreview.PixelSide) return;
        Pixels[y * BannerPreview.PixelSide + x] = Input.IsMouseButtonPressed(MouseButton.Left);
        QueueRedraw(); Changed?.Invoke(); AcceptEvent();
    }
}
