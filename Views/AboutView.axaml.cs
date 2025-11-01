using Avalonia.Controls;
using Avalonia.Input;
using System.Diagnostics;

namespace Kursovoy.Views
{
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        private void GitHub_Clicked(object sender, PointerPressedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/dangyyyy",
                UseShellExecute = true
            });
        }
    }
}