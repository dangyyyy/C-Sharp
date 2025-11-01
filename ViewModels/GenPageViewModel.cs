using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;

namespace Kursovoy.ViewModels;

public partial class GenPageViewModel : ViewModelBase
{
    [ObservableProperty] private string teacherName = string.Empty;
    [ObservableProperty] private string teacherPreferredDays = string.Empty;
    [ObservableProperty] private string teacherPreferredClassNumbers = string.Empty;

    [ObservableProperty] private string studentGroup = string.Empty;
    [ObservableProperty] private string studentPreferredDays = string.Empty;
    [ObservableProperty] private string studentPreferredClassNumbers = string.Empty;

    private readonly string teacherFilePath = "teacher_preferences.txt";
    private readonly string studentFilePath = "student_preferences.txt";

    [RelayCommand]
    private void SaveTeacherPreferences()
    {
        string data = $"ФИО: {TeacherName}\nДни: {TeacherPreferredDays}\nПары: {TeacherPreferredClassNumbers}\n---\n";
        File.AppendAllText(teacherFilePath, data);

        TeacherName = string.Empty;
        TeacherPreferredDays = string.Empty;
        TeacherPreferredClassNumbers = string.Empty;
    }

    [RelayCommand]
    private void SaveStudentPreferences()
    {
        string data = $"Группа: {StudentGroup}\nДни: {StudentPreferredDays}\nПары: {StudentPreferredClassNumbers}\n---\n";
        File.AppendAllText(studentFilePath, data);

        StudentGroup = string.Empty;
        StudentPreferredDays = string.Empty;
        StudentPreferredClassNumbers = string.Empty;
    }
}