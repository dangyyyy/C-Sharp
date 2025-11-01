using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

namespace Kursovoy.ViewModels
{
    public class BreakItem
    {
        public int? PairIndex { get; set; }
        public int? BreakDuration { get; set; }
    }

    public class BuildingItem
    {
        public string Name { get; set; } = string.Empty;
    }

    public partial class HolidayDateViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTimeOffset? date;

        public HolidayDateViewModel(DateTimeOffset? date)
        {
            this.date = date;
        }
    }

    public partial class SettingsViewModel : ViewModelBase
    {
        private const string DefaultSettingsFilePath = "settings.json";

        [ObservableProperty] private DateTimeOffset? semesterStart = DateTimeOffset.Now;
        [ObservableProperty] private int? weeks = 16;
        [ObservableProperty] private ObservableCollection<HolidayDateViewModel> holidays = new();
        [ObservableProperty] private int? pairsPerDay = 3;
        [ObservableProperty] private int? pairDuration = 90;
        [ObservableProperty] private ObservableCollection<BreakItem> breaksBetweenPairs = new();
        [ObservableProperty] private bool isSixDayWeek = false;
        [ObservableProperty] private ObservableCollection<BuildingItem> buildings = new();
        [ObservableProperty] private string newBuildingName = string.Empty;

        [ObservableProperty] private bool showWindows = true;
        [ObservableProperty] private bool highlightEveningClasses = true;
        [ObservableProperty] private bool flagOver4Pairs = true;
        [ObservableProperty] private bool flagOver6Pairs = true;

        public Func<Task<string?>>? ShowOpenFileDialogAsync { get; set; }
        public Func<Task<string?>>? ShowSaveFileDialogAsync { get; set; }

        public SettingsViewModel()
        {
            LoadSettings(DefaultSettingsFilePath);
            OnPairsPerDayChanged(PairsPerDay);
        }

        [ObservableProperty]
        private DateTimeOffset? selectedHolidayDate = DateTimeOffset.Now;

        [RelayCommand]
        private void AddHoliday()
        {
            if (SelectedHolidayDate is not null)
                Holidays.Add(new HolidayDateViewModel(SelectedHolidayDate));
        }

        [RelayCommand]
        public void AddBuilding()
        {
            if (!string.IsNullOrWhiteSpace(NewBuildingName))
            {
                Buildings.Add(new BuildingItem { Name = NewBuildingName.Trim() });
                NewBuildingName = string.Empty;
            }
        }

        [RelayCommand]
        public void RemoveBuilding(BuildingItem? building)
        {
            if (building != null && Buildings.Contains(building))
                Buildings.Remove(building);
        }

        [RelayCommand]
        private async Task SaveSettingsAsync()
        {
            try
            {
                var settingsData = new SettingsData
                {
                    SemesterStart = SemesterStart,
                    Weeks = Weeks ?? 16,
                    Holidays = Holidays.Select(h => (h.Date ?? DateTimeOffset.MinValue).Date).ToList(),
                    PairsPerDay = PairsPerDay ?? 3,
                    PairDuration = PairDuration ?? 90,
                    BreaksBetweenPairs = new ObservableCollection<BreakItem>(BreaksBetweenPairs),
                    IsSixDayWeek = IsSixDayWeek,
                    Buildings = new ObservableCollection<BuildingItem>(Buildings),
                    ShowWindows = ShowWindows,
                    HighlightEveningClasses = HighlightEveningClasses,
                    FlagOver4Pairs = FlagOver4Pairs,
                    FlagOver6Pairs = FlagOver6Pairs
                };

                var json = JsonSerializer.Serialize(settingsData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(DefaultSettingsFilePath, json);
                Console.WriteLine("Настройки успешно сохранены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                var filePath = ShowSaveFileDialogAsync != null
                    ? await ShowSaveFileDialogAsync.Invoke()
                    : DefaultSettingsFilePath;

                if (string.IsNullOrWhiteSpace(filePath)) return;

                var settingsData = new SettingsData
                {
                    SemesterStart = SemesterStart,
                    Weeks = Weeks ?? 16,
                    Holidays = Holidays.Select(h => (h.Date ?? DateTimeOffset.MinValue).Date).ToList(),
                    PairsPerDay = PairsPerDay ?? 3,
                    PairDuration = PairDuration ?? 90,
                    BreaksBetweenPairs = new ObservableCollection<BreakItem>(BreaksBetweenPairs),
                    IsSixDayWeek = IsSixDayWeek,
                    Buildings = new ObservableCollection<BuildingItem>(Buildings),
                    ShowWindows = ShowWindows,
                    HighlightEveningClasses = HighlightEveningClasses,
                    FlagOver4Pairs = FlagOver4Pairs,
                    FlagOver6Pairs = FlagOver6Pairs
                };

                var json = JsonSerializer.Serialize(settingsData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
                Console.WriteLine("Настройки успешно сохранены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                var filePath = ShowOpenFileDialogAsync != null
                    ? await ShowOpenFileDialogAsync.Invoke()
                    : DefaultSettingsFilePath;

                if (string.IsNullOrWhiteSpace(filePath)) return;

                LoadSettings(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        private void LoadSettings(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return;

                var json = File.ReadAllText(filePath);
                var settingsData = JsonSerializer.Deserialize<SettingsData>(json);

                if (settingsData != null)
                {
                    SemesterStart = settingsData.SemesterStart;
                    Weeks = settingsData.Weeks;
                    Holidays = new ObservableCollection<HolidayDateViewModel>(
                        settingsData.Holidays?.Select(d => new HolidayDateViewModel(d)) ?? []);
                    PairsPerDay = settingsData.PairsPerDay;
                    PairDuration = settingsData.PairDuration;
                    BreaksBetweenPairs = settingsData.BreaksBetweenPairs ?? new ObservableCollection<BreakItem>();
                    IsSixDayWeek = settingsData.IsSixDayWeek;
                    Buildings = settingsData.Buildings ?? new ObservableCollection<BuildingItem>();
                    ShowWindows = settingsData.ShowWindows;
                    HighlightEveningClasses = settingsData.HighlightEveningClasses;
                    FlagOver4Pairs = settingsData.FlagOver4Pairs;
                    FlagOver6Pairs = settingsData.FlagOver6Pairs;
                }

                Console.WriteLine("Настройки успешно загружены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке настроек: {ex.Message}");
            }
        }

        partial void OnPairsPerDayChanged(int? value)
        {
            if (value < 1) return;

            var newBreaks = new ObservableCollection<BreakItem>();
            for (int i = 1; i <= value - 1; i++)
            {
                var existing = BreaksBetweenPairs?.FirstOrDefault(b => b.PairIndex == i);
                newBreaks.Add(new BreakItem
                {
                    PairIndex = i,
                    BreakDuration = existing?.BreakDuration ?? 10
                });
            }

            BreaksBetweenPairs = newBreaks;
        }

        private class SettingsData
        {
            public DateTimeOffset? SemesterStart { get; set; }
            public int Weeks { get; set; }
            public List<DateTime> Holidays { get; set; } = new();
            public int PairsPerDay { get; set; }
            public int PairDuration { get; set; }
            public ObservableCollection<BreakItem> BreaksBetweenPairs { get; set; }
            public bool IsSixDayWeek { get; set; }
            public ObservableCollection<BuildingItem> Buildings { get; set; } = new();

            public bool ShowWindows { get; set; }
            public bool HighlightEveningClasses { get; set; }
            public bool FlagOver4Pairs { get; set; }
            public bool FlagOver6Pairs { get; set; }
        }
    }
}