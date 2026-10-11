using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Parking.Core.Models;

namespace ParkingManager.Converters;

public sealed class SpotStatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            SpotStatus.Free => Brushes.LightGreen,
            SpotStatus.Occupied => Brushes.IndianRed,
            SpotStatus.Booked => Brushes.Gold,
            _ => Brushes.LightGray,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
