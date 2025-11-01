using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using Microsoft.Data.Sqlite;

namespace Kursovoy.ViewModels;

public class ScheduleEntry
{
    public string GroupNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string ClassType { get; set; } = string.Empty;
    public string ClassNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Teacher { get; set; } = string.Empty;
    public string Classroom { get; set; } = string.Empty;
}
public partial class VisualViewModel : ViewModelBase
{
    [ObservableProperty] private List<string> groups = new();
    [ObservableProperty] private List<string> teachers = new();
    [ObservableProperty] private List<string> classrooms = new();
    [ObservableProperty] private List<string> dates = new();

    [ObservableProperty] private string? selectedGroup;
    [ObservableProperty] private string? selectedTeacher;
    [ObservableProperty] private string? selectedClassroom;
    [ObservableProperty] private string? selectedDate;
    public Dictionary<string, List<ScheduleEntry>> FilteredEntriesGroupedByDate => FilteredEntries
        .OrderBy(e => e.Date)
        .ThenBy(e => e.ClassNumber)
        .GroupBy(e => DateTime.Parse(e.Date).ToString("yyyy-MM-dd"))
        .ToDictionary(g => g.Key, g => g.ToList());
    public ObservableCollection<ScheduleEntry> AllEntries { get; } = new();

    public IEnumerable<ScheduleEntry> FilteredEntries =>
        AllEntries.Where(entry =>
            (string.IsNullOrEmpty(SelectedGroup) || entry.GroupNumber == SelectedGroup) &&
            (string.IsNullOrEmpty(SelectedTeacher) || entry.Teacher == SelectedTeacher) &&
            (string.IsNullOrEmpty(SelectedClassroom) || entry.Classroom == SelectedClassroom) &&
            (string.IsNullOrEmpty(SelectedDate) || entry.Date == SelectedDate)
        );

    public VisualViewModel()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(SelectedGroup) or nameof(SelectedTeacher)
                or nameof(SelectedClassroom) or nameof(SelectedDate))
            {
                OnPropertyChanged(nameof(FilteredEntries));
            }
        };
    }

    [RelayCommand]
    public async Task LoadScheduleAsync()
    {
        var topWindow = Application.Current?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            _ => null
        };

        if (topWindow is null)
            return;

        var dialog = new OpenFileDialog
        {
            Title = "Выберите файл базы данных расписания",
            Filters =
            {
                new FileDialogFilter { Name = "SQLite Database", Extensions = { "db" } }
            },
            AllowMultiple = false
        };

        var result = await dialog.ShowAsync(topWindow);
        if (result == null || result.Length == 0)
            return;

        var filePath = result[0];

        try
        {
            LoadFromDatabase(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке базы данных: {ex.Message}");
        }
    }

    private void LoadFromDatabase(string dbPath)
    {
        var connectionString = $"Data Source={dbPath};";
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT GroupNumber, Date, ClassType, ClassNumber, Subject, Teacher, Classroom FROM Schedule";
        using var reader = command.ExecuteReader();
        AllEntries.Clear();

        while (reader.Read())
        {
            var entry = new ScheduleEntry
            {
                GroupNumber = reader.GetString(0),
                Date = reader.GetString(1),
                ClassType = reader.GetString(2),
                ClassNumber = reader.GetString(3),
                Subject = reader.GetString(4),
                Teacher = reader.GetString(5),
                Classroom = reader.GetString(6)
            };

            AllEntries.Add(entry);
        }

        Groups = AllEntries.Select(e => e.GroupNumber).Distinct().ToList();
        Teachers = AllEntries.Select(e => e.Teacher).Distinct().ToList();
        Classrooms = AllEntries.Select(e => e.Classroom).Distinct().ToList();
        Dates = AllEntries.Select(e => e.Date).Distinct().ToList();

        OnPropertyChanged(nameof(Groups));
        OnPropertyChanged(nameof(Teachers));
        OnPropertyChanged(nameof(Classrooms));
        OnPropertyChanged(nameof(Dates));
        OnPropertyChanged(nameof(FilteredEntries));
    }
    [RelayCommand]

    public void ApplyFilters()
    {
        OnPropertyChanged(nameof(FilteredEntries));
        OnPropertyChanged(nameof(FilteredEntriesGroupedByDate));
    }
    [RelayCommand]
    public void ClearFilters()
    {
        SelectedGroup = null;
        SelectedTeacher = null;
        SelectedClassroom = null;
        SelectedDate = null;

        OnPropertyChanged(nameof(FilteredEntries));
        OnPropertyChanged(nameof(FilteredEntriesGroupedByDate));
    }
}