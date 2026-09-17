using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using GodotSyxPort.Resources;
using GodotSyxPort.Settlement;
using GodotSyxPort.Technology;
using GodotSyxPort.Law;
using GodotSyxPort.Citizens;
using GodotSyxPort.Governance;
using GodotSyxPort.World;
using GodotSyxPort.Rooms;
using GodotSyxPort.Military;
using GodotSyxPort.Bootstrap;

namespace GodotSyxPort.Save;

public sealed class SaveSnapshot
{
    public int Version { get; set; } = 36;
    public int WorldSeed { get; set; }
    public int SelectedRegionId { get; set; } = -1;
    public int WorldCapitalX { get; set; } = -1;
    public int WorldCapitalY { get; set; } = -1;
    public PlayerStartProfile? PlayerProfile { get; set; }
    public int LandingOrigin { get; set; } = -1;
    public string PlayerRace { get; set; } = "HUMAN";
    public SettlementWorldSnapshot? SettlementWorld { get; set; }
    public TechnologySnapshot? Technologies { get; set; }
    public SettlementLawSnapshot? Law { get; set; }
    public SettlementGovernanceSnapshot? Governance { get; set; }
    public StrategicWorldSnapshot? StrategicWorld { get; set; }
    public RegionalEconomySnapshot? RegionalEconomy { get; set; }
    public SpecialProductionSnapshot? SpecialProduction { get; set; }
    public LogisticsPolicySnapshot[] LogisticsPolicies { get; set; } = Array.Empty<LogisticsPolicySnapshot>();
    public SettlementInvasionSnapshot? Invasion { get; set; }
    public int MapWidth { get; set; }
    public int MapHeight { get; set; }
    public WorldGridMutableSnapshot? MutableTerrain { get; set; }
    public ulong Tick { get; set; }
    public double PlayedSeconds { get; set; }
    public int[] Walls { get; set; } = Array.Empty<int>();
    public int[] Roads { get; set; } = Array.Empty<int>();
    public string[] RoadTypes { get; set; } = Array.Empty<string>();
    public int[] RoomFloors { get; set; } = Array.Empty<int>();
    public string[] RoomFloorTypes { get; set; } = Array.Empty<string>();
    public int[] Zones { get; set; } = Array.Empty<int>();
    public int[] Doors { get; set; } = Array.Empty<int>();
    public int[] Furniture { get; set; } = Array.Empty<int>();
    public SavedJob[] Jobs { get; set; } = Array.Empty<SavedJob>();
    public int Wood { get; set; } = 5000;
    public int Stone { get; set; } = 10000;
    public int Grain { get; set; } = 500;
    public int Food { get; set; } = 100;
    public int Tools { get; set; }
    public int FurnitureStock { get; set; }
    public byte[] ResourceKinds { get; set; } = Array.Empty<byte>();
    public int[] ResourceAmounts { get; set; } = Array.Empty<int>();
    public SavedRoom[] Rooms { get; set; } = Array.Empty<SavedRoom>();
    public SavedRoomStorage[] InternalStorage { get; set; } = Array.Empty<SavedRoomStorage>();
    public SavedCitizen[] Citizens { get; set; } = Array.Empty<SavedCitizen>();
    public SavedLooseResource[] LooseResources { get; set; } = Array.Empty<SavedLooseResource>();
    public int BakerLimit { get; set; } = -1;
    public int CarpenterLimit { get; set; } = -1;
    public int[] TotalProduced { get; set; } = new int[ResourceLedger.KindCount];
    public int[] TotalConsumed { get; set; } = new int[ResourceLedger.KindCount];
    public SavedEconomySample[] EconomyHistory { get; set; } = Array.Empty<SavedEconomySample>();
}

public sealed class SavedEconomySample
{
    public double Time { get; set; }
    public int[] Produced { get; set; } = new int[ResourceLedger.KindCount];
    public int[] Consumed { get; set; } = new int[ResourceLedger.KindCount];
}

public sealed class SavedCitizen
{
    public int Id { get; set; }
    public int Cell { get; set; }
    public float Hunger { get; set; }
    public byte Profession { get; set; }
    public bool Alive { get; set; } = true;
    public byte FoodPlan { get; set; }
    public float EatTimeLeft { get; set; }
    public int FoodSource { get; set; }
    public bool FoodFromLoose { get; set; }
    public string Race { get; set; } = "HUMAN";
    public byte Class { get; set; } = (byte)SocialClass.Citizen;
    public byte Type { get; set; } = (byte)HumanoidType.Subject;
    public string FirstName { get; set; } = "";
    public string Surname { get; set; } = "";
    public byte Gender { get; set; }
    public byte Origin { get; set; } = (byte)CitizenOrigin.Immigrant;
    public int ParentId { get; set; }
    public int BirthDay { get; set; }
    public int AgeDays { get; set; }
    public int BabyDays { get; set; }
    public string Religion { get; set; } = "";
    public int HomeRoomId { get; set; }
    public double NeedTimeLeft { get; set; }
    public Dictionary<string, int> Needs { get; set; } = new();
    public CitizenHealthSnapshot? Health { get; set; }
}

public sealed class SavedLooseResource
{
    public int Cell { get; set; }
    public byte Resource { get; set; }
    public int Amount { get; set; }
    public bool AlreadyAccounted { get; set; }
}

public sealed class SavedRoomStorage
{
    public int RoomId { get; set; }
    public int Cell { get; set; }
    public int Resource { get; set; } = -1;
    public int Amount { get; set; }
}

public sealed class SavedRoom
{
    public int Id { get; set; }
    public byte Type { get; set; }
    public string DefinitionKey { get; set; } = "";
    public int[] Cells { get; set; } = Array.Empty<int>();
    public int[] RequiredWalls { get; set; } = Array.Empty<int>();
    public int[] RequiredDoors { get; set; } = Array.Empty<int>();
    public int WorkerLimit { get; set; } = -1;
    public int RecipeIndex { get; set; }
    public int RequiredFurniture { get; set; }
    public int UpgradeLevel { get; set; }
    public double Isolation { get; set; } = 1;
    public double Degradation { get; set; }
    public double MaintenanceDebt { get; set; }
    public int ToolTargetPerWorker { get; set; }
    public int ToolUnits { get; set; }
    public double ToolWearProgress { get; set; }
    public int[] ItemGroupKeys { get; set; } = Array.Empty<int>();
    public double[] ItemGroupAmounts { get; set; } = Array.Empty<double>();
    public byte[] MaintenanceResources { get; set; } = Array.Empty<byte>();
    public int[] MaintenanceResourceAmounts { get; set; } = Array.Empty<int>();
    public int[] FurnitureRepairCells { get; set; } = Array.Empty<int>();
    public byte[] FurnitureRepairResources { get; set; } = Array.Empty<byte>();
    public int[] FurnitureRepairAmounts { get; set; } = Array.Empty<int>();
    public SavedFurnitureFootprint[] FurnitureFootprints { get; set; } =
        Array.Empty<SavedFurnitureFootprint>();
    public int[] ProductionInputRecipes { get; set; } = Array.Empty<int>();
    public byte[] ProductionInputResources { get; set; } = Array.Empty<byte>();
    public double[] ProductionInputProgress { get; set; } = Array.Empty<double>();
    public int[] ProductionOutputRecipes { get; set; } = Array.Empty<int>();
    public double[] ProductionOutputProgressByRecipe { get; set; } = Array.Empty<double>();
    public int[] AdditionalOutputRecipes { get; set; } = Array.Empty<int>();
    public byte[] AdditionalOutputResources { get; set; } = Array.Empty<byte>();
    public double[] AdditionalOutputProgress { get; set; } = Array.Empty<double>();
    public int[] InputStorageCells { get; set; } = Array.Empty<int>();
    public byte[] InputStorageResources { get; set; } = Array.Empty<byte>();
    public int[] InputStorageAmounts { get; set; } = Array.Empty<int>();
    public byte[] ConstructionResources { get; set; } = Array.Empty<byte>();
    public int[] ConstructionRequired { get; set; } = Array.Empty<int>();
    public int[] ConstructionDelivered { get; set; } = Array.Empty<int>();
    // v10 compatibility field; v11 stores one accumulator per recipe.
    public double ProductionOutputProgress { get; set; }
}

public sealed class SavedFurnitureFootprint
{
    public int Anchor { get; set; }
    public int[] Cells { get; set; } = Array.Empty<int>();
    public bool Broken { get; set; }
}

public sealed class SavedJob
{
    public int Cell { get; set; }
    public byte Kind { get; set; }
    public float WorkLeft { get; set; }
    public bool ResourceReserved { get; set; }
    public byte Resource { get; set; }
    public int ResourceAmount { get; set; }
    public byte OutputResource { get; set; }
    public int OutputAmount { get; set; }
    public bool OutputAlreadyAccounted { get; set; }
    public int RoomId { get; set; }
    public int DestinationRoomId { get; set; }
    public int Destination { get; set; }
    public int CorpseId { get; set; }
    public int FacilitySlotId { get; set; }
    public byte LawProcessType { get; set; }
    public bool PickedUp { get; set; }
    public byte RequiredProfession { get; set; }
    public byte Priority { get; set; }
    public int ReservedAmount { get; set; }
    public int DeliveredAmount { get; set; }
    public byte BuildPhase { get; set; }
    public byte State { get; set; }
    public int[] FurnitureCells { get; set; } = Array.Empty<int>();
    public int[] FurnitureBlockerCells { get; set; } = Array.Empty<int>();
    public int[] FurnitureReachableCells { get; set; } = Array.Empty<int>();
    public int[] FurnitureWorkCells { get; set; } = Array.Empty<int>();
    public int[] FurnitureStorageCells { get; set; } = Array.Empty<int>();
    public string RoadKey { get; set; } = "DIRT";
    public string FloorKey { get; set; } = "DIRT";
    public byte[] InputResources { get; set; } = Array.Empty<byte>();
    public int[] InputAmounts { get; set; } = Array.Empty<int>();
    public int ConstructionTransitAmount { get; set; }
    public int ConstructionCell { get; set; }
}

public static class SaveGameService
{
    private const string SavePath = "user://city.save.json";
    private const string BackupPath = "user://city.save.backup.json";
    private const string TempPath = "user://city.save.tmp.json";
    private const int AutoSaveSlots = 3;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = false };

    public static bool HasSave()
    {
        if (File.Exists(ProjectSettings.GlobalizePath(SavePath)) ||
            File.Exists(ProjectSettings.GlobalizePath(BackupPath))) return true;
        for (var slot = 0; slot < AutoSaveSlots; slot++)
            if (File.Exists(AutoSavePath(slot))) return true;
        return false;
    }

    public static bool Save(SaveSnapshot snapshot)
    {
        var path = ProjectSettings.GlobalizePath(SavePath);
        var backup = ProjectSettings.GlobalizePath(BackupPath);
        var temporary = ProjectSettings.GlobalizePath(TempPath);
        return WriteSnapshot(snapshot, path, temporary, backup);
    }

    public static bool SaveAuto(SaveSnapshot snapshot)
    {
        try
        {
            for (var slot = AutoSaveSlots - 1; slot > 0; slot--)
            {
                var older = AutoSavePath(slot);
                var newer = AutoSavePath(slot - 1);
                if (File.Exists(newer)) File.Copy(newer, older, true);
            }
            var path = AutoSavePath(0);
            return WriteSnapshot(snapshot, path, path + ".tmp", null);
        }
        catch (Exception exception)
        {
            GD.PushWarning($"Не удалось выполнить автосохранение: {exception.Message}");
            return false;
        }
    }

    private static bool WriteSnapshot(
        SaveSnapshot snapshot,
        string path,
        string temporary,
        string? backup)
    {
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(snapshot, Options));
            if (backup is not null && File.Exists(path)) File.Copy(path, backup, true);
            File.Move(temporary, path, true);
            return true;
        }
        catch (Exception exception)
        {
            try
            {
                if (File.Exists(temporary)) File.Delete(temporary);
            }
            catch
            {
                // The original save and backup remain the recovery sources.
            }
            GD.PushError($"Не удалось сохранить игру: {exception.Message}");
            return false;
        }
    }

    public static SaveSnapshot? Load()
    {
        var path = ProjectSettings.GlobalizePath(SavePath);
        var save = TryLoad(path);
        if (save is not null) return save;
        var backup = ProjectSettings.GlobalizePath(BackupPath);
        save = TryLoad(backup);
        if (save is not null) GD.PushWarning("Основное сохранение повреждено; загружена резервная копия.");
        if (save is not null) return save;
        for (var slot = 0; slot < AutoSaveSlots; slot++)
        {
            save = TryLoad(AutoSavePath(slot));
            if (save is null) continue;
            GD.PushWarning($"Загружено автоматическое сохранение #{slot + 1}.");
            return save;
        }
        return null;
    }

    private static SaveSnapshot? TryLoad(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            return JsonSerializer.Deserialize<SaveSnapshot>(File.ReadAllText(path), Options);
        }
        catch (Exception exception) when (exception is IOException or JsonException)
        {
            GD.PushWarning($"Не удалось прочитать сохранение {Path.GetFileName(path)}: {exception.Message}");
            return null;
        }
    }

    private static string AutoSavePath(int slot) =>
        ProjectSettings.GlobalizePath($"user://city.autosave.{slot}.json");
}
