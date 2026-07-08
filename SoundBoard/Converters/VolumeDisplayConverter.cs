using System;
using System.Globalization;
using System.Windows.Data;

namespace SoundBoard.Converters
{
    /// <summary>Converte un volume 0.0-1.0 in stringa percentuale: "75%"</summary>
    public class VolumePercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return $"{(int)Math.Round(d * 100)}%";
            return "0%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>Converte un volume 0.0-1.0 in stringa decibel: "-6.0 dB" (0.0 → -∞, 1.0 → 0 dB)</summary>
    public class VolumeDbConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                if (d <= 0) return "-∞ dB";
                double db = 20 * Math.Log10(d);
                return $"{db:+0.0;-0.0;0.0} dB";
            }
            return "-∞ dB";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
