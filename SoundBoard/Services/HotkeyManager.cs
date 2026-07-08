using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SoundBoard.Services
{
    /// <summary>
    /// Registra combinazioni di tasti globali (attive anche quando l'app non ha il focus)
    /// usando le API Win32 RegisterHotKey/UnregisterHotKey.
    /// </summary>
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private HwndSource? _source;
        private int _nextId = 1;

        // Mappa: id numerico registrato -> (buttonId, callback)
        private readonly Dictionary<int, (string ButtonId, Action Callback)> _registrations = new();
        // Mappa inversa: buttonId -> id numerico, per poter deregistrare/aggiornare facilmente.
        private readonly Dictionary<string, int> _buttonToId = new();

        /// <summary>Collega il manager alla finestra principale. Va chiamato dopo che la finestra è stata mostrata.</summary>
        public void AttachToWindow(Window window)
        {
            var helper = new WindowInteropHelper(window);
            _source = HwndSource.FromHwnd(helper.Handle);
            _source?.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_registrations.TryGetValue(id, out var entry))
                {
                    entry.Callback.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Registra (o sostituisce) l'hotkey per un pulsante.
        /// La stringa gesture ha il formato "Ctrl+Alt+F1".
        /// Ritorna false se la combinazione non è valida o già occupata dal sistema.
        /// </summary>
        public bool Register(string buttonId, string gesture, Action callback)
        {
            Unregister(buttonId);

            if (_source == null) return false;
            if (!TryParseGesture(gesture, out uint modifiers, out uint vk)) return false;

            int id = _nextId++;
            bool ok = RegisterHotKey(_source.Handle, id, modifiers, vk);
            if (!ok) return false;

            _registrations[id] = (buttonId, callback);
            _buttonToId[buttonId] = id;
            return true;
        }

        /// <summary>Rimuove l'hotkey associata a un pulsante, se presente.</summary>
        public void Unregister(string buttonId)
        {
            if (_buttonToId.TryGetValue(buttonId, out int id))
            {
                if (_source != null)
                    UnregisterHotKey(_source.Handle, id);
                _registrations.Remove(id);
                _buttonToId.Remove(buttonId);
            }
        }

        /// <summary>Converte una stringa tipo "Ctrl+Alt+F1" nei flag Win32 corrispondenti.</summary>
        public static bool TryParseGesture(string gesture, out uint modifiers, out uint vk)
        {
            modifiers = 0;
            vk = 0;
            if (string.IsNullOrWhiteSpace(gesture)) return false;

            var parts = gesture.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0) return false;

            string keyPart = parts[^1];
            for (int i = 0; i < parts.Length - 1; i++)
            {
                switch (parts[i].ToLowerInvariant())
                {
                    case "ctrl": modifiers |= MOD_CONTROL; break;
                    case "alt": modifiers |= MOD_ALT; break;
                    case "shift": modifiers |= MOD_SHIFT; break;
                    case "win": modifiers |= MOD_WIN; break;
                    default: return false;
                }
            }

            if (!Enum.TryParse<Key>(keyPart, true, out var key)) return false;
            vk = (uint)KeyInterop.VirtualKeyFromKey(key);
            return vk != 0;
        }

        /// <summary>Genera una stringa gesture leggibile a partire da modificatori e tasto premuti nella UI.</summary>
        public static string BuildGestureString(ModifierKeys modifiers, Key key)
        {
            var parts = new List<string>();
            if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            parts.Add(key.ToString());
            return string.Join("+", parts);
        }

        public void Dispose()
        {
            foreach (var buttonId in new List<string>(_buttonToId.Keys))
                Unregister(buttonId);
            _source?.RemoveHook(WndProc);
        }
    }
}
