using System;
using System.Windows;
using System.Windows.Threading;

namespace SoundBoard
{
    public partial class App : Application
    {
        public App()
        {
            // Cattura eventuali eccezioni non gestite per evitare crash improvvisi dell'app,
            // mostrando invece un messaggio comprensibile all'utente.
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Si è verificato un errore imprevisto:\n{e.Exception.Message}",
                "SoundBoard - Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
