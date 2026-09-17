using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Bootstrap;
using GodotSyxPort.Citizens;
using GodotSyxPort.Core;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Settlement;
using GodotSyxPort.Stats;

namespace GodotSyxPort.UI;

/// <summary>
/// Godot side-panel adapter for the original UINobles, UIRooms, UIHomes, UIArmy,
/// UIHealth and UIFood settlement panels. All values and mutations are owned by the
/// corresponding simulation runtimes; this class contains presentation only.
/// </summary>
public sealed partial class SettlementCivicPanel : ColorRect
{
    private enum Page { Nobles, Rooms, Housing, Army, Health, Food }
    private static readonly ResourceKind[] Edible =
    {
        ResourceKind.Egg, ResourceKind.Fish, ResourceKind.Food, ResourceKind.Meat,
        ResourceKind.Mushroom, ResourceKind.Ration, ResourceKind.Fruit, ResourceKind.Vegetable
    };

    private readonly Dictionary<Page, List<Control>> _pages = new();
    private readonly List<int> _nobleIds = new(), _nobleCandidateIds = new();
    private readonly List<int> _roomIds = new(), _divisionIds = new(), _armyCitizenIds = new();
    private readonly List<int> _divisionMemberIds = new(), _healthIds = new();
    private readonly List<string> _officeKeys = new();
    private GridWorld _world = null!;
    private RoomSystem _rooms = null!;
    private CitizenSystem _citizens = null!;
    private ResourceLedger _resources = null!;
    private EconomyTracker _economy = null!;
    private SettlementStatsRuntime _stats = null!;
    private Page _page;
    private Label _title = null!, _summary = null!, _details = null!, _status = null!;
    private ItemList _primary = null!, _secondary = null!, _members = null!;
    private OptionButton _option = null!, _resource = null!, _socialClass = null!, _race = null!;
    private LineEdit _name = null!;
    private SpinBox _amount = null!;
    public event Action<GridCoord>? FocusRequested;

    public void Initialize(GridWorld world, RoomSystem rooms, CitizenSystem citizens,
        ResourceLedger resources, EconomyTracker economy, SettlementStatsRuntime stats)
    {
        _world = world;
        _rooms = rooms;
        _citizens = citizens;
        _resources = resources;
        _economy = economy;
        _stats = stats;
        Position = new Vector2(188, 70);
        Size = new Vector2(904, 574);
        Color = new Color("101410f8");
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;
        foreach (var page in Enum.GetValues<Page>()) _pages[page] = new List<Control>();
        _title = LabelAt("", 18, 12, 21);
        _title.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        var close = ButtonAt("×", 852, 10, 34);
        close.Pressed += () => Visible = false;
        _status = LabelAt("", 18, 542, 12);
        _status.AddThemeColorOverride("font_color", new Color("efc971"));
    }

    public void OpenNobles() => Open(Page.Nobles);
    public void OpenRooms() => Open(Page.Rooms);
    public void OpenHousing() => Open(Page.Housing);
    public void OpenArmy() => Open(Page.Army);
    public void OpenHealth() => Open(Page.Health);
    public void OpenFood() => Open(Page.Food);

    public void Refresh()
    {
        if (!Visible) return;
        switch (_page)
        {
            case Page.Nobles:
                var nobles = _rooms.Governance.Nobility;
                _summary.Text = $"Знать: {nobles.Active.Count}/{nobles.NobleCapacity}    " +
                                $"Ранги: {nobles.RanksAllocated}/{nobles.PromotionCapacity}";
                break;
            case Page.Rooms:
                _summary.Text = $"Помещений: {_rooms.All.Count}, действует: " +
                                $"{_rooms.All.Count(value => value.State == RoomState.Operational)}    " +
                                $"Работники: {_rooms.All.Sum(value => value.Employment.Employed)}/" +
                                $"{_rooms.All.Sum(value => value.Employment.Needed)}";
                break;
            case Page.Housing:
                _summary.Text = $"Размещено: {_rooms.Housing.Occupants}/{_rooms.Housing.Capacity}    " +
                                $"Бездомные: {Math.Max(0, _citizens.Count - _rooms.Housing.Occupants)}";
                break;
            case Page.Army:
                _summary.Text = $"Дивизии: {_rooms.Military.Divisions.Count}/" +
                                $"{GodotSyxPort.Military.SettlementMilitaryRuntime.DivisionsPerArmy}    " +
                                $"Рекруты: {_rooms.Military.Recruits}    Учебные места: " +
                                $"{_rooms.Military.TrainingRooms.Sum(value => value.Stations.Length)}    " +
                                $"Орудия: {_rooms.Military.Artillery.Count}";
                break;
            case Page.Health:
                _summary.Text = $"Эпидемия: {(_citizens.CurrentEpidemic.Length == 0 ? "—" : _citizens.CurrentEpidemic)}    " +
                                $"Больные: {_citizens.SickCount}    Раненые: {_citizens.InjuredCount}    " +
                                $"Лечебные места: {_rooms.Services.Available("DOCTOR")}/" +
                                $"{_rooms.Services.Total("DOCTOR")}";
                break;
            case Page.Food: RefreshFoodSummary(); break;
        }
    }

    private void Open(Page page)
    {
        _page = page;
        Visible = true;
        Rebuild(page);
    }

    private void Rebuild(Page page)
    {
        foreach (var controls in _pages.Values)
            foreach (var control in controls) control.QueueFree();
        foreach (var controls in _pages.Values) controls.Clear();
        _nobleIds.Clear(); _nobleCandidateIds.Clear(); _roomIds.Clear(); _divisionIds.Clear();
        _armyCitizenIds.Clear(); _divisionMemberIds.Clear(); _healthIds.Clear(); _officeKeys.Clear();
        _status.Text = "";
        switch (page)
        {
            case Page.Nobles: BuildNobles(); break;
            case Page.Rooms: BuildRooms(); break;
            case Page.Housing: BuildHousing(); break;
            case Page.Army: BuildArmy(); break;
            case Page.Health: BuildHealth(); break;
            case Page.Food: BuildFood(); break;
        }
    }

    private void BuildNobles()
    {
        _title.Text = "Знать";
        var nobles = _rooms.Governance.Nobility;
        _summary = PageLabel(Page.Nobles, 18, 54, 868, 42);
        _summary.Text = $"Знать: {nobles.Active.Count}/{nobles.NobleCapacity}    " +
                        $"Ранги: {nobles.RanksAllocated}/{nobles.PromotionCapacity}";
        _primary = PageList(Page.Nobles, 18, 104, 420, 366);
        foreach (var noble in nobles.Active.OrderBy(value => value.CitizenId))
        {
            var person = _citizens.InspectCitizen(noble.CitizenId);
            var rankNames = OriginalGameData.Current.NobleRankNames;
            var rank = noble.Rank < rankNames.Count ? rankNames[noble.Rank] : $"Ранг {noble.Rank + 1}";
            _primary.AddItem($"{person?.Name ?? $"#{noble.CitizenId}"} · {rank} · " +
                             $"{(noble.OfficeKey.Length == 0 ? "без должности" : noble.OfficeKey)}");
            _nobleIds.Add(noble.CitizenId);
        }
        _primary.ItemSelected += index => ShowNoble((int)index);
        _secondary = PageList(Page.Nobles, 456, 104, 430, 174);
        foreach (var person in _citizens.InspectCitizens().Where(value =>
                     value.Type == HumanoidType.Subject).OrderBy(value => value.Id))
        {
            _secondary.AddItem($"{person.Name} · {person.Race}");
            _nobleCandidateIds.Add(person.Id);
        }
        _option = PageOption(Page.Nobles, 456, 290, 260);
        _option.AddItem("Без должности");
        _officeKeys.Add("");
        foreach (var office in nobles.Offices.OrderBy(value => value.Key))
        {
            _option.AddItem($"{office.Key} · назначений {nobles.Allocations(office.Key)}");
            _officeKeys.Add(office.Key);
        }
        var appoint = PageButton(Page.Nobles, "Возвести выбранного", 456, 334, 205);
        appoint.Pressed += AppointNoble;
        var assign = PageButton(Page.Nobles, "Назначить должность", 674, 334, 212);
        assign.Pressed += AssignNobleOffice;
        var promote = PageButton(Page.Nobles, "Повысить ранг", 456, 382, 205);
        promote.Pressed += PromoteNoble;
        var dismiss = PageButton(Page.Nobles, "Лишить титула", 674, 382, 212);
        dismiss.Pressed += DismissNoble;
        var focus = PageButton(Page.Nobles, "Показать на карте", 456, 430, 205);
        focus.Pressed += () => FocusPerson(SelectedId(_primary, _nobleIds));
        if (_nobleIds.Count > 0) ShowNoble(0);
    }

    private void ShowNoble(int index)
    {
        if ((uint)index >= (uint)_nobleIds.Count) return;
        var id = _nobleIds[index];
        var noble = _rooms.Governance.Nobility.Active.First(value => value.CitizenId == id);
        var person = _citizens.InspectCitizen(id);
        _status.Text = $"{person?.Name ?? $"#{id}"} · ранг {noble.Rank + 1} · " +
                       $"{(noble.OfficeKey.Length == 0 ? "должность не назначена" : noble.OfficeKey)}";
        var officeIndex = Math.Max(0, _officeKeys.FindIndex(value =>
            value.Equals(noble.OfficeKey, StringComparison.OrdinalIgnoreCase)));
        _option.Select(officeIndex);
    }

    private void AppointNoble()
    {
        var id = SelectedId(_secondary, _nobleCandidateIds);
        var office = Selected(_option, _officeKeys);
        var message = id > 0 && _citizens.AppointNoble(id, office)
            ? "Новый дворянин назначен." : "Назначение невозможно.";
        Rebuild(Page.Nobles);
        _status.Text = message;
    }

    private void AssignNobleOffice()
    {
        var id = SelectedId(_primary, _nobleIds);
        var office = Selected(_option, _officeKeys);
        var message = id > 0 && office.Length > 0 && _rooms.Governance.Nobility.SetOffice(id, office)
            ? "Должность назначена." : "Выберите дворянина и должность.";
        Rebuild(Page.Nobles);
        _status.Text = message;
    }

    private void PromoteNoble()
    {
        var id = SelectedId(_primary, _nobleIds);
        var message = id > 0 && _citizens.PromoteNoble(id) ? "Ранг повышен." : "Повышение недоступно.";
        Rebuild(Page.Nobles);
        _status.Text = message;
    }

    private void DismissNoble()
    {
        var id = SelectedId(_primary, _nobleIds);
        var message = id > 0 && _citizens.DismissNoble(id) ? "Титул снят." : "Выберите дворянина.";
        Rebuild(Page.Nobles);
        _status.Text = message;
    }

    private void BuildRooms()
    {
        _title.Text = "Рабочая сила и помещения";
        var operational = _rooms.All.Count(value => value.State == RoomState.Operational);
        var employed = _rooms.All.Sum(value => value.Employment.Employed);
        var needed = _rooms.All.Sum(value => value.Employment.Needed);
        _summary = PageLabel(Page.Rooms, 18, 54, 868, 42);
        _summary.Text = $"Помещений: {_rooms.All.Count}, действует: {operational}    Работники: {employed}/{needed}";
        _primary = PageList(Page.Rooms, 18, 104, 520, 414);
        foreach (var room in _rooms.All.OrderBy(value => value.DefinitionKey).ThenBy(value => value.Id))
        {
            _primary.AddItem($"#{room.Id} {room.DefinitionKey} · {room.State} · " +
                             $"работники {room.Employment.Employed}/{room.Employment.Needed} · " +
                             $"эфф. {room.Employment.TotalEfficiency:P0} · износ {room.Degradation:P0}");
            _roomIds.Add(room.Id);
        }
        _details = PageLabel(Page.Rooms, 556, 104, 330, 270);
        _primary.ItemSelected += index => ShowRoom((int)index);
        var focus = PageButton(Page.Rooms, "Показать помещение", 556, 390, 240);
        focus.Pressed += FocusRoom;
        if (_roomIds.Count > 0) ShowRoom(0);
    }

    private void ShowRoom(int index)
    {
        if ((uint)index >= (uint)_roomIds.Count) return;
        var room = _rooms.All.First(value => value.Id == _roomIds[index]);
        _details.Text = $"{room.DefinitionKey} #{room.Id}\n\nСостояние: {room.State}\nПлощадь: {room.Cells.Count}\n" +
                        $"Работники: {room.Employment.Employed}/{room.Employment.Needed} (макс. {room.Employment.Maximum})\n" +
                        $"Эффективность: {room.Employment.TotalEfficiency:P0}\nИзнос: {room.Degradation:P0}\n" +
                        $"Улучшение: {room.UpgradeLevel}\nИнструменты: {room.ToolUnits}";
    }

    private void FocusRoom()
    {
        var id = SelectedId(_primary, _roomIds);
        var room = _rooms.All.FirstOrDefault(value => value.Id == id);
        if (room?.Cells.Count > 0) FocusRequested?.Invoke(_world.FromIndex(room.Cells.First()));
    }

    private void BuildHousing()
    {
        _title.Text = "Жильё";
        var homes = _rooms.Housing.Homes.OrderBy(value => value.RoomId).ToArray();
        _summary = PageLabel(Page.Housing, 18, 54, 868, 42);
        _summary.Text = $"Размещено: {_rooms.Housing.Occupants}/{_rooms.Housing.Capacity}    " +
                        $"Бездомные: {Math.Max(0, _citizens.Count - _rooms.Housing.Occupants)}";
        _primary = PageList(Page.Housing, 18, 104, 432, 286);
        foreach (var home in homes)
            _primary.AddItem($"Дом #{home.RoomId} · жильцы {home.Occupants.Count}/{home.Capacity} · " +
                             $"изоляция {home.Isolation:P0}{(home.NobleOnly ? " · знать" : "")}");
        _details = PageLabel(Page.Housing, 468, 104, 418, 180);
        _details.Text = "Нормы обстановки задаются отдельно для каждой расы и класса, как в UIHomesFurniture оригинала.";
        _race = PageOption(Page.Housing, 468, 300, 180);
        foreach (var value in OriginalGameData.Current.Races.Keys.OrderBy(value => value)) _race.AddItem(value);
        _socialClass = PageOption(Page.Housing, 664, 300, 150);
        _socialClass.AddItem("Граждане", (int)SocialClass.Citizen);
        _socialClass.AddItem("Рабы", (int)SocialClass.Slave);
        _socialClass.AddItem("Знать", (int)SocialClass.Noble);
        _resource = PageOption(Page.Housing, 468, 346, 180);
        foreach (var value in Enum.GetValues<ResourceKind>()) _resource.AddItem(value.ToString(), (int)value);
        _amount = PageSpin(Page.Housing, 664, 346, 150, 0, 255, 1);
        var apply = PageButton(Page.Housing, "Установить норму", 468, 398, 220);
        apply.Pressed += SetFurnitureTarget;
        var focus = PageButton(Page.Housing, "Показать выбранный дом", 18, 406, 250);
        focus.Pressed += () =>
        {
            var index = _primary.GetSelectedItems().FirstOrDefault();
            if ((uint)index < (uint)homes.Length) FocusRequested?.Invoke(homes[index].ServiceCell);
        };
    }

    private void SetFurnitureTarget()
    {
        if (_race.ItemCount == 0) return;
        var race = _race.GetItemText(_race.Selected);
        var socialClass = (SocialClass)_socialClass.GetItemId(_socialClass.Selected);
        var resource = (ResourceKind)_resource.GetItemId(_resource.Selected);
        _rooms.Housing.SetTarget(race, socialClass, resource, (int)_amount.Value);
        var maximum = _rooms.Housing.Residents.Where(value => value.Race.Equals(race,
                StringComparison.OrdinalIgnoreCase) && value.Class == socialClass)
            .Select(value => _rooms.Housing.Maximum(value.CitizenId, resource)).DefaultIfEmpty(0).Max();
        _status.Text = $"Норма {resource} для {race}/{socialClass}: {Math.Min((int)_amount.Value, maximum)}/{maximum}.";
    }

    private void BuildArmy()
    {
        _title.Text = "Призывники";
        var army = _rooms.Military;
        _summary = PageLabel(Page.Army, 18, 54, 868, 42);
        var trainers = army.TrainingRooms.Sum(value => value.Stations.Length);
        _summary.Text = $"Дивизии: {army.Divisions.Count}/{GodotSyxPort.Military.SettlementMilitaryRuntime.DivisionsPerArmy}    " +
                        $"Рекруты: {army.Recruits}    Учебные места: {trainers}    Орудия: {army.Artillery.Count}";
        _primary = PageList(Page.Army, 18, 104, 418, 220);
        foreach (var division in army.Divisions.OrderBy(value => value.Id))
        {
            _primary.AddItem($"{division.Name} · {division.Race} · {division.Members.Count}/{division.TargetMen}");
            _divisionIds.Add(division.Id);
        }
        _primary.ItemSelected += index => SelectDivision((int)index);
        _name = PageLine(Page.Army, 18, 338, 248, "Название дивизии");
        _amount = PageSpin(Page.Army, 278, 338, 158, 0,
            GodotSyxPort.Military.SettlementMilitaryRuntime.MenPerDivision, 10);
        var create = PageButton(Page.Army, "Создать", 18, 386, 128);
        create.Pressed += CreateDivision;
        var save = PageButton(Page.Army, "Применить", 154, 386, 128);
        save.Pressed += ConfigureDivision;
        var disband = PageButton(Page.Army, "Расформировать", 290, 386, 146);
        disband.Pressed += DisbandDivision;
        _secondary = PageList(Page.Army, 454, 104, 432, 172);
        foreach (var person in _citizens.InspectCitizens().Where(value => value.Type == HumanoidType.Subject))
        {
            _secondary.AddItem($"{person.Name} · {person.Race}");
            _armyCitizenIds.Add(person.Id);
        }
        var enlist = PageButton(Page.Army, "Зачислить в дивизию", 454, 288, 210);
        enlist.Pressed += Enlist;
        _details = PageLabel(Page.Army, 454, 338, 432, 48);
        _divisionMemberIds.Clear();
        _members = PageList(Page.Army, 454, 396, 432, 122);
        _members.ItemSelected += index =>
        {
            if ((uint)index < (uint)_divisionMemberIds.Count)
                FocusPerson(_divisionMemberIds[(int)index]);
        };
        if (_divisionIds.Count > 0) SelectDivision(0);
        var discharge = PageButton(Page.Army, "Уволить выбранного бойца", 18, 444, 264);
        discharge.Pressed += Discharge;
    }

    private void SelectDivision(int index)
    {
        if ((uint)index >= (uint)_divisionIds.Count) return;
        var division = _rooms.Military.Divisions.First(value => value.Id == _divisionIds[index]);
        _name.Text = division.Name;
        _amount.Value = division.TargetMen;
        _details.Text = "Подготовка: " + string.Join(", ", division.TrainingTargets.Select(value =>
            $"{value.Key} {value.Value:P0}")) + "\nСнаряжение: " + string.Join(", ",
            division.IssuedEquipment.Select(value => $"{value.Key} {value.Value}"));
        _members.Clear(); _divisionMemberIds.Clear();
        foreach (var id in division.Members.OrderBy(value => value))
        {
            var person = _citizens.InspectCitizen(id);
            _members.AddItem(person?.Name ?? $"#{id}");
            _divisionMemberIds.Add(id);
        }
    }

    private void CreateDivision()
    {
        var race = GameSession.Current?.PlayerRace ?? OriginalGameData.Current.Races.Keys.FirstOrDefault() ?? "HUMAN";
        var id = _citizens.CreateDivision(race, _name.Text);
        var message = id > 0 ? $"Дивизия #{id} создана." : "Лимит дивизий достигнут.";
        Rebuild(Page.Army);
        _status.Text = message;
    }

    private void ConfigureDivision()
    {
        var id = SelectedId(_primary, _divisionIds);
        var message = id > 0 && _rooms.Military.ConfigureDivision(id, _name.Text, (int)_amount.Value)
            ? "Параметры дивизии применены." : "Выберите дивизию.";
        Rebuild(Page.Army);
        _status.Text = message;
    }

    private void DisbandDivision()
    {
        var id = SelectedId(_primary, _divisionIds);
        if (id <= 0) return;
        var division = _rooms.Military.Divisions.FirstOrDefault(value => value.Id == id);
        if (division is not null)
            foreach (var citizenId in division.Members.ToArray()) _citizens.DischargeCitizen(citizenId);
        _rooms.Military.RemoveDivision(id);
        Rebuild(Page.Army);
    }

    private void Enlist()
    {
        var divisionId = SelectedId(_primary, _divisionIds);
        var citizenId = SelectedId(_secondary, _armyCitizenIds);
        var message = divisionId > 0 && citizenId > 0 && _citizens.EnlistCitizen(citizenId, divisionId)
            ? "Рекрут зачислен." : "Зачисление невозможно.";
        Rebuild(Page.Army);
        _status.Text = message;
    }

    private void Discharge()
    {
        var id = SelectedId(_members, _divisionMemberIds);
        var message = id > 0 && _citizens.DischargeCitizen(id) ? "Боец уволен." : "Выберите бойца.";
        Rebuild(Page.Army);
        _status.Text = message;
    }

    private void BuildHealth()
    {
        _title.Text = "Здоровье";
        var hospitalAvailable = _rooms.Services.Available("DOCTOR");
        var hospitalTotal = _rooms.Services.Total("DOCTOR");
        _summary = PageLabel(Page.Health, 18, 54, 868, 64);
        _summary.Text = $"Эпидемия: {(_citizens.CurrentEpidemic.Length == 0 ? "—" : _citizens.CurrentEpidemic)}    " +
                        $"Больные: {_citizens.SickCount}    Раненые: {_citizens.InjuredCount}    " +
                        $"Лечебные места: {hospitalAvailable}/{hospitalTotal}";
        _primary = PageList(Page.Health, 18, 126, 470, 392);
        foreach (var person in _citizens.InspectCitizens().Where(value =>
                     value.Health != "Здоров").OrderBy(value => value.Id))
        {
            _primary.AddItem($"{person.Name} · {person.Health} · травма {person.Injury}");
            _healthIds.Add(person.Id);
        }
        _details = PageLabel(Page.Health, 506, 126, 380, 270);
        _details.Text = "Известные болезни\n\n" + string.Join("\n", OriginalGameData.Current.Diseases.Values
            .OrderBy(value => value.Key).Select(value =>
                $"{value.Key}: распространение {value.Spread:P0}, смертность {value.FatalityRate:P0}, " +
                $"болезнь {value.InfectionDays} дн."));
        var focus = PageButton(Page.Health, "Показать больного", 506, 414, 220);
        focus.Pressed += () => FocusPerson(SelectedId(_primary, _healthIds));
    }

    private void BuildFood()
    {
        _title.Text = "Еда";
        _summary = PageLabel(Page.Food, 18, 54, 868, 88);
        RefreshFoodSummary();
        var latest = _economy.Latest;
        var produced = latest?.Produced ?? new int[ResourceLedger.KindCount];
        var consumed = latest?.Consumed ?? new int[ResourceLedger.KindCount];
        _primary = PageList(Page.Food, 18, 152, 868, 366);
        foreach (var resource in Edible)
        {
            var venue = _rooms.FoodVenues.Instances.Where(value =>
                resource == ResourceKind.Food && value.RoomKey != "TAVERN_NORMAL").Sum(value => value.Stock);
            _primary.AddItem($"{resource} · хранится {_resources.Get(resource) + venue} · " +
                             $"+{produced[(int)resource]} / −{consumed[(int)resource]} за 60 сек");
        }
    }

    private void RefreshFoodSummary()
    {
        var latest = _economy.Latest;
        var produced = latest?.Produced ?? new int[ResourceLedger.KindCount];
        var consumed = latest?.Consumed ?? new int[ResourceLedger.KindCount];
        var stored = Edible.Sum(_resources.Get) + _rooms.FoodVenues.Instances.Where(value =>
            value.RoomKey != "TAVERN_NORMAL").Sum(value => value.Stock);
        _summary.Text = $"Производство: +{Edible.Sum(value => produced[(int)value])}/60 сек    " +
                        $"Потребление: −{Edible.Sum(value => consumed[(int)value])}/60 сек    " +
                        $"Хранится: {stored}\nЗапас: {_stats.FoodDays:0.0} дней    Голодают: {_stats.Starving}";
    }

    private void FocusPerson(int id)
    {
        var person = id <= 0 ? null : _citizens.InspectCitizen(id);
        if (person is not null) FocusRequested?.Invoke(person.Cell);
    }

    private T AddPage<T>(Page page, T control) where T : Control
    {
        AddChild(control); _pages[page].Add(control); return control;
    }

    private Label PageLabel(Page page, float x, float y, float width, float height)
    {
        var label = new Label
        {
            Position = new Vector2(x, y), Size = new Vector2(width, height),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        label.AddThemeFontSizeOverride("font_size", 14);
        return AddPage(page, label);
    }

    private ItemList PageList(Page page, float x, float y, float width, float height) =>
        AddPage(page, new ItemList { Position = new Vector2(x, y), Size = new Vector2(width, height) });

    private Button PageButton(Page page, string text, float x, float y, float width) =>
        AddPage(page, new Button { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, 38) });

    private OptionButton PageOption(Page page, float x, float y, float width) =>
        AddPage(page, new OptionButton { Position = new Vector2(x, y), Size = new Vector2(width, 38) });

    private SpinBox PageSpin(Page page, float x, float y, float width, double min, double max, double step) =>
        AddPage(page, new SpinBox
        {
            Position = new Vector2(x, y), Size = new Vector2(width, 38), MinValue = min,
            MaxValue = max, Step = step, AllowGreater = false, AllowLesser = false
        });

    private LineEdit PageLine(Page page, float x, float y, float width, string placeholder) =>
        AddPage(page, new LineEdit
        {
            Position = new Vector2(x, y), Size = new Vector2(width, 38), PlaceholderText = placeholder
        });

    private Label LabelAt(string text, float x, float y, int size)
    {
        var label = new Label { Text = text, Position = new Vector2(x, y) };
        label.AddThemeFontSizeOverride("font_size", size); AddChild(label); return label;
    }

    private Button ButtonAt(string text, float x, float y, float width)
    {
        var button = new Button { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, 34) };
        AddChild(button); return button;
    }

    private static int SelectedId(ItemList list, IReadOnlyList<int> ids)
    {
        var index = list.GetSelectedItems().FirstOrDefault();
        return (uint)index < (uint)ids.Count ? ids[index] : 0;
    }

    private static string Selected(OptionButton option, IReadOnlyList<string> values) =>
        (uint)option.Selected < (uint)values.Count ? values[option.Selected] : "";
}
