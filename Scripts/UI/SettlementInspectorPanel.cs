using System;
using System.Linq;
using Godot;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;

namespace GodotSyxPort.UI;

/// <summary>Single Godot inspector for a map cell, its room and nearest resident.</summary>
public sealed partial class SettlementInspectorPanel : ColorRect
{
    private Label _title = null!;
    private Label _terrain = null!;
    private Label _room = null!;
    private Label _citizen = null!;

    public void Initialize()
    {
        Color = new Color(0.03f, 0.04f, 0.05f, 0.9f);
        Position = new Vector2(12, 308);
        Size = new Vector2(420, 318);
        MouseFilter = MouseFilterEnum.Stop;
        _title = AddLabel(new Vector2(12, 10), 16, new Color("f2dfaa"));
        _terrain = AddLabel(new Vector2(12, 38), 13, new Color("d7e3e6"));
        _room = AddLabel(new Vector2(12, 112), 13, new Color("d7e3e6"));
        _citizen = AddLabel(new Vector2(12, 218), 13, new Color("d7e3e6"));
        ClearSelection();
    }

    public void ShowSelection(
        GridCoord cell,
        WorldGridData data,
        RoomRecord? room,
        RoomSystem rooms,
        CitizenInspectionSnapshot? citizen)
    {
        _title.Text = $"Клетка {cell.X}:{cell.Z}";
        var flags = string.Join(", ", Enum.GetValues<TileFlags>()
            .Where(flag => flag != TileFlags.None && data.Has(cell, flag))
            .Select(flag => flag.ToString()));
        _terrain.Text = $"{data.Ground(cell)} · высота {data.Elevation(cell)} · фундамент {data.Foundation(cell)}/3\n" +
            $"Плодородие {data.Fertility(cell)}/15 · влага {data.Moisture(cell)}/15\n" +
            $"Минерал {data.MineralType(cell)} × {data.MineralAmount(cell)} · {flags}";
        _room.Text = room is null
            ? "Комната: нет"
            : $"Комната: {room.DefinitionKey} #{room.Id} · {room.State}\n" +
              $"Площадь {room.Cells.Count} · штат {room.Employment.Employed}/" +
              $"{rooms.EffectiveWorkerLimit(room)} · эффективность {room.Employment.TotalEfficiency:P0}\n" +
              $"Износ {room.Degradation:P0} · upgrade {room.UpgradeLevel}\n" +
              $"{rooms.RecipeDescription(room)}";
        _citizen.Text = citizen is null
            ? "Житель рядом: нет"
            : $"Житель #{citizen.Id}: {citizen.Name}\n" +
              $"{citizen.Race} · {citizen.Class}/{citizen.Type} · {citizen.Profession}\n" +
              $"Возраст {citizen.AgeDays} дней · голод {citizen.Hunger:0} · дом #{citizen.HomeRoomId}\n" +
              $"{citizen.Health}, травма {citizen.Injury} · работа {citizen.CurrentJob}\n" +
              $"Потребности: {string.Join(", ", citizen.HighestNeeds.Select(pair => $"{pair.Key} {pair.Value}"))}";
    }

    public void ClearSelection()
    {
        _title.Text = "Инспектор поселения";
        _terrain.Text = "Выберите клетку инструментом I или на глобальной карте.";
        _room.Text = "Комната: —";
        _citizen.Text = "Житель: —";
    }

    private Label AddLabel(Vector2 position, int size, Color color)
    {
        var label = new Label
        {
            Position = position,
            Size = new Vector2(396, 100),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        AddChild(label);
        return label;
    }
}
