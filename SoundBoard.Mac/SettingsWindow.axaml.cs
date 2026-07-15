using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SoundBoard.ViewModels;

namespace SoundBoard.Mac
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void Close_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OpenDataFolder_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundBoard");
                Directory.CreateDirectory(folder);

                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    System.Diagnostics.Process.Start("explorer.exe", folder);
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    System.Diagnostics.Process.Start("open", folder);
                }
            }
            catch { }
        }

        private void ResetOnboarding_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.IsOnboarded = false;
                // Save immediately
                vm.SaveState();
            }
        }
    }
}
