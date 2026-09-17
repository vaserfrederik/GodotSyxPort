using System;
using System.Threading.Tasks;
using Godot;
using GodotSyxPort.Data;
using GodotSyxPort.Save;
using GodotSyxPort.UI;

namespace GodotSyxPort.Bootstrap;

/// <summary>Top-level C# scene flow: menu, new-game setup, world and settlement.</summary>
public sealed partial class AppBootstrap : Node
{
    private CanvasLayer _screens = null!;
    private Control? _screen;
    private GameBootstrap? _settlement;
    private Label? _menuStatus;
    private PlayerStartProfile? _newGameProfile;

    public override void _Ready()
    {
        GameLanguage.LoadAndApply();
        GD.Print($"[LAUNCH] locale={TranslationServer.GetLocale()}");
        GD.Print("[LAUNCH] app_ready");
        OriginalGameData.Load();
        _screens = new CanvasLayer { Layer = 100 };
        AddChild(_screens);
        ShowMainMenu();
    }

    private void ShowMainMenu()
    {
        if (_settlement is not null)
        {
            _settlement.QueueFree();
            _settlement = null;
        }
        GameSession.Clear();
        _newGameProfile = null;
        var menu = new OriginalMainMenuScreen();
        menu.Initialize(ShowNewGame, ContinueGame, ShowOptions, () => GetTree().Quit(),
            SaveGameService.HasSave());
        SetScreen(menu);
        _menuStatus = menu.StatusLabel;
    }

    private void ShowNewGame()
        => ShowNewGame(_newGameProfile);

    private void ShowNewGame(PlayerStartProfile? initial)
    {
        var profile = new NewGameProfileScreen();
        profile.Initialize(initial);
        SetScreen(profile);
        profile.Completed += completed =>
        {
            _newGameProfile = completed;
            ShowWorldSetup(completed);
        };
        profile.BackRequested += ShowMainMenu;
    }

    private void ShowWorldSetup(PlayerStartProfile playerProfile)
    {
        var screen = new WorldSetupScreen();
        SetScreen(screen);
        var seed = unchecked((int)Time.GetUnixTimeFromSystem());
        screen.InitializeNew(seed, playerProfile);
        screen.StartRequested += async (world, worldSeed, regionId, capitalX, capitalY, race) =>
        {
            try
            {
                GameSession.StartNew(world, worldSeed, regionId, race, playerProfile, capitalX, capitalY);
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                await StartSettlementAsync(false);
            }
            catch (Exception exception)
            {
                screen.ShowLaunchError(exception);
                GD.PushError(exception.ToString());
            }
        };
        screen.BackRequested += () => ShowNewGame(playerProfile);
    }

    private async void ContinueGame()
    {
        var save = SaveGameService.Load();
        if (save is null)
        {
            if (_menuStatus is not null) _menuStatus.Text = "Сохранение не найдено или повреждено.";
            return;
        }
        if (!GameSession.RestoreFromSave(save)) GameSession.EnsureDefault();
        try
        {
            await StartSettlementAsync(true);
        }
        catch (Exception exception)
        {
            if (_menuStatus is not null) _menuStatus.Text = exception.Message;
            GD.PushError(exception.ToString());
        }
    }

    private async Task StartSettlementAsync(bool loadExisting)
    {
        GD.Print($"[LAUNCH] settlement_create load_existing={loadExisting}");
        var settlement = new GameBootstrap
        {
            Name = "Settlement",
            LoadExisting = loadExisting
        };
        _settlement = settlement;
        _settlement.WorldRequested += ShowWorldOverview;
        _settlement.MainMenuRequested += () =>
        {
            _settlement?.SaveNow();
            ShowMainMenu();
        };
        try
        {
            AddChild(_settlement);
            var deadline = Time.GetTicksMsec() + 120_000;
            while (GodotObject.IsInstanceValid(settlement) && !settlement.Initialized &&
                   settlement.InitializationError is null && Time.GetTicksMsec() < deadline)
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            if (settlement.InitializationError is { } error)
                throw new InvalidOperationException("Ошибка инициализации поселения:\n" + error);
            if (!settlement.Initialized)
                throw new TimeoutException("Инициализация поселения не завершилась за 120 секунд.");
        }
        catch
        {
            if (GodotObject.IsInstanceValid(settlement)) settlement.QueueFree();
            _settlement = null;
            throw;
        }
        ClearScreen();
    }

    private void ShowWorldOverview()
    {
        if (_settlement is null || GameSession.Current is null || GameSession.World is null) return;
        _settlement.SaveNow();
        _settlement.Visible = false;
        _settlement.ProcessMode = ProcessModeEnum.Disabled;
        var screen = new WorldSetupScreen();
        SetScreen(screen);
        var configuration = GameSession.Current;
        screen.InitializeOverview(
            GameSession.World,
            configuration.WorldSeed,
            configuration.SelectedRegionId,
            configuration.PlayerRace);
        screen.BackRequested += ResumeSettlement;
        screen.MainMenuRequested += () =>
        {
            _settlement?.SaveNow();
            ShowMainMenu();
        };
    }

    private void ResumeSettlement()
    {
        ClearScreen();
        if (_settlement is null) return;
        _settlement.Visible = true;
        _settlement.ProcessMode = ProcessModeEnum.Inherit;
    }

    private void ShowOptions()
    {
        var root = ScreenRoot();
        var panel = new PanelContainer { CustomMinimumSize = new Vector2(440, 380) };
        Center(panel, new Vector2(440, 380));
        root.AddChild(panel);
        var column = new VBoxContainer();
        column.AddThemeConstantOverride("separation", 14);
        panel.AddChild(column);
        var title = new Label
        {
            Text = "Настройки",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(410, 60)
        };
        title.AddThemeFontSizeOverride("font_size", 24);
        column.AddChild(title);
        var fullscreen = new CheckButton
        {
            Text = "Полноэкранный режим",
            ButtonPressed = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen
        };
        fullscreen.Toggled += enabled => DisplayServer.WindowSetMode(enabled
            ? DisplayServer.WindowMode.Fullscreen
            : DisplayServer.WindowMode.Windowed);
        column.AddChild(fullscreen);
        var resolution = new OptionButton { TooltipText = "Разрешение окна" };
        var modes = new[] { new Vector2I(1280, 720), new Vector2I(1600, 900), new Vector2I(1920, 1080), new Vector2I(2560, 1440) };
        var current = DisplayServer.WindowGetSize();
        for (var i = 0; i < modes.Length; i++)
        {
            resolution.AddItem($"{modes[i].X} × {modes[i].Y}", i);
            if (modes[i] == current || (modes[i] == new Vector2I(1600, 900) && current.X <= 0)) resolution.Select(i);
        }
        resolution.ItemSelected += index =>
        {
            var size = modes[index];
            DisplayServer.WindowSetSize(size);
            DisplayServer.WindowSetPosition((DisplayServer.ScreenGetSize() - size) / 2);
        };
        column.AddChild(new Label { Text = "Разрешение" });
        column.AddChild(resolution);
        var language = new OptionButton { TooltipText = "Язык интерфейса" };
        language.AddItem("Русский");
        language.SetItemMetadata(0, GameLanguage.Russian);
        language.AddItem("English");
        language.SetItemMetadata(1, GameLanguage.English);
        language.Select(GameLanguage.IsRussian ? 0 : 1);
        language.ItemSelected += index =>
        {
            GameLanguage.Apply(language.GetItemMetadata((int)index).AsString());
            _menuStatus = null;
        };
        column.AddChild(new Label { Text = "Язык / Language" });
        column.AddChild(language);
        var help = new Label
        {
            Text = "Камера: WASD/стрелки · масштаб: колесо\nПауза: Space · мир: кнопка «В мир»",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(410, 100)
        };
        column.AddChild(help);
        AddMenuButton(column, "Назад", ShowMainMenu);
    }

    private Control ScreenRoot()
    {
        var root = new Control();
        SetScreen(root);
        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        var background = new ColorRect
        {
            Color = new Color("101419"),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(background);
        return root;
    }

    private void SetScreen(Control screen)
    {
        ClearScreen();
        _screen = screen;
        _screens.AddChild(screen);
        screen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
    }

    private void ClearScreen()
    {
        if (_screen is null) return;
        _screen.QueueFree();
        _screen = null;
    }

    private static Button AddMenuButton(Control parent, string text, Action pressed)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(410, 52) };
        button.Pressed += pressed;
        parent.AddChild(button);
        return button;
    }

    private static void Center(Control control, Vector2 size)
    {
        control.SetAnchorsPreset(Control.LayoutPreset.Center);
        control.Position = -size / 2f;
        control.Size = size;
    }
}
