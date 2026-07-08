using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SoundBoard.Converters
{
    /// <summary>Converte una stringa "#RRGGBB" in un SolidColorBrush per il binding del colore di sfondo dei pulsanti.</summary>
    public class HexColorToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(hex);
                    return new SolidColorBrush(color);
                }
                catch
                {
                    // Colore non valido: fallback a un grigio neutro.
                }
            }
            return new SolidColorBrush(Color.FromRgb(0x3A, 0x3D, 0x4D));
        }

        public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
