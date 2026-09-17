using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Events;

namespace GodotSyxPort.UI;

/// <summary>Godot presentation adapter for immutable settlement event notices.</summary>
public sealed partial class SettlementNotificationFeed : ColorRect
{
    private readonly Queue<string> _visible = new();
    private Label _feed = null!;
    private long _lastSequence;

    public void Initialize()
    {
        Color = new Color(0.03f, 0.04f, 0.05f, 0.88f);
        Position = new Vector2(1000, 116);
        Size = new Vector2(268, 180);
        MouseFilter = MouseFilterEnum.Stop;
        var title = AddLabel("События поселения", new Vector2(10, 8), 15);
        title.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        _feed = AddLabel("Событий пока нет", new Vector2(10, 34), 12);
        _feed.Size = new Vector2(248, 136);
        _feed.AutowrapMode = TextServer.AutowrapMode.WordSmart;
    }

    public void Refresh(IReadOnlyList<EventNotice> notices, int secondsPerDay)
    {
        foreach (var notice in notices.Where(value => value.Sequence > _lastSequence)
                     .OrderBy(value => value.Sequence))
        {
            _lastSequence = notice.Sequence;
            _visible.Enqueue(Format(notice, secondsPerDay));
            while (_visible.Count > 6) _visible.Dequeue();
        }
        if (_visible.Count > 0) _feed.Text = string.Join("\n", _visible.Reverse());
    }

    private static string Format(EventNotice notice, int secondsPerDay)
    {
        var day = Math.Max(0, (int)Math.Floor(notice.StartedAt / Math.Max(1, secondsPerDay)) + 1);
        var amount = notice.Amount > 0 ? $" ×{notice.Amount}" : "";
        var context = notice.Context.Count == 0 ? "" : " · " +
            string.Join(", ", notice.Context.Select(pair => $"{pair.Key}: {pair.Value}"));
        return $"Д{day} · {Title(notice.Kind)}: {notice.Key}{amount}{context}";
    }

    private static string Title(SettlementEventKind kind) => kind switch
    {
        SettlementEventKind.Emigration => "эмиграция",
        SettlementEventKind.RaceBrawl => "драка рас",
        SettlementEventKind.ReligiousBrawl => "религиозная драка",
        SettlementEventKind.Strike => "забастовка",
        SettlementEventKind.Riot => "бунт",
        SettlementEventKind.Epidemic => "эпидемия",
        SettlementEventKind.MildDisease => "болезнь",
        SettlementEventKind.Temperature => "погода",
        SettlementEventKind.WorkAccident => "несчастный случай",
        SettlementEventKind.SerialKiller => "убийство",
        SettlementEventKind.SlaveUprising => "восстание",
        _ => "событие"
    };

    private Label AddLabel(string text, Vector2 position, int size)
    {
        var label = new Label { Text = text, Position = position };
        label.AddThemeFontSizeOverride("font_size", size);
        AddChild(label);
        return label;
    }
}
