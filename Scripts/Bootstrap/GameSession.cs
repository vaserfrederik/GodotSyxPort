using System.Collections.Generic;
using System.Linq;
using GodotSyxPort.Data;
using GodotSyxPort.Save;
using GodotSyxPort.World;

namespace GodotSyxPort.Bootstrap;

public sealed record GameStartConfiguration(
    int WorldSeed,
    int SelectedRegionId,
    string PlayerRace,
    PlayerStartProfile? Profile = null,
    int WorldCapitalX = -1,
    int WorldCapitalY = -1);

public sealed record PlayerStartProfile(
    string Race,
    string FactionName,
    string RulerName,
    int BannerType,
    string BannerBackground,
    string BannerForeground,
    string BannerBorder,
    string BannerPole,
    IReadOnlyList<string> Titles,
    string BannerPixels = "");

/// <summary>Owns the in-memory world and the choices made before entering a settlement.</summary>
public static class GameSession
{
    public const int DefaultSeed = 0x535958;
    public static GameStartConfiguration? Current { get; private set; }
    public static StrategicWorldRuntime? World { get; private set; }
    public static PlayerTitleBonusRuntime TitleBonuses { get; private set; } = new();

    public static void StartNew(StrategicWorldRuntime world, int seed, int regionId, string race,
        PlayerStartProfile? profile = null, int capitalX = -1, int capitalY = -1)
    {
        world.ConfigurePlayerStart(regionId, race);
        var region = world.Region(regionId);
        if (region is not null && (capitalX < 0 || capitalY < 0))
        {
            capitalX = region.CenterTileX;
            capitalY = region.CenterTileY;
        }
        World = world;
        Current = new GameStartConfiguration(seed, regionId, race, profile, capitalX, capitalY);
        TitleBonuses = PlayerTitleBonusRuntime.Load(profile?.Titles);
    }

    public static bool RestoreFromSave(SaveSnapshot save)
    {
        if (save.Version < 14 || save.SelectedRegionId < 0 ||
            string.IsNullOrWhiteSpace(save.PlayerRace)) return false;
        OriginalGameData.Load();
        var world = new StrategicWorldRuntime();
        if (save.StrategicWorld?.Terrain is { } terrain)
        {
            world.RestoreTerrain(terrain, save.WorldSeed);
            if (!world.CanSetPlayerStart(save.SelectedRegionId)) return false;
            world.GenerateCivilizations(OriginalGameData.Current.Races, save.WorldSeed,
                save.SelectedRegionId, save.PlayerRace, save.PlayerProfile?.FactionName);
        }
        else world.Generate(OriginalGameData.Current.Races, save.WorldSeed);
        if (save.StrategicWorld is not null) world.Restore(save.StrategicWorld);
        if (!world.CanSetPlayerStart(save.SelectedRegionId) ||
            !OriginalGameData.Current.Races.ContainsKey(save.PlayerRace)) return false;
        StartNew(world, save.WorldSeed, save.SelectedRegionId, save.PlayerRace,
            save.PlayerProfile, save.WorldCapitalX, save.WorldCapitalY);
        return true;
    }

    public static void EnsureDefault()
    {
        if (Current is not null && World is not null) return;
        OriginalGameData.Load();
        var world = new StrategicWorldRuntime();
        world.Generate(OriginalGameData.Current.Races, DefaultSeed);
        var region = world.Regions.First(candidate => candidate.Habitable &&
            world.CanSetPlayerStart(candidate.Id));
        var race = OriginalGameData.Current.Races.Values
            .Where(candidate => candidate.Playable)
            .Select(candidate => candidate.Key)
            .OrderBy(candidate => candidate)
            .FirstOrDefault() ?? "HUMAN";
        StartNew(world, DefaultSeed, region.Id, race);
    }

    public static void Clear()
    {
        Current = null;
        World = null;
        TitleBonuses = new PlayerTitleBonusRuntime();
    }
}
