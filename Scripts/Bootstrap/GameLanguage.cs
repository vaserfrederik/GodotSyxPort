using System;
using Godot;

namespace GodotSyxPort.Bootstrap;

/// <summary>Two explicitly supported interface languages. Russian is the default.</summary>
public static class GameLanguage
{
    public const string Russian = "ru";
    public const string English = "en";
    private const string SettingsPath = "user://settings.cfg";

    public static string Current { get; private set; } = Russian;
    public static bool IsRussian => Current == Russian;

    public static void LoadAndApply()
    {
        var config = new ConfigFile();
        var locale = Russian;
        if (config.Load(SettingsPath) == Error.Ok)
            locale = Normalize(config.GetValue("interface", "language", Russian).AsString());
        Apply(locale, false);
    }

    public static void Apply(string locale, bool save = true)
    {
        Current = Normalize(locale);
        TranslationServer.SetLocale(Current);
        if (!save) return;
        var config = new ConfigFile();
        config.Load(SettingsPath);
        config.SetValue("interface", "language", Current);
        var error = config.Save(SettingsPath);
        if (error != Error.Ok) GD.PushWarning($"Не удалось сохранить язык интерфейса: {error}");
    }

    private static string Normalize(string locale) =>
        locale.StartsWith(English, StringComparison.OrdinalIgnoreCase) ? English : Russian;
}
