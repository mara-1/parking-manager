using System.Globalization;
using Parking.Core.Models;
using Parking.Core.Services;

namespace ParkingManager.ViewModels;

// Строка списка броней.
public sealed class BookingViewModel
{
    public BookingViewModel(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        Id = booking.Id;
        CarNumber = booking.CarNumber;
        CategoryName = booking.Category.ToShortName();
        ArrivalText = booking.ArrivalTime.ToString("HH:mm", CultureInfo.InvariantCulture);
        IsWaiting = booking.Status == BookingStatus.Waiting;
        StatusText = booking.Status switch
        {
            BookingStatus.Waiting => $"ожидается, снимется в {BookingRules.HeldUntil(booking).ToString("HH:mm", CultureInfo.InvariantCulture)}",
            BookingStatus.Arrived => "заехал",
            BookingStatus.Cancelled => "отменена",
            BookingStatus.NoShow => "снята — неявка",
            _ => booking.Status.ToString(),
        };
    }

    public int Id { get; }

    public string CarNumber { get; }

    public string CategoryName { get; }

    public string ArrivalText { get; }

    public bool IsWaiting { get; }

    public string StatusText { get; }
}
