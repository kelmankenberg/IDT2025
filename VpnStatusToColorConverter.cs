using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IDT2025
{
    public class VpnStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00cc00")) : Brushes.Red;
            }
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
