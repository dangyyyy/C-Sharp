using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;

namespace Kursovoy.ViewModels
{
    public class ScheduleEntr
    {
        public string GroupNumber { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string ClassType { get; set; } = string.Empty;
        public string ClassNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Teacher { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;
    }

    public class AnalysError
    {
        public string Description { get; set; } = string.Empty;
    }

    public partial class AnalysViewModel : ViewModelBase
    {
        private const string DefaultSettingsPath = "settings.json";

        [ObservableProperty] private ObservableCollection<ScheduleEntr> scheduleEntries = new();
        [ObservableProperty] private ObservableCollection<AnalysError> analysErrors = new();

        public ObservableCollection<string> SettingsWarnings { get; } = new();
        public ObservableCollection<string> ScheduleConflicts { get; } = new();

        private SettingsData? settings;

        private bool showWindows = true;
        private bool flagOver4Pairs = true;
        private bool flagOver6Pairs = true;
        private bool highlightEveningClasses = true;

        [RelayCommand]
        public void LoadSettings()
        {
            if (!File.Exists(DefaultSettingsPath))
                return;

            try
            {
                var json = File.ReadAllText(DefaultSettingsPath);
                settings = JsonSerializer.Deserialize<SettingsData>(json);

                if (settings != null)
                {
                    showWindows = settings.ShowWindows;
                    flagOver4Pairs = settings.FlagOver4Pairs;
                    flagOver6Pairs = settings.FlagOver6Pairs;
                    highlightEveningClasses = settings.HighlightEveningClasses;
                }
            }
            catch (Exception ex)
            {
                analysErrors.Add(new AnalysError { Description = $"Ошибка загрузки настроек: {ex.Message}" });
            }
        }

        [RelayCommand]
        public async Task LoadScheduleAsync()
        {
            var topWindow = Avalonia.Application.Current?.ApplicationLifetime switch
            {
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
                _ => null
            };

            if (topWindow == null) return;

            var dialog = new Avalonia.Controls.OpenFileDialog
            {
                Title = "Выберите файл базы данных расписания",
                Filters = { new Avalonia.Controls.FileDialogFilter { Name = "SQLite DB", Extensions = { "db" } } },
                AllowMultiple = false
            };

            var result = await dialog.ShowAsync(topWindow);
            if (result == null || result.Length == 0)
                return;

            var filePath = result[0];
            LoadFromDatabase(filePath);
        }

        private void LoadFromDatabase(string dbPath)
        {
            try
            {
                var connectionString = $"Data Source={dbPath};";
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT GroupNumber, Date, ClassType, ClassNumber, Subject, Teacher, Classroom FROM Schedule";

                using var reader = command.ExecuteReader();
                ScheduleEntries.Clear();

                while (reader.Read())
                {
                    ScheduleEntries.Add(new ScheduleEntr
                    {
                        GroupNumber = reader.GetString(0),
                        Date = reader.GetString(1),
                        ClassType = reader.GetString(2),
                        ClassNumber = reader.GetString(3),
                        Subject = reader.GetString(4),
                        Teacher = reader.GetString(5),
                        Classroom = reader.GetString(6)
                    });
                }
            }
            catch (Exception ex)
            {
                analysErrors.Add(new AnalysError { Description = $"Ошибка загрузки расписания: {ex.Message}" });
            }
        }

        [RelayCommand]
        public void RunAnalysis()
        {
            analysErrors.Clear();
            SettingsWarnings.Clear();
            ScheduleConflicts.Clear();

            if (ScheduleEntries.Count == 0)
            {
                analysErrors.Add(new AnalysError { Description = "Расписание не загружено." });
                return;
            }

            if (settings == null)
                LoadSettings();

            if (settings != null && showWindows)
            {
                CheckPairsCountWarnings();
                CheckEveningPairsWarnings();
            }

            CheckTeacherAndClassroomConflicts();
            CheckNinePairsErrors();

            foreach (var error in analysErrors)
            {
                var descLower = error.Description.ToLower();

                if (descLower.Contains("пересечение") || descLower.Contains("ошибка"))
                    ScheduleConflicts.Add(error.Description);
                else
                    SettingsWarnings.Add(error.Description);
            }
        }

        private void CheckPairsCountWarnings()
        {
            var grouped = ScheduleEntries
                .GroupBy(e => (e.GroupNumber, e.Date))
                .Select(g => new
                {
                    Group = g.Key.GroupNumber,
                    Date = g.Key.Date,
                    Count = g.Count()
                });

            foreach (var day in grouped)
            {
                if (flagOver6Pairs && day.Count > 6)
                {
                    analysErrors.Add(new AnalysError
                    {
                        Description = $"Группа {day.Group} имеет более 6 пар ({day.Count}) в день {day.Date}"
                    });
                }
                else if (flagOver4Pairs && day.Count > 4)
                {
                    analysErrors.Add(new AnalysError
                    {
                        Description = $"Группа {day.Group} имеет более 4 пар ({day.Count}) в день {day.Date}"
                    });
                }
            }
        }

        private void CheckEveningPairsWarnings()
        {
            if (!highlightEveningClasses)
                return;

            var eveningPairs = new HashSet<string> { "5", "6", "7" };
            var eveningClasses = ScheduleEntries
                .Where(e => eveningPairs.Contains(e.ClassNumber))
                .ToList();

            if (eveningClasses.Count > 0)
            {
                var groups = eveningClasses.Select(e => e.GroupNumber).Distinct();
                var dates = eveningClasses.Select(e => e.Date).Distinct();

                analysErrors.Add(new AnalysError
                {
                    Description = $"Обнаружены вечерние пары (5,6 или 7) у групп: {string.Join(", ", groups)} в даты: {string.Join(", ", dates)}"
                });
            }
        }

        private void CheckTeacherAndClassroomConflicts()
        {
            var grouped = ScheduleEntries
                .GroupBy(e => (e.Date, e.ClassNumber))
                .ToList();

            foreach (var group in grouped)
            {
                var teachers = group
                    .GroupBy(e => e.Teacher)
                    .Where(g => g.Select(e => e.GroupNumber).Distinct().Count() > 1)
                    .ToList();

                foreach (var conflictTeacher in teachers)
                {
                    analysErrors.Add(new AnalysError
                    {
                        Description = $"Пересечение преподавателя '{conflictTeacher.Key}' в {group.Key.Date} на паре {group.Key.ClassNumber} у групп: {string.Join(", ", conflictTeacher.Select(e => e.GroupNumber).Distinct())}"
                    });
                }

                var classrooms = group
                    .GroupBy(e => e.Classroom)
                    .Where(g => g.Select(e => e.GroupNumber).Distinct().Count() > 1)
                    .ToList();

                foreach (var conflictClassroom in classrooms)
                {
                    analysErrors.Add(new AnalysError
                    {
                        Description = $"Пересечение аудитории '{conflictClassroom.Key}' в {group.Key.Date} на паре {group.Key.ClassNumber} у групп: {string.Join(", ", conflictClassroom.Select(e => e.GroupNumber).Distinct())}"
                    });
                }
            }
        }

        private void CheckNinePairsErrors()
        {
            var grouped = ScheduleEntries
                .GroupBy(e => (e.GroupNumber, e.Date))
                .Where(g => g.Count() == 9);

            foreach (var day in grouped)
            {
                analysErrors.Add(new AnalysError
                {
                    Description = $"Группа {day.Key.GroupNumber} имеет 9 пар в день {day.Key.Date} — это ошибка!"
                });
            }
        }

        public void AddFakeConflicts()
        {
            analysErrors.Add(new AnalysError { Description = "Искусственная ошибка пересечения 1" });
            analysErrors.Add(new AnalysError { Description = "Искусственная ошибка пересечения 2" });
            analysErrors.Add(new AnalysError { Description = "Искусственная ошибка пересечения 3" });
            analysErrors.Add(new AnalysError { Description = "Искусственная ошибка пересечения 4" });
            analysErrors.Add(new AnalysError { Description = "Искусственная ошибка пересечения 5" });
        }

        private class SettingsData
        {
            public bool ShowWindows { get; set; } = true;
            public bool HighlightEveningClasses { get; set; } = true;
            public bool FlagOver4Pairs { get; set; } = true;
            public bool FlagOver6Pairs { get; set; } = true;
        }
    }
}