using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Data;
using GodotSyxPort.Events;

namespace GodotSyxPort.UI;

/// <summary>Source-text reader used by the world log, advice and wiki top-panel buttons.</summary>
public sealed partial class SettlementReferencePanel : ColorRect
{
    private sealed record Entry(string Name, string Text);
    private readonly List<Entry> _entries = new();
    private Label _title = null!;
    private ItemList _list = null!;
    private RichTextLabel _body = null!;

    public void Initialize()
    {
        Position = new Vector2(130, 62);
        Size = new Vector2(1000, 580);
        Color = new Color("101410fa");
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;
        _title = new Label { Position = new Vector2(16, 10), Size = new Vector2(900, 36) };
        _title.AddThemeFontSizeOverride("font_size", 21);
        _title.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        AddChild(_title);
        var close = new Button { Text = "×", Position = new Vector2(950, 8), Size = new Vector2(36, 34) };
        close.Pressed += () => Visible = false;
        AddChild(close);
        _list = new ItemList { Position = new Vector2(14, 54), Size = new Vector2(300, 510) };
        _list.ItemSelected += index => ShowEntry((int)index);
        AddChild(_list);
        _body = new RichTextLabel
        {
            Position = new Vector2(328, 54), Size = new Vector2(658, 510),
            FitContent = false, ScrollActive = true
        };
        AddChild(_body);
    }

    public void OpenWorldLog(IReadOnlyList<EventNotice> notices, int secondsPerDay)
    {
        _entries.Clear();
        foreach (var notice in notices.OrderByDescending(value => value.Sequence))
        {
            var day = Math.Max(1, (int)Math.Floor(notice.StartedAt / Math.Max(1, secondsPerDay)) + 1);
            var context = notice.Context.Count == 0 ? "" : "\n" +
                string.Join("\n", notice.Context.Select(pair => $"{pair.Key}: {pair.Value}"));
            _entries.Add(new Entry($"День {day} · {notice.Kind}",
                $"{notice.Key}\nКоличество: {notice.Amount}{context}"));
        }
        if (_entries.Count == 0) _entries.Add(new Entry("Журнал пуст", "Событий мира и поселения пока нет."));
        Open("Журнал мира");
    }

    public void OpenAdvice() => OpenSourceEntries(
        "Советы", "res://Data/Original/text/campaign/000_Tutorial.txt", "MISSIONS");

    public void OpenWiki() => OpenSourceEntries(
        "Энциклопедия", "res://Data/Original/text/wiki/GUIDE.txt", "WIKIS");

    private void OpenSourceEntries(string title, string path, string collection)
    {
        _entries.Clear();
        try
        {
            var items = SyxDataParser.Parse(FileAccess.GetFileAsString(path)).Get(collection)?.Items;
            if (items is not null)
                foreach (var item in items)
                {
                    var name = item.Get("NAME")?.Text();
                    var text = item.Get("TEXT")?.Text(item.Get("DESC")?.Text() ?? "") ??
                               item.Get("DESC")?.Text() ?? "";
                    var goal = item.Get("GOAL")?.Text() ?? "";
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    _entries.Add(new Entry(name, text.Replace("%r%", "\n") +
                        (goal.Length == 0 ? "" : $"\n\nЦель: {goal}")));
                }
        }
        catch (Exception exception)
        {
            _entries.Add(new Entry("Ошибка чтения", exception.Message));
        }
        if (_entries.Count == 0) _entries.Add(new Entry("Нет записей", "Исходный текст не содержит записей."));
        Open(title);
    }

    private void Open(string title)
    {
        _title.Text = title;
        _list.Clear();
        foreach (var entry in _entries) _list.AddItem(entry.Name);
        _list.Select(0);
        ShowEntry(0);
        Visible = true;
    }

    private void ShowEntry(int index)
    {
        if ((uint)index >= (uint)_entries.Count) return;
        _body.Text = _entries[index].Text;
    }
}
