using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SoundBoard.Converters
{
    /// <summary>Converte un bool in Visibility. Passare "Invert" come ConverterParameter per invertire la logica.</summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool b = value is bool bb && bb;
            if (parameter as string == "Invert") b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
