using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Kursovoy.ViewModels;
using System.Threading.Tasks;
using System.Globalization;

namespace Kursovoy.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            this.AttachedToVisualTree += SettingsView_AttachedToVisualTree;
        }
        
        private void OnRemoveHolidayClicked(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is HolidayDateViewModel holiday &&
                DataContext is SettingsViewModel viewModel)
            {
                viewModel.Holidays.Remove(holiday);
            }
        }
        private void OnRemoveBuildingClicked(object? sender, RoutedEventArgs e)
        {
            if (sender is Button button &&
                button.Tag is BuildingItem building &&
                DataContext is SettingsViewModel viewModel)
            {
                viewModel.RemoveBuilding(building);
            }
        }
        
        private void SettingsView_AttachedToVisualTree(object? sender, EventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                vm.ShowOpenFileDialogAsync = ShowOpenFileDialogAsync;
                vm.ShowSaveFileDialogAsync = ShowSaveFileDialogAsync;
            }
        }

        private async Task<string?> ShowOpenFileDialogAsync()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл настроек",
                AllowMultiple = false,
                Filters = { new FileDialogFilter { Name = "JSON файлы", Extensions = { "json" } } }
            };

            var topLevel = this.VisualRoot as Window;
            if (topLevel != null)
            {
                var result = await dialog.ShowAsync(topLevel);
                return result?.Length > 0 ? result[0] : null;
            }

            return null;
        }

        private async Task<string?> ShowSaveFileDialogAsync()
        {
            var dialog = new SaveFileDialog
            {
                Title = "Сохранить файл настроек",
                Filters = { new FileDialogFilter { Name = "JSON файлы", Extensions = { "json" } } },
                DefaultExtension = "json"
            };

            var topLevel = this.VisualRoot as Window;
            if (topLevel != null)
            {
                return await dialog.ShowAsync(topLevel);
            }

            return null;
        }
        
    }
}




