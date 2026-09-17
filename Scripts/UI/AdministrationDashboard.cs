using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotSyxPort.Citizens;
using GodotSyxPort.Data;
using GodotSyxPort.Resources;
using GodotSyxPort.Rooms;
using GodotSyxPort.Stats;

namespace GodotSyxPort.UI;

/// <summary>Godot-native demographic, law, technology and trade administration surface.</summary>
public sealed partial class AdministrationDashboard : ColorRect
{
    private enum Page { Overview, Standing, Citizens, Law, Technology, Trade }
    private readonly Dictionary<Page, List<Control>> _pages = new();
    private readonly List<string> _races = new(), _crimes = new(), _punishments = new(), _technologies = new();
    private RoomSystem _rooms = null!;
    private CitizenSystem _citizens = null!;
    private SettlementStatsRuntime? _stats;
    private Page _page;
    private Label _overview = null!, _standingSummary = null!, _lawSummary = null!, _techSummary = null!, _tradeSummary = null!, _status = null!;
    private OptionButton _race = null!, _class = null!, _crime = null!, _punishment = null!;
    private OptionButton _citizenClassFilter = null!;
    private OptionButton _standingClass = null!;
    private OptionButton _technology = null!, _resource = null!;
    private SpinBox _importLevel = null!, _importPrice = null!, _retain = null!, _exportPrice = null!;
    private ItemList _citizenList = null!, _caseList = null!;
    private Label _citizenDetails = null!, _caseDetails = null!;
    private readonly List<int> _citizenIds = new(), _caseCitizenIds = new();
    public event Action<GodotSyxPort.Core.GridCoord>? CitizenFocusRequested;

    public void ShowOverview() => ShowPage(Page.Overview);
    public void ShowCitizens() { _standingClass.Select((int)SocialClass.Citizen); ShowPage(Page.Standing); }
    public void ShowSlaves() { _standingClass.Select((int)SocialClass.Slave); ShowPage(Page.Standing); }
    public void ShowSubjects() { _citizenClassFilter.Select(0); ShowPage(Page.Citizens); }
    public void ShowLaw() => ShowPage(Page.Law);
    public void ShowTechnology() => ShowPage(Page.Technology);
    public void ShowTrade() => ShowPage(Page.Trade);

    public void Initialize(RoomSystem rooms, CitizenSystem citizens)
    {
        _rooms = rooms;
        _citizens = citizens;
        Color = new Color(0.025f, 0.035f, 0.045f, 0.97f);
        Position = new Vector2(240, 80);
        Size = new Vector2(800, 560);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;
        var title = AddLabel("Управление поселением", new Vector2(18, 14), 20);
        title.AddThemeColorOverride("font_color", new Color("f2dfaa"));
        AddLabel("F2 — закрыть", new Vector2(690, 18), 12)
            .AddThemeColorOverride("font_color", new Color("93a5aa"));
        AddPageButton("Сводка", Page.Overview, 18);
        AddPageButton("Благополучие", Page.Standing, 128);
        AddPageButton("Подданные", Page.Citizens, 238);
        AddPageButton("Закон", Page.Law, 348);
        AddPageButton("Технологии", Page.Technology, 458);
        AddPageButton("Торговля", Page.Trade, 568);
        foreach (var page in Enum.GetValues<Page>()) _pages[page] = new List<Control>();
        BuildOverview(); BuildStanding(); BuildCitizens(); BuildLaw(); BuildTechnology(); BuildTrade();
        _status = AddLabel("", new Vector2(18, 526), 12);
        _status.AddThemeColorOverride("font_color", new Color("efc971"));
        ShowPage(Page.Overview);
    }

    public void Refresh(RoomSystem rooms, SettlementStatsRuntime stats)
    {
        _stats = stats;
        if (Visible) RefreshCurrentPage();
    }

    private void BuildOverview() =>
        _overview = PageLabel(Page.Overview, new Vector2(18, 98), new Vector2(764, 414), 14);

    private void BuildStanding()
    {
        _standingClass = PageOption(Page.Standing, 18, 102, 210);
        foreach (var value in Enum.GetValues<SocialClass>()) _standingClass.AddItem(value.ToString());
        _standingClass.Select((int)SocialClass.Citizen);
        _standingClass.ItemSelected += _ => RefreshStanding();
        _standingSummary = PageLabel(Page.Standing, new Vector2(18, 150), new Vector2(764, 356), 14);
    }

    private void BuildCitizens()
    {
        _citizenClassFilter = PageOption(Page.Citizens, 18, 102, 210);
        _citizenClassFilter.AddItem("Все подданные");
        _citizenClassFilter.AddItem("Граждане");
        _citizenClassFilter.AddItem("Рабы");
        _citizenClassFilter.ItemSelected += _ => RefreshCitizens();
        _citizenList = PageItemList(Page.Citizens, 18, 140, 340, 346);
        _citizenList.ItemSelected += SelectCitizen;
        _citizenDetails = PageLabel(Page.Citizens, new Vector2(374, 104), new Vector2(408, 330), 14);
        var focus = PageButton(Page.Citizens, "Показать на карте", 374, 450, 190);
        focus.Pressed += FocusCitizen;
    }

    private void BuildLaw()
    {
        _race = PageOption(Page.Law, 18, 110, 170);
        _class = PageOption(Page.Law, 198, 110, 130);
        foreach (var value in Enum.GetValues<SocialClass>()) _class.AddItem(value.ToString());
        _class.Select((int)SocialClass.Citizen);
        _class.ItemSelected += _ => RefreshLaw();
        _crime = PageOption(Page.Law, 338, 110, 190);
        _punishment = PageOption(Page.Law, 538, 110, 180);
        _race.ItemSelected += _ => SelectCurrentDecree();
        _crime.ItemSelected += _ => SelectCurrentDecree();
        var apply = PageButton(Page.Law, "Назначить наказание", 18, 150, 220);
        apply.Pressed += ApplyDecree;
        _lawSummary = PageLabel(Page.Law, new Vector2(18, 196), new Vector2(764, 72), 14);
        _caseList = PageItemList(Page.Law, 18, 278, 360, 228);
        _caseList.ItemSelected += SelectCase;
        _caseDetails = PageLabel(Page.Law, new Vector2(394, 278), new Vector2(388, 228), 14);
        var focusCase = PageButton(Page.Law, "Показать обвиняемого", 394, 476, 210);
        focusCase.Pressed += FocusCaseCitizen;
    }

    private void BuildTechnology()
    {
        _technology = PageOption(Page.Technology, 18, 110, 520);
        _technologies.AddRange(OriginalGameData.Current.Technologies.Keys
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
        foreach (var key in _technologies) _technology.AddItem(key);
        var unlock = PageButton(Page.Technology, "Открыть следующий уровень", 548, 110, 230);
        unlock.Pressed += UnlockTechnology;
        _technology.ItemSelected += _ => RefreshTechnology();
        _techSummary = PageLabel(Page.Technology, new Vector2(18, 154), new Vector2(764, 352), 14);
    }

    private void BuildTrade()
    {
        _resource = PageOption(Page.Trade, 18, 110, 210);
        foreach (var resource in Enum.GetValues<ResourceKind>()) _resource.AddItem(resource.ToString());
        _resource.ItemSelected += _ => LoadTradePolicy();
        _importLevel = PageSpin(Page.Trade, "Импорт, %", 18, 164, 0, 100, 5);
        _importPrice = PageSpin(Page.Trade, "Макс. цена импорта", 208, 164, 0, 1_000_000, 10);
        _retain = PageSpin(Page.Trade, "Оставлять на складе, %", 398, 164, 0, 100, 5);
        _exportPrice = PageSpin(Page.Trade, "Мин. цена экспорта", 588, 164, 1, 1_000_000, 10);
        var apply = PageButton(Page.Trade, "Применить политику", 18, 232, 220);
        apply.Pressed += ApplyTradePolicy;
        _tradeSummary = PageLabel(Page.Trade, new Vector2(18, 278), new Vector2(764, 228), 14);
        LoadTradePolicy();
    }

    private void ShowPage(Page page)
    {
        _page = page;
        foreach (var pair in _pages)
            foreach (var control in pair.Value) control.Visible = pair.Key == page;
        RefreshCurrentPage();
    }

    private void RefreshCurrentPage()
    {
        if (_stats is null) return;
        if (_page == Page.Overview) RefreshOverview();
        else if (_page == Page.Standing) RefreshStanding();
        else if (_page == Page.Citizens) RefreshCitizens();
        else if (_page == Page.Law) RefreshLaw();
        else if (_page == Page.Technology) RefreshTechnology();
        else RefreshTrade();
    }

    private void RefreshCitizens()
    {
        var selectedClass = _citizenClassFilter.Selected;
        var population = _citizens.EventPopulation().Where(value => selectedClass == 0 ||
                value.Class == (selectedClass == 1 ? SocialClass.Citizen : SocialClass.Slave))
            .OrderBy(value => value.Id).ToArray();
        var ids = population.Select(value => value.Id).ToArray();
        if (!_citizenIds.SequenceEqual(ids))
        {
            var selectedId = SelectedCitizenId();
            _citizenIds.Clear(); _citizenIds.AddRange(ids); _citizenList.Clear();
            foreach (var citizen in population)
            {
                var identity = _citizens.Identity(citizen.Id);
                _citizenList.AddItem($"#{citizen.Id} {identity?.FullName ?? citizen.Race} · " +
                                     $"{citizen.Race}/{citizen.Class} · {citizen.Profession}");
            }
            var restored = _citizenIds.IndexOf(selectedId);
            if (restored >= 0) _citizenList.Select(restored);
        }
        RefreshCitizenDetails();
    }

    private void RefreshStanding()
    {
        if (_stats is null) return;
        var socialClass = (SocialClass)_standingClass.Selected;
        var groups = _stats.Standing.Groups.Where(value => value.Class == socialClass)
            .OrderByDescending(value => _citizens.PopulationByRaceAndClass()
                .GetValueOrDefault((value.Race, value.Class))).ToArray();
        _standingSummary.Text = $"{socialClass}\n\n" + string.Join("\n\n", groups.Select(value =>
            $"{value.Race} · население {_citizens.PopulationByRaceAndClass().GetValueOrDefault((value.Race, value.Class))}\n" +
            $"Ожидания {value.Expectation:P0} · удовлетворение {value.Fulfillment:P0} · " +
            $"счастье {value.Happiness:P0}\nЛояльность {value.Loyalty:P0} → {value.LoyaltyTarget:P0}"));
    }

    private void SelectCitizen(long index)
    {
        if ((uint)index < (uint)_citizenIds.Count) RefreshCitizenDetails();
    }

    private void RefreshCitizenDetails()
    {
        var snapshot = _citizens.InspectCitizen(SelectedCitizenId());
        _citizenDetails.Text = snapshot is null ? "Выберите жителя." :
            $"#{snapshot.Id} · {snapshot.Name}\n{snapshot.Race} · {snapshot.Class}/{snapshot.Type}\n" +
            $"Профессия {snapshot.Profession} · возраст {snapshot.AgeDays} дн.\n" +
            $"Клетка {snapshot.Cell.X}:{snapshot.Cell.Z} · дом #{snapshot.HomeRoomId}\n" +
            $"Религия {snapshot.Religion} · голод {snapshot.Hunger:0}\n" +
            $"{snapshot.Health} · травма {snapshot.Injury}\nТекущая работа: {snapshot.CurrentJob}\n" +
            $"Потребности: {string.Join(", ", snapshot.HighestNeeds.Select(pair => $"{pair.Key} {pair.Value}"))}";
    }

    private void FocusCitizen()
    {
        var snapshot = _citizens.InspectCitizen(SelectedCitizenId());
        if (snapshot is not null)
        {
            CitizenFocusRequested?.Invoke(snapshot.Cell);
            Visible = false;
        }
    }

    private int SelectedCitizenId()
    {
        var selected = _citizenList.GetSelectedItems();
        return selected.Length == 0 ? 0 : _citizenIds[selected[0]];
    }

    private void RefreshOverview()
    {
        var stats = _stats!;
        var races = _citizens.PopulationByRace().OrderByDescending(pair => pair.Value)
            .Select(pair => $"{pair.Key}: {pair.Value}");
        var classes = _citizens.PopulationByClass().OrderByDescending(pair => pair.Value)
            .Select(pair => $"{pair.Key}: {pair.Value}");
        var groups = _citizens.PopulationByRaceAndClass().OrderByDescending(pair => pair.Value)
            .Take(18).Select(pair => $"{pair.Key.Race}/{pair.Key.Class}: {pair.Value}");
        var governance = _rooms.Governance;
        _overview.Text = $"ДЕМОГРАФИЯ\nВсего {stats.Population} · {string.Join(" · ", classes)}\n" +
            $"Расы: {string.Join(" · ", races)}\nГруппы: {string.Join(" · ", groups)}\n\n" +
            $"БЛАГОПОЛУЧИЕ\nГолодают {stats.Starving} · запас еды {stats.FoodDays:0.0} дн. · " +
            $"жильё {stats.Housed}/{stats.HousingCapacity} ({stats.HousingAccess:P0}) · " +
            $"образование {stats.AverageEducation:P0}\n\n" +
            $"УПРАВЛЕНИЕ\nАдминистрация {governance.Administration:0.#} · дипломатия " +
            $"{governance.Diplomacy:0.#} · знать {governance.Nobility.Active.Count} · " +
            $"казна {governance.Treasury.Balance:0} · прибыль года {governance.Treasury.YearlyProfit:0}";
    }

    private void RefreshLaw()
    {
        var currentRaces = _citizens.PopulationByRace().Keys.OrderBy(value => value).ToArray();
        if (!_races.SequenceEqual(currentRaces))
        {
            _races.Clear(); _races.AddRange(currentRaces); _race.Clear();
            foreach (var value in _races) _race.AddItem(value);
        }
        var socialClass = SelectedClass();
        var crimes = OriginalGameData.Current.Crimes.Values.Where(value => value.Class == socialClass)
            .Select(value => value.Key).OrderBy(value => value).ToArray();
        if (!_crimes.SequenceEqual(crimes))
        {
            _crimes.Clear(); _crimes.AddRange(crimes); _crime.Clear();
            foreach (var value in _crimes) _crime.AddItem(value);
        }
        var punishments = OriginalGameData.Current.Punishments.Values
            .Where(value => value.AvailableClasses.Contains(socialClass)).Select(value => value.Key)
            .OrderBy(value => value).ToArray();
        if (!_punishments.SequenceEqual(punishments))
        {
            _punishments.Clear(); _punishments.AddRange(punishments); _punishment.Clear();
            foreach (var value in _punishments) _punishment.AddItem(value);
        }
        var law = _rooms.Law;
        var facilities = law.Facilities.GroupBy(value => value.Kind).OrderBy(value => value.Key)
            .Select(group => $"{group.Key}: {group.Sum(value => value.Used)}/{group.Sum(value => value.Capacity)}, " +
                             $"работников {group.Sum(value => value.Workers)}").DefaultIfEmpty("нет учреждений");
        _lawSummary.Text = $"Дел {law.Cases.Count} · под стражей {law.InCustody} · охранников " +
            $"{law.GuardWorkers} · комендантский час {(law.Curfew ? "да" : "нет")}\n" +
            string.Join(" · ", facilities);
        var cases = law.Cases.OrderByDescending(value => value.State != GodotSyxPort.Law.LawCaseState.Completed)
            .ThenBy(value => value.CitizenId).ToArray();
        var selectedCase = SelectedCaseId();
        _caseCitizenIds.Clear(); _caseList.Clear();
        foreach (var lawCase in cases)
        {
            _caseCitizenIds.Add(lawCase.CitizenId);
            _caseList.AddItem($"#{lawCase.CitizenId} · {lawCase.Crime} · {lawCase.State}");
        }
        var caseIndex = _caseCitizenIds.IndexOf(selectedCase);
        if (caseIndex >= 0) _caseList.Select(caseIndex);
        RefreshCaseDetails();
        SelectCurrentDecree();
    }

    private void SelectCase(long index) => RefreshCaseDetails();

    private void RefreshCaseDetails()
    {
        var lawCase = _rooms.Law.Cases.FirstOrDefault(value => value.CitizenId == SelectedCaseId());
        _caseDetails.Text = lawCase is null ? "Выберите судебное дело." :
            $"Житель #{lawCase.CitizenId}\n{lawCase.Race} · {lawCase.Class}\n" +
            $"Преступление: {lawCase.Crime}\nНаказание: {lawCase.Punishment}\n" +
            $"Состояние: {lawCase.State}\nПод стражей: {lawCase.DaysInCustody} дн.\n" +
            $"Осталось: {lawCase.SentenceDaysLeft} дн. · учреждение #{lawCase.FacilityRoomId}";
    }

    private void FocusCaseCitizen()
    {
        var snapshot = _citizens.InspectCitizen(SelectedCaseId());
        if (snapshot is null) return;
        CitizenFocusRequested?.Invoke(snapshot.Cell);
        Visible = false;
    }

    private int SelectedCaseId()
    {
        var selected = _caseList.GetSelectedItems();
        return selected.Length == 0 ? 0 : _caseCitizenIds[selected[0]];
    }

    private void SelectCurrentDecree()
    {
        if (_race.Selected < 0 || _race.Selected >= _races.Count ||
            _crime.Selected < 0 || _crime.Selected >= _crimes.Count) return;
        var current = _rooms.Law.Decree(_crimes[_crime.Selected], _races[_race.Selected], SelectedClass());
        var index = _punishments.FindIndex(value => value.Equals(current, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) _punishment.Select(index);
    }

    private void ApplyDecree()
    {
        if (_race.Selected < 0 || _crime.Selected < 0 || _punishment.Selected < 0) return;
        var race = _races[_race.Selected];
        var crime = _crimes[_crime.Selected];
        var punishment = _punishments[_punishment.Selected];
        _rooms.Law.SetDecree(crime, race, SelectedClass(), punishment);
        _status.Text = $"Декрет: {race}/{SelectedClass()} · {crime} → {punishment}";
    }

    private void RefreshTechnology()
    {
        if (_technology.Selected < 0 || _technology.Selected >= _technologies.Count) return;
        var key = _technologies[_technology.Selected];
        var rule = OriginalGameData.Current.Technology(key)!;
        var currencies = _rooms.Technologies.Currencies.Values.OrderBy(value => value.Key).Select(value =>
            $"{value.Key}: всего {value.Total}, занято {value.Allocated}, заморожено {value.Frozen:0.#}, " +
            $"доступно {value.Available}");
        var costs = rule.Costs.Keys.Select(currency =>
            $"{currency} {_rooms.Technologies.CostOfNext(key, currency)}");
        _techSummary.Text = $"{key} · уровень {_rooms.Technologies.Level(key)}/{rule.LevelMaximum} · " +
            $"штраф {_rooms.Technologies.Penalty(key):P0}\nСледующий уровень: " +
            $"{string.Join(", ", costs.DefaultIfEmpty("максимум"))}\n" +
            $"Доступен: {(_rooms.Technologies.CanUnlockNext(key) ? "да" : "нет")}\n\n" +
            string.Join("\n", currencies);
    }

    private void UnlockTechnology()
    {
        if (_technology.Selected < 0 || _technology.Selected >= _technologies.Count) return;
        var key = _technologies[_technology.Selected];
        _status.Text = _rooms.Technologies.UnlockNext(key)
            ? $"Открыта технология {key}, уровень {_rooms.Technologies.Level(key)}"
            : $"Недостаточно знаний или достигнут максимум: {key}";
        RefreshTechnology();
    }

    private void LoadTradePolicy()
    {
        var policy = _rooms.Trade.Policy(SelectedResource());
        _importLevel.Value = policy.ImportLevel * 100;
        _importPrice.Value = policy.MaximumImportPrice;
        _retain.Value = policy.RetainedExportFraction * 100;
        _exportPrice.Value = policy.MinimumExportPrice;
        RefreshTrade();
    }

    private void ApplyTradePolicy()
    {
        var resource = SelectedResource();
        _rooms.Trade.ConfigureImport(resource, _importLevel.Value / 100.0, (int)_importPrice.Value);
        _rooms.Trade.ConfigureExport(resource, _retain.Value / 100.0, (int)_exportPrice.Value);
        _status.Text = $"Торговая политика обновлена: {resource}";
        RefreshTrade();
    }

    private void RefreshTrade()
    {
        var policy = _rooms.Trade.Policy(SelectedResource());
        var transactions = _rooms.Trade.RecentTransactions.TakeLast(8).Select(value =>
            $"{(value.Import ? "импорт" : "экспорт")} {value.Resource} ×{value.Amount} @ {value.UnitPrice}, сбор {value.Fee}")
            .DefaultIfEmpty("Сделок нет: торговля ждёт реальные котировки глобальных фракций.");
        _tradeSummary.Text = $"Партнёров {_rooms.Trade.Quotes.Count} · кредитов {_rooms.Trade.Credits:0}\n" +
            $"{policy.Resource}: импорт {policy.ImportLevel:P0}, максимум {policy.MaximumImportPrice}; " +
            $"сохранить {policy.RetainedExportFraction:P0}, минимум экспорта {policy.MinimumExportPrice}\n\n" +
            string.Join("\n", transactions);
    }

    private SocialClass SelectedClass() => _class.Selected < 0 ? SocialClass.Citizen : (SocialClass)_class.Selected;
    private ResourceKind SelectedResource() => _resource.Selected < 0 ? ResourceKind.Wood : (ResourceKind)_resource.Selected;

    private void AddPageButton(string text, Page page, float x)
    {
        var button = new Button { Text = text, Position = new Vector2(x, 58), Size = new Vector2(100, 30) };
        button.Pressed += () => ShowPage(page); AddChild(button);
    }

    private Label PageLabel(Page page, Vector2 position, Vector2 size, int fontSize)
    {
        var label = AddLabel("", position, fontSize); label.Size = size;
        label.AutowrapMode = TextServer.AutowrapMode.WordSmart; _pages[page].Add(label); return label;
    }

    private OptionButton PageOption(Page page, float x, float y, float width)
    {
        var option = new OptionButton { Position = new Vector2(x, y), Size = new Vector2(width, 30) };
        AddChild(option); _pages[page].Add(option); return option;
    }

    private Button PageButton(Page page, string text, float x, float y, float width)
    {
        var button = new Button { Text = text, Position = new Vector2(x, y), Size = new Vector2(width, 30) };
        AddChild(button); _pages[page].Add(button); return button;
    }

    private ItemList PageItemList(Page page, float x, float y, float width, float height)
    {
        var list = new ItemList { Position = new Vector2(x, y), Size = new Vector2(width, height),
            SelectMode = ItemList.SelectModeEnum.Single };
        AddChild(list); _pages[page].Add(list); return list;
    }

    private SpinBox PageSpin(Page page, string title, float x, float y, double min, double max, double step)
    {
        var label = AddLabel(title, new Vector2(x, y), 12); _pages[page].Add(label);
        var spin = new SpinBox { Position = new Vector2(x, y + 24), Size = new Vector2(174, 30),
            MinValue = min, MaxValue = max, Step = step, AllowGreater = false, AllowLesser = false };
        AddChild(spin); _pages[page].Add(spin); return spin;
    }

    private Label AddLabel(string text, Vector2 position, int size)
    {
        var label = new Label { Text = text, Position = position };
        label.AddThemeFontSizeOverride("font_size", size); AddChild(label); return label;
    }
}
