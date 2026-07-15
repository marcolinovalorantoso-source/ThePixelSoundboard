using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SoundBoard.ViewModels;

namespace SoundBoard.Mac
{
    public partial class App : Application
    {
        private MainViewModel? _mainViewModel;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                _mainViewModel = new MainViewModel();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _mainViewModel
                };

                desktop.Exit += (s, e) =>
                {
                    _mainViewModel?.Dispose();
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}