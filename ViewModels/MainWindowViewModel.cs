using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kursovoy.Views;

namespace Kursovoy.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isPaneOpen = false;

    [ObservableProperty] 
    private ViewModelBase _currentPage = new GenPageViewModel();
    
    [ObservableProperty]
    private ListItemTemplate? _selectedListItem;

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value is null) return;
    
        Console.WriteLine($"Создание экземпляра для: {value.ModelType}");

        var instance = Activator.CreateInstance(value.ModelType);
    
        if (instance is null)
        {
            Console.WriteLine("Не удалось создать экземпляр.");
            return;
        }

        Console.WriteLine($"Экземпляр создан: {instance.GetType().Name}");
        
        CurrentPage = (ViewModelBase)instance;
    }
    
    public ObservableCollection<ListItemTemplate> Items { get; } = new()
    {
        new ListItemTemplate(typeof(GenPageViewModel), "Generate", "Генерация расписания"),
        new ListItemTemplate(typeof(VisualViewModel), "Data", "Визуализация расписания"),
        new ListItemTemplate(typeof(AnalysViewModel),"Analys", "Анализ расписания"),
        new ListItemTemplate(typeof(SettingsViewModel), "SettingsRegular", "Настройки"),
        new ListItemTemplate(typeof(AboutViewModel), "About", "Об авторе"),

    };
    
    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}

public class ListItemTemplate
{
    public ListItemTemplate(Type type, string iconKey, string title)
    {
        ModelType = type;
        Label = title;
        
        Application.Current!.TryFindResource(iconKey, out var res);
        ListItemIcon = (StreamGeometry)res!;
    }
    public string Label { get;}
    public Type ModelType { get; }
    public StreamGeometry ListItemIcon { get;}
}