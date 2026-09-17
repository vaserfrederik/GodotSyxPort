using System;
using System.Linq;
using Godot;
using GodotSyxPort.Military;
using GodotSyxPort.Settlement;
using GodotSyxPort.World;

namespace GodotSyxPort.UI;

/// <summary>Decision surface corresponding to VIEW.world().UI.battle.</summary>
public sealed partial class WorldBattlePanel : ColorRect
{
    private SettlementWorldRuntime _world = null!;
    private SettlementInvasionRuntime _invasion = null!;
    private Label _title = null!, _summary = null!, _status = null!;
    private Button _auto = null!, _retreat = null!, _defend = null!, _capture = null!, _release = null!;
    private long _shownBattleId = -1;
    public event Action? SettlementDefenseRequested;

    public void Initialize(SettlementWorldRuntime world, SettlementInvasionRuntime invasion)
    {
        _world = world;
        _invasion = invasion;
        Color = new Color("111311f8");
        MouseFilter = MouseFilterEnum.Stop;
        AnchorLeft = 0.5f; AnchorTop = 0.5f; AnchorRight = 0.5f; AnchorBottom = 0.5f;
        OffsetLeft = -330; OffsetTop = -220; OffsetRight = 330; OffsetBottom = 220;
        Visible = false;
        _title = LabelAt("Сражение", 20, 14, 560, 36, 24);
        _title.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        var close = ButtonAt("×", 606, 12, 34, 32);
        close.Pressed += () => Visible = false;
        _summary = LabelAt("", 24, 62, 612, 250, 15);
        _summary.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _status = LabelAt("", 24, 320, 612, 28, 13);
        _status.AddThemeColorOverride("font_color", new Color("efc971"));
        _retreat = ButtonAt("Отступить", 24, 370, 180, 42);
        _retreat.Icon = OriginalUiIcons.Small(28);
        _retreat.Pressed += () => Decide(WorldBattleDecision.Retreat);
        _auto = ButtonAt("Автоматический бой", 214, 370, 220, 42);
        _auto.Icon = OriginalUiIcons.MainCategory(13);
        _auto.Pressed += () => Decide(WorldBattleDecision.AutoResolve);
        _defend = ButtonAt("Оборонять город", 444, 370, 192, 42);
        _defend.Icon = OriginalUiIcons.MainCategory(13);
        _defend.Pressed += () => Decide(WorldBattleDecision.DefendSettlement);
        _capture = ButtonAt("Принять пленных", 214, 370, 220, 42);
        _capture.Pressed += () => ResolvePrisoners(true);
        _release = ButtonAt("Не принимать", 444, 370, 192, 42);
        _release.Pressed += () => ResolvePrisoners(false);
    }

    public void Open()
    {
        Visible = true;
        Refresh(true);
    }

    public void Refresh(bool force = false)
    {
        if (_invasion.AwaitingPrisonerDecision)
        {
            Visible = true;
            _title.Text = "Победа";
            _summary.Text = $"Враг разбит. Бегущих, доступных для пленения: {_invasion.RoutedEnemies:N0}.\n\n" +
                $"Свободных мест заключения: {_invasion.PrisonCapacity:N0}.";
            _status.Text = "Решите, принимать ли выживших врагов в места заключения.";
            _auto.Visible = _retreat.Visible = _defend.Visible = false;
            _capture.Visible = _release.Visible = true;
            return;
        }
        _capture.Visible = _release.Visible = false;
        var battle = _world.Battles.Pending;
        if (battle is null)
        {
            if (!Visible) return;
            _title.Text = "Военные события";
            _summary.Text = _world.Battles.History.Count == 0
                ? "Сражений пока не происходило."
                : string.Join("\n", _world.Battles.History.TakeLast(8).Reverse().Select(HistoryLine));
            _status.Text = _world.Battles.ActiveSettlementInvasion is null
                ? "" : "Идёт вторжение на карту столицы.";
            _auto.Visible = _retreat.Visible = _defend.Visible = false;
            return;
        }
        if (!force && battle.Id == _shownBattleId) return;
        _shownBattleId = battle.Id;
        Visible = true;
        _title.Text = battle.SettlementInvasion ? "Вторжение в столицу" : BattleTitle(battle.Kind);
        _summary.Text = SideText("Атакующие", battle.Attacker) + "\n\n" +
                        SideText("Защитники", battle.Defender) + "\n\n" +
                        (battle.SettlementInvasion
                            ? "Враг прорвал региональную оборону. Бой переносится на карту города."
                            : "Выберите автоматическое разрешение или организованное отступление.");
        _status.Text = "Мировое время ожидает решения игрока.";
        _retreat.Visible = true;
        _auto.Visible = !battle.SettlementInvasion;
        _defend.Visible = battle.SettlementInvasion;
    }

    private void ResolvePrisoners(bool accept)
    {
        _invasion.ResolvePrisoners(accept);
        Refresh(true);
    }

    private void Decide(WorldBattleDecision decision)
    {
        if (!_world.Battles.ResolvePending(decision))
        {
            _status.Text = "Это решение сейчас недоступно.";
            return;
        }
        if (decision == WorldBattleDecision.DefendSettlement)
            SettlementDefenseRequested?.Invoke();
        _shownBattleId = -1;
        Refresh(true);
    }

    private static string BattleTitle(WorldBattleKind kind) => kind switch
    {
        WorldBattleKind.Siege => "Осада региона",
        WorldBattleKind.Garrison => "Атака гарнизона",
        _ => "Полевое сражение"
    };

    private static string SideText(string title, WorldBattleSideSnapshot side) =>
        $"{title}\nФракция: {side.FactionId}\nБойцы: {side.Men:N0}\n" +
        $"Сила: {side.Power:N0}\nАрмии: {side.ArmyIds.Count}";

    private static string HistoryLine(WorldBattleSnapshot battle) =>
        $"#{battle.Id} · {BattleTitle(battle.Kind)} · победитель {battle.WinnerFactionId} · " +
        $"потери {battle.AttackerLosses}/{battle.DefenderLosses}";

    private Label LabelAt(string text, float x, float y, float width = 560,
        float height = 30, int fontSize = 14)
    {
        var label = new Label { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, height) };
        label.AddThemeFontSizeOverride("font_size", fontSize);
        AddChild(label);
        return label;
    }

    private Button ButtonAt(string text, float x, float y, float width, float height)
    {
        var button = new Button { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, height) };
        AddChild(button);
        return button;
    }
}
