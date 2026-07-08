using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace SoundBoard.Services
{
    /// <summary>
    /// Gestisce l'iscrizione/rimozione dell'app dalla chiave di avvio automatico
    /// di Windows (HKCU\...\Run), senza richiedere permessi di amministratore.
    /// </summary>
    public static class StartupService
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "SoundBoard";

        public static void SetStartWithWindows(bool enabled)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
            if (key == null) return;

            if (enabled)
            {
                string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (!string.IsNullOrEmpty(exePath))
                    key.SetValue(AppName, $"\"{exePath}\"");
            }
            else
            {
                if (key.GetValue(AppName) != null)
                    key.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }

        public static bool IsStartWithWindowsEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
            return key?.GetValue(AppName) != null;
        }
    }
}
