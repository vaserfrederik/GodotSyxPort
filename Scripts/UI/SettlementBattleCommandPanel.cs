using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Core;
using GodotSyxPort.Military;

namespace GodotSyxPort.UI;

/// <summary>Settlement adaptation of BattlePanel, DivSelection and UISelection.</summary>
public sealed partial class SettlementBattleCommandPanel : ColorRect
{
    private SettlementMilitaryRuntime _military = null!;
    private readonly List<int> _divisionIds = new();
    private readonly List<(bool Artillery, int Id)> _selection = new();
    private ItemList _divisions = null!;
    private Label _status = null!;
    public int SelectedDivisionId { get; private set; } = -1;
    public int SelectedArtilleryRoomId { get; private set; } = -1;
    public DivisionBattleTask PlacementTask { get; private set; } = DivisionBattleTask.Move;
    public bool AwaitingMapOrder { get; private set; }
    public event Action? CloseRequested;
    public event Action<int, DivisionBattleTask>? ImmediateOrderIssued;

    public void Initialize(SettlementMilitaryRuntime military)
    {
        _military = military;
        Color = new Color("0d0f0fee"); MouseFilter = MouseFilterEnum.Stop;
        AnchorLeft = 0.5f; AnchorRight = 0.5f; AnchorTop = 1; AnchorBottom = 1;
        OffsetLeft = -420; OffsetRight = 420; OffsetTop = -176; OffsetBottom = -10;
        var title = LabelAt("Дивизии", 12, 8, 160, 26); title.AddThemeFontSizeOverride("font_size", 18);
        var close = ButtonAt("×", 798, 6, 32, 28); close.Pressed += () => CloseRequested?.Invoke();
        _divisions = new ItemList { Position = new Vector2(12, 40), Size = new Vector2(270, 112) };
        _divisions.ItemSelected += index => Select((int)index); AddChild(_divisions);
        Command("Двигаться", DivisionBattleTask.Move, 294, 40, OriginalUiIcons.MainCategory(3));
        Command("Атаковать", DivisionBattleTask.AttackMelee, 420, 40, OriginalUiIcons.MainCategory(13));
        Command("Натиск", DivisionBattleTask.Charge, 546, 40, OriginalUiIcons.MainCategory(13));
        var stop = ButtonAt("Стоп", 672, 40, 146, 36); stop.Icon = OriginalUiIcons.Small(28);
        stop.Pressed += () => IssueImmediate(DivisionBattleTask.Stop);
        var tight = ButtonAt("Плотный строй", 294, 84, 160, 34);
        tight.Pressed += () => SetFormation(DivisionFormation.Tight);
        var loose = ButtonAt("Свободный строй", 462, 84, 170, 34);
        loose.Pressed += () => SetFormation(DivisionFormation.Loose);
        var muster = ButtonAt("Мобилизовать орудие", 640, 84, 178, 34);
        muster.Pressed += ToggleArtilleryMuster;
        _status = LabelAt("Выберите дивизию", 294, 126, 524, 28);
        Refresh();
    }

    public void Refresh()
    {
        var previousDivision = SelectedDivisionId;
        var previousArtillery = SelectedArtilleryRoomId;
        _divisionIds.Clear(); _selection.Clear(); _divisions.Clear();
        foreach (var division in _military.Divisions.OrderBy(value => value.Id))
        {
            _divisionIds.Add(division.Id);
            _selection.Add((false, division.Id));
            _divisions.AddItem($"{division.Name}  {division.Members.Count}/{division.TargetMen}  " +
                $"{division.Order.Formation} · {division.Order.Task} · мораль {division.Morale:P0} · " +
                $"усталость {division.Exhaustion:P0}" +
                (division.AmmunitionMaximum > 0 ? $" · стрелы {division.AmmunitionRemaining}" : "") +
                (division.Routing ? " · БЕЖИТ" : division.Engaged ? " · бой" : ""));
        }
        foreach (var artillery in _military.Artillery.OrderBy(value => value.RoomId))
        {
            _selection.Add((true, artillery.RoomId));
            _divisions.AddItem($"Орудие #{artillery.RoomId} · " +
                (artillery.Mustered ? "мобилизовано" : "не мобилизовано") +
                $" · расчёт {artillery.Crew}/6 · " +
                (artillery.Loaded ? "заряжено" : $"зарядка {artillery.LoadProgress:P0}"));
        }
        var selected = _selection.FindIndex(value => previousArtillery >= 0
            ? value.Artillery && value.Id == previousArtillery
            : !value.Artillery && value.Id == previousDivision);
        if (selected < 0 && _selection.Count > 0) selected = 0;
        if (selected >= 0) { _divisions.Select(selected); Select(selected); }
        else { SelectedDivisionId = SelectedArtilleryRoomId = -1; _status.Text = "Нет боевых подразделений"; }
    }

    public bool CommitMapOrder(GridCoord start, GridCoord end, int targetDivisionId = -1,
        bool buildingTarget = false)
    {
        if (!AwaitingMapOrder || (SelectedDivisionId < 0 && SelectedArtilleryRoomId < 0)) return false;
        if (SelectedArtilleryRoomId >= 0)
        {
            var artilleryOk = _military.SetArtilleryTarget(SelectedArtilleryRoomId, end,
                targetDivisionId, targetDivisionId < 0);
            AwaitingMapOrder = false;
            _status.Text = artilleryOk ? "Цель орудия назначена" : "Орудие не мобилизовано или без расчёта";
            return artilleryOk;
        }
        var division = _military.Divisions.First(value => value.Id == SelectedDivisionId);
        var task = buildingTarget && PlacementTask == DivisionBattleTask.AttackMelee
            ? DivisionBattleTask.AttackBuilding
            : PlacementTask == DivisionBattleTask.AttackMelee && targetDivisionId >= 0 &&
              division.HasRangedWeapon ? DivisionBattleTask.AttackRanged : PlacementTask;
        var target = task == DivisionBattleTask.AttackBuilding ? end : start;
        var ok = _military.IssueOrder(SelectedDivisionId, task, start,
            targetDivisionId, target, fighting: true);
        AwaitingMapOrder = false;
        _status.Text = ok ? $"Приказ: {task} к {start.X}:{start.Z}" : "Приказ не принят";
        Refresh(); return ok;
    }

    private void Select(int index)
    {
        if ((uint)index >= (uint)_selection.Count) return;
        var selected = _selection[index];
        if (selected.Artillery)
        {
            SelectedDivisionId = -1; SelectedArtilleryRoomId = selected.Id;
            _status.Text = $"Орудие #{selected.Id}: выберите цель"; return;
        }
        SelectedArtilleryRoomId = -1; SelectedDivisionId = selected.Id;
        var division = _military.Divisions.First(value => value.Id == SelectedDivisionId);
        _status.Text = $"{division.Name}: {division.Order.Formation}, {division.Order.Task}";
    }

    private void Command(string text, DivisionBattleTask task, float x, float y, Texture2D? icon)
    {
        var button = ButtonAt(text, x, y, 118, 36); button.Icon = icon;
        button.Pressed += () => { PlacementTask = task;
            AwaitingMapOrder = SelectedDivisionId >= 0 || SelectedArtilleryRoomId >= 0;
            _status.Text = AwaitingMapOrder ? "ЛКМ и протяжка — задать линию построения" : "Выберите дивизию"; };
    }

    private void IssueImmediate(DivisionBattleTask task)
    {
        if (SelectedDivisionId >= 0) _military.IssueOrder(SelectedDivisionId, task, fighting: true);
        if (SelectedDivisionId >= 0) ImmediateOrderIssued?.Invoke(SelectedDivisionId, task);
        AwaitingMapOrder = false; Refresh();
    }

    private void SetFormation(DivisionFormation formation)
    {
        if (SelectedDivisionId >= 0) _military.SetFormation(SelectedDivisionId, formation);
        Refresh();
    }

    private void ToggleArtilleryMuster()
    {
        if (SelectedArtilleryRoomId < 0) return;
        var artillery = _military.Artillery.First(value => value.RoomId == SelectedArtilleryRoomId);
        _military.MusterArtillery(artillery.RoomId, !artillery.Mustered);
        Refresh();
    }

    private Label LabelAt(string text, float x, float y, float width, float height)
    { var value = new Label { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, height) };
        AddChild(value); return value; }
    private Button ButtonAt(string text, float x, float y, float width, float height)
    { var value = new Button { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, height) };
        AddChild(value); return value; }
}
