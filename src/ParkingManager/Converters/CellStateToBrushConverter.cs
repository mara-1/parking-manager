using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ParkingManager.ViewModels;

namespace ParkingManager.Converters;

// Цвет квадратика на схеме занятости: занято — красный, бронь — оранжевый, свободно — зелёный.
public sealed class CellStateToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            CellState.Occupied => Brushes.IndianRed,
            CellState.Booked => Brushes.Orange,
            CellState.Free => Brushes.LightGreen,
            _ => Brushes.LightGray,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
