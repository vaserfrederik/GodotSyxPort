using System;
using Godot;

namespace GodotSyxPort.UI;

/// <summary>
/// Main-menu shell based on menu.ScMain, menu.Background and menu.RESOURCES.
/// The source background is a 237x12 atlas of 32x32 diffuse cells rendered as 64x64
/// world-space tiles, exactly as the Java menu's Save(2) sheet does.
/// </summary>
public sealed partial class OriginalMainMenuScreen : Control
{
    private const string BackgroundPath = "res://Data/Original/assets/sprite/menu/Background.png";
    private const string LogoPath = "res://Data/Original/assets/sprite/menu/Logo.png";
    private const int BackgroundColumns = 237;
    private const int BackgroundRows = 12;
    private const int SourceTileWidth = 32;
    private const int SourceTileHeight = 32;
    private const int RenderTileSize = 64;
    private const float ScrollSpeed = 24f;

    private Texture2D? _background;
    private Texture2D? _logoSheet;
    private float _scroll;
    private float _direction = 1f;
    private VBoxContainer _navigation = null!;
    private Action _newGame = null!;
    private Action _continueGame = null!;
    private Action _options = null!;
    private Action _quit = null!;
    private bool _hasSave;

    public Label StatusLabel { get; private set; } = null!;

    public void Initialize(Action newGame, Action continueGame, Action options, Action quit, bool hasSave)
    {
        _newGame = newGame;
        _continueGame = continueGame;
        _options = options;
        _quit = quit;
        _hasSave = hasSave;
    }

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        _background = GD.Load<Texture2D>(BackgroundPath);
        _logoSheet = GD.Load<Texture2D>(LogoPath);
        BuildLayout();
        ShowPrimaryNavigation();
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        var maximum = Math.Max(0f, BackgroundColumns * RenderTileSize - Size.X);
        _scroll += ScrollSpeed * _direction * (float)delta;
        if (_scroll >= maximum) { _scroll = maximum; _direction = -1f; }
        else if (_scroll <= 0) { _scroll = 0; _direction = 1f; }
        QueueRedraw();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color("08090b"));
        if (_background is null) return;
        var top = (Size.Y - BackgroundRows * RenderTileSize) * 0.5f;
        var firstColumn = Math.Max(0, (int)(_scroll / RenderTileSize));
        var offset = -(_scroll - firstColumn * RenderTileSize);
        var visibleColumns = (int)Math.Ceiling(Size.X / RenderTileSize) + 2;
        for (var row = 0; row < BackgroundRows; row++)
        for (var columnOffset = 0; columnOffset < visibleColumns; columnOffset++)
        {
            var column = firstColumn + columnOffset;
            if (column >= BackgroundColumns) break;
            var destination = new Rect2(offset + columnOffset * RenderTileSize,
                top + row * RenderTileSize, RenderTileSize, RenderTileSize);
            var source = new Rect2(6 + column * SourceTileWidth,
                6 + row * SourceTileHeight, SourceTileWidth, SourceTileHeight);
            DrawTextureRectRegion(_background, destination, source);
        }
        DrawRect(new Rect2(Vector2.Zero, Size), new Color(0.015f, 0.01f, 0.008f, 0.36f));
    }

    private void BuildLayout()
    {
        var content = new Control();
        content.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(content);

        if (_logoSheet is not null)
        {
            var logo = new TextureRect
            {
                Texture = new AtlasTexture
                {
                    Atlas = _logoSheet,
                    Region = new Rect2(6, 6, 352, 224)
                },
                Position = new Vector2(300, 338),
                Size = new Vector2(352, 224),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = MouseFilterEnum.Ignore
            };
            logo.Material = new ShaderMaterial
            {
                Shader = new Shader
                {
                    Code = "shader_type canvas_item;\n" +
                           "void fragment(){ vec4 c = texture(TEXTURE, UV); " +
                           "if(c.r > 0.95 && c.g < 0.08 && c.b > 0.95) c.a = 0.0; COLOR = c; }"
                }
            };
            content.AddChild(logo);
        }

        _navigation = new VBoxContainer
        {
            Position = new Vector2(930, 300),
            Size = new Vector2(360, 420)
        };
        _navigation.AddThemeConstantOverride("separation", 8);
        content.AddChild(_navigation);

        StatusLabel = new Label
        {
            Position = new Vector2(880, 748),
            Size = new Vector2(460, 80),
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore
        };
        StatusLabel.AddThemeColorOverride("font_color", new Color("c9b585"));
        content.AddChild(StatusLabel);
    }

    private void ShowPrimaryNavigation()
    {
        ClearNavigation();
        AddNavigationButton("ИГРАТЬ", ShowPlayNavigation);
        var continueButton = AddNavigationButton("ПРОДОЛЖИТЬ", _continueGame);
        continueButton.Disabled = !_hasSave;
        AddNavigationButton("НАСТРОЙКИ", _options);
        AddNavigationButton("ТИТРЫ", () => StatusLabel.Text = "Экран титров будет перенесён после запуска городской карты.");
        AddNavigationButton("ВЫХОД", _quit);
    }

    private void ShowPlayNavigation()
    {
        ClearNavigation();
        var load = AddNavigationButton("ЗАГРУЗИТЬ", _continueGame);
        load.Disabled = !_hasSave;
        AddNavigationButton("КАМПАНИЯ", () => StatusLabel.Text = "Кампании относятся к этапу полного переноса после запуска города.");
        AddNavigationButton("ПЕСОЧНИЦА", _newGame);
        AddNavigationButton("БЫСТРАЯ БИТВА", () => StatusLabel.Text = "Боевая карта отложена до этапа доделки.");
        AddNavigationButton("РЕДАКТОР", () => StatusLabel.Text = "Редактор мира будет подключён после основной цепочки запуска.");
        AddNavigationButton("< НАЗАД", ShowPrimaryNavigation);
    }

    private Button AddNavigationButton(string text, Action action)
    {
        var button = new Button
        {
            Text = text,
            Flat = true,
            Alignment = HorizontalAlignment.Left,
            CustomMinimumSize = new Vector2(350, 54),
            FocusMode = FocusModeEnum.All
        };
        button.AddThemeFontSizeOverride("font_size", 27);
        button.AddThemeColorOverride("font_color", new Color("e6dfce"));
        button.AddThemeColorOverride("font_hover_color", new Color("bca553"));
        button.AddThemeColorOverride("font_focus_color", new Color("bca553"));
        button.Pressed += action;
        _navigation.AddChild(button);
        return button;
    }

    private void ClearNavigation()
    {
        foreach (var child in _navigation.GetChildren())
        {
            _navigation.RemoveChild(child);
            child.QueueFree();
        }
        StatusLabel.Text = "";
    }
}
