using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using SoundBoard.ViewModels;

namespace SoundBoard.Mac
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Set up Drag & Drop event handlers
            AddHandler(DragDrop.DragOverEvent, DragOver);
            AddHandler(DragDrop.DropEvent, Drop);
        }

        private void DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                e.DragEffects = DragDropEffects.Copy;
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private void Drop(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files) && DataContext is MainViewModel vm)
            {
                var files = e.Data.GetFiles();
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            var path = file.Path.LocalPath;
                            vm.ImportFile(path);
                        }
                        catch { }
                    }
                }
            }
        }

        private void OpenSettings_Click(object? sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow
            {
                DataContext = DataContext
            };
            settings.ShowDialog(this);
        }

        private void ClearAll_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ClearAllSounds();
            }
        }

        private void StopAll_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.StopAll();
            }
        }
    }
}