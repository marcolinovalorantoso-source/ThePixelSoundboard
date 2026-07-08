using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SoundBoard.Models;
using SoundBoard.Services;
using SoundBoard.ViewModels;

namespace SoundBoard.Views
{
    public partial class MyInstantsWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly MyInstantsService _service = new();
        private readonly ObservableCollection<MyInstantUIItem> _items = new();
        private string _currentCategory = "";
        private int _currentPage = 1;
        private bool _isLoading = false;
        private string? _nextPageUrl = null;

        private static readonly Dictionary<string, string> Categories = new()
        {
            { "Tutte le categorie", "" },
            { "Meme", "meme" },
            { "Giochi", "games" },
            { "Musica", "music" },
            { "Effetti Sonori", "sound-effects" },
            { "Film", "movie" },
            { "Anime", "anime" },
            { "Televisione", "television" },
            { "Sport", "sports" },
            { "Politica", "politics" }
        };

        public MyInstantsWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            ThemeService.ApplyDarkTheme(this);
            
            SoundsItemsControl.ItemsSource = _items;
            
            // Imposta cartella selezionata nella barra di stato
            SelectedFolderTextBlock.Text = "Importa in: " + (_viewModel.SelectedFolder?.Name ?? "Tutti i suoni");

            InitializeCategories();
            _ = PerformSearchAsync(reset: true);
        }

        private void InitializeCategories()
        {
            foreach (var cat in Categories)
            {
                var btn = new Button
                {
                    Content = cat.Key,
                    Tag = cat.Value,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Margin = new Thickness(0, 2, 0, 2),
                    Padding = new Thickness(8, 6, 8, 6),
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    Cursor = Cursors.Hand
                };

                btn.Click += CategoryButton_Click;
                CategoriesStack.Children.Add(btn);
            }
            HighlightActiveCategory("Tutte le categorie");
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                _currentCategory = btn.Tag?.ToString() ?? "";
                HighlightActiveCategory(btn.Content.ToString() ?? "");
                _ = PerformSearchAsync(reset: true);
            }
        }

        private void HighlightActiveCategory(string categoryName)
        {
            foreach (var child in CategoriesStack.Children)
            {
                if (child is Button btn)
                {
                    if (btn.Content.ToString() == categoryName)
                    {
                        btn.Background = (Brush)FindResource("BgElevatedBrush");
                        btn.Foreground = (Brush)FindResource("TextPrimaryBrush");
                        btn.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        btn.Background = Brushes.Transparent;
                        btn.Foreground = (Brush)FindResource("TextSecondaryBrush");
                        btn.FontWeight = FontWeights.Normal;
                    }
                }
            }
        }

        private async Task PerformSearchAsync(bool reset = false)
        {
            if (_isLoading) return;
            _isLoading = true;

            if (reset)
            {
                _currentPage = 1;
                _items.Clear();
                _nextPageUrl = null;
                LoadingOverlay.Visibility = Visibility.Visible;
                NoResultsTextBlock.Visibility = Visibility.Collapsed;
                LoadMoreButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                BottomLoadingProgress.Visibility = Visibility.Visible;
            }

            StatusTextBlock.Text = "Ricerca in corso...";

            try
            {
                MyInstantsResponse? response = null;
                if (!reset && !string.IsNullOrEmpty(_nextPageUrl))
                {
                    response = await _service.FetchPageAsync(_nextPageUrl);
                }
                else
                {
                    response = await _service.SearchAsync(SearchTextBox.Text.Trim(), _currentPage, _currentCategory);
                }

                if (response != null && response.Results != null)
                {
                    foreach (var item in response.Results)
                    {
                        _items.Add(new MyInstantUIItem(item));
                    }

                    _nextPageUrl = response.Next;
                    _currentPage++;

                    NoResultsTextBlock.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    LoadMoreButton.Visibility = !string.IsNullOrEmpty(_nextPageUrl) ? Visibility.Visible : Visibility.Collapsed;
                    StatusTextBlock.Text = $"Trovati {_items.Count} suoni";
                }
                else
                {
                    StatusTextBlock.Text = "Errore durante la ricerca.";
                    if (reset) NoResultsTextBlock.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Errore: " + ex.Message;
            }
            finally
            {
                _isLoading = false;
                LoadingOverlay.Visibility = Visibility.Collapsed;
                BottomLoadingProgress.Visibility = Visibility.Collapsed;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _ = PerformSearchAsync(reset: true);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _ = PerformSearchAsync(reset: true);
            }
        }

        private void LoadMoreButton_Click(object sender, RoutedEventArgs e)
        {
            _ = PerformSearchAsync(reset: false);
        }

        private void ResultsScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Scroll infinito: se arriviamo quasi in fondo, carica la pagina successiva
            if (e.ViewportHeight > 0 && e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - 100)
            {
                if (!_isLoading && !string.IsNullOrEmpty(_nextPageUrl))
                {
                    _ = PerformSearchAsync(reset: false);
                }
            }
        }

        private async void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: MyInstantUIItem item })
            {
                StatusTextBlock.Text = $"Riproduzione anteprima: {item.Name}...";
                try
                {
                    var tempFolder = Path.Combine(Path.GetTempPath(), "SoundBoardPreviews");
                    var localPath = await _service.DownloadSoundAsync(item.RawItem, tempFolder);
                    
                    if (localPath != null && File.Exists(localPath))
                    {
                        _viewModel.PlayPreview(localPath);
                    }
                    else
                    {
                        MessageBox.Show("Impossibile scaricare l'anteprima audio.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore di riproduzione: " + ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    StatusTextBlock.Text = "Pronto";
                }
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MyInstantUIItem item)
            {
                btn.IsEnabled = false;
                var originalContent = btn.Content;
                btn.Content = "⌛...";
                StatusTextBlock.Text = $"Download di {item.Name}...";

                try
                {
                    // Cartella permanente per i suoni scaricati
                    var destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds");
                    var localPath = await _service.DownloadSoundAsync(item.RawItem, destFolder);

                    if (localPath != null && File.Exists(localPath))
                    {
                        // Importa nella SoundBoard usando la cartella attualmente selezionata
                        _viewModel.ImportFile(localPath);
                        
                        btn.Content = "✔️ Importato";
                        btn.Background = (Brush)FindResource("SuccessBrush");
                        StatusTextBlock.Text = $"Importato con successo: {item.Name}";
                    }
                    else
                    {
                        MessageBox.Show("Download fallito. Verifica la connessione.", "Errore", MessageBoxButton.OK, MessageBoxImage.Warning);
                        btn.Content = originalContent;
                        btn.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore durante l'importazione: " + ex.Message, "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
                    btn.Content = originalContent;
                    btn.IsEnabled = true;
                }
                finally
                {
                    await Task.Delay(2000);
                    if (btn.Content.ToString() == "✔️ Importato")
                    {
                        btn.Content = originalContent;
                        btn.ClearValue(Button.BackgroundProperty);
                        btn.IsEnabled = true;
                    }
                }
            }
        }
    }

    public class MyInstantUIItem
    {
        public MyInstantItem RawItem { get; }
        public string Name => RawItem.Name;
        
        public Brush ColorBrush
        {
            get
            {
                try
                {
                    var hex = RawItem.Color;
                    if (string.IsNullOrEmpty(hex)) return new SolidColorBrush(Color.FromRgb(91, 140, 90)); // Accent color di fallback
                    if (!hex.StartsWith("#")) hex = "#" + hex;
                    return (Brush?)new BrushConverter().ConvertFromString(hex) ?? new SolidColorBrush(Color.FromRgb(91, 140, 90));
                }
                catch
                {
                    return new SolidColorBrush(Color.FromRgb(91, 140, 90));
                }
            }
        }

        public MyInstantUIItem(MyInstantItem rawItem)
        {
            RawItem = rawItem;
        }
    }
}
