using System;
using Godot;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Simulation;

namespace GodotSyxPort.UI;

/// <summary>One policy surface for every data-driven room blueprint.</summary>
public sealed partial class RoomPolicyPanel : ColorRect
{
    private RoomSystem _rooms = null!;
    private JobBoard _jobs = null!;
    private ResourceLedger _resources = null!;
    private Func<RoomRecord?> _selected = null!;
    private Label _summary = null!;
    private OptionButton _upgrade = null!;
    private OptionButton _stockResource = null!;
    private Label _stockSummary = null!;
    private readonly Control[] _stockControls = new Control[8];
    private bool _refreshing;

    public void Initialize(RoomSystem rooms, JobBoard jobs, ResourceLedger resources,
        Func<RoomRecord?> selected)
    {
        _rooms = rooms;
        _jobs = jobs;
        _resources = resources;
        _selected = selected;
        Color = new Color(0.03f, 0.04f, 0.05f, 0.88f);
        Position = new Vector2(12, 116);
        Size = new Vector2(420, 266);
        MouseFilter = MouseFilterEnum.Stop;
        var title = LabelAt("Политика комнаты", 10, 7, 15);
        title.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        _summary = LabelAt("Комната не выбрана", 10, 32, 12);
        _summary.Size = new Vector2(400, 74);
        AddButton("− штат", 10, 112, () => Workers(-1));
        AddButton("+ штат", 82, 112, () => Workers(1));
        AddButton("Рецепт", 154, 112, Recipe);
        AddButton("− инструм.", 226, 112, () => Tools(-1));
        AddButton("+ инструм.", 318, 112, () => Tools(1));
        _upgrade = new OptionButton { Position = new Vector2(10, 146), Size = new Vector2(132, 28) };
        _upgrade.ItemSelected += Upgrade;
        AddChild(_upgrade);
        _stockResource = new OptionButton { Position = new Vector2(150, 146), Size = new Vector2(155, 28) };
        foreach (var resource in Enum.GetValues<ResourceKind>())
            _stockResource.AddItem(resource.ToString(), (int)resource);
        _stockResource.ItemSelected += _ => Refresh();
        AddChild(_stockResource);
        _stockSummary = LabelAt("", 10, 178, 11);
        _stockSummary.Size = new Vector2(400, 22);
        _stockControls[0] = AddButton("− ящик", 10, 204, () => StockCrates(-1));
        _stockControls[1] = AddButton("+ ящик", 82, 204, () => StockCrates(1));
        _stockControls[2] = AddButton("− лимит", 154, 204, () => StockLimit(-1));
        _stockControls[3] = AddButton("+ лимит", 226, 204, () => StockLimit(1));
        _stockControls[4] = AddButton("Fetch", 298, 204, ToggleFetching);
        _stockControls[5] = AddButton("− приор.", 10, 232, () => StockPriority(-1));
        _stockControls[6] = AddButton("+ приор.", 82, 232, () => StockPriority(1));
        _stockControls[7] = _stockResource;
    }

    public void Refresh()
    {
        var room = _selected();
        _refreshing = true;
        _upgrade.Clear();
        if (room is null)
        {
            _summary.Text = "Комната не выбрана. I — выбрать клетку комнаты.";
            _upgrade.Disabled = true;
            ShowStockpile(false);
            _refreshing = false;
            return;
        }
        var blueprint = _rooms.Blueprints.Get(room.DefinitionKey);
        var maximum = blueprint?.MaximumUpgrade ?? 0;
        for (var level = 0; level <= maximum; level++)
        {
            var item = _upgrade.ItemCount;
            _upgrade.AddItem($"Улучшение {level}");
            _upgrade.SetItemDisabled(item, !_rooms.CanSetUpgrade(room, level));
        }
        _upgrade.Disabled = maximum == 0;
        _upgrade.Select(Math.Clamp(room.UpgradeLevel, 0, maximum));
        _summary.Text = $"{room.DefinitionKey} #{room.Id} · {room.State} · площадь {room.Cells.Count}\n" +
            $"Штат {room.Employment.Employed}/{_rooms.EffectiveWorkerLimit(room)} · " +
            $"эфф. {room.Employment.TotalEfficiency:P0} · износ {room.Degradation:P0}\n" +
            $"{_rooms.RecipeDescription(room)}\nИнструменты {room.ToolUnits}/" +
            $"{room.ToolTargetPerWorker * room.Employment.Employed}";
        var logistics = _rooms.LogisticsInstance(room);
        var stockpile = logistics?.Kind == LogisticsKind.Stockpile;
        ShowStockpile(stockpile);
        if (stockpile)
        {
            var resource = SelectedStockResource();
            _stockSummary.Text = $"Склад: {resource} · ящики " +
                $"{logistics!.AllocatedCratesFor(resource)}/{logistics.TotalCrates} · " +
                $"лимит {(logistics.CrateLimit(resource) == 0 ? logistics.CrateCapacity : logistics.CrateLimit(resource))} · " +
                $"fetch {(logistics.Fetching ? "on" : "off")} · приоритет {logistics.Priority}";
        }
        _refreshing = false;
    }

    private void Workers(int delta)
    {
        if (_selected() is { } room) _rooms.AdjustWorkerLimit(room, delta, _jobs, _resources);
        Refresh();
    }

    private void Recipe()
    {
        if (_selected() is { } room) _rooms.CycleRecipe(room, _jobs, _resources);
        Refresh();
    }

    private void Tools(int delta)
    {
        if (_selected() is { } room) _rooms.AdjustToolTarget(room, delta);
        Refresh();
    }

    private void Upgrade(long level)
    {
        if (!_refreshing && _selected() is { } room) _rooms.SetUpgrade(room, (int)level);
        Refresh();
    }

    private ResourceKind SelectedStockResource() =>
        (ResourceKind)_stockResource.GetItemId(Math.Max(0, _stockResource.Selected));

    private void StockCrates(int delta)
    {
        if (_selected() is { } room) _rooms.AdjustStockpileCrates(room, SelectedStockResource(), delta);
        Refresh();
    }

    private void StockLimit(int delta)
    {
        if (_selected() is { } room) _rooms.AdjustStockpileCrateLimit(room, SelectedStockResource(), delta);
        Refresh();
    }

    private void ToggleFetching()
    {
        if (_selected() is { } room) _rooms.ToggleStockpileFetching(room);
        Refresh();
    }

    private void StockPriority(int delta)
    {
        if (_selected() is { } room) _rooms.AdjustStockpilePriority(room, delta);
        Refresh();
    }

    private void ShowStockpile(bool visible)
    {
        _stockSummary.Visible = visible;
        foreach (var control in _stockControls) control.Visible = visible;
    }

    private Button AddButton(string text, float x, float y, Action action)
    {
        var button = new Button { Text = text, Position = new Vector2(x, y), Size = new Vector2(68, 28) };
        if (text.Contains("инструм.")) button.Size = new Vector2(88, 28);
        button.Pressed += action;
        AddChild(button);
        return button;
    }

    private Label LabelAt(string text, float x, float y, int size)
    {
        var label = new Label { Text = text, Position = new Vector2(x, y) };
        label.AddThemeFontSizeOverride("font_size", size);
        AddChild(label);
        return label;
    }
}
