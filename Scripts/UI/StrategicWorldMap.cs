using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Data;
using GodotSyxPort.Governance;
using GodotSyxPort.Rendering;
using GodotSyxPort.Settlement;
using GodotSyxPort.Trade;
using GodotSyxPort.World;

namespace GodotSyxPort.UI;

/// <summary>Godot map/controller for the peaceful strategic world runtime.</summary>
public sealed partial class StrategicWorldMap : ColorRect
{
    private enum WorldLayer
    {
        Terrain, Factions, Elevation, Forest, Mountains, Ocean, Rivers,
        Fertility, Moisture, Water, Prospects, Loyalty,
        Population, Race, Religion, ReligiousOpposition
    }
    private static readonly Vector2 MapOrigin = new(18, 52);
    private const float MapSize = 596;
    private StrategicWorldRuntime _world = null!;
    private SettlementTradeRuntime _trade = null!;
    private RegionalEconomyRuntime _economy = null!;
    private SettlementWorldRuntime _settlementWorld = null!;
    private SettlementGovernanceRuntime _governance = null!;
    private Label _details = null!;
    private OptionButton _stance = null!;
    private Button _apply = null!;
    private OptionButton _building = null!;
    private SpinBox _buildingLevel = null!;
    private Label _economyDetails = null!;
    private OptionButton _mission = null!;
    private SpinBox _missionPoints = null!;
    private Label _diplomacyStatus = null!;
    private readonly System.Collections.Generic.List<string> _buildingKeys = new();
    private readonly Dictionary<string, Texture2D?> _townTextures = new(StringComparer.OrdinalIgnoreCase);
    private OptionButton _layer = null!, _edict = null!, _edictRace = null!;
    private WorldLayer _activeLayer;
    private int _selectedRegion = -1;
    private int _drawRevision = -1;
    private ImageTexture? _terrainTexture;
    private ImageTexture? _roadTexture;
    private ImageTexture? _layerTexture;
    private ImageTexture? _fogTexture;
    private int _entityRevision = -1;

    public void Initialize(StrategicWorldRuntime world, RegionalEconomyRuntime economy,
        SettlementTradeRuntime trade, SettlementWorldRuntime settlementWorld,
        SettlementGovernanceRuntime governance)
    {
        _world = world;
        _economy = economy;
        _trade = trade;
        _settlementWorld = settlementWorld;
        _governance = governance;
        BuildTerrainTexture();
        BuildRoadTexture();
        BuildLayerTexture();
        BuildFogTexture();
        Color = new Color(0.018f, 0.026f, 0.036f, 0.985f);
        Position = new Vector2(112, 34);
        Size = new Vector2(1056, 690);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;
        var title = LabelAt("Глобальная карта мира", 18, 12, 20);
        title.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        LabelAt("F3 — закрыть", 916, 17, 12)
            .AddThemeColorOverride("font_color", new Color("93a5aa"));
        _layer = new OptionButton { Position = new Vector2(620, 12), Size = new Vector2(220, 30) };
        foreach (var layer in Enum.GetValues<WorldLayer>()) _layer.AddItem(layer.ToString());
        _layer.ItemSelected += value =>
        {
            _activeLayer = (WorldLayer)value;
            BuildLayerTexture();
            QueueRedraw();
        };
        AddChild(_layer);
        _details = LabelAt("Выберите регион", 636, 62, 14);
        _details.Size = new Vector2(398, 330);
        _details.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        LabelAt("Дипломатия с владельцем", 636, 402, 13);
        _stance = new OptionButton { Position = new Vector2(636, 430), Size = new Vector2(218, 30) };
        foreach (var value in new[]
                 {
                     DiplomacyStance.Neutral, DiplomacyStance.Trade, DiplomacyStance.Pact,
                     DiplomacyStance.Allied, DiplomacyStance.Vassal, DiplomacyStance.Overlord
                 })
        {
            _stance.AddItem(value.ToString());
            _stance.SetItemMetadata(_stance.ItemCount - 1, (int)value);
        }
        AddChild(_stance);
        _apply = new Button
        {
            Text = "Применить мирный статус", Position = new Vector2(864, 430),
            Size = new Vector2(170, 30), Disabled = true
        };
        _apply.Pressed += ApplyStance;
        AddChild(_apply);
        _mission = new OptionButton { Position = new Vector2(636, 466), Size = new Vector2(142, 28) };
        foreach (var mission in Enum.GetValues<EmissaryMissionKind>()) _mission.AddItem(mission.ToString());
        _mission.ItemSelected += _ => LoadMission();
        AddChild(_mission);
        _missionPoints = new SpinBox
        {
            Position = new Vector2(784, 466), Size = new Vector2(74, 28), MinValue = 0,
            MaxValue = WorldDiplomacyRuntime.MaximumPointsPerMission, Step = 1,
            AllowGreater = false, AllowLesser = false
        };
        AddChild(_missionPoints);
        var assign = new Button { Text = "Назначить", Position = new Vector2(864, 466), Size = new Vector2(82, 28) };
        assign.Pressed += ApplyMission;
        AddChild(assign);
        var gift = new Button { Text = "Дар 400", Position = new Vector2(952, 466), Size = new Vector2(82, 28) };
        gift.Pressed += SendGift;
        AddChild(gift);
        LabelAt("Региональное здание", 636, 500, 13);
        _building = new OptionButton { Position = new Vector2(636, 522), Size = new Vector2(250, 30) };
        _buildingKeys.AddRange(_economy.Buildings.All.Keys.OrderBy(value => value));
        foreach (var key in _buildingKeys) _building.AddItem(key);
        _building.ItemSelected += _ => LoadBuildingLevel();
        AddChild(_building);
        _buildingLevel = new SpinBox
        {
            Position = new Vector2(894, 522), Size = new Vector2(70, 30),
            MinValue = 0, MaxValue = 10, Step = 1, AllowGreater = false, AllowLesser = false
        };
        AddChild(_buildingLevel);
        var build = new Button { Text = "Строить", Position = new Vector2(972, 522), Size = new Vector2(62, 30) };
        build.Pressed += ApplyBuilding;
        AddChild(build);
        _economyDetails = LabelAt("", 636, 556, 12);
        _economyDetails.Size = new Vector2(398, 70);
        _economyDetails.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _diplomacyStatus = LabelAt("", 636, 628, 11);
        _diplomacyStatus.Size = new Vector2(398, 18);
        _edictRace = new OptionButton { Position = new Vector2(636, 646), Size = new Vector2(180, 28) };
        foreach (var race in OriginalGameData.Current.Races.Keys.OrderBy(value => value)) _edictRace.AddItem(race);
        _edictRace.ItemSelected += _ => LoadEdict();
        AddChild(_edictRace);
        _edict = new OptionButton { Position = new Vector2(824, 646), Size = new Vector2(132, 28) };
        foreach (var edict in Enum.GetValues<RegionalEdict>()) _edict.AddItem(edict.ToString());
        AddChild(_edict);
        var applyEdict = new Button
        {
            Text = "Применить", Position = new Vector2(964, 646), Size = new Vector2(70, 28)
        };
        applyEdict.Pressed += ApplyEdict;
        AddChild(applyEdict);
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible) { RefreshSelection(); QueueRedraw(); }
    }

    public override void _Process(double delta)
    {
        if (!Visible || _drawRevision == _world.Revision &&
            _entityRevision == _settlementWorld.Entities.Revision) return;
        _drawRevision = _world.Revision;
        _entityRevision = _settlementWorld.Entities.Revision;
        BuildRoadTexture();
        BuildLayerTexture();
        BuildFogTexture();
        RefreshSelection();
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouse)
            return;
        var local = mouse.Position - MapOrigin;
        if (local.X < 0 || local.Y < 0 || local.X >= MapSize || local.Y >= MapSize) return;
        var x = Math.Clamp((int)(local.X / MapSize * StrategicWorldRuntime.TileDimension),
            0, StrategicWorldRuntime.TileDimension - 1);
        var y = Math.Clamp((int)(local.Y / MapSize * StrategicWorldRuntime.TileDimension),
            0, StrategicWorldRuntime.TileDimension - 1);
        var region = _world.RegionAtTile(x, y);
        _selectedRegion = region is not null && RegionVisible(region.Id) ? region.Id : -1;
        RefreshSelection();
        QueueRedraw();
        AcceptEvent();
    }

    public override void _Draw()
    {
        var cell = MapSize / StrategicWorldRuntime.RegionsAcross;
        DrawRect(new Rect2(MapOrigin, new Vector2(MapSize, MapSize)), new Color("101b24"));
        if (_activeLayer == WorldLayer.Terrain && _terrainTexture is not null)
            DrawTextureRect(_terrainTexture, new Rect2(MapOrigin, new Vector2(MapSize, MapSize)), false);
        else if (_layerTexture is not null)
            DrawTextureRect(_layerTexture, new Rect2(MapOrigin, new Vector2(MapSize, MapSize)), false);
        foreach (var region in _world.Regions)
        {
            var center = MapOrigin + new Vector2(
                (region.CenterTileX + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize,
                (region.CenterTileY + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize);
            if (region.Capital && RegionVisible(region.Id))
            {
                DrawCircle(center, Math.Max(1.5f, cell * 0.22f), new Color("f3df9b"));
                DrawString(ThemeDB.FallbackFont, center + new Vector2(3, -3), region.Name,
                    HorizontalAlignment.Left, -1, 9, new Color("ead9aa"));
            }
            if (region.Id == _selectedRegion)
                DrawCircle(center, Math.Max(3f, cell * 0.42f), Colors.White, false, 1.5f);
        }
        if ((_activeLayer is WorldLayer.Terrain or WorldLayer.Factions) && _roadTexture is not null)
            DrawTextureRect(_roadTexture, new Rect2(MapOrigin, new Vector2(MapSize, MapSize)), false);
        if (_activeLayer == WorldLayer.Terrain)
            foreach (var city in _world.Cities)
            {
                var region = _world.Region(city.RegionId);
                if (region is null || !RegionVisible(region.Id)) continue;
                DrawOriginalTown(city, MapOrigin + new Vector2(
                    (region.CenterTileX + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize,
                    (region.CenterTileY + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize));
            }
        if (_activeLayer == WorldLayer.Terrain)
            foreach (var landmark in _world.Landmarks)
            {
                var position = MapOrigin + new Vector2(
                    (landmark.CenterTileX + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize,
                    (landmark.CenterTileY + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize);
                var region = _world.RegionAtTile(landmark.CenterTileX, landmark.CenterTileY);
                if (region is null || !RegionVisible(region.Id)) continue;
                DrawString(ThemeDB.FallbackFont, position, landmark.Name,
                    HorizontalAlignment.Left, -1, 8, new Color("c7d7df"));
            }
        foreach (var caravan in _economy.Caravans)
        {
            var region = _world.Region(caravan.CurrentRegionId);
            if (region is null || !RegionVisible(region.Id)) continue;
            var center = MapOrigin + new Vector2(
                (region.CenterTileX + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize,
                (region.CenterTileY + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize);
            DrawCircle(center, Math.Max(2f, cell * 0.16f), new Color("f4b860"));
        }
        foreach (var shipment in _settlementWorld.Shipments.VisibleShipments(RegionVisible))
        {
            var region = _world.Region(shipment.CurrentRegionId);
            if (region is null) continue;
            var center = MapOrigin + new Vector2(
                (region.CenterTileX + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize,
                (region.CenterTileY + 0.5f) / StrategicWorldRuntime.TileDimension * MapSize);
            var color = shipment.Direction == ShipmentDirection.Import
                ? new Color("69c5e8") : new Color("e89069");
            DrawRect(new Rect2(center - new Vector2(2.5f, 2.5f), new Vector2(5, 5)), color);
        }
        if (_fogTexture is not null)
            DrawTextureRect(_fogTexture, new Rect2(MapOrigin, new Vector2(MapSize, MapSize)), false);
        foreach (var entity in _settlementWorld.VisibleWorldEntities(RegionVisible))
        {
            if (entity.Kind is WorldEntityKind.Caravan or WorldEntityKind.ImportShipment or
                WorldEntityKind.ExportShipment) continue;
            var center = EntityPosition(entity);
            var color = entity.Kind switch
            {
                WorldEntityKind.Army => new Color("d95d4f"),
                WorldEntityKind.TouristParty => new Color("e8d56b"),
                WorldEntityKind.Haven => new Color("b989d6"),
                _ => new Color("f1eee4")
            };
            DrawCircle(center, entity.Kind == WorldEntityKind.Army ? 4.5f : 3f, color);
            DrawCircle(center, entity.Kind == WorldEntityKind.Army ? 6f : 4.5f,
                new Color(0, 0, 0, 0.8f), false, 1f);
        }
    }

    private void BuildRoadTexture()
    {
        var size = StrategicWorldRuntime.TileDimension;
        var pixels = new byte[size * size * 4];
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var region = _world.RegionAtTile(x, y);
            if (region is null || region.OwnerFactionId < 0) continue;
            var border = _world.RegionAtTile(x - 1, y)?.OwnerFactionId != region.OwnerFactionId ||
                         _world.RegionAtTile(x + 1, y)?.OwnerFactionId != region.OwnerFactionId ||
                         _world.RegionAtTile(x, y - 1)?.OwnerFactionId != region.OwnerFactionId ||
                         _world.RegionAtTile(x, y + 1)?.OwnerFactionId != region.OwnerFactionId;
            if (!border) continue;
            WritePixel(pixels, x + y * size, OriginalWorldPalette.Faction(region.OwnerFactionId), 220);
        }
        foreach (var tile in _world.RoadTiles)
        {
            var x = tile % size;
            var y = tile / size;
            var water = _world.Terrain.Water(x, y);
            var color = water is StrategicWaterKind.Ocean or StrategicWaterKind.DeepOcean or
                StrategicWaterKind.Lake or StrategicWaterKind.DeepLake
                ? new Color("d3b773") : new Color("715849");
            var offset = tile * 4;
            pixels[offset] = (byte)color.R8;
            pixels[offset + 1] = (byte)color.G8;
            pixels[offset + 2] = (byte)color.B8;
            pixels[offset + 3] = 255;
        }
        foreach (var site in _world.RuralSites)
        {
            var color = site.Kind == StrategicRuralSiteKind.Farm
                ? new Color("b69a55") : new Color("d4c28a");
            var offset = (site.TileX + site.TileY * size) * 4;
            pixels[offset] = (byte)color.R8;
            pixels[offset + 1] = (byte)color.G8;
            pixels[offset + 2] = (byte)color.B8;
            pixels[offset + 3] = 255;
        }
        foreach (var haven in _world.HavenPlacements)
        {
            var color = new Color("b989d6");
            var offset = (haven.TileX + haven.TileY * size) * 4;
            pixels[offset] = (byte)color.R8;
            pixels[offset + 1] = (byte)color.G8;
            pixels[offset + 2] = (byte)color.B8;
            pixels[offset + 3] = 255;
        }
        var image = Image.CreateFromData(size, size, false, Image.Format.Rgba8, pixels);
        if (_roadTexture is null) _roadTexture = ImageTexture.CreateFromImage(image);
        else _roadTexture.Update(image);
    }

    private static void WritePixel(byte[] pixels, int index, Color color, byte alpha)
    {
        var offset = index * 4;
        pixels[offset] = (byte)color.R8; pixels[offset + 1] = (byte)color.G8;
        pixels[offset + 2] = (byte)color.B8; pixels[offset + 3] = alpha;
    }

    private void DrawOriginalTown(StrategicCity city, Vector2 center)
    {
        var race = _world.Faction(city.FactionId)?.Race ?? "Normal";
        var sheet = race.Equals("HUMAN", StringComparison.OrdinalIgnoreCase) ? "Human" :
            race.Equals("CRETONIAN", StringComparison.OrdinalIgnoreCase) ? "Cretonian" :
            race.Equals("GARTHIMI", StringComparison.OrdinalIgnoreCase) ? "Garthimi" : "Normal";
        if (!_townTextures.TryGetValue(sheet, out var texture))
        {
            texture = GD.Load<Texture2D>($"res://Data/Original/assets/sprite/world/centre/town/{sheet}.png");
            _townTextures[sheet] = texture;
        }
        if (texture is null) return;
        var dimension = city.Capital ? 4 : 2;
        const float spriteSize = 5f;
        for (var y = 0; y < dimension; y++)
        for (var x = 0; x < dimension; x++)
        {
            var variant = (int)((uint)((city.Id * 7 + x) * 73856093 ^
                (city.RegionId * 11 + y) * 19349663) % 16u);
            var source = new Rect2(6 + variant * 14, 6 + (city.Capital ? 3 : 1) * 14, 8, 8);
            var destination = new Rect2(center.X + (x - dimension / 2f) * spriteSize,
                center.Y + (y - dimension / 2f) * spriteSize, spriteSize, spriteSize);
            DrawTextureRectRegion(texture, destination, source);
        }
    }

    private void BuildLayerTexture()
    {
        if (_world is null || _activeLayer == WorldLayer.Terrain)
        {
            _layerTexture = null;
            return;
        }
        var size = StrategicWorldRuntime.TileDimension;
        var pixels = new byte[size * size * 4];
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var region = _world.RegionAtTile(x, y);
            if (region is null) continue;
            var color = RegionColor(region);
            var offset = (x + y * size) * 4;
            pixels[offset] = (byte)color.R8;
            pixels[offset + 1] = (byte)color.G8;
            pixels[offset + 2] = (byte)color.B8;
            pixels[offset + 3] = (byte)color.A8;
        }
        var image = Image.CreateFromData(size, size, false, Image.Format.Rgba8, pixels);
        if (_layerTexture is null) _layerTexture = ImageTexture.CreateFromImage(image);
        else _layerTexture.Update(image);
    }

    private bool RegionVisible(int regionId)
    {
        var region = _world.Region(regionId);
        if (region is null) return false;
        if (region.OwnerFactionId == _world.PlayerFactionId) return true;
        return _world.Roads.Any(road =>
            road.FirstRegionId == regionId && _world.Region(road.SecondRegionId)?.OwnerFactionId == _world.PlayerFactionId ||
            road.SecondRegionId == regionId && _world.Region(road.FirstRegionId)?.OwnerFactionId == _world.PlayerFactionId);
    }

    private Vector2 EntityPosition(WorldEntityRecord entity)
    {
        var from = _world.Region(entity.CurrentRegionId);
        if (from is null) return MapOrigin;
        var point = new Vector2(from.CenterTileX + 0.5f, from.CenterTileY + 0.5f);
        if (entity.IsMoving && _world.Region(entity.NextRegionId) is { } to)
            point = point.Lerp(new Vector2(to.CenterTileX + 0.5f, to.CenterTileY + 0.5f),
                (float)Math.Clamp(entity.EdgeProgress, 0, 1));
        return MapOrigin + point / StrategicWorldRuntime.TileDimension * MapSize;
    }

    private void BuildFogTexture()
    {
        var size = StrategicWorldRuntime.TileDimension;
        var pixels = new byte[size * size * 4];
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var region = _world.RegionAtTile(x, y);
            if (region is not null && RegionVisible(region.Id)) continue;
            var offset = (x + y * size) * 4;
            pixels[offset] = 3; pixels[offset + 1] = 7; pixels[offset + 2] = 10;
            pixels[offset + 3] = 220;
        }
        var image = Image.CreateFromData(size, size, false, Image.Format.Rgba8, pixels);
        if (_fogTexture is null) _fogTexture = ImageTexture.CreateFromImage(image);
        else _fogTexture.Update(image);
    }

    private void RefreshSelection()
    {
        var region = _world.Region(_selectedRegion);
        if (region is null)
        {
            _details.Text = $"Регионов: {_world.Regions.Count}\nФракций: {_world.Factions.Count}\n" +
                $"Размер мира: {StrategicWorldRuntime.TileDimension}×{StrategicWorldRuntime.TileDimension}\n" +
                $"Средняя площадь региона: {StrategicWorldRuntime.AverageRegionArea}";
            _apply.Disabled = true;
            return;
        }
        var faction = _world.Faction(region.OwnerFactionId);
        var factionState = faction is null ? null : _settlementWorld.Factions.State(faction.Id);
        var king = faction is null ? null : _settlementWorld.Factions.King(faction.Id);
        var realmRegions = faction is null ? 0 : _world.Regions.Count(value => value.OwnerFactionId == faction.Id);
        var distance = faction is null ? -1 : _world.DistanceBetweenCapitals(_world.PlayerFactionId, faction.Id);
        var stance = faction is null ? DiplomacyStance.Neutral : _world.Stance(_world.PlayerFactionId, faction.Id);
        _details.Text = $"{region.Name} · регион #{region.Id} · {region.X}:{region.Y}\n" +
            $"Население: {region.Population} · климат {region.Climate}\n" +
            $"Плодородие {region.Fertility:P0} · влага {region.Moisture:P0} · вода {region.Water:P0}\n" +
            $"Столица: {(region.Capital ? "да" : "нет")}\n\n" +
            (faction is null ? "Владелец: нейтральная территория" :
                $"Фракция: {faction.Name}\nРаса: {faction.Race}\nРегионов в державе: {realmRegions}\n" +
                $"Расстояние до столицы: {distance}\nСтатус: {stance}\n" +
                $"Мнение {factionState?.Opinion:P0} · доверие {factionState?.Trust:P0}\n" +
                $"Двор {_settlementWorld.Factions.Court(faction.Id).Count}/{WorldFactionRuntime.MaximumCourtMembers}" +
                (king is null ? "" : $" · правитель #{king.Id}"));
        var economy = _economy.State(region.Id);
        var stock = economy is null ? "нет" : string.Join(", ", economy.Stock
            .Where(value => value.Value > 0).Take(8).Select(value => $"{value.Key} {value.Value}")
            .DefaultIfEmpty("пусто"));
        var races = economy is null ? "" : string.Join(", ", economy.RacePopulation
            .Where(value => value.Value > 0).OrderByDescending(value => value.Value).Take(4)
            .Select(value => $"{value.Key} {value.Value} ({economy.RaceLoyalty.GetValueOrDefault(value.Key):P0})"));
        var religions = economy is null ? "" : string.Join(", ", economy.Religions
            .OrderByDescending(value => value.Value).Take(3).Select(value => $"{value.Key} {value.Value:P0}"));
        _economyDetails.Text = economy is null ? "Региональная экономика отсутствует." :
            $"Население {economy.Population}/{economy.PopulationTarget} · рабочая сила " +
            $"{economy.Workforce} · лояльность {economy.Loyalty:P0}\n" +
            $"Здоровье {economy.Health:P0}/{economy.HealthTarget:P0}" +
            (economy.DiseaseOutbreak ? " · ЭПИДЕМИЯ" : "") +
            (economy.Besieged ? " · ОСАДА" : "") +
            $" · налоги {economy.TaxIncome:0} ({economy.AccruedTaxes:0}) · гарнизон {economy.Garrison:0} · " +
            $"укрепления {economy.Fortification:0}\n" +
            $"Дороги {economy.VisualRoads:P0} · стены {economy.VisualWalls:P0} · шахты {economy.VisualMines:P0}\n" +
            $"Расы: {races}\nРелигии: {religions} · оппозиция {economy.ReligiousOpposition:P0}\n" +
            $"Поддержка игрока {economy.PlayerSupport:P0} · налоговое давление {economy.TaxSqueeze:P0} · " +
            $"зданий {economy.Buildings.Count(value => value.Value > 0)}\n" +
            $"Склад: {stock}";
        var diplomacy = _settlementWorld.Diplomacy;
        _diplomacyStatus.Text = $"Эмиссары {diplomacy.Produced} − {diplomacy.Spent} = {diplomacy.Available}; " +
                                  $"эффективность {diplomacy.Efficiency:P0}. {diplomacy.LastResult}";
        LoadEdict();
        _apply.Disabled = faction is null || faction.Player;
        var option = Enumerable.Range(0, _stance.ItemCount).FirstOrDefault(index =>
            (int)_stance.GetItemMetadata(index) == (int)stance);
        _stance.Select(option);
        LoadMission();
        LoadBuildingLevel();
    }

    private void LoadBuildingLevel()
    {
        var state = _economy.State(_selectedRegion);
        if (state is null || _building.Selected < 0 || _building.Selected >= _buildingKeys.Count) return;
        var key = _buildingKeys[_building.Selected];
        _buildingLevel.MaxValue = _economy.Buildings.All[key].Levels.Count;
        _buildingLevel.Value = state.Buildings.GetValueOrDefault(key);
    }

    private void ApplyBuilding()
    {
        var region = _world.Region(_selectedRegion);
        if (region?.OwnerFactionId != _world.PlayerFactionId ||
            _building.Selected < 0 || _building.Selected >= _buildingKeys.Count) return;
        _economy.SetBuildingLevel(region.Id, _buildingKeys[_building.Selected], (int)_buildingLevel.Value);
        RefreshSelection();
    }

    private void ApplyEdict()
    {
        var region = _world.Region(_selectedRegion);
        var state = _economy.State(_selectedRegion);
        if (region?.OwnerFactionId != _world.PlayerFactionId || state is null) return;
        if (_edictRace.Selected < 0) return;
        _economy.SetEdict(region.Id, _edictRace.GetItemText(_edictRace.Selected),
            (RegionalEdict)_edict.Selected);
        RefreshSelection(); QueueRedraw();
    }

    private void LoadEdict()
    {
        var state = _economy.State(_selectedRegion);
        if (state is null || _edictRace.Selected < 0) return;
        _edict.Select((int)state.Edicts.GetValueOrDefault(_edictRace.GetItemText(_edictRace.Selected)));
    }

    private Color RegionColor(StrategicRegion region)
    {
        var economy = _economy.State(region.Id);
        return _activeLayer switch
        {
            WorldLayer.Fertility => Heat(region.Fertility, new Color("513d2c"), new Color("72b35a")),
            WorldLayer.Elevation => Heat(region.Elevation, new Color("24344a"), new Color("ddd5bd")),
            WorldLayer.Forest => Heat(region.Forest, new Color("6f7551"), new Color("173d28")),
            WorldLayer.Mountains => Heat(region.Mountain, new Color("343a3e"), new Color("c2c0b7")),
            WorldLayer.Ocean => Heat(region.Ocean, new Color("564f39"), new Color("315f91")),
            WorldLayer.Rivers => Heat(region.River * 8, new Color("4c5145"), new Color("48a3ca")),
            WorldLayer.Moisture => Heat(region.Moisture, new Color("c29b5b"), new Color("397fa5")),
            WorldLayer.Water => Heat(region.Water, new Color("40392f"), new Color("4c9bd1")),
            WorldLayer.Prospects => Heat(economy?.Prospects.Values.DefaultIfEmpty(0).Max() / 3.0 ?? 0,
                new Color("302c38"), new Color("d19b45")),
            WorldLayer.Loyalty => Heat(economy?.Loyalty ?? 0, new Color("9b3942"), new Color("4ea66a")),
            WorldLayer.Population => Heat((economy?.Population ?? 0) /
                (double)Math.Max(1, economy?.NaturalPopulationTarget ?? 1), new Color("252936"), new Color("f0c15d")),
            WorldLayer.Race => KeyColor(economy?.MajorityRace ?? ""),
            WorldLayer.Religion => KeyColor(economy?.Religions.OrderByDescending(value => value.Value)
                .FirstOrDefault().Key ?? ""),
            WorldLayer.ReligiousOpposition => Heat(economy?.ReligiousOpposition ?? 0,
                new Color("344c65"), new Color("c13f52")),
            _ => region.OwnerFactionId < 0 ? new Color("26333b") : FactionColor(region.OwnerFactionId)
        };
    }

    private static Color Heat(double value, Color low, Color high) => low.Lerp(high, (float)Math.Clamp(value, 0, 1));

    private void ApplyStance()
    {
        var region = _world.Region(_selectedRegion);
        var faction = region is null ? null : _world.Faction(region.OwnerFactionId);
        if (faction is null || faction.Player) return;
        var stance = (DiplomacyStance)(int)_stance.GetSelectedMetadata();
        var day = _settlementWorld.LastTick?.Day ?? 0;
        if (_settlementWorld.Diplomacy.TrySetStance(faction.Id, stance, day))
            _world.PublishTradeQuotes(_trade);
        RefreshSelection();
    }

    private void LoadMission()
    {
        var region = _world.Region(_selectedRegion);
        if (region is null || _mission.Selected < 0) return;
        var kind = (EmissaryMissionKind)_mission.Selected;
        var target = kind == EmissaryMissionKind.SupportRegion ? region.Id : region.OwnerFactionId;
        _missionPoints.Value = target < 0 ? 0 : _settlementWorld.Diplomacy.Allocation(kind, target);
    }

    private void ApplyMission()
    {
        var region = _world.Region(_selectedRegion);
        if (region is null || _mission.Selected < 0) return;
        var kind = (EmissaryMissionKind)_mission.Selected;
        var target = kind == EmissaryMissionKind.SupportRegion ? region.Id : region.OwnerFactionId;
        _settlementWorld.Diplomacy.SetAllocation(kind, target, (int)_missionPoints.Value);
        RefreshSelection();
    }

    private void SendGift()
    {
        var region = _world.Region(_selectedRegion);
        var faction = region is null ? null : _world.Faction(region.OwnerFactionId);
        if (faction is null || faction.Player) return;
        const double amount = WorldDiplomacyRuntime.GiftWorkdayCredits;
        if (_governance.Treasury.Balance < amount)
        {
            _diplomacyStatus.Text = "Для дара требуется 400 кредитов в казне.";
            return;
        }
        var day = _settlementWorld.LastTick?.Day ?? 0;
        if (!_settlementWorld.Diplomacy.ApplyGift(faction.Id, amount, day)) return;
        _governance.Treasury.Add(-amount, TreasuryCategory.Diplomacy, _trade);
        RefreshSelection();
    }

    private static Color FactionColor(int id)
        => OriginalWorldPalette.Faction(id);

    private static Color KeyColor(string key)
    {
        uint hash = 2166136261;
        foreach (var character in key) hash = (hash ^ character) * 16777619;
        return Color.FromHsv((hash % 1000) / 1000f, 0.55f, key.Length == 0 ? 0.22f : 0.78f);
    }

    private void BuildTerrainTexture()
    {
        _terrainTexture = OriginalWorldMapTextureBuilder.Build(_world);
    }

    private Label LabelAt(string text, float x, float y, int size)
    {
        var label = new Label { Text = text, Position = new Vector2(x, y) };
        label.AddThemeFontSizeOverride("font_size", size);
        AddChild(label);
        return label;
    }
}
