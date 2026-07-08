using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SoundBoard.Services
{
    public static class ThemeService
    {
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        public static void ApplyDarkTheme(Window window)
        {
            if (window == null) return;

            var hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != IntPtr.Zero)
            {
                SetDarkTitleBar(hwnd);
            }
            else
            {
                window.SourceInitialized += (s, e) =>
                {
                    var h = new WindowInteropHelper(window).Handle;
                    SetDarkTitleBar(h);
                };
            }
        }

        private static void SetDarkTitleBar(IntPtr hwnd)
        {
            try
            {
                if (hwnd == IntPtr.Zero) return;

                int attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                if (!IsWindows10OrGreater(20180))
                {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                }

                int useDarkMode = 1;
                DwmSetWindowAttribute(hwnd, attribute, ref useDarkMode, sizeof(int));
            }
            catch
            {
                // Ignora eventuali errori su sistemi non compatibili
            }
        }

        private static bool IsWindows10OrGreater(int build)
        {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }
    }
}
