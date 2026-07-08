using System;
using System.Globalization;
using System.Windows.Data;

namespace SoundBoard.Converters
{
    /// <summary>Sceglie l'icona da mostrare sul pulsante mute a seconda dello stato IsMuted.</summary>
    public class MuteIconConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => (value is bool muted && muted) ? "🔇" : "🔈";

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
